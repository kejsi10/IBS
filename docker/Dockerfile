# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["IBS.sln", "./"]
COPY ["src/BuildingBlocks/IBS.BuildingBlocks.Domain/IBS.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/IBS.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/IBS.BuildingBlocks.Application/IBS.BuildingBlocks.Application.csproj", "src/BuildingBlocks/IBS.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/IBS.BuildingBlocks.Infrastructure/IBS.BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/IBS.BuildingBlocks.Infrastructure/"]
COPY ["src/Contexts/Tenants/IBS.Tenants.Domain/IBS.Tenants.Domain.csproj", "src/Contexts/Tenants/IBS.Tenants.Domain/"]
COPY ["src/Contexts/Tenants/IBS.Tenants.Application/IBS.Tenants.Application.csproj", "src/Contexts/Tenants/IBS.Tenants.Application/"]
COPY ["src/Contexts/Tenants/IBS.Tenants.Infrastructure/IBS.Tenants.Infrastructure.csproj", "src/Contexts/Tenants/IBS.Tenants.Infrastructure/"]
COPY ["src/Contexts/Identity/IBS.Identity.Domain/IBS.Identity.Domain.csproj", "src/Contexts/Identity/IBS.Identity.Domain/"]
COPY ["src/Contexts/Identity/IBS.Identity.Application/IBS.Identity.Application.csproj", "src/Contexts/Identity/IBS.Identity.Application/"]
COPY ["src/Contexts/Identity/IBS.Identity.Infrastructure/IBS.Identity.Infrastructure.csproj", "src/Contexts/Identity/IBS.Identity.Infrastructure/"]
COPY ["src/Contexts/Clients/IBS.Clients.Domain/IBS.Clients.Domain.csproj", "src/Contexts/Clients/IBS.Clients.Domain/"]
COPY ["src/Contexts/Clients/IBS.Clients.Application/IBS.Clients.Application.csproj", "src/Contexts/Clients/IBS.Clients.Application/"]
COPY ["src/Contexts/Clients/IBS.Clients.Infrastructure/IBS.Clients.Infrastructure.csproj", "src/Contexts/Clients/IBS.Clients.Infrastructure/"]
COPY ["src/Contexts/Carriers/IBS.Carriers.Domain/IBS.Carriers.Domain.csproj", "src/Contexts/Carriers/IBS.Carriers.Domain/"]
COPY ["src/Contexts/Carriers/IBS.Carriers.Application/IBS.Carriers.Application.csproj", "src/Contexts/Carriers/IBS.Carriers.Application/"]
COPY ["src/Contexts/Carriers/IBS.Carriers.Infrastructure/IBS.Carriers.Infrastructure.csproj", "src/Contexts/Carriers/IBS.Carriers.Infrastructure/"]
COPY ["src/Contexts/Policies/IBS.Policies.Domain/IBS.Policies.Domain.csproj", "src/Contexts/Policies/IBS.Policies.Domain/"]
COPY ["src/Contexts/Policies/IBS.Policies.Application/IBS.Policies.Application.csproj", "src/Contexts/Policies/IBS.Policies.Application/"]
COPY ["src/Contexts/Policies/IBS.Policies.Infrastructure/IBS.Policies.Infrastructure.csproj", "src/Contexts/Policies/IBS.Policies.Infrastructure/"]
COPY ["src/Contexts/Claims/IBS.Claims.Domain/IBS.Claims.Domain.csproj", "src/Contexts/Claims/IBS.Claims.Domain/"]
COPY ["src/Contexts/Claims/IBS.Claims.Application/IBS.Claims.Application.csproj", "src/Contexts/Claims/IBS.Claims.Application/"]
COPY ["src/Contexts/Claims/IBS.Claims.Infrastructure/IBS.Claims.Infrastructure.csproj", "src/Contexts/Claims/IBS.Claims.Infrastructure/"]
COPY ["src/Contexts/Commissions/IBS.Commissions.Domain/IBS.Commissions.Domain.csproj", "src/Contexts/Commissions/IBS.Commissions.Domain/"]
COPY ["src/Contexts/Commissions/IBS.Commissions.Application/IBS.Commissions.Application.csproj", "src/Contexts/Commissions/IBS.Commissions.Application/"]
COPY ["src/Contexts/Commissions/IBS.Commissions.Infrastructure/IBS.Commissions.Infrastructure.csproj", "src/Contexts/Commissions/IBS.Commissions.Infrastructure/"]
COPY ["src/Contexts/Documents/IBS.Documents.Domain/IBS.Documents.Domain.csproj", "src/Contexts/Documents/IBS.Documents.Domain/"]
COPY ["src/Contexts/Documents/IBS.Documents.Application/IBS.Documents.Application.csproj", "src/Contexts/Documents/IBS.Documents.Application/"]
COPY ["src/Contexts/Documents/IBS.Documents.Infrastructure/IBS.Documents.Infrastructure.csproj", "src/Contexts/Documents/IBS.Documents.Infrastructure/"]
COPY ["src/Contexts/PolicyAssistant/IBS.PolicyAssistant.Domain/IBS.PolicyAssistant.Domain.csproj", "src/Contexts/PolicyAssistant/IBS.PolicyAssistant.Domain/"]
COPY ["src/Contexts/PolicyAssistant/IBS.PolicyAssistant.Application/IBS.PolicyAssistant.Application.csproj", "src/Contexts/PolicyAssistant/IBS.PolicyAssistant.Application/"]
COPY ["src/Contexts/PolicyAssistant/IBS.PolicyAssistant.Infrastructure/IBS.PolicyAssistant.Infrastructure.csproj", "src/Contexts/PolicyAssistant/IBS.PolicyAssistant.Infrastructure/"]
COPY ["src/IBS.Infrastructure/IBS.Infrastructure.csproj", "src/IBS.Infrastructure/"]
COPY ["src/IBS.Api/IBS.Api.csproj", "src/IBS.Api/"]

# Restore dependencies
RUN dotnet restore "src/IBS.Api/IBS.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish
WORKDIR /src/src/IBS.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Download Playwright Chromium into a dedicated directory that will be copied to the runtime stage
ENV PLAYWRIGHT_BROWSERS_PATH=/playwright-browsers
RUN dotnet tool install --global Microsoft.Playwright.CLI \
    && /root/.dotnet/tools/playwright install chromium

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install Chromium system dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libpango-1.0-0 \
    libpangocairo-1.0-0 \
    libx11-xcb1 \
    libxcb-dri3-0 \
    fonts-liberation \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser

# Copy published app and Playwright browsers
COPY --from=build /app/publish .
COPY --from=build /playwright-browsers /playwright-browsers

# Grant appuser access
RUN chown -R appuser:appuser /app /playwright-browsers

ENV PLAYWRIGHT_BROWSERS_PATH=/playwright-browsers

USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "IBS.Api.dll"]
