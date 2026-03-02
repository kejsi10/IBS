using '../main.bicep'

// Dev environment parameters
// Sensitive values (sqlAdminPassword, jwtSecretKey) are injected at deploy time
// via GitHub Actions secrets — do NOT commit real values here.

param location = 'eastus'
param environment = 'dev'
param prefix = 'ibs'
param sqlAdminLogin = 'ibsadmin'
param containerImage = 'mcr.microsoft.com/dotnet/samples:aspnetapp'

// These are overridden by --parameters flags in the deploy workflow:
// --parameters sqlAdminPassword=${{ secrets.SQL_ADMIN_PASSWORD }}
// --parameters jwtSecretKey=${{ secrets.JWT_SECRET_KEY }}
param sqlAdminPassword = ''
param jwtSecretKey = ''
