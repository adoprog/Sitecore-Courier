using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using ConsoleApp1;
using Rainbow.Storage.Yaml;
using Sitecore.Courier.Rainbow;
using Sitecore.Courier.Rainbow.Configuration;
using Unicorn.Roles.Data;
using Unicorn.Roles.Formatting;
using Unicorn.Users.Data;
using Unicorn.Users.Formatting;

namespace Sitecore.Courier.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "CourierSecurityPackage", DefaultParameterSetName = "default", SupportsShouldProcess = true)]
    [OutputType(new Type[] {typeof(string)})]
    public class NewCourierSecurityPackageCommand : Cmdlet
    {
        public static class ParameterSets
        {
            public const string DEFAULT = "default";
        }

        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSets.DEFAULT)]
        public string Items { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ParameterSets.DEFAULT)]
        public string Output { get; set; }

        protected override void BeginProcessing()
        {
            try
            {
                var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE",
                    Path.Combine(currentDirectory, "Sitecore.Courier.dll.config"));
                ResetConfigMechanism();

                Console.WriteLine("Items: {0}", Items);
                Console.WriteLine("Output: {0}", Output);

                RainbowSerializationProvider.Enabled = true;
                RainbowSerializationProvider.IncludeFiles = false;
                DiffGenerator.IncludeSecurity = true;

                var result = new StringBuilder();
                var sqlGenerator = new RainbowSecuritySqlGenerator();

                var config = (RainbowConfigSection)ConfigurationManager.GetSection("rainbow");
                var rainbowConfigFactory = new RainbowConfigFactory(config);
                var formatter = rainbowConfigFactory.CreateFormatter() as YamlSerializationFormatter;
                var iterator = new RainbowIterator(Items, formatter);

                var rolesTemp = Path.GetTempPath() + Guid.NewGuid();
                var usersTemp = Path.GetTempPath() + Guid.NewGuid();
                Directory.CreateDirectory(rolesTemp);
                Directory.CreateDirectory(usersTemp);

                var item = iterator.Next();
                while (item != null)
                {
                    if (item is UserFileSystemDataItem)
                    {
                        File.Copy(item.ItemPath, Path.Combine(usersTemp, Path.GetFileName(item.ItemPath)));
                    }

                    if (item is RoleFileSystemDataItem)
                    {
                        File.Copy(item.ItemPath, Path.Combine(rolesTemp, Path.GetFileName(item.ItemPath)));
                    }

                    item = iterator.Next();
                }

                var roleStore = new FilesystemRoleDataStore(rolesTemp, new YamlRoleSerializationFormatter());
                var roles = roleStore.GetAll();
                foreach (var role in roles)
                {
                    Console.WriteLine("Generating SQL for role: " + role.RoleName);
                    var clause = sqlGenerator.GenerateAddRoleStatements(role);
                    result.Append(clause);
                }

                var userStore = new FilesystemUserDataStore(usersTemp, new YamlUserSerializationFormatter());
                var users = userStore.GetAll();
                foreach (var user in users)
                {
                    Console.WriteLine("Generating SQL for user: " + user.User.UserName);
                    result.Append(sqlGenerator.GenerateAddUserStatements(user.User));
                    foreach (var userRole in user.User.Roles)
                    {
                        result.Append(sqlGenerator.GenerateAddUserToRoleStatements(user.User.UserName, userRole));
                    }
                }

                var sqlScriptTemp = Path.GetTempPath() + Guid.NewGuid();
                Console.WriteLine("Dumping generated SQL script to: " + sqlScriptTemp);
                File.WriteAllText(sqlScriptTemp, result.ToString());
                var builder = new DacPacBuilder();
                builder.ConvertToDacPac(sqlScriptTemp, Output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        private static void ResetConfigMechanism()
        {
            typeof(ConfigurationManager)
                .GetField("s_initState", BindingFlags.NonPublic |
                                         BindingFlags.Static)
                .SetValue(null, 0);

            typeof(ConfigurationManager)
                .GetField("s_configSystem", BindingFlags.NonPublic |
                                            BindingFlags.Static)
                .SetValue(null, null);

            typeof(ConfigurationManager)
                .Assembly.GetTypes()
                .Where(x => x.FullName ==
                            "System.Configuration.ClientConfigPaths")
                .First()
                .GetField("s_current", BindingFlags.NonPublic |
                                       BindingFlags.Static)
                .SetValue(null, null);
        }
    }
}