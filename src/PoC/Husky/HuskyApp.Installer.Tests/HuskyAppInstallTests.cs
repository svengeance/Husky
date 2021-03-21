using System.IO;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Tests.Sdk;
using NUnit.Framework;

namespace HuskyApp.Installer.Tests
{
    [TestFixture]
    public class HuskyAppInstallTests : HuskyTest
    {
        [OneTimeSetUp]
        public void SetupInstall() => ExecuteHuskyInstaller(new[] { "install" });

        [Test]
        [Category("e2e")]
        [Order(1)]
        public void Installer_creates_desktop_shortcut()
        {
            FileAssert.Exists(Path.Combine(HuskyVariables.Folders.Desktop, "Husky App.lnk"));
        }

        [Test]
        [Category("e2e")]
        [Order(2)]
        public void Installer_creates_program_files_dir()
        {
            DirectoryAssert.Exists(Path.Combine(HuskyVariables.Folders.ProgramFiles, "HuskyApp"));
        }

        [Test]
        [Category("e2e")]
        [Order(3)]
        public void Husky_app_can_be_uninstalled()
        {
            ExecuteHuskyInstaller(new[] { "uninstall" });
            FileAssert.DoesNotExist(Path.Combine(HuskyVariables.Folders.Desktop, "Husky App.lnk"));
            DirectoryAssert.DoesNotExist(Path.Combine(HuskyVariables.Folders.ProgramFiles, "HuskyApp"));
        }
    }
}