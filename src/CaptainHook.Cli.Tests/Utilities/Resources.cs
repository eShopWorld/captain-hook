
namespace CaptainHook.Cli.Tests.Utilities
{
    internal static class Resources
    {
        public static string ValidFileNoHeaderContent = @"
            setConfig 'event--1--type' 'xxxxxx.yyyyyy.zzzzzzzzz.aaaaaaaaa.ccccccccccccccccccccc' $KeyVault
            setConfig 'event--1--name' 'xxxxxx.yyyyyy.zzzzzzzzz.aaaaaaaaa.ccccccccccccccccccccc' $KeyVault
            setConfig 'event--1--webhookconfig--name' 'xxxxxx.yyyyyy.zzzzzzzzz.aaaaaaaaa.ccccccccccccccccccccc-webhook' $KeyVault
        ";

        public static string SingleRouteFileContent = @"
            setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--1--uri' 'https://xxxxxx-yyy-abcde.ci.eshopworld.net/api/v2/webhook/WebhookName' $KeyVault
        ";

        public static string AuthFileContent = @"
            setConfig 'event--1--type' 'xxxxxx.yyyyyy.zzzzzzzzz.aaaaaaaaa.ccccccccccccccccccccc' $KeyVault
            setConfig 'event--1--name' 'xxxxxx.yyyyyy.zzzzzzzzz.aaaaaaaaa.ccccccccccccccccccccc' $KeyVault

            setConfig 'event--1--webhookconfig--name' 'xxxxxx.yyyyyy.zzzzzzzzz.aaaaaaaaa.ccccccccccccccccccccc-webhook' $KeyVault
            setConfig 'event--1--webhookconfig--uri' 'https://re-api.test.net/api/Order' $KeyVault
            setConfig 'event--1--webhookconfig--authenticationconfig--type' 'none' $KeyVault
            setConfig 'event--1--webhookconfig--httpverb' 'POST' $KeyVault
            setConfig 'event--1--webhookconfig--webhookrequestrules--1--source--path' 'OrderRequestDto' $KeyVault
            setConfig 'event--1--webhookconfig--webhookrequestrules--1--source--type' 'Model' $KeyVault
            setConfig 'event--1--webhookconfig--webhookrequestrules--1--destination--type' 'Model' $KeyVault

            setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--uri' 'https://xxxxxx-yyy-abcde.ci.eshopworld.net/api/v2/webhook/WebhookName' $KeyVault
            setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--selector' 'test' $KeyVault
            setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--type' 2 $KeyVault
            setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--clientid' 'test.eda.client' $KeyVault
            setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--uri' 'https://authority.site/connect/token' $KeyVault
            setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--clientsecret' 'verylongsecret' $KeyVault
            setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--authenticationconfig--scopes' 'random.webhook.api.all' $KeyVault
            setConfig 'event--1--callbackconfig--webhookrequestrules--1--routes--2--httpverb' 'POST' $KeyVault
        ";
    }
}
