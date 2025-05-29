/*
 * Checkers
 *
 * @project lead      : Blake Pell
 * @company           : ApexGate
 * @website           : https://www.apexgate.net
 * @website           : https://www.blakepell.com
 * @copyright         : Copyright (c), 2023-2025 All rights reserved.
 * @license           : MIT
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Checkers.Common;
using Checkers.Dialogs;
using Checkers.Managers;

namespace Checkers
{

    /// <summary>
    /// Represents the main window of the Checkers application, providing the user interface and managing interactions
    /// between the game board, game manager, and user input.
    /// </summary>
    /// <remarks>The <see cref="MainWindow"/> class initializes the game board, manages game state through the
    /// <see cref="GameManager"/>, and provides UI elements such as menus and dialogs for starting new games and
    /// interacting with the application. It also handles visual elements like board colors, piece rendering, and
    /// animations (e.g., confetti).  This class is responsible for creating and managing the checkers board,
    /// initializing game settings, and responding to user actions such as starting a new game or forfeiting.</remarks>
    public partial class MainWindow
    {
        // Store references to all board squares
        private Button[,] _boardSquares = new Button[8, 8];
    
        // Board colors
        private SolidColorBrush _lightSquareColor;
        private SolidColorBrush _darkSquareColor;
    
        // Highlight colors for valid moves
        private SolidColorBrush _lightSquareHighlightColor;
        private SolidColorBrush _darkSquareHighlightColor;
    
        // Game manager
        private GameManager? _gameManager;

        // Public properties for GameManager to access
        public SolidColorBrush LightSquareColor => _lightSquareColor;
        public SolidColorBrush DarkSquareColor => _darkSquareColor;
        public SolidColorBrush LightSquareHighlightColor => _lightSquareHighlightColor;
        public SolidColorBrush DarkSquareHighlightColor => _darkSquareHighlightColor;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the main window for the checkers game by performing the
        /// following tasks: <list type="bullet"> <item><description>Retrieves color resources for the board and
        /// pieces.</description></item> <item><description>Generates highlight colors for board
        /// squares.</description></item> <item><description>Initializes the checkers board layout.</description></item>
        /// <item><description>Creates and initializes the game manager to handle game logic.</description></item>
        /// <item><description>Adds a "New Game" menu item to the user interface.</description></item> </list> If the
        /// required "CheckersBoard" grid control is not found in the XAML, an error message is displayed.</remarks>
        public MainWindow()
        {
            InitializeComponent();
        
            // Get colors from resources
            _lightSquareColor = (SolidColorBrush)FindResource("LightSquareColor");
            _darkSquareColor = (SolidColorBrush)FindResource("DarkSquareColor");
        
            // Create highlight colors (lighter versions of the regular colors)
            _lightSquareHighlightColor = new SolidColorBrush(Color.FromRgb(
                (byte)Math.Min(_lightSquareColor.Color.R + 20, 255),
                (byte)Math.Min(_lightSquareColor.Color.G + 20, 255),
                (byte)Math.Min(_lightSquareColor.Color.B - 20, 255)
            ));
        
            _darkSquareHighlightColor = new SolidColorBrush(Color.FromRgb(
                (byte)Math.Min(_darkSquareColor.Color.R + 20, 255),
                (byte)Math.Min(_darkSquareColor.Color.G + 20, 255),
                (byte)Math.Min(_darkSquareColor.Color.B - 20, 255)
            ));
        
            // Create the checkers board squares
            InitializeCheckerBoard();
        
            // Initialize the game manager after creating the board
            try 
            {
                // Make sure we have a reference to the grid named CheckersBoard from XAML
                var checkersBoard = (Grid?)FindName("CheckersBoard");

                if (checkersBoard != null) 
                {
                    _gameManager = new GameManager(this, checkersBoard, _boardSquares);
                }
                else 
                {
                    MessageBox.Show("Could not find the CheckersBoard control.");
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Error initializing game manager: {ex.Message}");
            }
        
            // Add New Game menu item
            CreateGameMenu();
        }

        /// <summary>
        /// Initializes the checkerboard by creating and styling the squares, and adding them to the grid.
        /// </summary>
        /// <remarks>This method dynamically generates an 8x8 grid of buttons to represent the
        /// checkerboard. Each square is styled based on its position to alternate between light and dark colors, and
        /// its position is stored in the <see cref="Tag"/> property of the button. The method requires a grid named
        /// "CheckersBoard" to be defined in the XAML file.</remarks>
        private void InitializeCheckerBoard()
        {
            // Make sure we have a reference to the grid named CheckersBoard from XAML
            var checkersBoard = (Grid?)FindName("CheckersBoard");

            if (checkersBoard == null)
            {
                MessageBox.Show("Could not find the CheckersBoard control.");
                return;
            }
            
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    // Create a button for each square
                    var square = new Button
                    {
                        Style = (Style)FindResource("CheckersSquareStyle"),
                        Background = (row + col) % 2 == 0 ? _lightSquareColor : _darkSquareColor,
                        Tag = new Point(row, col) // Store the position for later use
                    };

                    // Add to our reference array and to the grid
                    _boardSquares[row, col] = square;
                
                    // Add the button to the grid
                    checkersBoard.Children.Add(square);
                    Grid.SetRow(square, row);
                    Grid.SetColumn(square, col);
                }
            }
        }

        /// <summary>
        /// Creates and initializes the game menu, including options for starting new games, forfeiting,  and testing
        /// debug features. The menu is added to the main window's layout.
        /// </summary>
        /// <remarks>This method dynamically constructs a menu bar with various game-related options, such
        /// as starting  a new game or forfeiting the current game. It also includes a debug menu for testing features
        /// like  confetti animations. The menu is integrated into the main window's layout, adjusting existing  content
        /// to accommodate the new menu.  If an error occurs during menu creation, an error message is displayed to the
        /// user.</remarks>
        private void CreateGameMenu()
        {
            try
            {
                // Create a menu bar
                var menu = new Menu();
                var gameMenuItem = new MenuItem { Header = "Game" };
            
                var newGameMenuItem = new MenuItem { Header = "New Game..." };
                newGameMenuItem.Click += (s, e) => ShowGameModeDialog();
                gameMenuItem.Items.Add(newGameMenuItem);
            
                gameMenuItem.Items.Add(new Separator());
            
                var newSinglePlayerMenuItem = new MenuItem { Header = "New Single Player Game" };
                newSinglePlayerMenuItem.Click += NewSinglePlayerGame_Click;
                gameMenuItem.Items.Add(newSinglePlayerMenuItem);
            
                var newTwoPlayerMenuItem = new MenuItem { Header = "New Two Player Game" };
                newTwoPlayerMenuItem.Click += NewTwoPlayerGame_Click;
                gameMenuItem.Items.Add(newTwoPlayerMenuItem);

                // Add forfeit option
                gameMenuItem.Items.Add(new Separator());
                var forfeitMenuItem = new MenuItem { Header = "Forfeit" };
                forfeitMenuItem.Click += ForfeitMenuItem_Click;
                gameMenuItem.Items.Add(forfeitMenuItem);

                menu.Items.Add(gameMenuItem);
            
                // Add Debug menu for testing confetti
                var debugMenuItem = new MenuItem { Header = "Debug" };
                var testConfettiMenuItem = new MenuItem { Header = "Test Confetti" };
                testConfettiMenuItem.Click += (s, e) => StartConfetti();
                debugMenuItem.Items.Add(testConfettiMenuItem);
                menu.Items.Add(debugMenuItem);

                // Add menu to the window
                var mainGrid = (Grid)Content;
                if (mainGrid.RowDefinitions.Count == 0)
                {
                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                }
                else
                {
                    mainGrid.RowDefinitions.Insert(0, new RowDefinition { Height = GridLength.Auto });
                }
            
                // Move existing content down
                foreach (UIElement child in mainGrid.Children)
                {
                    if (child != menu) 
                    {
                        Grid.SetRow(child, 1);
                    }
                }
            
                // Add the menu
                mainGrid.Children.Add(menu);
                Grid.SetRow(menu, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating menu: {ex.Message}");
            }
        }    private void NewSinglePlayerGame_Click(object sender, RoutedEventArgs e)
        {
            // Start a new single-player game directly
            NewGame(GameMode.SinglePlayer);
        }
    
        /// <summary>
        /// Handles the click event to start a new two-player game.
        /// </summary>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void NewTwoPlayerGame_Click(object sender, RoutedEventArgs e)
        {
            // Start a new two-player game directly
            NewGame(GameMode.TwoPlayer);
        }
    
        /// <summary>
        /// Displays a dialog for selecting a game mode and starts a new game if a selection is confirmed.
        /// </summary>
        /// <remarks>The dialog allows the user to choose a game mode. If the user confirms their
        /// selection,  the method initiates a new game using the selected game mode.</remarks>
        private void ShowGameModeDialog()
        {
            var dialog = new GameModeDialog();
            dialog.Owner = this;
        
            bool? result = dialog.ShowDialog();
        
            if (result == true)
            {
                NewGame(dialog.SelectedGameMode);
            }
        }
    
        /// <summary>
        /// Starts a new game with the specified game mode.
        /// </summary>
        /// <remarks>This method initializes a new game session using the provided game mode. If the game
        /// manager  is not properly initialized, an error message is displayed to the user. Additionally, the  window
        /// title is updated to reflect the selected game mode.</remarks>
        /// <param name="gameMode">The mode of the game to start. Use <see cref="GameMode.SinglePlayer"/> for single-player mode  or <see
        /// cref="GameMode.TwoPlayer"/> for two-player mode.</param>
        public void NewGame(GameMode gameMode)
        {
            // Use the game manager to start a new game if it's initialized
            if (_gameManager != null)
            {
                try
                {
                    _gameManager.NewGame(gameMode);
                
                    // Update window title based on game mode
                    Title = gameMode == GameMode.SinglePlayer ? 
                        "Checkers - Single Player Mode" : 
                        "Checkers - Two Player Mode";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error starting new game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Game manager not initialized correctly. Please restart the application.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the "Forfeit" menu item.
        /// </summary>
        /// <remarks>This method triggers the forfeit action in the game manager, if available.</remarks>
        /// <param name="sender">The source of the event, typically the menu item that was clicked.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void ForfeitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _gameManager?.Forfeit();
        }

        /// <summary>
        /// Displays confetti animation for the specified duration (ms), then clears it.
        /// </summary>
        public void StartConfetti(int durationMs = 5000)
        {
            // Find the confetti canvas in case it's not exposed as a field
            var canvas = FindName("ConfettiCanvas") as Canvas;

            if (canvas == null)
            {
                return;
            }

            // Ensure canvas is ready
            canvas.Children.Clear();
            canvas.Visibility = Visibility.Visible;
            canvas.UpdateLayout(); // force measure so ActualWidth/Height are valid

            var rand = new Random();
            var colors = new[] { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Orange, Colors.Purple };
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;
            int count = 200; // increased number of confetti pieces
            int minDur = durationMs / 2;
            int maxDurPerParticle = durationMs + minDur;

            for (int i = 0; i < count; i++)
            {
                var rect = new Rectangle
                {
                    Width = rand.Next(5, 12),
                    Height = rand.Next(5, 12),
                    Fill = new SolidColorBrush(colors[rand.Next(colors.Length)]),
                    RenderTransform = new RotateTransform(rand.Next(360))
                };
                double startX = rand.NextDouble() * canvasWidth;
                Canvas.SetLeft(rect, startX);
                Canvas.SetTop(rect, -20);
                canvas.Children.Add(rect);

                int particleDuration = rand.Next(minDur, maxDurPerParticle);

                // Vertical fall animation
                var animY = new DoubleAnimation
                {
                    From = -20,
                    To = canvasHeight + 20,
                    Duration = TimeSpan.FromMilliseconds(particleDuration)
                };
                rect.BeginAnimation(Canvas.TopProperty, animY);

                // Horizontal drift animation
                var animX = new DoubleAnimation
                {
                    From = startX,
                    To = startX + (rand.NextDouble() * 100 - 50),
                    Duration = TimeSpan.FromMilliseconds(particleDuration)
                };
                rect.BeginAnimation(Canvas.LeftProperty, animX);
            }

            // Stop and clear after all particles have fallen
            var totalDuration = maxDurPerParticle + 500;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(totalDuration) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                canvas.Visibility = Visibility.Collapsed;
                canvas.Children.Clear();
            };
            timer.Start();
        }
    }
}
