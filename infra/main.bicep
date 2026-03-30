targetScope = 'resourceGroup'

@description('Primary location for all resources')
param location string = resourceGroup().location

@description('Environment name (dev, staging, prod)')
param environmentName string = 'dev'

@description('Unique resource token')
param resourceToken string = uniqueString(resourceGroup().id)

var tags = {
  'azd-env-name': environmentName
  application: 'app-modernization-wizard'
}

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    location: location
    tags: tags
    resourceToken: resourceToken
  }
}

module appService 'modules/appService.bicep' = {
  name: 'appService'
  params: {
    location: location
    tags: tags
    resourceToken: resourceToken
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
  }
}

output SERVICE_WEB_ENDPOINT string = appService.outputs.endpoint
output SERVICE_WEB_NAME string = appService.outputs.appServiceName
