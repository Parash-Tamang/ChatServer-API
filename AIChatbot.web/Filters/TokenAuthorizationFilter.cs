using AIChatbot.web.Services;
using AIChatbot.web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AIChatbot.web.Filters
{
    public class TokenAuthorizationFilter : IAsyncAuthorizationFilter
    {
        
            private readonly ITokenService _tokenService;
            private readonly IAuthService _authService;

            public TokenAuthorizationFilter(TokenService tokenService, IAuthService authService)
            {
                _tokenService = tokenService;
                _authService = authService;
            }

            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                var accessToken = _tokenService.GetAccessToken();

                if (string.IsNullOrEmpty(accessToken))
                {
                    RedirectToLogin(context);
                    return;
                }

                if (_tokenService.IsAccessTokenExpired())
                {
                    var refreshToken = _tokenService.GetRefreshToken();

                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        var result = await _authService.RefreshTokenAsync(refreshToken);

                        if (result.Success)
                        {
                            _tokenService.SaveTokens(
                                result.AccessToken!,
                                result.RefreshToken!,
                                result.ExpiresIn);

                            return;
                        }
                    }

                    _tokenService.ClearTokens();
                    RedirectToLogin(context);
                }
            }

            private static void RedirectToLogin(AuthorizationFilterContext context)
            {
                var http = context.HttpContext;
                var returnUrl = http.Request.Path + http.Request.QueryString;

                context.Result = new RedirectToActionResult(
                    "Login",
                    "Auth",
                    new { returnUrl });
            }
        }
    }

