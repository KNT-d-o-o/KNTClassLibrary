using KNTCommon.Business.Models;
using KNTToolsAndAccessories;
using Radzen.Blazor.Rendering;

namespace KNTCommon.Blazor.Components.Base
{
    public abstract class SearchPageBaseCS : PageBaseCS
    {
        abstract public SearchPageArgs searchPageArgs { get; set; }

        abstract override public Task LoadData();        

        public void SetSearchPageArgsFromUrl<T>()
        {
            var filters = GetUriData<T>();

            if (filters is null)
                return;

            foreach(var filter in filters)
            {
                if (filter.Value is not null)
                {
                    var searchPageArg = searchPageArgs.columns.Where(x => x.FilterColumn == filter.Key).FirstOrDefault();

                    if (searchPageArg is not null)
                        searchPageArg.FilterParam = filter.Value;
                }
            }
        }

        public string GetCondition(string key)
        {
            var column = GetColumn(key);

            return column.FilterCondition;
        }

        public async Task SetCondition(string key, object? value)
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                column.FilterParam = string.Empty;

            if (value is null || value.ToString() == "...")
                column.FilterCondition = string.Empty;
            else
                column.FilterCondition = value.ToString()!;

            if (column.FilterParam.ToString() != "")
                await LoadData();
        }

        public string? GetStringValue(string key)
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                return null;

            return column.FilterParam.ToString();
        }

        public async Task SetStringValue(string key, object? value, bool loadData = true)
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                column.FilterParam = string.Empty;

            if(value is null)
                column.FilterParam = string.Empty;
            else 
                column.FilterParam = value;

            if (loadData)
                await LoadData();
        }

        public bool? GetBoolValue(string key)
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                return null;

            return bool.Parse(column.FilterParam.ToString()!);
        }

        public async Task SetBoolValue(string key, object? value, bool loadData = true)
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                column.FilterParam = string.Empty;

            if (value is null)
                column.FilterParam = string.Empty;
            else
                column.FilterParam = value;

            if (loadData)
                await LoadData();
        }

        public int? GetIntValue(string key)
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                return null;

            return int.Parse(column.FilterParam.ToString()!);
        }

        public async Task SetIntValue(string key, object? value, bool loadData = true)
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                column.FilterParam = string.Empty;

            if (value is null)
                column.FilterParam = string.Empty;
            else
                column.FilterParam = value;


            if (loadData)
                await LoadData();
        }

        public DateTime? GetDateTimeValue(string key)
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                return null;

            return DateTime.Parse(column.FilterParam.ToString()!);
        }

        public async Task SetDateTimeValue(string key, DateTime? value, bool loadData = true)
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                column.FilterParam = string.Empty;

            if (value is null)
                column.FilterParam = string.Empty;
            else
                column.FilterParam = value;

            if (loadData)
                await LoadData();
        }
                
        public string? GetLeakUnitValue(string key, string leakUnit)
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                return null;
            
            if (!string.IsNullOrWhiteSpace(column.FilterParam.ToString()) && double.TryParse(column.FilterParam.ToString(), out double parsedValue))
                column.FilterParam = Accessories.UnitToLeak(parsedValue, leakUnit).ToString();

            return column.FilterParam.ToString();
        }

        public async Task SetLeakUnitValue(string key, object? value) // TODO test
        {
            var column = GetColumn(key);

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                column.FilterParam = string.Empty;

            if (value is null)
                column.FilterParam = string.Empty;
            else
                column.FilterParam = value;

            await LoadData();
        }


        Column GetColumn(string key)
        {
            var column = searchPageArgs.columns.Where(x => x.FilterColumn == key).FirstOrDefault();

            if (column is not null)
                return column;

            throw new Exception($"Form dont have search criteria with Key: '{key}'");
        }


        /*
         *** TODO FOR other forms
        string? GetPascalToIntValue(string key)
        {
            var column = pageArgs.columns.Where(x => x.FilterColumn == key).FirstOrDefault();        

            if (column is null)
                return null;

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                return null;

            if (!string.IsNullOrWhiteSpace(column.FilterParam.ToString()) && float.TryParse(column.FilterParam.ToString(), out float parsedValue))
                column.FilterParam = Accessories.PascalToInt(parsedValue).ToString();

            return column.FilterParam.ToString();
        }

        async Task SetPascalToIntValue(string key, object? value) // TODO test
        {
            var column = pageArgs.columns.Where(x => x.FilterColumn == key).FirstOrDefault();

            if (column is null)
                return;

            if (string.IsNullOrEmpty(column.FilterParam.ToString()))
                column.FilterParam = String.Empty;

            column.FilterParam = value;
            await LoadDataAsync();
        }

        
        */

    }
}
