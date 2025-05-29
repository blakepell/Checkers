using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Checkers;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Store references to all board squares
    private Button[,] boardSquares = new Button[8, 8];
    
    // Board colors
    private SolidColorBrush lightSquareColor;
    private SolidColorBrush darkSquareColor;
    private SolidColorBrush redPieceColor;
    private SolidColorBrush blackPieceColor;
    
    // Highlight colors for valid moves
    private SolidColorBrush lightSquareHighlightColor;
    private SolidColorBrush darkSquareHighlightColor;
    
    // Game manager
    private GameManager? gameManager;
    
    // Public properties for GameManager to access
    public SolidColorBrush LightSquareColor => lightSquareColor;
    public SolidColorBrush DarkSquareColor => darkSquareColor;
    public SolidColorBrush LightSquareHighlightColor => lightSquareHighlightColor;
    public SolidColorBrush DarkSquareHighlightColor => darkSquareHighlightColor;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Get colors from resources
        lightSquareColor = (SolidColorBrush)FindResource("LightSquareColor");
        darkSquareColor = (SolidColorBrush)FindResource("DarkSquareColor");
        redPieceColor = (SolidColorBrush)FindResource("RedPieceColor");
        blackPieceColor = (SolidColorBrush)FindResource("BlackPieceColor");
        
        // Create highlight colors (lighter versions of the regular colors)
        lightSquareHighlightColor = new SolidColorBrush(Color.FromRgb(
            (byte)Math.Min(lightSquareColor.Color.R + 20, 255),
            (byte)Math.Min(lightSquareColor.Color.G + 20, 255),
            (byte)Math.Min(lightSquareColor.Color.B - 20, 255)
        ));
        
        darkSquareHighlightColor = new SolidColorBrush(Color.FromRgb(
            (byte)Math.Min(darkSquareColor.Color.R + 20, 255),
            (byte)Math.Min(darkSquareColor.Color.G + 20, 255),
            (byte)Math.Min(darkSquareColor.Color.B - 20, 255)
        ));
        
        // Create the checker board squares
        InitializeCheckerBoard();
        
        // Initialize the game manager after creating the board
        try 
        {
            // Make sure we have a reference to the grid named CheckersBoard from XAML
            Grid checkersBoard = (Grid)FindName("CheckersBoard");
            if (checkersBoard != null) 
            {
                gameManager = new GameManager(this, checkersBoard, boardSquares);
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

    private void InitializeCheckerBoard()
    {
        // Make sure we have a reference to the grid named CheckersBoard from XAML
        Grid checkersBoard = (Grid)FindName("CheckersBoard");
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
                    Background = (row + col) % 2 == 0 ? lightSquareColor : darkSquareColor,
                    Tag = new Point(row, col) // Store the position for later use
                };

                // Add to our reference array and to the grid
                boardSquares[row, col] = square;
                
                // Add the button to the grid
                checkersBoard.Children.Add(square);
                Grid.SetRow(square, row);
                Grid.SetColumn(square, col);
            }
        }
    }

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
    
    private void NewTwoPlayerGame_Click(object sender, RoutedEventArgs e)
    {
        // Start a new two-player game directly
        NewGame(GameMode.TwoPlayer);
    }
    
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
    
    public void NewGame(GameMode gameMode)
    {
        // Use the game manager to start a new game if it's initialized
        if (gameManager != null)
        {
            try
            {
                gameManager.NewGame(gameMode);
                
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

    // Legacy methods from the original code, can be removed if not used
    public void AddPiece(int row, int col, bool isRed, bool isKing = false)
    {
        if (row < 0 || row > 7 || col < 0 || col > 7)
            return;
            
        var square = boardSquares[row, col];
        
        // Create the piece visual
        var ellipse = new Ellipse
        {
            Width = 40,
            Height = 40,
            Fill = isRed ? redPieceColor : blackPieceColor,
            Stroke = Brushes.White,
            StrokeThickness = 2,
            Margin = new Thickness(5)
        };
        
        // If it's a king, add a crown or marker
        if (isKing)
        {
            // Simple crown representation
            var crownPath = new Path
            {
                Data = Geometry.Parse("M 10,15 L 15,5 L 20,15 L 25,5 L 30,15 L 25,20 L 15,20 Z"),
                Fill = Brushes.Gold,
                Stretch = Stretch.Uniform,
                Width = 20,
                Height = 10
            };
            
            var panel = new Grid();
            panel.Children.Add(ellipse);
            panel.Children.Add(crownPath);
            
            square.Content = panel;
        }
        else
        {
            square.Content = ellipse;
        }
    }
    
    // Method to clear a piece from a specific square
    public void ClearPiece(int row, int col)
    {
        if (row < 0 || row > 7 || col < 0 || col > 7)
            return;
            
        boardSquares[row, col].Content = null;
    }
    
    // Method to clear all pieces from the board
    public void ClearBoard()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                boardSquares[row, col].Content = null;
            }
        }
    }

    private void ForfeitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        gameManager?.Forfeit();
    }

    /// <summary>
    /// Displays confetti animation for the specified duration (ms), then clears it.
    /// </summary>
    public void StartConfetti(int durationMs = 5000)
    {
        // Find the confetti canvas in case it's not exposed as a field
        var canvas = FindName("ConfettiCanvas") as Canvas;
        if (canvas == null)
            return;

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
