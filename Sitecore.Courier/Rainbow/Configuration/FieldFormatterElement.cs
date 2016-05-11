using System.Configuration;

namespace Sitecore.Courier.Rainbow.Configuration
{
    public class FieldFormatterElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsKey = true, IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
            set { this["type"] = value; }
        }
    }
}
