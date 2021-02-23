using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CaptainHook.Api.Tests
{
    public static class TestOutputHelperExtensions
    {
        public static async Task PrintIfInvalidHttpResponse(this ITestOutputHelper outputHelper, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                outputHelper.WriteLine("Invalid response:");
                var text = await response.Content.ReadAsStringAsync();
                outputHelper.WriteLine(text);
            }
        }
    }
}
