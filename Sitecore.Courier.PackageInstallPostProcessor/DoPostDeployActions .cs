using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Sitecore.Data.Serialization.Yaml;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Install.Framework;
using Sitecore.IO;

namespace Sitecore.Courier.PackageInstallPostProcessor
{
    public class DoPostDeployActions : IPostStep
    {
        public void Run(ITaskOutput output, NameValueCollection metaData)
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Log.Info("Courier: PostDeployActions. Using assembly version {0}", new object[] { versionInfo.FileVersion });
            Log.Info("Courier: Action metaData: ", this);
            {
                foreach (string key in metaData.AllKeys)
                {
                    Log.Info($"  {key} : {metaData[key]}", this);
                }
            }

            var version = metaData["Version"];

            LoadUsers(version);
            LoadRoles(version);

            Log.Info("Courier: PostDeployActions complete.", new object[0]);
        }

        private static void LoadUsers(string version)
        {
            try
            {
                string tempFolder = FileUtil.MapPath(Sitecore.Configuration.Settings.TempFolderPath);
                var userFolder = $"{tempFolder}\\{version}\\users";
                var userService =
                    ServiceLocator.ServiceProvider.GetService(typeof(YamlUserSerializationManager)) as
                        YamlUserSerializationManager;
                if (Directory.Exists(userFolder))
                {
                    foreach (var user in Directory.GetFiles(userFolder, "*.yml"))
                    {
                        userService.LoadEntity(user);
                        Log.Info($"Courier: Loaded user: {user}", new object[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private static void LoadRoles(string version)
        {
            string tempFolder = FileUtil.MapPath(Sitecore.Configuration.Settings.TempFolderPath);
            var roleService =
                ServiceLocator.ServiceProvider.GetService(typeof(YamlRoleSerializationManager)) as YamlRoleSerializationManager;
            var roleFolder = $"{tempFolder}\\{version}\\roles";
            if (Directory.Exists(roleFolder))
            {
                foreach (var role in Directory.GetFiles(roleFolder, "*.yml"))
                {
                    roleService.LoadEntity(role);
                    Log.Info($"Courier: Loaded role: {role}", new object[0]);
                }
            }
        }
    }
}
