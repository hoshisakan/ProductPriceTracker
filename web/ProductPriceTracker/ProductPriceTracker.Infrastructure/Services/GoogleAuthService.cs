using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using ProductPriceTracker.Core.Interface.IServices;

namespace ProductPriceTracker.Infrastructure.Services
{
public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _config;

    public GoogleAuthService(IConfiguration config)
    {
        _config = config;
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