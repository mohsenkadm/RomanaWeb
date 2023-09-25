
using RomanaWeb.Model.General;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RomanaWeb.Classes
{
    public static class JsonWebToken
    {
        public static string GenerateToken(UserManager UserManager)
        {
           try{ byte[] symmetricKey = Convert.FromBase64String(Key.SecretKey);
            SymmetricSecurityKey securityKey = new(symmetricKey);
            string algorithms = SecurityAlgorithms.HmacSha256Signature;
            JwtSecurityTokenHandler tokenHandler = new();

            SecurityToken stoken = tokenHandler.CreateToken(
                new SecurityTokenDescriptor
                {
                    Issuer = "RomanaWeb",
                    Audience = "Subscriber", 
                    Expires = Key.DateTimeIQ.AddDays(30),
                    Subject = new ClaimsIdentity(new[] {
                        new Claim(ClaimInfo.UserManager, JsonConvert.SerializeObject(UserManager))
                    }),
                    SigningCredentials = new SigningCredentials(securityKey, algorithms)
                });

            return tokenHandler.WriteToken(stoken);
            }
            catch (Exception ex)
            {
                return "InvalidToken";
            }
        }       
    }
}
 
