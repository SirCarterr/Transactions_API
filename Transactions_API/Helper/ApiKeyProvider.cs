using AspNetCore.Authentication.ApiKey;
using System.Security.Claims;
using Transactions_API.Models;

namespace Transactions_API.Helper
{
    public class ApiKeyProvider : IApiKeyProvider
    {
        public async Task<IApiKey> ProvideAsync(string key)
        {
            return new ApiKey("1d23th56hj33", "Client", new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "client")
            });
        }
    }
}
