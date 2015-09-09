using System.Collections.Generic;
using System.Xml;

namespace Sitecore.Courier.Runner
{
    // Reads the scproj file for excludedfiles and returns them for processing.
    public static class ExclusionReader
    {
        public static List<string> GetExcludedItems(string projectFilePath, string buildConfiguration)        
        {
            var excludedItems = new List<string>();
            var xmldocument = new XmlDocument();
            xmldocument.Load(projectFilePath);
            using (var reader = XmlReader.Create(projectFilePath))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "SitecoreItem")
                    {
                        var node = xmldocument.ReadNode(reader);
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            if (child.NodeType == XmlNodeType.Element && child.Name == "ExcludeItemFrom" &&
                                child.InnerText == buildConfiguration)
                            {
                                var path = node.Attributes[0].Value;
                                excludedItems.Add(path);
                            }
                        }
                    }
                }
            }

            return excludedItems;
        }
    }
}
