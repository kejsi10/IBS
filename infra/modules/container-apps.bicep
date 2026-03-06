/// <summary>
/// Provisions a Container Apps Environment and the IBS API Container App.
/// Uses a user-assigned managed identity for Key Vault secret access.
/// (User-assigned MI avoids the circular dependency with system-assigned MI.)
/// Cost: ~$0 on consumption plan (first 180K vCPU-seconds and 360K GB-seconds free/month).
/// </summary>

@description('Azure region for all resources')
param location string

@description('Environment tag (dev, prod)')
param environment string

@description('Resource name prefix')
param prefix string

@description('Log Analytics Workspace resource ID for Container Apps environment')
param logAnalyticsWorkspaceId string

@description('Application Insights connection string')
param appInsightsConnectionString string

@description('Container image to deploy (e.g. ghcr.io/org/ibs-api:sha)')
param containerImage string

@description('Key Vault URI for secret references')
param keyVaultUri string

@description('Resource ID of the user-assigned managed identity for Key Vault access')
param managedIdentityId string

@description('SQL connection string secret name in Key Vault')
param sqlSecretName string = 'SqlConnectionString'

@description('JWT secret key secret name in Key Vault')
param jwtSecretName string = 'JwtSecretKey'

@description('Azure OpenAI API key secret name in Key Vault')
param openAiSecretName string = 'AzureOpenAiApiKey'

@description('Azure OpenAI endpoint URL')
param azureOpenAiEndpoint string

@description('Static Web App hostname (for CORS allow-list)')
param swaHostname string

resource containerAppsEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${prefix}-cae-${environment}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: reference(logAnalyticsWorkspaceId, '2023-09-01').customerId
        sharedKey: listKeys(logAnalyticsWorkspaceId, '2023-09-01').primarySharedKey
      }
    }
  }
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${prefix}-api-${environment}'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        corsPolicy: {
          allowedOrigins: [
            'https://${swaHostname}'
            'http://localhost:5173'
          ]
          allowedMethods: ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'OPTIONS']
          allowedHeaders: ['*']
          allowCredentials: true
        }
      }
      secrets: [
        {
          name: 'sql-connection'
          keyVaultUrl: '${keyVaultUri}secrets/${sqlSecretName}'
          identity: managedIdentityId
        }
        {
          name: 'jwt-secret'
          keyVaultUrl: '${keyVaultUri}secrets/${jwtSecretName}'
          identity: managedIdentityId
        }
        {
          name: 'openai-api-key'
          keyVaultUrl: '${keyVaultUri}secrets/${openAiSecretName}'
          identity: managedIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'ibs-api'
          image: containerImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'sql-connection'
            }
            {
              name: 'Jwt__SecretKey'
              secretRef: 'jwt-secret'
            }
            {
              name: 'AzureOpenAI__Endpoint'
              value: azureOpenAiEndpoint
            }
            {
              name: 'AzureOpenAI__DeploymentName'
              value: 'gpt-4o-mini'
            }
            {
              name: 'AzureOpenAI__ApiKey'
              secretRef: 'openai-api-key'
            }
            {
              name: 'Cors__AllowedOrigins__0'
              value: 'https://${swaHostname}'
            }
            {
              name: 'PolicyAssistant__Provider'
              value: 'Azure'
            }
            {
              name: 'Documents__Provider'
              value: 'Azure'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsightsConnectionString
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 8080
              }
              periodSeconds: 30
              failureThreshold: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health'
                port: 8080
              }
              initialDelaySeconds: 5
              periodSeconds: 10
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0  // Scale to zero when idle to minimize cost
        maxReplicas: 3
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '20'
              }
            }
          }
        ]
      }
    }
  }
}

output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output containerAppName string = containerApp.name
