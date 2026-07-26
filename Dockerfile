FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project files first so restore is cached unless a .csproj changes
COPY Pulse.slnx ./
COPY src/Pulse.Domain/Pulse.Domain.csproj src/Pulse.Domain/
COPY src/Pulse.Application/Pulse.Application.csproj src/Pulse.Application/
COPY src/Pulse.Infrastructure/Pulse.Infrastructure.csproj src/Pulse.Infrastructure/
COPY src/Pulse.API/Pulse.API.csproj src/Pulse.API/
RUN dotnet restore src/Pulse.API/Pulse.API.csproj

COPY src/ src/
RUN dotnet publish src/Pulse.API/Pulse.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Pulse.API.dll"]
