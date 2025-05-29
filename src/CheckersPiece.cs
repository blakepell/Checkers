using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Checkers
{
    public class CheckersPiece : UserControl
    {        private Ellipse _pieceEllipse = null!;
        private Path _crownPath = null!;
        private Grid _pieceGrid = null!;
        
        public static readonly DependencyProperty IsKingProperty =
            DependencyProperty.Register("IsKing", typeof(bool), typeof(CheckersPiece), 
                new PropertyMetadata(false, OnIsKingChanged));
                
        public static readonly DependencyProperty PlayerProperty =
            DependencyProperty.Register("Player", typeof(Player), typeof(CheckersPiece),
                new PropertyMetadata(Player.Red, OnPlayerChanged));

        public bool IsKing
        {
            get { return (bool)GetValue(IsKingProperty); }
            set { SetValue(IsKingProperty, value); }
        }

        public Player Player
        {
            get { return (Player)GetValue(PlayerProperty); }
            set { SetValue(PlayerProperty, value); }
        }

        // Position in the board (0-7, 0-7)
        public int Row { get; set; }
        public int Column { get; set; }

        public CheckersPiece()
        {
            // Initialize the control layout
            InitializeControlLayout();
            UpdateVisuals();
        }

        private void InitializeControlLayout()
        {
            _pieceGrid = new Grid();
            
            _pieceEllipse = new Ellipse
            {
                Width = 40,
                Height = 40,
                StrokeThickness = 2,
                Margin = new Thickness(5)
            };

            _crownPath = new Path
            {
                Data = Geometry.Parse("M 10,15 L 15,5 L 20,15 L 25,5 L 30,15 L 25,20 L 15,20 Z"),
                Fill = Brushes.Gold,
                Stretch = Stretch.Uniform,
                Width = 20,
                Height = 10,
                Visibility = Visibility.Collapsed
            };

            _pieceGrid.Children.Add(_pieceEllipse);
            _pieceGrid.Children.Add(_crownPath);
            
            Content = _pieceGrid;
            
            // Set up drag & drop
            MouseMove += CheckersPiece_MouseMove;
        }

        private static void OnIsKingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckersPiece piece)
            {
                piece.UpdateVisuals();
            }
        }

        private static void OnPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckersPiece piece)
            {
                piece.UpdateVisuals();
            }
        }        private void UpdateVisuals()
        {
            try
            {
                // Update the ellipse color based on player
                if (Player == Player.Red)
                {
                    try
                    {
                        _pieceEllipse.Fill = (SolidColorBrush)FindResource("RedPieceColor");
                    }
                    catch
                    {
                        // Fallback if resource not found
                        _pieceEllipse.Fill = new SolidColorBrush(Colors.DarkRed);
                    }
                }
                else
                {
                    try
                    {
                        _pieceEllipse.Fill = (SolidColorBrush)FindResource("BlackPieceColor");
                    }
                    catch
                    {
                        // Fallback if resource not found
                        _pieceEllipse.Fill = new SolidColorBrush(Colors.Black);
                    }
                }
                
                _pieceEllipse.Stroke = Brushes.White;

                // Show/hide crown based on king status
                _crownPath.Visibility = IsKing ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error in UpdateVisuals: {ex.Message}");
            }
        }

        // Handles drag & drop initialization
        private void CheckersPiece_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Only start drag if left button is pressed
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
            }
        }
    }
    
    public enum Player
    {
        Red,
        Black
    }
}
