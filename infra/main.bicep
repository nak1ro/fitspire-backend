targetScope = 'resourceGroup'

@description('Poland Central is the selected production region.')
param location string = 'polandcentral'

@minLength(5)
@maxLength(30)
@description('Lowercase letters and digits only. It must make the ACR and storage-account names globally unique.')
param resourcePrefix string

@description('Globally unique App Service name for the API.')
param backendWebAppName string

@description('Globally unique App Service name for the frontend.')
param frontendWebAppName string

@description('PostgreSQL administrator login name.')
param postgresAdministratorLogin string

@secure()
@description('PostgreSQL administrator password. Supply at deployment time; never commit it.')
param postgresAdministratorPassword string

@secure()
@minLength(32)
@description('Random JWT signing key, at least 32 bytes.')
param jwtSigningKey string

@secure()
@minLength(32)
@description('Random NextAuth secret, at least 32 bytes.')
param nextAuthSecret string

param frontendHostname string = 'app.fitspire.life'
param apiHostname string = 'api.fitspire.life'

var planName = '${resourcePrefix}-plan'
var registryName = '${resourcePrefix}acr'
var storageName = '${resourcePrefix}store'
var keyVaultName = '${resourcePrefix}-kv'
var postgresName = '${resourcePrefix}-pg'
var databaseName = 'fitspire'
var appServicePlanSku = 'B1'

var acrPullRoleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
var storageBlobDataContributorRoleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
var keyVaultSecretsUserRoleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  kind: 'linux'
  sku: {
    name: appServicePlanSku
    tier: 'Basic'
  }
  properties: {
    reserved: true
  }
}

resource registry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: registryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: 'Enabled'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: [
            'https://${frontendHostname}'
          ]
          allowedMethods: [
            'GET'
            'HEAD'
            'PUT'
            'OPTIONS'
          ]
          allowedHeaders: [
            'content-type'
            'x-ms-blob-type'
            'x-ms-client-request-id'
            'x-ms-date'
            'x-ms-version'
          ]
          exposedHeaders: [
            'etag'
            'x-ms-request-id'
            'x-ms-version'
          ]
          maxAgeInSeconds: 3600
        }
      ]
    }
  }
}

resource mediaContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'media'
  properties: {
    publicAccess: 'None'
  }
}

resource dataProtectionContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'data-protection'
  properties: {
    publicAccess: 'None'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    enableRbacAuthorization: true
    enablePurgeProtection: true
    publicNetworkAccess: 'Enabled'
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
  }
}

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: postgresName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: postgresAdministratorLogin
    administratorLoginPassword: postgresAdministratorPassword
    version: '16'
    storage: {
      storageSizeGB: 32
      autoGrow: 'Enabled'
      tier: 'P4'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: postgres
  name: databaseName
  properties: {}
}

resource backendApp 'Microsoft.Web/sites@2024-04-01' = {
  name: backendWebAppName
  location: location
  kind: 'app,linux,container'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    serverFarmId: appServicePlan.id
    siteConfig: {
      acrUseManagedIdentityCreds: true
      alwaysOn: true
      ftpsState: 'Disabled'
      healthCheckPath: '/health/ready'
      linuxFxVersion: 'DOCKER|${registry.properties.loginServer}/fitspire-backend:bootstrap'
      minTlsVersion: '1.2'
    }
  }
}

resource frontendApp 'Microsoft.Web/sites@2024-04-01' = {
  name: frontendWebAppName
  location: location
  kind: 'app,linux,container'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    serverFarmId: appServicePlan.id
    siteConfig: {
      acrUseManagedIdentityCreds: true
      alwaysOn: true
      ftpsState: 'Disabled'
      linuxFxVersion: 'DOCKER|${registry.properties.loginServer}/fitspire-web:bootstrap'
      minTlsVersion: '1.2'
    }
  }
}

resource postgresConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'postgres-connection-string'
  properties: {
    value: 'Host=${postgres.properties.fullyQualifiedDomainName};Port=5432;Database=${databaseName};Username=${postgresAdministratorLogin};Password=${postgresAdministratorPassword};Ssl Mode=Require;Trust Server Certificate=false'
  }
}

resource jwtSigningKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'jwt-signing-key'
  properties: {
    value: jwtSigningKey
  }
}

resource nextAuthSecretSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'nextauth-secret'
  properties: {
    value: nextAuthSecret
  }
}

resource backendAppSettings 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: backendApp
  name: 'appsettings'
  properties: {
    ASPNETCORE_ENVIRONMENT: 'Production'
    WEBSITES_PORT: '8080'
    ConnectionStrings__DefaultConnection: '@Microsoft.KeyVault(SecretUri=${postgresConnectionStringSecret.properties.secretUriWithVersion})'
    JWT__SigningKey: '@Microsoft.KeyVault(SecretUri=${jwtSigningKeySecret.properties.secretUriWithVersion})'
    Cors__AllowedOrigins__0: 'https://${frontendHostname}'
    Frontend__BaseUrl: 'https://${frontendHostname}'
    Email__UseMockEmail: 'true'
    OpenAI__Enabled: 'false'
    MediaStorage__ContainerName: mediaContainer.name
    MediaStorage__ServiceUrl: 'https://${storageAccount.name}.blob.core.windows.net'
    DataProtection__ContainerName: dataProtectionContainer.name
    DataProtection__ServiceUri: 'https://${storageAccount.name}.blob.core.windows.net'
    Startup__ApplyMigrationsOnStartup: 'true'
  }
}

resource frontendAppSettings 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: frontendApp
  name: 'appsettings'
  properties: {
    WEBSITES_PORT: '3000'
    AUTH_SECRET: '@Microsoft.KeyVault(SecretUri=${nextAuthSecretSecret.properties.secretUriWithVersion})'
    AUTH_TRUST_HOST: 'true'
    NEXT_PUBLIC_API_URL: 'https://${apiHostname}'
  }
}

resource backendAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: registry
  name: guid(registry.id, backendApp.id, acrPullRoleDefinitionId)
  properties: {
    principalId: backendApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: acrPullRoleDefinitionId
  }
}

resource frontendAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: registry
  name: guid(registry.id, frontendApp.id, acrPullRoleDefinitionId)
  properties: {
    principalId: frontendApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: acrPullRoleDefinitionId
  }
}

resource backendStorageAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, backendApp.id, storageBlobDataContributorRoleDefinitionId)
  properties: {
    principalId: backendApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataContributorRoleDefinitionId
  }
}

resource backendKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, backendApp.id, keyVaultSecretsUserRoleDefinitionId)
  properties: {
    principalId: backendApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: keyVaultSecretsUserRoleDefinitionId
  }
}

resource frontendKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, frontendApp.id, keyVaultSecretsUserRoleDefinitionId)
  properties: {
    principalId: frontendApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: keyVaultSecretsUserRoleDefinitionId
  }
}

var appServiceOutboundIps = union(split(backendApp.properties.outboundIpAddresses, ','), split(frontendApp.properties.outboundIpAddresses, ','))

resource appServiceFirewallRules 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = [for (ip, index) in appServiceOutboundIps: {
  parent: postgres
  name: 'app-service-${index}'
  properties: {
    startIpAddress: ip
    endIpAddress: ip
  }
}]

output backendDefaultHostname string = backendApp.properties.defaultHostName
output frontendDefaultHostname string = frontendApp.properties.defaultHostName
output registryLoginServer string = registry.properties.loginServer
output keyVaultUri string = keyVault.properties.vaultUri
output postgresServerName string = postgres.name
