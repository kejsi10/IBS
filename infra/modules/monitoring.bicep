/// <summary>
/// Provisions a Log Analytics Workspace and Application Insights instance.
/// Both use the free/consumption tier to stay within the Azure Monitor free allowances.
/// </summary>

@description('Azure region for all resources')
param location string

@description('Environment tag (dev, prod)')
param environment string

@description('Resource name prefix')
param prefix string

// Log Analytics Workspace (free 5 GB/month ingestion)
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${prefix}-logs-${environment}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// Application Insights (free 5 GB/month ingestion)
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${prefix}-ai-${environment}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
  }
}

output logAnalyticsWorkspaceId string = logAnalytics.id
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
