using System.Windows;
using Argus.Memory;
using Checkers.Common;

namespace Checkers
{
    /// <summary>
    /// Application class for the Checkers game.
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Register an AppSettings instance in the DI service collection
            AppServices.AddSingleton(new AppSettings());
        }
    }
}

