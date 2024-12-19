using KNTCommon.Business.DTOs;
using KNTCommon.Business.Repositories;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using KNTCommon.Blazor.Services;
using KNTCommon.Data.Models;
using System.Globalization;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KNTCommon.Blazor
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal? LoggedInUser;
        private LocalStorageService LocalStorageService;
        private IUsersAndGroupsRepository UsersRepo;

        public AuthStateProvider(LocalStorageService srv, IUsersAndGroupsRepository iUser)
        {
            LocalStorageService = srv;
            UsersRepo = iUser;
        }
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal();

            if (LoggedInUser != null)
            {
                user = LoggedInUser;
            }
            return Task.FromResult(new AuthenticationState(user));
        }

        public void Login(UserDTO creds)
        {
            var identity = CreateClaimIdentity(creds);
            LoggedInUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void LoginIfThereIsSomethingInLocalStorage(string ls)
        {
            //var userId = 2;
            //var date = DateTime.Now.AddHours(Convert.ToDouble(2, CultureInfo.InvariantCulture)).ToString("yyyy-MM-dd HH:mm");
            //var data = $"{userId};{date}";
            //localStorage.Set(data);

            var userIdentity = new ClaimsIdentity();
            var user_ = UsersRepo.GetUserById(Convert.ToInt32(LocalStorageService.GetUserIdFromCookie(ls)));
            var identity = CreateClaimIdentity(user_);
            LoggedInUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Logout()
        {
            await LocalStorageService.Remove();
            LoggedInUser = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private ClaimsIdentity CreateClaimIdentity(UserDTO creds)
        {
            {
                var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, creds.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Role, Convert.ToString(creds.GroupId ?? 0)),
                ], "login");

                return identity;
            }
        }
    }
}