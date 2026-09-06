// OpenDXP - Azure deployment (Container Apps).
//
// NOT YET DEPLOYED. This has been syntax-validated locally (`az bicep build`) but never run
// against a real subscription - see infra/README.md for what that would take and why it hasn't
// happened yet (needs real Azure credentials and explicit approval to spend money).
//
// Deploy at resource-group scope: az deployment group create -g <rg> -f infra/main.bicep

@description('Short name used as a prefix for all resources, e.g. "opendxp".')
param namePrefix string = 'opendxp'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Postgres administrator password. Pass via --parameters postgresAdminPassword=... at deploy time, never commit it.')
@secure()
param postgresAdminPassword string

@description('Container image tag to deploy (e.g. a git SHA or "latest").')
param imageTag string = 'latest'

var containerRegistryName = '${namePrefix}acr${uniqueString(resourceGroup().id)}'
var postgresServerName = '${namePrefix}-postgres-${uniqueString(resourceGroup().id)}'
var redisName = '${namePrefix}-redis-${uniqueString(resourceGroup().id)}'
var logAnalyticsName = '${namePrefix}-logs'
var containerAppsEnvName = '${namePrefix}-env'
var apiAppName = '${namePrefix}-api'
var adminAppName = '${namePrefix}-admin'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: containerRegistryName
  location: location
  sku: { name: 'Basic' }
  properties: { adminUserEnabled: true }
}

// Flexible Server supports the pgvector extension via azure.extensions - required for the
// related-pages feature (Phase 5). Sku is deliberately small (dev/demo scale, not production).
resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2023-06-01-preview' = {
  name: postgresServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: 'opendxp'
    administratorLoginPassword: postgresAdminPassword
    storage: { storageSizeGB: 32 }
    backup: { backupRetentionDays: 7 }
  }
}

resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  parent: postgres
  name: 'opendxp'
}

resource postgresAllowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-06-01-preview' = {
  parent: postgres
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource postgresVectorExtension 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2023-06-01-preview' = {
  parent: postgres
  name: 'azure.extensions'
  properties: {
    value: 'VECTOR'
    source: 'user-override'
  }
}

resource redis 'Microsoft.Cache/redis@2023-08-01' = {
  name: redisName
  location: location
  properties: {
    sku: { name: 'Basic', family: 'C', capacity: 0 }
  }
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: { retentionInDays: 30 }
}

resource containerAppsEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerAppsEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource apiApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: apiAppName
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.listCredentials().username
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        { name: 'registry-password', value: containerRegistry.listCredentials().passwords[0].value }
        { name: 'postgres-password', value: postgresAdminPassword }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: '${containerRegistry.properties.loginServer}/opendxp-api:${imageTag}'
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
            {
              name: 'ConnectionStrings__Postgres'
              value: 'Host=${postgres.properties.fullyQualifiedDomainName};Database=opendxp;Username=opendxp;Password=${postgresAdminPassword};Ssl Mode=Require'
            }
            { name: 'ConnectionStrings__Redis', value: '${redis.properties.hostName}:6380,ssl=true,password=${redis.listKeys().primaryKey}' }
            // Kafka: no managed equivalent wired here yet. Azure Event Hubs' Kafka-compatible
            // endpoint is the closest option, but topic/consumer-group semantics differ enough
            // from Redpanda that it needs its own testing pass, not a blind connection-string swap.
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
}

resource adminApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: adminAppName
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 80
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.listCredentials().username
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        { name: 'registry-password', value: containerRegistry.listCredentials().passwords[0].value }
      ]
    }
    template: {
      containers: [
        {
          name: 'admin'
          image: '${containerRegistry.properties.loginServer}/opendxp-admin:${imageTag}'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 2
      }
    }
  }
}

output apiUrl string = 'https://${apiApp.properties.configuration.ingress.fqdn}'
output adminUrl string = 'https://${adminApp.properties.configuration.ingress.fqdn}'
output containerRegistryLoginServer string = containerRegistry.properties.loginServer
