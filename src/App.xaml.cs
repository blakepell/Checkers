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
        /// <summary>
        /// Handles the application startup event and performs initialization tasks.
        /// </summary>
        /// <param name="sender">The source of the startup event.</param>
        /// <param name="e">The event data associated with the startup event.</param>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Register an AppSettings instance in the DI service collection
            AppServices.AddSingleton(new AppSettings());
        }
    }
}

