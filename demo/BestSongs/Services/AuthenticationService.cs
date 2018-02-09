﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Identity.Client;
using System.Diagnostics;

namespace BestSongs
{
    public class AuthenticationService
    {
        static PublicClientApplication AuthClient;

        static void Init()
        {
            if (AuthClient != null)
                return;

            AuthClient = new PublicClientApplication(B2CConstants.ClientId);
            AuthClient.ValidateAuthority = false;
            AuthClient.RedirectUri = B2CConstants.MSALRedirectUri;
        }

        public async static Task<string> GetSignInUpToken()
        {
            Init();

            string token = default(string);

            //token = await GetCachedSignInToken();

            if (!string.IsNullOrWhiteSpace(token))
                return token;

            try
            {
                var result = await AuthClient.AcquireTokenAsync(
                    B2CConstants.ApplicationScopes,
                    GetUserByPolicy(AuthClient.Users, B2CConstants.SignInUpPolicy),
                    UIBehavior.ForceLogin,
                    null, null,
                    B2CConstants.SignInUpAuthority,
                    App.UiParent
                );

                token = result.AccessToken;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR: {ex.Message}");
            }

            return token;
        }

        public async static Task<bool> IsLoggedIn()
        {
            Init();

            var cachedToken = await GetCachedSignInToken();

            return !string.IsNullOrWhiteSpace(cachedToken);
        }

        public async static Task<string> GetCachedSignInToken()
        {
            Init();

            try
            {
                // This checks to see if there's already a user in the cache
                var result = await AuthClient.AcquireTokenSilentAsync(
                    B2CConstants.ApplicationScopes,
                    GetUserByPolicy(AuthClient.Users, B2CConstants.SignInUpPolicy),
                    B2CConstants.SignInUpAuthority,
                    false
                );

                return result.AccessToken;
            }
            catch (MsalUiRequiredException ex)
            {
                // happens if the user hasn't logged in yet & isn't in the cache
                Debug.WriteLine($"*** ERROR: {ex.Message}");
            }

            return null;
        }

        public static void Logout()
        {
            Init();

            foreach (var user in AuthClient.Users)
            {
                AuthClient.Remove(user);
            }
        }

        static IUser GetUserByPolicy(IEnumerable<IUser> users, string policy)
        {
            foreach (var user in users)
            {
                string userIdentifier = Base64UrlDecode(user.Identifier.Split('.')[0]);

                if (userIdentifier.EndsWith(policy.ToLower(), StringComparison.OrdinalIgnoreCase)) return user;
            }

            return null;
        }

        static string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            var byteArray = Convert.FromBase64String(s);
            var decoded = Encoding.UTF8.GetString(byteArray, 0, byteArray.Count());
            return decoded;
        }

    }
}
