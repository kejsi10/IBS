/// <summary>
/// Provisions an LRS Storage Account with the ibs-documents blob container.
/// Cost: ~$0.02/GB/month (LRS hot tier).
/// </summary>

@description('Azure region for all resources')
param location string

@description('Environment tag (dev, prod)')
param environment string

@description('Resource name prefix (lowercase, no dashes for storage)')
param prefix string

// Storage account name must be 3-24 chars, lowercase alphanumeric only
var storageAccountName = replace('${prefix}stor${environment}', '-', '')

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: length(storageAccountName) > 24 ? substring(storageAccountName, 0, 24) : storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource documentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'ibs-documents'
  properties: {
    publicAccess: 'None'
  }
}

output storageAccountName string = storageAccount.name
#disable-next-line outputs-should-not-contain-secrets
output storageConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${az.environment().suffixes.storage}'
