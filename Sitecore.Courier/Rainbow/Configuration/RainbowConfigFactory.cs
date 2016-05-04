using System;
using System.Xml;
using Rainbow.Formatting;

namespace Sitecore.Courier.Rainbow.Configuration
{
    public class RainbowConfigFactory
    {
        private readonly RainbowConfigSection configSection;

        public RainbowConfigFactory(RainbowConfigSection configSection)
        {
            this.configSection = configSection;
        }

        /// <summary>
        /// Creates a <see cref="ISerializationFormatter"/> based on the rainbow config section.
        /// </summary>
        public ISerializationFormatter CreateFormatter()
        {
            var formatterType = Type.GetType(this.configSection.SerializationFormatterElement.Type, false, true);
            var configSectionxml = this.configSection.GetSectionXml();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(configSectionxml);

            XmlNode serializationFormatterNode = null;

            if (xmlDoc.FirstChild != null)
            {
                serializationFormatterNode = xmlDoc.FirstChild.FirstChild;
            }

            return Activator.CreateInstance(formatterType, serializationFormatterNode, null) as ISerializationFormatter;
        }
    }
}