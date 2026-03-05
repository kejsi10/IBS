/// <summary>
/// Provisions an Azure OpenAI account with a gpt-4o-mini deployment.
/// Cost: ~$0.15/$0.60 per 1M input/output tokens (pay-per-token).
/// Deployment capacity: 10K TPM (sufficient for dev usage).
/// </summary>

@description('Azure region — must support Azure OpenAI (e.g., eastus, swedencentral)')
param location string

@description('Environment tag (dev, prod)')
param environment string

@description('Resource name prefix')
param prefix string

resource openAiAccount 'Microsoft.CognitiveServices/accounts@2024-04-01-preview' = {
  name: '${prefix}-openai-${environment}'
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: '${prefix}-openai-${environment}'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

resource gpt4oMiniDeployment 'Microsoft.CognitiveServices/accounts/deployments@2024-04-01-preview' = {
  parent: openAiAccount
  name: 'gpt-4o-mini'
  sku: {
    name: 'Standard'
    capacity: 10  // 10K tokens per minute
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o-mini'
      version: '2024-07-18'
    }
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
  }
}

output openAiEndpoint string = openAiAccount.properties.endpoint
#disable-next-line outputs-should-not-contain-secrets
output openAiApiKey string = openAiAccount.listKeys().key1
