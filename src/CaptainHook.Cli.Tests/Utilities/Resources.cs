
namespace CaptainHook.Cli.Tests.Utilities
{
    internal static class Resources
    {
        public static string ValidFileContentNoHeader = @"
            setConfig 'event--1--type' 'checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent' $KeyVault
            setConfig 'event--1--name' 'checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent' $KeyVault
            setConfig 'event--1--webhookconfig--name' 'checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent-webhook' $KeyVault
        ";
    }
}
