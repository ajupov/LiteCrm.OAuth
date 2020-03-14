using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace LiteCrm.OAuth.Helpers
{
    public static class UserInfoHelper
    {
        public static async Task<string> GetUserInfoJsonAsync(OAuthCreatingTicketContext context)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint)
            {
                Headers =
                {
                    Accept = {new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json)},
                    Authorization =
                        new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, context.AccessToken)
                },
            };

            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}