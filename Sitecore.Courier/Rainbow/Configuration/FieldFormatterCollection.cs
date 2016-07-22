using System.Collections.Generic;
using System.Configuration;

namespace Sitecore.Courier.Rainbow.Configuration
{
    public class FieldFormatterCollection : ConfigurationElementCollection, IEnumerable<FieldFormatterElement>
    {
        private readonly List<FieldFormatterElement> elements;

        public FieldFormatterCollection()
        {
            this.elements = new List<FieldFormatterElement>();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            var element = new FieldFormatterElement();
            this.elements.Add(element);
            return element;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FieldFormatterElement)element).Type;
        }

        public new IEnumerator<FieldFormatterElement> GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }
    }
}