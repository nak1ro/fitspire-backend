using './main.bicep'

param resourcePrefix = 'replace-with-unique-prefix'
param backendWebAppName = 'replace-with-globally-unique-api-name'
param frontendWebAppName = 'replace-with-globally-unique-web-name'
param postgresAdministratorLogin = 'fitspireadmin'

// Provide the three secure parameters at deployment time. Do not write them here.
