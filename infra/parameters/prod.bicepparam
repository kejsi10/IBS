using '../main.bicep'

// Production environment parameters — placeholder for future use.
// Copy dev values and adjust sizing before deploying to prod.

param location = 'eastus'
param environment = 'prod'
param prefix = 'ibs'
param sqlAdminLogin = 'ibsadmin'
param containerImage = 'mcr.microsoft.com/dotnet/samples:aspnetapp'

param sqlAdminPassword = ''
param jwtSecretKey = ''
