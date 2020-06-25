using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CaptainHook.Common.Configuration.KeyVault
{
    public interface ISecretProvider
    {
        Task<string> GetSecretValueAsync([NotNull] string secretName);
    }
}