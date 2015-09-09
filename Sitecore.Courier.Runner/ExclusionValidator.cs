using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Courier.Runner
{
    public static class ExclusionValidator
    {
        public static bool HasValidExclusions(string buildConfiguration, string xmlFilePath)
        {
            if (string.IsNullOrEmpty(buildConfiguration))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(buildConfiguration) && string.IsNullOrEmpty(xmlFilePath))
            {
                throw new NullReferenceException("ScProjectFilePath is required if Build Configuration is provided.");
            }

            if (!File.Exists(xmlFilePath))
            {
                throw new FileNotFoundException(string.Format("Project file path {0} does not exist.", xmlFilePath));
            }

            return true;
        }
    }
}
