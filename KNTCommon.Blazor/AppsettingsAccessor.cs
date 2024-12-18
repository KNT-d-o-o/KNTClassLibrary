using Newtonsoft.Json.Linq;

namespace KNTCommon.Blazor
{
    public class AppsettingsAccessor
    {
        public string SettingsName = "appsettings.json";
        public AppsettingsAccessor()
        {

        }
        private void ChangeData(string value, string section, string property)
        {
            try
            {
                var json = File.ReadAllText("appsettings.json");
                var settings = JObject.Parse(json);

                if (settings[section] == null)
                {
                    settings[section] = new JObject();
                }

                // Cast the section to JObject
                var sectionObject = (JObject)settings[section]!;
                sectionObject[property] = JToken.FromObject(value);

                File.WriteAllText("appsettings.json", settings.ToString());
            }
            catch
            {

            }
            finally
            {

            }
        }
        public void ChangeNumberOfThreads(int numberOfThreads)
        {

        }
        public void ChangeConnectionData()
        {

        }
    }
}
