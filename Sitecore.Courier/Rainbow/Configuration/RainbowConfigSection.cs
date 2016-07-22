using System.Configuration;

namespace Sitecore.Courier.Rainbow.Configuration
{
    public class RainbowConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("serializationFormatter")]
        public SerializationFormatterElement SerializationFormatterElement
        {
            get { return (SerializationFormatterElement)this["serializationFormatter"]; }
            set { this["serializationFormatter"] = value; }
        }

        public string GetSectionXml()
        {
            return SerializeSection(null, SectionInformation.SectionName, ConfigurationSaveMode.Modified);
        }
    }
}