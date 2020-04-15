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


#First Domain Event 
setConfig 'event--1--type' 'activity1.domain.infrastructure.domainevents.activityconfirmationdomainevent' $KeyVault
setConfig 'event--1--name' 'activity1.domain.infrastructure.domainevents.activityconfirmationdomainevent' $KeyVault
setConfig 'event--1--webhookconfig--name' 'activity1.domain.infrastructure.domainevents.activityconfirmationdomainevent-webhook' $KeyVault
setConfig 'event--1--webhookconfig--uri' 'https://ct.site.com/api/v1/WebHook/Update' $KeyVault
setConfig 'event--1--webhookconfig--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--authenticationconfig--uri' 'https://security.site.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--authenticationconfig--clientid' 't.abc.client' $KeyVault
setConfig 'event--1--webhookconfig--authenticationconfig--clientsecret' 'verylongsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--authenticationconfig--scopes' 't.abc.client.api.all' $KeyVault
setConfig 'event--1--webhookconfig--httpverb' 'POST' $KeyVault

# Rules
setConfig 'event--1--webhookconfig--webhookrequestrules--1--Source--path' 'ActivityConfirmationRequestDto' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--1--Source--type' 'Model' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--1--destination--type' 'Model' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--source--path' 'TenantCode' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--destination--ruleaction' 'route' $KeyVault

# GOC
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--uri' 'https://activity1-api-casgo.d1.companyname.com/api/v2/retailer/confirmation' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--selector' 'casgo' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--1--httpverb' 'POST' $KeyVault

# xam
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--uri' 'https://activity1-api-xam.d1.companyname.com/api/v2/retailer/confirmation/' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--selector' 'xam' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--2--httpverb' 'POST' $KeyVault

# DT
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--uri' 'https://activity1-api-ftides.d1.companyname.com/api/v2/retailer/confirmation' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--selector' 'ftides' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--3--httpverb' 'POST' $KeyVault

# IBlue
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--4--uri' 'https://activity1-api-bluimm.d1.companyname.com/api/v2/retailer/confirmation' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--4--selector' 'bluimm' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--4--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--4--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--4--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--4--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--4--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--4--httpverb' 'POST' $KeyVault

# bonnet
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--5--uri' 'https://activity1-api-bonnet.d1.companyname.com/api/v2/retailer/confirmation' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--5--selector' 'bonnet' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--5--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--5--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--5--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--5--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--5--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--5--httpverb' 'POST' $KeyVault

# bicce
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--6--uri' 'https://activity1-api-bicce.d1.companyname.com/api/v2/retailer/confirmation' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--6--selector' 'bicce' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--6--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--6--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--6--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--6--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--6--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--6--httpverb' 'POST' $KeyVault

# Marella
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--7--uri' 'https://activity1-api-llarmm.d1.companyname.com/api/v2/retailer/confirmation' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--7--selector' 'llarmm' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--7--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--7--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--7--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--7--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--7--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--7--httpverb' 'POST' $KeyVault

# Sports xam
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--8--uri' 'https://activity1-api-ptmsmm.d1.companyname.com/api/v2/retailer/confirmation' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--8--selector' 'ptmsmm' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--8--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--8--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--8--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--8--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--8--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--8--httpverb' 'POST' $KeyVault

# Too Faced
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--9--uri' 'https://activity1-api-fcdtoo.d1.companyname.com/api/v2/retailer/confirmation' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--9--selector' 'fcdtoo' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--9--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--9--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--9--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--9--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--9--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--9--httpverb' 'POST' $KeyVault

# Weekend xamAram
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--10--uri' 'https://activity1-api-wkndmm.d1.companyname.com/api/v2/retailer/confirmation' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--10--selector' 'wkndmm' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--10--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--10--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--10--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--10--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--10--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--webhookconfig--webhookrequestrules--2--routes--10--httpverb' 'POST' $KeyVault

# Callback
setConfig 'event--1--callbackconfig--name' 'activity1.domain.infrastructure.domainevents.activityorderconfirmationdomainevent-callback' $KeyVault

# Rules
setConfig 'event--1--callbackconfig--webhookrequestrules--1--source--path' 'TenantCode' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--destination--ruleaction' 'route' $KeyVault

# GOC
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--uri' 'https://activity1-api-casgo.d1.companyname.com/api/v2/webhook/PutMessageActionResult' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--selector' 'casgo' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--httpverb' 'POST' $KeyVault

# xam
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--uri' 'https://activity1-api-xam.d1.companyname.com/api/v2/webhook/PutMessageActionResult' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--selector' 'xam' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--httpverb' 'POST' $KeyVault

# DT
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--uri' 'https://activity1-api-ftides.d1.companyname.com/api/v2/webhook/PutMessageActionResult' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--selector' 'ftides' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--3--httpverb' 'POST' $KeyVault

# iBlu
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--4--uri' 'https://activity1-api-bluimm.d1.companyname.com/api/v2/webhook/PutMessageActionResult' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--4--selector' 'bluimm' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--4--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--4--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--4--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--4--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--4--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--4--httpverb' 'POST' $KeyVault

# bonnet
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--5--uri' 'https://activity1-api-bonnet.d1.companyname.com/api/v2/webhook/PutMessageActionResult' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--5--selector' 'bonnet' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--5--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--5--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--5--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--5--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--5--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--5--httpverb' 'POST' $KeyVault

# bicce
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--6--uri' 'https://activity1-api-bicce.d1.companyname.com/api/v2/webhook/PutMessageActionResult' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--6--selector' 'bicce' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--6--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--6--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--6--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--6--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--6--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--6--httpverb' 'POST' $KeyVault

# Marella
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--7--uri' 'https://activity1-api-llarmm.d1.companyname.com/api/v2/webhook/PutMessageActionResult' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--7--selector' 'llarmm' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--7--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--7--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--7--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--7--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--7--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--7--httpverb' 'POST' $KeyVault

# Sports xam
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--8--uri' 'https://activity1-api-ptmsmm.d1.companyname.com/api/v2/webhook/PutMessageActionResult' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--8--selector' 'ptmsmm' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--8--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--8--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--8--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--8--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--8--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--8--httpverb' 'POST' $KeyVault

# Too Faced
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--9--uri' 'https://activity1-api-fcdtoo.d1.companyname.com/api/v2/webhook/PutMessageActionResult' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--9--selector' 'fcdtoo' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--9--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--9--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--9--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--9--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--9--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--9--httpverb' 'POST' $KeyVault

# Weekend xamAram
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--10--uri' 'https://activity1-api-wkndmm.d1.companyname.com/api/v2/webhook/PutMessageActionResult' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--10--selector' 'wkndmm' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--10--authenticationconfig--type' 2 $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--10--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--10--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--10--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--10--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--10--httpverb' 'POST' $KeyVault

setConfig 'event--1--callbackconfig--webhookrequestrules--2--source--type' 'HttpStatusCode' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--2--destination--path' 'StatusCode' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--3--source--type' 'HttpContent' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--3--destination--path' 'Content' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--3--destination--type' 'String' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--4--source--path' 'OrderCode' $KeyVault
setConfig 'event--1--callbackconfig--webhookrequestrules--4--destination--location' 'Uri' $KeyVault


#Second Domain Event for activity1 
setConfig 'event--2--type' 'activity1.domain.infrastructure.domainevents.platformactivitycreatedomainevent' $KeyVault
setConfig 'event--2--name' 'activity1.domain.infrastructure.domainevents.platformactivitycreatedomainevent' $KeyVault

setConfig 'event--2--webhookconfig--name' 'activity1.domain.infrastructure.domainevents.platformactivitycreatedomainevent-webhook' $KeyVault
setConfig 'event--2--webhookconfig--uri' 'https://re1-apiname1.company.com/api/Order' $KeyVault
setConfig 'event--2--webhookconfig--authenticationconfig--type' 'none' $KeyVault
setConfig 'event--2--webhookconfig--httpverb' 'POST' $KeyVault
setConfig 'event--2--webhookconfig--webhookrequestrules--1--source--path' 'PreActivityApiInternalModelActivityRequestDto' $KeyVault
setConfig 'event--2--webhookconfig--webhookrequestrules--1--source--type' 'Model' $KeyVault
setConfig 'event--2--webhookconfig--webhookrequestrules--1--destination--type' 'Model' $KeyVault

# Rules Callback
setConfig 'event--2--callbackconfig--name' 'activity1.domain.infrastructure.domainevents.platformactivitycreatedomainevent-callback' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--destination--ruleaction' 'route' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--source--path' 'TenantCode' $KeyVault

# GOC
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--1--uri' 'https://activity1-api-casgo.d1.companyname.com/api/v2/webhook/PutPlatformOrderCreateResult' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--1--selector' 'casgo' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--type' 2 $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--1--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--1--httpverb' 'POST' $KeyVault

# xam
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--2--uri' 'https://activity1-api-xam.d1.companyname.com/api/v2/webhook/PutPlatformOrderCreateResult' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--2--selector' 'xam' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--type' 2 $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--2--httpverb' 'POST' $KeyVault

# DT
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--3--uri' 'https://activity1-api-ftides.d1.companyname.com/api/v2/webhook/PutPlatformOrderCreateResult' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--3--selector' 'ftides' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--type' 2 $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--3--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--3--httpverb' 'POST' $KeyVault

# iBlu
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--4--uri' 'https://activity1-api-bluimm.d1.companyname.com/api/v2/webhook/PutPlatformOrderCreateResult' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--4--selector' 'bluimm' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--4--authenticationconfig--type' 2 $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--4--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--4--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--4--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--4--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--4--httpverb' 'POST' $KeyVault

# bonnet
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--5--uri' 'https://activity1-api-bonnet.d1.companyname.com/api/v2/webhook/PutPlatformOrderCreateResult' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--5--selector' 'bonnet' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--5--authenticationconfig--type' 2 $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--5--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--5--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--5--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--5--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--5--httpverb' 'POST' $KeyVault

# bicce
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--6--uri' 'https://activity1-api-bicce.d1.companyname.com/api/v2/webhook/PutPlatformOrderCreateResult' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--6--selector' 'bicce' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--6--authenticationconfig--type' 2 $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--6--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--6--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--6--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--6--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--6--httpverb' 'POST' $KeyVault

# Marella
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--7--uri' 'https://activity1-api-llarmm.d1.companyname.com/api/v2/webhook/PutPlatformOrderCreateResult' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--7--selector' 'llarmm' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--7--authenticationconfig--type' 2 $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--7--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--7--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--7--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--7--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--7--httpverb' 'POST' $KeyVault

# Sports xam
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--8--uri' 'https://activity1-api-ptmsmm.d1.companyname.com/api/v2/webhook/PutPlatformOrderCreateResult' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--8--selector' 'ptmsmm' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--8--authenticationconfig--type' 2 $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--8--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--8--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--8--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--8--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--8--httpverb' 'POST' $KeyVault

# Too Faced
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--9--uri' 'https://activity1-api-fcdtoo.d1.companyname.com/api/v2/webhook/PutPlatformOrderCreateResult' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--9--selector' 'fcdtoo' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--9--authenticationconfig--type' 2 $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--9--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--9--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--9--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--9--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--9--httpverb' 'POST' $KeyVault

# Weekend xamAram
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--10--uri' 'https://activity1-api-wkndmm.d1.companyname.com/api/v2/webhook/PutPlatformOrderCreateResult' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--10--selector' 'wkndmm' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--10--authenticationconfig--type' 2 $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--10--authenticationconfig--clientid' 'clientid.test.client' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--10--authenticationconfig--uri' 'https://security.test.company.com/connect/token' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--10--authenticationconfig--clientsecret' 'verylongandsecuresecret' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--10--authenticationconfig--scopes' 'activity1.webhook.api.all' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--1--routes--10--httpverb' 'POST' $KeyVault

#Parsing Rules
setConfig 'event--2--callbackconfig--webhookrequestrules--2--source--type' 'HttpStatusCode' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--2--destination--path' 'StatusCode' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--3--source--type' 'HttpContent' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--3--destination--path' 'Content' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--3--destination--type' 'String' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--4--source--path' 'OrderCode' $KeyVault
setConfig 'event--2--callbackconfig--webhookrequestrules--4--destination--location' 'Uri' $KeyVault