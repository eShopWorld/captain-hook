using Eshopworld.Tests.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CaptainHook.Tests.Cli
{
    public class GenerateJsonCommandTest
    {
        [Fact, IsLayer0]
        [Trait("Command", "GenerateJson")]
        public void Test_SourceWithMore_ThanTarget()
        {
            const string source = @"
                function setConfig ([string]$Name, [string]$Value, [string]$VaultName) {

                    # $secret = Get-AzureKeyVaultSecret -VaultName $VaultName -Name $Name;
                    # if($secret -eq $Value){
                        # Write-Host 'Nothing to update $Name are the same'
                        # return
                    # }

                    Write-Host ""Updating Key $Name\""
                    $secretvalue = ConvertTo - SecureString $Value - AsPlainText - Force
                    $secret = Set - AzureKeyVaultSecret - VaultName $VaultName - Name $Name - SecretValue $secretvalue
                    Write - Host ""$Name has been updated""
                }

                function removeConfig([string]$Name, [string]$Value, [string]$VaultName) {

                    $secret = Get-AzureKeyVaultSecret -VaultName $VaultName -Name $Name;
                    if($secret -eq $Value){
                        Write-Host 'Nothing to update $Name are the same'
                        return
                    }

                    Write-Host ""Deleting Key $Name""
                    Remove-AzureKeyVaultSecret -VaultName $VaultName -Name $Name -Force -Confirm:$False
                    Write-Host ""$Name has been deleted""
                }

                $KeyVault = 'esw-tooling-ci-we'

                ###Add
                setConfig 'AzureSubscriptionId' 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' $KeyVault
                setConfig 'ServiceBusNamespace' 'yyy-yyy-yy' $KeyVault
                setConfig 'ServiceBusConnectionString' 'Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xxx' $KeyVault

                setConfig 'InstrumentationKey' 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' $KeyVault
                setConfig 'CaptainHook--ServiceBusSubscriptionId' 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' $KeyVault
                setConfig 'CaptainHook--ServiceBusConnectionString' 'Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xxx' $KeyVault
                setConfig 'CaptainHook--ServiceBusNamespace' 'xxx' $KeyVault

                setConfig 'CaptainHook--ApiName' 'FancyApiName' $KeyVault
                setConfig 'CaptainHook--ApiSecret' '1234567890' $KeyVault
                setConfig 'CaptainHook--Authority' 'https://authority.domain.com' $KeyVault
                setConfig 'CaptainHook--RequiredScopes--1' 'xxx.yyy.all' $KeyVault


                #First Domain Event for Checkout
                setConfig 'event--1--type' 'checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent' $KeyVault
                setConfig 'event--1--name' 'checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent' $KeyVault
                setConfig 'event--1--webhookconfig--name' 'checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent-webhook' $KeyVault

                # Rules
                setConfig 'event--1--webhookconfig--webhookrequestrules--1--Source--path' 'OrderConfirmationRequestDto' $KeyVault
                setConfig 'event--1--webhookconfig--webhookrequestrules--1--Source--type' 'Model' $KeyVault
                setConfig 'event--1--webhookconfig--webhookrequestrules--1--destination--type' 'Model' $KeyVault
                setConfig 'event--1--webhookconfig--webhookrequestrules--2--source--path' 'TenantCode' $KeyVault
                setConfig 'event--1--webhookconfig--webhookrequestrules--2--destination--ruleaction' 'route' $KeyVault
            ";
        }
    }
}
