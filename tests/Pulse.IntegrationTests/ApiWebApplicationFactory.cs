using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pulse.API;
using Pulse.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Pulse.IntegrationTests
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-alpine").Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString());
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            // Accessing Services forces the WebApplicationFactory to build the host, which
            // reads the container's connection string set in ConfigureWebHost above - the
            // container must already be running by this point for GetConnectionString() to
            // return the real mapped port rather than throwing.
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PulseDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }
    }
}
