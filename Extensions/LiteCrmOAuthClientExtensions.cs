using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LiteCrm.OAuth.Helpers;
using LiteCrm.OAuth.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace LiteCrm.OAuth.Extensions
{
    public static class LiteCrmOAuthClientExtensions
    {
        public static AuthenticationBuilder AddLiteCrmOAuth(
            this AuthenticationBuilder builder,
            IConfiguration configuration)
        {
            var liteCrmOAuthOptions = configuration.GetSection(nameof(LiteCrmOAuthOptions));

            var loginPath = liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.LoginPath));
            var callbackPath = liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.CallbackPath));

            var clientId = liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.ClientId));
            var clientSecret = liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.ClientSecret));
            var scopes = liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.Scopes));

            var authorizationUrl = liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.AuthorizationUrl));
            var userInfoUrl = liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.UserInfoUrl));
            var tokenUrl = liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.TokenUrl));

            return builder
                .AddOAuth(JwtDefaults.AuthenticationScheme, options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                    options.Scope.Add(LiteCrmOAuthDefaults.OpenIdScope);
                    options.Scope.Add(LiteCrmOAuthDefaults.ProfileScope);

                    if (!string.IsNullOrWhiteSpace(scopes))
                    {
                        scopes
                            .Split(",")
                            .ToList()
                            .ForEach(x => options.Scope.Add(x.Trim()));
                    }

                    options.CallbackPath = new PathString(callbackPath);

                    options.AuthorizationEndpoint = authorizationUrl ?? LiteCrmOAuthDefaults.AuthorizationUrl;
                    options.UserInformationEndpoint = userInfoUrl ?? LiteCrmOAuthDefaults.UserInfoUrl;
                    options.TokenEndpoint = tokenUrl ?? LiteCrmOAuthDefaults.TokenUrl;

                    options.SaveTokens = true;

                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, JwtDefaults.IdentifierClaimType);
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, JwtDefaults.EmailClaimType);
                    options.ClaimActions.MapJsonKey(ClaimTypes.HomePhone, JwtDefaults.PhoneClaimType);
                    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, JwtDefaults.SurnameClaimType);
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, JwtDefaults.NameClaimType);
                    options.ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, JwtDefaults.BirthDateClaimType);
                    options.ClaimActions.MapJsonKey(ClaimTypes.Gender, JwtDefaults.GenderClaimType);

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var userInfoJson = await UserInfoHelper.GetUserInfoJsonAsync(context);

                            context.AppendRolesToClaims(userInfoJson);
                        },
                        OnRedirectToAuthorizationEndpoint = context =>
                        {
                            if (HasUserAgent(context.HttpContext))
                            {
                                context.Response.Redirect(context.RedirectUri);
                            }
                            else
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            }

                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                            if (HasUserAgent(context.HttpContext))
                            {
                                context.Response.Redirect(loginPath);
                            }
                            else
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
        }

        private static bool HasUserAgent(HttpContext context)
        {
            return context.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgent)
                   && !string.IsNullOrWhiteSpace(userAgent.ToString());
        }
    }
}
