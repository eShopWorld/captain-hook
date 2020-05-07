param ([string]$inputFileName =  ".\KeyVaultConfigV2_CI.ps1")

$replacementMap = @{ 
    "'route'" = "'Route'";
    "'oidc'" = "'OIDC'";
    "--SourceSubscriptionName'" = "--sourcesubscriptionname'";
    "--Source--" = "--source--";
    "authenticationconfig--type' 2" = "authenticationconfig--type' 'OIDC'";
    "keyvault" = "KeyVault";
    "--dlqmode' '1'" = "--dlqmode' 'WebHookMode'";
    "setconfig" = "setConfig";
    "--type' 'property'" = "--type' 'Property'";
    "--type' 'none'" = "--type' 'None'";
    "--type' 'model'" = "--type' 'Model'";
}

$ignoreList = @(
    "^\s*$", 
    "^\s*#.*$",
    "source--type' 'Property'",
    "subscribers--\d+--type"
)

[string[]]$original = cat $inputFileName

$newLines = [System.Collections.ArrayList]@()

foreach($line in $original){
    $ignoreList | % { 
        if ($line -match $_){
            continue;
        }
    }
  
    $replacementMap.Keys | % {  
        $line = $line -replace $_, $replacementMap.Item($_)
    }
    
    [void]$newLines.Add($line)
}

$ToNatural = { [regex]::Replace($_, '\d+', { $args[0].Value.PadLeft(20) }) }

[string[]]$sorted = $newLines | Sort-Object $ToNatural

[string]$outFile = ([IO.FileInfo]$inputFileName).BaseName + "-clean" + ([IO.FileInfo]$inputFileName).Extension

set-content -Path $outFile -Value $sorted