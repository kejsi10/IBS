/// <summary>
/// IBS Azure Infrastructure — main orchestration template.
/// Deploys all resources for the given environment to rg-ibs-{environment}.
///
/// Deployment order:
///   1. Managed Identity (user-assigned) — avoids circular dependency between Container App and Key Vault
///   2. Monitoring (Log Analytics + Application Insights)
///   3. Storage
///   4. SQL Database
///   5. Azure OpenAI
///   6. Key Vault (grants MI access, stores secrets)
///   7. Static Web App
///   8. Container Apps (references KV secrets via MI)
/// </summary>

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Environment name used in resource names and tags (dev, prod)')
@allowed(['dev', 'prod'])
param environment string = 'dev'

@description('Short prefix for resource names')
param prefix string = 'ibs'

@description('SQL administrator login')
param sqlAdminLogin string = 'ibsadmin'

@description('SQL administrator password — injected from Key Vault / GitHub Secrets')
@secure()
param sqlAdminPassword string

@description('JWT secret key — injected from GitHub Secrets')
@secure()
param jwtSecretKey string

@description('Initial container image to deploy')
param containerImage string = 'mcr.microsoft.com/dotnet/samples:aspnetapp'

// ---------------------------------------------------------------------------
// User-Assigned Managed Identity
// Using user-assigned MI avoids the circular dependency that would occur with
// system-assigned MI (Container App needs KV URI; KV needs Container App's MI).
// ---------------------------------------------------------------------------
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${prefix}-mi-${environment}'
  location: location
}

// ---------------------------------------------------------------------------
// Monitoring
// ---------------------------------------------------------------------------
module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    location: location
    environment: environment
    prefix: prefix
  }
}

// ---------------------------------------------------------------------------
// Storage
// ---------------------------------------------------------------------------
module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    location: location
    environment: environment
    prefix: prefix
  }
}

// ---------------------------------------------------------------------------
// SQL Database
// ---------------------------------------------------------------------------
module sql 'modules/sql-database.bicep' = {
  name: 'sql-database'
  params: {
    location: location
    environment: environment
    prefix: prefix
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
  }
}

// ---------------------------------------------------------------------------
// Azure OpenAI (must be in a supported region — eastus, swedencentral, etc.)
// ---------------------------------------------------------------------------
module openai 'modules/openai.bicep' = {
  name: 'openai'
  params: {
    location: location
    environment: environment
    prefix: prefix
  }
}

// ---------------------------------------------------------------------------
// Static Web App (deploy first so we have the hostname for CORS)
// ---------------------------------------------------------------------------
module swa 'modules/static-web-app.bicep' = {
  name: 'static-web-app'
  params: {
    location: location
    environment: environment
    prefix: prefix
  }
}

// ---------------------------------------------------------------------------
// Key Vault — stores secrets and grants MI access
// ---------------------------------------------------------------------------
module keyVault 'modules/key-vault.bicep' = {
  name: 'key-vault'
  params: {
    location: location
    environment: environment
    prefix: prefix
    containerAppPrincipalId: managedIdentity.properties.principalId
    sqlConnectionString: sql.outputs.connectionString
    jwtSecretKey: jwtSecretKey
    azureOpenAiApiKey: openai.outputs.openAiApiKey
  }
}

// ---------------------------------------------------------------------------
// Container Apps
// ---------------------------------------------------------------------------
module containerApps 'modules/container-apps.bicep' = {
  name: 'container-apps'
  params: {
    location: location
    environment: environment
    prefix: prefix
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
    containerImage: containerImage
    keyVaultUri: keyVault.outputs.keyVaultUri
    managedIdentityId: managedIdentity.id
    managedIdentityClientId: managedIdentity.properties.clientId
    azureOpenAiEndpoint: openai.outputs.openAiEndpoint
    swaHostname: swa.outputs.staticWebAppDefaultHostname
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------
output frontendUrl string = 'https://${swa.outputs.staticWebAppDefaultHostname}'
output apiUrl string = 'https://${containerApps.outputs.containerAppFqdn}'
output apiHealthUrl string = 'https://${containerApps.outputs.containerAppFqdn}/health'
output swaggerUrl string = 'https://${containerApps.outputs.containerAppFqdn}/swagger'
output containerAppName string = containerApps.outputs.containerAppName
output staticWebAppName string = swa.outputs.staticWebAppName
output sqlServerFqdn string = sql.outputs.sqlServerFqdn
output keyVaultName string = keyVault.outputs.keyVaultName
