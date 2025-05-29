/*
 * Checkers
 *
 * @project lead      : Blake Pell
 * @company           : ApexGate
 * @website           : https://www.apexgate.net
 * @website           : https://www.blakepell.com
 * @copyright         : Copyright (c), 2025 All rights reserved.
 * @license           : MIT
 */

using System.Windows;

namespace Checkers
{
    /// <summary>
    /// Represents a dialog for selecting a game mode.
    /// </summary>
    /// <remarks>
    /// This dialog allows the user to choose between single-player and two-player game modes. The
    /// selected game mode can be accessed through the <see cref="SelectedGameMode"/> property after the dialog is
    /// closed with a positive result.
    /// </remarks>
    public partial class GameModeDialog
    {
        /// <summary>
        /// Gets the currently selected game mode.
        /// </summary>
        public GameMode SelectedGameMode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameModeDialog"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor initializes the dialog and sets the default game mode to
        /// single-player.
        /// </remarks>
        public GameModeDialog()
        {
            InitializeComponent();
            SinglePlayerRadio.IsChecked = true;
        }

        /// <summary>
        /// Handles the click event for the Start button, finalizing the game mode selection and closing the dialog.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Start button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedGameMode = SinglePlayerRadio.IsChecked == true 
                ? GameMode.SinglePlayer 
                : GameMode.TwoPlayer;
                
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handles the click event of the Cancel button, closing the dialog and setting the result to indicate
        /// cancellation.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Cancel button.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
