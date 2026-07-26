using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NLog;
using NLog.Web;
using Pulse.API.Hubs;
using Pulse.Application;
using Pulse.Application.Common.Interfaces;
using Pulse.Infrastructure;

namespace Pulse.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // NLog bootstraps here directly (the one place in this codebase that talks to NLog's
            // own API, rather than the ILogger<T> abstraction) so startup failures - which happen
            // before DI is even built - still get logged instead of disappearing silently.
            var nlogLogger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

            try
            {
                RunApp(args);
            }
            catch (Exception ex)
            {
                nlogLogger.Error(ex, "Pulse.API stopped unexpectedly during startup");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static void RunApp(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddSignalR();

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddScoped<IChatNotificationService, ChatNotificationService>();

            builder.Services.AddProblemDetails();

            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var jwtSection = builder.Configuration.GetSection("Jwt");

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!))
                    };

                    // The SignalR JS client can't attach an Authorization header to the WebSocket
                    // handshake, so it sends the JWT as a query string param instead - accept it
                    // there, but only for requests actually targeting the hub.
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pulse API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter a JWT token to authorize requests."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            // Expected failures (not found, bad input, conflicts) are handled by each
            // controller/handler returning the right result. This only catches genuinely
            // unexpected exceptions, logs the real error server-side, and returns a generic
            // ProblemDetails response so internals never leak to the client.
            app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                if (exception is null)
                {
                    return;
                }

                logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred",
                    Detail = "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path
                };

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(problemDetails);
            }));

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pulse API v1");
                });
            }

            // Railway (and most PaaS hosts) terminate TLS at their edge and forward plain
            // HTTP to the container - without this, the app never sees the request as
            // HTTPS and UseHttpsRedirection() below would force a bad redirect loop.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();

            app.UseCors("Frontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<ChatHub>("/hubs/chat");

            app.Run();
        }
    }
}
