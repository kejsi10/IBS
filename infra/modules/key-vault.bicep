/// <summary>
/// Provisions an Azure Key Vault with RBAC authorization model.
/// The Container App's system-assigned managed identity is granted Key Vault Secrets User.
/// </summary>

@description('Azure region for all resources')
param location string

@description('Environment tag (dev, prod)')
param environment string

@description('Resource name prefix')
param prefix string

@description('Object ID of the Container App managed identity to grant secrets access')
param containerAppPrincipalId string

@description('SQL connection string to store as secret')
@secure()
param sqlConnectionString string

@description('JWT secret key to store as secret')
@secure()
param jwtSecretKey string

@description('Azure OpenAI API key to store as secret')
@secure()
param azureOpenAiApiKey string

// Key Vault Secrets User role definition ID (built-in)
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${prefix}-kv-${environment}'
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    publicNetworkAccess: 'Enabled'
  }
}

// Grant the Container App managed identity read access to secrets
resource kvRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, containerAppPrincipalId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: containerAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource secretSqlConnection 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'SqlConnectionString'
  properties: {
    value: sqlConnectionString
  }
}

resource secretJwt 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'JwtSecretKey'
  properties: {
    value: jwtSecretKey
  }
}

resource secretOpenAi 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'AzureOpenAiApiKey'
  properties: {
    value: azureOpenAiApiKey
  }
}

output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
