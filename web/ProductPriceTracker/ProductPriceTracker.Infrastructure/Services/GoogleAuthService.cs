using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace ProductPriceTracker.Infrastructure.Services
{
public class GoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public GoogleAuthService(HttpClient httpClient, IConfiguration config)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task<GoogleJsonWebSignature.Payload?> VerifyTokenAsync(string googleToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientId"] } // Use the ClientId from configuration
            };
            return await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);
        }
        catch
            {
                return null;
            }
        }
    }
}