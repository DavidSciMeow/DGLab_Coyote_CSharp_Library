using System.IO;
using System.Windows;

namespace UIHostCoyoteDevice
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string pluginsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

            // Ensure the Plugins folder exists
            if (!Directory.Exists(pluginsFolderPath))
            {
                Directory.CreateDirectory(pluginsFolderPath);
            }

        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

    }
}
