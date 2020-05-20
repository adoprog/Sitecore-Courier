using System.IO;
using Ionic.Zip;

namespace Sitecore.Courier.DacPac
{
    public class DacPacBuilder
    {
        private MemoryStream GenerateDacPac(string sqlScriptPath)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (MemoryStream memoryStream1 = new MemoryStream(Sitecore.Courier.Properties.Resources.empty_dacpac))
            {
                using (ZipFile zipFiles = ZipFile.Read(memoryStream1))
                {
                    zipFiles.AddEntry("postdeploy.sql", File.ReadAllBytes(sqlScriptPath));
                    zipFiles.Save(memoryStream);
                }
            }
            return memoryStream;
        }

        public void ConvertToDacPac(string sqlScriptPath, string dacpacPath)
        {
            using (MemoryStream dacPac = this.GenerateDacPac(sqlScriptPath))
            {
                byte[] array = dacPac.ToArray();
                File.WriteAllBytes(dacpacPath, array);
            }
        }
    }
}
