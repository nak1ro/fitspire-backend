[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$ResourceGroup,
    [Parameter(Mandatory)] [string]$PostgresServerName,
    [Parameter(Mandatory)] [string]$BackendWebAppName,
    [Parameter(Mandatory)] [string]$FrontendWebAppName
)

$ErrorActionPreference = 'Stop'

function Get-WebAppOutboundIps {
    param([string]$WebAppName)

    $rawIps = az webapp show `
        --resource-group $ResourceGroup `
        --name $WebAppName `
        --query outboundIpAddresses `
        --output tsv

    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($rawIps)) {
        throw "Could not retrieve outbound IP addresses for App Service '$WebAppName'."
    }

    $rawIps.Split(',', [System.StringSplitOptions]::RemoveEmptyEntries).Trim()
}

$outboundIps = @(
    Get-WebAppOutboundIps -WebAppName $BackendWebAppName
    Get-WebAppOutboundIps -WebAppName $FrontendWebAppName
) | Sort-Object -Unique

$existingRuleNames = az postgres flexible-server firewall-rule list `
    --resource-group $ResourceGroup `
    --server-name $PostgresServerName `
    --query "[?starts_with(name, 'app-service-')].name" `
    --output tsv

if ($LASTEXITCODE -ne 0) {
    throw "Could not list PostgreSQL firewall rules for '$PostgresServerName'."
}

foreach ($ruleName in $existingRuleNames) {
    az postgres flexible-server firewall-rule delete `
        --resource-group $ResourceGroup `
        --server-name $PostgresServerName `
        --name $ruleName `
        --yes

    if ($LASTEXITCODE -ne 0) {
        throw "Could not delete obsolete firewall rule '$ruleName'."
    }
}

for ($index = 0; $index -lt $outboundIps.Count; $index++) {
    $ip = $outboundIps[$index]
    az postgres flexible-server firewall-rule create `
        --resource-group $ResourceGroup `
        --server-name $PostgresServerName `
        --name "app-service-$index" `
        --start-ip-address $ip `
        --end-ip-address $ip `
        --output none

    if ($LASTEXITCODE -ne 0) {
        throw "Could not allow App Service outbound IP '$ip'."
    }
}

Write-Host "Allowed $($outboundIps.Count) current App Service outbound IP address(es) on PostgreSQL '$PostgresServerName'."
