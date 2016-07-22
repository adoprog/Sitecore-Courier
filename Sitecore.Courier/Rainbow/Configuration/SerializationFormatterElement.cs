using System.Configuration;

namespace Sitecore.Courier.Rainbow.Configuration
{
    public class SerializationFormatterElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsKey = true, IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
            set { this["type"] = value; }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(FieldFormatterCollection), AddItemName = "fieldFormatter")]
        public FieldFormatterCollection FieldFormatters
        {
            get { return (FieldFormatterCollection)this[""]; }
        }
    }
}