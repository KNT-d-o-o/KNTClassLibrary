using Microsoft.AspNetCore.Components;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using KNTCommon.Blazor.Services;


namespace KNTCommon.Blazor.Components.Base
{
    public abstract class PageBaseCS : ComponentBase, IDisposable
    {
        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected HelperService Helper { get; set; }

        abstract public Task LoadData();

        public void Navigate(string pageName)
        {
            NavigationManager.NavigateTo(pageName);
        }

        // TODO maybe set args with search page type?
        public void Navigate(Type searchPage, params object[] args)
        {
            var param = Helper.CreateDialogBlazorComponentParameters(args);       
            var json = JsonSerializer.Serialize(param);
            var encoded = HttpUtility.UrlEncode(json);
            var url = $"{searchPage.Name}?data={encoded}";
            Navigate(url);
        }

        public Dictionary<string, object> GetUriData<T>()
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("data", out var json))
            {
                var dict = new Dictionary<string, object>();
                var tmpDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json!);
                var data = JsonSerializer.Deserialize<T>(json!);                
                
                foreach(var (key, val) in tmpDict)
                {
                    var v = data.GetType().GetProperty(key).GetValue(data);
                    dict.Add(key, v);
                }

                return dict;
            }

            return null;
        }

        Timer? timer;
        public bool enablePeriodicReload { get; set; } = false;

        public PageBaseCS()
        {
            timer = new Timer(Tick, null, 0, 1000);
        }

        private async void Tick(object? _)
        {
            // TODO BUG when enablePeriodicReload= true sometime on button clicked no popup is open 
            if (enablePeriodicReload)
            {
                await LoadData();
                // BUG: used for reloading grid - to update data in grid, but at same time it invoke NumericInput2 - check grid.Reload() might be better
                //await InvokeAsync(StateHasChanged); 
            }
        }

        public void Dispose()
        {
            timer?.Dispose();
        }


    }
}