using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace LiteCrm.OAuth.Extensions
{
    public static class LiteCrmOAuthRolesExtensions
    {
        public static void AppendRolesToClaims(this OAuthCreatingTicketContext context, string userInfoJson)
        {
            var userData = JsonDocument.Parse(userInfoJson).RootElement;

            context.RunClaimActions(userData);

            var token = new JwtSecurityTokenHandler().ReadToken(context.AccessToken) as JwtSecurityToken;
            if (token?.Claims == null)
            {
                return;
            }

            foreach (var claim in token.Claims)
            {
                if (claim.Type != ClaimTypes.Role)
                {
                    continue;
                }

                context.Identity.AddClaim(claim);
            }
        }
    }
}