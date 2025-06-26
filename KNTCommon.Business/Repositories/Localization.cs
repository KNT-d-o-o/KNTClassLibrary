//backup
using AutoMapper.Internal.Mappers;
using KNTCommon.Data.Models;
using KNTToolsAndAccessories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTCommon.Business.Repositories
{
    public class Localization
    {
        Dictionary<string, List<string>> translations = new Dictionary<string, List<string>>();
        IParametersRepository _parameters;
        private readonly Tools t = new();

        public Localization(IServiceProvider _serviceProvider) // TODO use di -ParametersRepository _parameters
        {
            AddTranslations();

            using (var scope = _serviceProvider.CreateScope())
                _parameters = scope.ServiceProvider.GetRequiredService<IParametersRepository>();
        }

        void AddTranslations()
        {
            using var context = new EdnKntControllerMysqlContext();
            var languageDictionaries = context.LanguageDictionaries.ToList();

            translations.Clear();
            foreach (var languageDictionarie in languageDictionaries)
            {
                if (languageDictionarie.Key is null) // TODO change key in database as not null
                    continue;

                AddOrUpdateTranslation(languageDictionarie.Key, languageDictionarie.English ?? "", languageDictionarie.Slovene ?? "", languageDictionarie.German ?? "", languageDictionarie.Croatian ?? "", languageDictionarie.Serbian ?? "");
            }
        }

        public void AddOrUpdateTranslation(string key, string english, string slovene, string german, string croatian, string serbian)
        {
            key = key.ToLower();
            translations[key] = new List<string>() { english, slovene, german, croatian, serbian };
        }

        public bool ContainKey(string key)
        {
            key = key.ToLower();
            return translations.ContainsKey(key);
        }

        public string Get(string[] keys, params string[] args)
        {
            foreach (var key in keys)
            {
                if (ContainKey(key))
                    return Get(key, args);
            }

#if DEBUG
            throw new Exception($"Translation is missing. LanguageDictionarie does not contain any keys: '{string.Join(", ", keys)}'");// TODO write in error log
#endif
            return "";
        }

        public string Get(string key, Enum @enum, params object[] args)
        {
            var k = $"{key}_{@enum}";
            return Get(k, args);
        }

        public string Get(string key, params object[] args)
        {
            key = key.ToLower();

            if (!ContainKey(key))
            {
#if DEBUG
                //throw new Exception($"Translation is missing. LanguageDictionarie does not contain key: '{key}'");// TODO write in error log
#endif

                //return $"|{key}";
                return key;
            }

            int selectedLanguage = 1;

            try
            {
                selectedLanguage = Convert.ToInt16(_parameters.GetParametersStr("activeLanguage")) - 1;
            }
            catch { }

            var value = string.Empty;

            try
            {
                // TODO TEMP code, all languages values should have value
                value = translations[key][selectedLanguage];
            }
            catch (Exception ex)
            {
                t.LogEvent("KNTCommon.Business.Repositories.Localization #2 " + ex.Message);
                return value;
            }

            if (value is null) // TODO translation should not be null temp soultion
                return key;

            try
            {
                value = string.Format(value, args);
            }
            catch (Exception ex)
            {
                // throw e;
                t.LogEvent("KNTCommon.Business.Repositories.Localization #1 " + ex.Message);
            }

            //return $"*{value}";
            return value;
        }
    }
}
