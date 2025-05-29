using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Checkers.Common
{
    /// <summary>
    /// AppSettings that are persisted to storage and reloaded on application launch.
    /// </summary>
    public partial class AppSettings : ObservableObject
    {
        [property: Category("Audio")]
        [property: DisplayName("Sound Enabled")]
        [property: Description("If the sound is currently enabled for the game.")]
        [property: ReadOnly(false)]
        [property: Browsable(true)]
        [ObservableProperty]
        private bool _soundEnabled = true;
    }
}
