param location string
param tags object
param resourceToken string
param appInsightsConnectionString string

var appServicePlanName = 'plan-appmod-${resourceToken}'
var appServiceName = 'app-appmod-${resourceToken}'

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource appService 'Microsoft.Web/sites@2024-04-01' = {
  name: appServiceName
  location: location
  tags: union(tags, { 'azd-service-name': 'web' })
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: '/health'
      webSocketsEnabled: true
      alwaysOn: true
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
      ]
    }
    clientAffinityEnabled: true // Required for Blazor Server SignalR sticky sessions
  }
}

output endpoint string = 'https://${appService.properties.defaultHostName}'
output appServiceName string = appService.name
output principalId string = appService.identity.principalId
