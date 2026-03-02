/// <summary>
/// Provisions an Azure Static Web App on the Free tier.
/// Cost: $0/month. Includes 100 GB bandwidth, custom domains, SSL.
/// The React/Vite frontend is deployed via the GitHub Actions SWA Action.
/// </summary>

@description('Azure region for the SWA resource (metadata only — CDN is global)')
param location string

@description('Environment tag (dev, prod)')
param environment string

@description('Resource name prefix')
param prefix string

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: '${prefix}-swa-${environment}'
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    buildProperties: {
      skipGithubActionWorkflowGeneration: true  // We manage our own workflow
    }
  }
}

output staticWebAppDefaultHostname string = staticWebApp.properties.defaultHostname
output staticWebAppName string = staticWebApp.name
// Deployment token is retrieved via listSecrets in the GitHub Actions workflow
output staticWebAppId string = staticWebApp.id
