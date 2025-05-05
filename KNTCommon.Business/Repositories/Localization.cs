using AutoMapper.Internal.Mappers;
using KNTCommon.Data.Models;
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
        IParametersRepository Parameters;

        public Localization(IServiceProvider _serviceProvider)
        {
            AddTranslations();

            using (var scope = _serviceProvider.CreateScope())
                Parameters = scope.ServiceProvider.GetRequiredService<IParametersRepository>();
        }

        public void AddTranslations()
        {
            using var context = new EdnKntControllerMysqlContext();

            var languageDictionaries = context.LanguageDictionaries.ToList();

            foreach (var languageDictionarie in languageDictionaries)
            {                
                if (languageDictionarie.Key is null) // TODO change key in database as not null
                    continue;

                AddOrUpdateTranslation(languageDictionarie.Key, languageDictionarie.English, languageDictionarie.Slovene, languageDictionarie.German, languageDictionarie.Croatian, languageDictionarie.Serbian);
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
            foreach(var key in keys)
            {
                if (ContainKey(key))
                    return Get(key, args);
            }

#if DEBUG
            throw new Exception($"Translation is missing. LanguageDictionarie does not contain any keys: '{string.Join(", ", keys)}'");// TODO write in error log
#endif
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

            int selectedLanguage = Convert.ToInt16(Parameters.GetParametersStr("activeLanguage")) - 1;
            //int selectedLanguage = 1;

            var value = translations[key][selectedLanguage];
            try
            {
                value = string.Format(value, args);
            } catch (Exception e)
            {
                throw e;
            }

            //return $"*{value}";
            return value;
        }
    }
}
