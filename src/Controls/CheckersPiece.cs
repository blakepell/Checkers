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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Checkers.Controls
{
    /// <summary>
    /// Represents a visual element for a checkers piece, including its position, player association, and king status.
    /// </summary>
    /// <remarks>
    /// This control is used to visually represent a checkers piece on a game board. It supports
    /// properties for  determining the piece's player, position, and whether it is a king. The visual appearance
    /// updates automatically  when these properties change. The control also supports drag-and-drop functionality for
    /// moving pieces.
    /// </remarks>
    public class CheckersPiece : UserControl
    {
        private Ellipse _pieceEllipse = null!;
        private Path _crownPath = null!;
        private Grid _pieceGrid = null!;

        /// <summary>
        /// Gets or sets if the piece is a king, it will display a crown.
        /// </summary>
        public static readonly DependencyProperty IsKingProperty =
            DependencyProperty.Register(nameof(IsKing), typeof(bool), typeof(CheckersPiece),
                new PropertyMetadata(false, OnIsKingChanged));

        /// <summary>
        /// Gets or sets if the piece is a king, it will display a crown.
        /// </summary>
        public bool IsKing
        {
            get => (bool)GetValue(IsKingProperty);
            set => SetValue(IsKingProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Player"/> instance associated with this object.
        /// </summary>
        public static readonly DependencyProperty PlayerProperty =
            DependencyProperty.Register(nameof(Player), typeof(Player), typeof(CheckersPiece),
                new PropertyMetadata(Player.Red, OnPlayerChanged));

        /// <summary>
        /// Gets or sets the <see cref="Player"/> instance associated with this object.
        /// </summary>
        public Player Player
        {
            get => (Player)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        /// <summary>
        /// Row position in the board (0-7, 0-7).
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Column position in the board (0-7, 0-7).
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckersPiece"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the initial layout and visual appearance of the checkers piece.
        /// </remarks>
        public CheckersPiece()
        {
            // Initialize the control layout
            InitializeControlLayout();
            UpdateVisuals();
        }

        /// <summary>
        /// Initializes the layout and visual elements of the control.
        /// </summary>
        /// <remarks>
        /// This method sets up the visual structure of the control by creating and configuring a
        /// grid, an ellipse, and a crown path. The elements are added to the grid, which is then assigned to the
        /// control's content. Additionally, it attaches a mouse move event handler to enable drag-and-drop
        /// functionality.
        /// </remarks>
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

        /// <summary>
        /// Handles changes to the <see cref="IsKing"/> dependency property.
        /// </summary>
        /// <remarks>This method updates the visuals of the <see cref="CheckersPiece"/> when the <see
        /// cref="IsKing"/> property changes.</remarks>
        /// <param name="d">The <see cref="DependencyObject"/> on which the property value has changed. Expected to be a <see
        /// cref="CheckersPiece"/>.</param>
        /// <param name="e">The event data containing information about the property change.</param>
        private static void OnIsKingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckersPiece piece)
            {
                piece.UpdateVisuals();
            }
        }

        /// <summary>
        /// Handles changes to the Player property of a <see cref="CheckersPiece"/> instance.
        /// </summary>
        /// <remarks>This method updates the visual representation of the <see cref="CheckersPiece"/> when
        /// the Player property changes.</remarks>
        /// <param name="d">The <see cref="DependencyObject"/> whose Player property has changed.</param>
        /// <param name="e">The event data containing information about the property change.</param>
        private static void OnPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckersPiece piece)
            {
                piece.UpdateVisuals();
            }
        }

        /// <summary>
        /// Updates the visual appearance of the game piece based on its current state.
        /// </summary>
        /// <remarks>This method adjusts the color of the game piece to reflect the player's color  (red
        /// or black) and displays a crown if the piece is a king. If the required  color resources are not found,
        /// fallback colors are used. Any errors during  the update process are logged but do not interrupt
        /// execution.</remarks>
        private void UpdateVisuals()
        {
            try
            {
                // Update the ellipse appearance with a radial gradient and drop shadow for depth
                var gradientBrush = new RadialGradientBrush
                {
                    GradientOrigin = new Point(0.3, 0.3),
                    Center = new Point(0.5, 0.5),
                    RadiusX = 0.5,
                    RadiusY = 0.5
                };
                if (Player == Player.Red)
                {
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 0));
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.DarkRed, 1));
                }
                else
                {
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.Gray, 0));
                    gradientBrush.GradientStops.Add(new GradientStop(Colors.Black, 1));
                }
                _pieceEllipse.Fill = gradientBrush;
                _pieceEllipse.Stroke = Brushes.White;
                _pieceEllipse.Effect = new DropShadowEffect { Color = Colors.Black, Direction = 270, ShadowDepth = 2, BlurRadius = 4, Opacity = 0.5 };

                // Show/hide crown based on king status
                _crownPath.Visibility = IsKing ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error in UpdateVisuals: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the mouse move event for a checkers piece to initiate a drag-and-drop operation.
        /// </summary>
        /// <remarks>This method starts a drag-and-drop operation when the left mouse button is pressed
        /// while moving the mouse. The drag-and-drop operation allows the checkers piece to be moved to a new
        /// position.</remarks>
        /// <param name="sender">The source of the event, typically the checkers piece being moved.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void CheckersPiece_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Only start drag if left button is pressed
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
            }
        }
    }

    /// <summary>
    /// Represents the players in a two-player game.
    /// </summary>
    /// <remarks>This enumeration defines the two possible players: <see cref="Red"/> and <see cref="Black"/>.
    /// It can be used to identify or differentiate between the players in game logic.</remarks>
    public enum Player
    {
        Red,
        Black
    }
}
