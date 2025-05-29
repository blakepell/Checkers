using System.Windows;

namespace Checkers
{
    /// <summary>
    /// Interaction logic for GameModeDialog.xaml
    /// </summary>
    public partial class GameModeDialog : Window
    {
        public GameMode SelectedGameMode { get; private set; }

        public GameModeDialog()
        {
            InitializeComponent();
            SinglePlayerRadio.IsChecked = true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedGameMode = SinglePlayerRadio.IsChecked == true 
                ? GameMode.SinglePlayer 
                : GameMode.TwoPlayer;
                
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
