=== ItemManagement.Api/Dockerfile ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything (keeps Dockerfile robust even if project name differs)
COPY . .

# Find and restore the first csproj found
RUN set -eux; \
    CSPROJ="$(find . -maxdepth 2 -name '*.csproj' | head -n 1)"; \
    echo "Using csproj: $CSPROJ"; \
    dotnet restore "$CSPROJ"

# Publish
RUN set -eux; \
    CSPROJ="$(find . -maxdepth 2 -name '*.csproj' | head -n 1)"; \
    dotnet publish "$CSPROJ" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080

# Run the first DLL in output (robust if assembly name changes)
ENTRYPOINT ["bash","-lc","DLL=$(ls -1 *.dll | head -n 1); echo Running $DLL; dotnet $DLL"]
