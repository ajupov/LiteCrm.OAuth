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

            return builder
                .AddOAuth(JwtDefaults.AuthenticationScheme, options =>
                    {
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                        options.ClientId =
                            liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.ClientId));

                        options.ClientSecret =
                            liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.ClientSecret));

                        options.Scope.Add(LiteCrmOAuthDefaults.OpenIdScope);
                        options.Scope.Add(LiteCrmOAuthDefaults.ProfileScope);

                        var scopes = liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.Scopes));
                        if (!string.IsNullOrWhiteSpace(scopes))
                        {
                            scopes
                                .Split(",")
                                .ToList()
                                .ForEach(x => options.Scope.Add(x.Trim()));
                        }

                        options.CallbackPath = new PathString(
                            liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.CallbackPath)));

                        options.AuthorizationEndpoint =
                            liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions
                                .AuthorizationUrl)) ?? LiteCrmOAuthDefaults.AuthorizationUrl;

                        options.UserInformationEndpoint =
                            liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.UserInfoUrl)) ??
                            LiteCrmOAuthDefaults.UserInfoUrl;

                        options.TokenEndpoint =
                            liteCrmOAuthOptions.GetValue<string>(nameof(LiteCrmOAuthOptions.TokenUrl)) ??
                            LiteCrmOAuthDefaults.TokenUrl;

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
                                var hasUserAgent =
                                    context.HttpContext.Request.Headers.TryGetValue(
                                        HeaderNames.UserAgent, out var userAgent) &&
                                    !string.IsNullOrWhiteSpace(userAgent.ToString());

                                if (hasUserAgent)
                                {
                                    context.Response.Redirect(context.RedirectUri);
                                }
                                else
                                {
                                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                }

                                return Task.CompletedTask;
                            }
                        };
                    }
                );
        }
    }
}