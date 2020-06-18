function setConfig ([string]$Name, [string]$Value, [string]$VaultName) {

    # $secret = Get-AzureKeyVaultSecret -VaultName $VaultName -Name $Name;
    # if($secret -eq $Value){
        # Write-Host 'Nothing to update $Name are the same'
        # return
    # }

    Write-Host "Updating Key $Name"
    $secretvalue = ConvertTo-SecureString $Value -AsPlainText -Force
    $secret = Set-AzureKeyVaultSecret -VaultName $VaultName -Name $Name -SecretValue $secretvalue    
    Write-Host "$Name has been updated"
}

function removeConfig ([string]$Name, [string]$Value, [string]$VaultName) {

    $secret = Get-AzureKeyVaultSecret -VaultName $VaultName -Name $Name;
    if($secret -eq $Value){
        Write-Host 'Nothing to update $Name are the same'
        return
    }

    Write-Host "Deleting Key $Name"
    Remove-AzureKeyVaultSecret -VaultName $VaultName -Name $Name -Force -Confirm:$False
    Write-Host "$Name has been deleted"
}

$KeyVault = 'esw-sample-kv-we'

###Add
setConfig 'AzureSubscriptionId' 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' $KeyVault
setConfig 'ServiceBusNamespace' 'aaa-bbb-cc' $KeyVault
setConfig 'ServiceBusConnectionString' 'Endpoint=sb://aaa-bbb-cc.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abcdefghijklmnopqrstuvwxyz' $KeyVault

setConfig 'InstrumentationKey' 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' $KeyVault
setConfig 'CaptainHook--ServiceBusSubscriptionId' 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee' $KeyVault
setConfig 'CaptainHook--ServiceBusConnectionString' 'Endpoint=sb://aaa-bbb-cc.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abcdefghijklmnopqrstuvwxyz' $KeyVault 
setConfig 'CaptainHook--ServiceBusNamespace' 'aaa-bbb-cc' $KeyVault

setConfig 'CaptainHook--ApiName' 'Captain Hook' $KeyVault
setConfig 'CaptainHook--ApiSecret' 'verylongandsecuresecret' $KeyVault
setConfig 'CaptainHook--Authority' 'https://security-sts.d1.companyname.com' $KeyVault
setConfig 'CaptainHook--RequiredScopes--1' 'aaa.bbb.ccc' $KeyVault
