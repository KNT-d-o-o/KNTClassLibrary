using Blazored.LocalStorage;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace KNTCommon.Blazor.Services
{
    public class LocalStorageService
    {
        private readonly string Key = "KNT";
        private readonly IJSRuntime JS;
        private readonly IConfiguration Config;

        public LocalStorageService(IJSRuntime jsRuntime, IConfiguration config)
        {
            Config = config;
            JS = jsRuntime;
        }
        public async Task Set(string value)
        {
            await JS.InvokeVoidAsync("writeToStorage", Key, value);
        }

        public async Task<string?> Get()
        {
            var var = await JS.InvokeAsync<string>("getFromStorage", Key);
            return var;
        }

        public async Task Remove()
        {
            await JS.InvokeVoidAsync("removeFromStorage", Key);
        }

        public string GetUserIdFromCookie(string fakeCookie)
        {
            var usersId = fakeCookie.Split(';')[0];
            return usersId;
        }

        public string GetDateExpired(string fakeCookie)
        {
            var usersId = fakeCookie.Split(';')[1];
            return usersId;
        }

        public string GenerateLocalStorageData(int usersId, string date)
        {
            return $"{usersId};{date}";
        }

        public string GetMaxSessionTime()
        {
            var maxTime = Config.GetSection("MaxSessionTime").Value;
            var dt = DateTime.Now.AddHours(Convert.ToDouble(maxTime, CultureInfo.InvariantCulture)).ToString("yyyy-MM-dd HH:mm");
            return dt;
        }
    }
}
