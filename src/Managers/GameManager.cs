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
using Checkers.Common;
using Checkers.Controls;

namespace Checkers.Managers
{
    /// <summary>
    /// Manages the state and logic of a checkers game, including player turns, piece movements,  game rules, and
    /// interactions with the game board UI.
    /// </summary>
    /// <remarks>The <see cref="GameManager"/> class handles the core functionality of a checkers game,  such
    /// as initializing the board, managing player turns, validating moves, and determining  game outcomes. It supports
    /// both single-player (with AI) and two-player modes. The class  interacts with the UI elements of the game board
    /// to reflect the current state of the game.</remarks>
    public class GameManager
    {
        private readonly MainWindow _mainWindow;
        private readonly Grid _board;
        private readonly Button[,]? _boardSquares;
        private Player _currentPlayer;
        private CheckersPiece? _selectedPiece;
        private List<Point>? _validMoves;
        // Track game state
        private CheckersPiece?[,]? _pieces;
        private bool _gameInProgress;
        private bool _isInMultiJump; // Flag to track if a player is in the middle of multiple jumps
        private CheckersPiece? _multiJumpPiece; // The piece that's currently performing multiple jumps
        private GameMode _gameMode; // Current game mode (single or two player)
        private Random _random; // For AI moves
        private bool _soundsEnabled = true;
        // Last move accenting
        private Point? _lastMoveTarget;
        private readonly Brush _lastMoveAccentBrush = Brushes.Gold;
        private readonly Thickness _lastMoveAccentThickness = new Thickness(3);

        public bool SoundsEnabled { get => _soundsEnabled; set => _soundsEnabled = value; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameManager"/> class, setting up the game board, UI elements,
        /// and sound effects for a game of checkers.
        /// </summary>
        /// <remarks>This constructor initializes the game manager with default settings, including no
        /// game in progress and a default game mode of two-player. It also sets up sound effects for various game
        /// actions, such as moves, jumps, and game over events. The sound files are expected to be located in the
        /// "Assets/Audio"  directory relative to the application's base directory.</remarks>
        /// <param name="mainWindow">The main application window that hosts the game interface.</param>
        /// <param name="board">The <see cref="Grid"/> control representing the game board in the UI.</param>
        /// <param name="boardSquares">A two-dimensional array of <see cref="Button"/> controls representing the individual squares on the game
        /// board.</param>
        public GameManager(MainWindow mainWindow, Grid board, Button[,] boardSquares)
        {
            _mainWindow = mainWindow;
            _board = board;
            _boardSquares = boardSquares;
            _pieces = new CheckersPiece[8, 8];
            _validMoves = new List<Point>();
            _random = new Random();

            // Initialize with no game in progress
            _gameInProgress = false;
            _gameMode = GameMode.TwoPlayer; // Default to two-player mode

            // Initialize sound players
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            _soundsEnabled = true;
        }
        
        /// <summary>
        /// Initializes a new game with the specified game mode.
        /// </summary>
        /// <remarks>This method resets the game state, clears the board, and sets up the initial piece
        /// positions. It also ensures that the game is ready for play by initializing necessary variables and updating
        /// the game window.</remarks>
        /// <param name="gameMode">The mode of the game to be started, determining the rules and behavior of the game.</param>
        public void NewGame(GameMode gameMode)
        {
            try
            {
                // Clear the board first
                ClearBoard();

                // Reset game state
                _currentPlayer = Player.Red;  // Red goes first
                _gameInProgress = true;
                _selectedPiece = null;
                _isInMultiJump = false;
                _multiJumpPiece = null;
                _gameMode = gameMode;

                if (_validMoves == null)
                {
                    _validMoves = new List<Point>();
                }
                else
                {
                    _validMoves.Clear();
                }

                // Set up initial piece positions
                SetupInitialBoard();

                // Update window title
                UpdateTitle();

                // Debug info
                System.Diagnostics.Debug.WriteLine("New game successfully started");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting new game: {ex.Message}\n\n{ex.StackTrace}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Initializes the game board by placing the initial pieces for both players.
        /// </summary>
        /// <remarks>Black pieces are placed on the top three rows of the board (rows 0-2),  and Red
        /// pieces are placed on the bottom three rows (rows 5-7). Pieces are  only placed on dark squares, which are
        /// determined by the condition  <c>(row + col) % 2 == 1</c>.</remarks>
        private void SetupInitialBoard()
        {
            // Place Black pieces (top of board, rows 0-2)
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    // Only place pieces on dark squares: (row + col) % 2 == 1
                    if ((row + col) % 2 == 1)
                    {
                        AddPiece(row, col, Player.Black);
                    }
                }
            }

            // Place Red pieces (bottom of board, rows 5-7)
            for (int row = 5; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    // Only place pieces on dark squares: (row + col) % 2 == 1
                    if ((row + col) % 2 == 1)
                    {
                        AddPiece(row, col, Player.Red);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new checkers piece to the game board at the specified position.
        /// </summary>
        /// <remarks>This method validates the specified position and adds the piece to the internal data
        /// structure and the UI representation of the board. If the position is invalid or the board is not
        /// initialized, the method exits without making changes.</remarks>
        /// <param name="row">The row index of the board where the piece will be placed. Must be within the valid range of the board.</param>
        /// <param name="col">The column index of the board where the piece will be placed. Must be within the valid range of the board.</param>
        /// <param name="player">The player to whom the piece belongs.</param>
        /// <param name="isKing">A value indicating whether the piece is a king. Defaults to <see langword="false"/>.</param>
        private void AddPiece(int row, int col, Player player, bool isKing = false)
        {
            try
            {
                if (!IsValidPosition(row, col) || _boardSquares == null || _pieces == null)
                {
                    return;
                }

                // Create the checkers piece
                var piece = new CheckersPiece
                {
                    Row = row,
                    Column = col,
                    Player = player,
                    IsKing = isKing
                };

                // Add to our data structure
                _pieces[row, col] = piece;

                // Add to UI
                if (row >= 0 && row < 8 && col >= 0 && col < 8 && _boardSquares[row, col] != null)
                {
                    var square = _boardSquares[row, col];

                    // Add piece to the square
                    square.Content = piece;

                    // Set up drop handling on the square
                    square.AllowDrop = true;

                    // Remove any existing handlers first to avoid duplicates
                    square.Drop -= Square_Drop;
                    square.DragEnter -= Square_DragEnter;
                    square.DragLeave -= Square_DragLeave;

                    // Add the handlers
                    square.Drop += Square_Drop;
                    square.DragEnter += Square_DragEnter;
                    square.DragLeave += Square_DragLeave;

                    // Set up click handling for the piece
                    piece.MouseDown -= Piece_MouseDown; // Remove first to avoid duplicates
                    piece.MouseDown += Piece_MouseDown;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding piece at {row},{col}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Clears the game board by removing all pieces, resetting board squares, and detaching event handlers.
        /// </summary>
        /// <remarks>This method ensures that all event handlers associated with the board squares and
        /// pieces are removed to prevent memory leaks. It also resets the internal state of the board and pieces to
        /// their initial state. If the board is not initialized, the method exits without performing any
        /// actions.</remarks>
        private void ClearBoard()
        {
            if (_boardSquares == null)
            {
                return;
            }

            try
            {
                // First, remove event handlers from all pieces to prevent memory leaks
                if (_pieces != null)
                {
                    for (int row = 0; row < 8; row++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            if (_pieces[row, col] != null)
                            {
                                var piece = _pieces[row, col];
                                if (piece != null)
                                {
                                    piece.MouseDown -= Piece_MouseDown;
                                }
                            }
                        }
                    }
                }

                // Reset the board squares
                if (_boardSquares != null)
                {
                    for (int row = 0; row < 8; row++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            if (_boardSquares[row, col] != null)
                            {
                                // Remove the content
                                _boardSquares[row, col].Content = null;

                                // Clean up event handlers
                                _boardSquares[row, col].Drop -= Square_Drop;
                                _boardSquares[row, col].DragEnter -= Square_DragEnter;
                                _boardSquares[row, col].DragLeave -= Square_DragLeave;
                            }
                        }
                    }
                }

                // Reset the pieces array
                _pieces = new CheckersPiece?[8, 8];

                // Reset the board handlers (this is more reliable than trying to remove old ones)
                ResetBoardSquares();

                System.Diagnostics.Debug.WriteLine("Board successfully cleared");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing board: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Exception in ClearBoard: {ex}");
            }
        }

        /// <summary>
        /// Resets the state of all board squares, re-enabling drag-and-drop functionality and reattaching necessary
        /// event handlers.
        /// </summary>
        /// <remarks>This method ensures that all squares in the board are properly configured for
        /// interaction by enabling drop functionality and attaching or reattaching event handlers for drag-and-drop and
        /// click events. If the board squares are not initialized, the method exits without making changes.</remarks>
        private void ResetBoardSquares()
        {
            if (_boardSquares == null)
            {
                return;
            }

            // Re-configure all square event handlers
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_boardSquares == null || _boardSquares[row, col] == null)
                    {
                        continue;
                    }

                    var square = _boardSquares[row, col];

                    // Enable drop functionality for all squares
                    square.AllowDrop = true;

                    // Re-attach event handlers
                    square.Drop += Square_Drop;
                    square.DragEnter += Square_DragEnter;
                    square.DragLeave += Square_DragLeave;
                    // Attach click handler for click-to-move
                    square.Click -= Square_Click;
                    square.Click += Square_Click;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="UIElement.MouseDown"/> event for a checkers piece.
        /// </summary>
        /// <remarks>This method is invoked when a checkers piece is clicked during the game. It performs
        /// the following actions: <list type="bullet"> <item> <description>Ignores the click if the game is not in
        /// progress.</description> </item> <item> <description>Restricts selection to the currently jumping piece if
        /// the game is in multi-jump mode.</description> </item> <item> <description>Ensures that only the current
        /// player's pieces can be selected during their turn.</description> </item> <item> <description>Highlights
        /// valid moves for the selected piece.</description> </item> </list></remarks>
        /// <param name="sender">The checkers piece that was clicked.</param>
        /// <param name="e">The event data associated with the mouse down action.</param>
        private void Piece_MouseDown(object sender, RoutedEventArgs e)
        {
            // If game is not in progress, do nothing
            if (!_gameInProgress)
            {
                return;
            }

            var piece = sender as CheckersPiece;

            // If in multi-jump mode, only the piece that's jumping can be selected
            if (_isInMultiJump)
            {
                if (piece != _multiJumpPiece)
                {
                    return;
                }
            }

            // Otherwise, can only select own pieces during your turn
            else if (piece?.Player != _currentPlayer)
            {
                return;
            }

            // Select this piece and find valid moves
            _selectedPiece = piece;

            // Clear previous valid moves
            ClearHighlights();

            // Find and highlight valid moves
            _validMoves = _selectedPiece != null ? GetValidMoves(_selectedPiece) : new List<Point>();
            HighlightValidMoves(_validMoves);

            e.Handled = true;
        }
        private void Square_Drop(object sender, DragEventArgs e)
        {
            // If game is not in progress or no piece selected, do nothing
            if (!_gameInProgress || _selectedPiece == null)
            {
                return;
            }

            var square = sender as Button;

            if (square == null || square.Tag == null)
            {
                return;
            }

            var position = (Point)square.Tag;
            int targetRow = (int)position.X;
            int targetCol = (int)position.Y;

            // Check if this is a valid move and if _validMoves is initialized
            if (_validMoves == null || !IsValidMove(_selectedPiece, targetRow, targetCol))
            {
                return;
            }

            // If in multi-jump mode, only allow the multi-jump piece to move
            if (_isInMultiJump && _multiJumpPiece != _selectedPiece)
            {
                return;
            }

            // Execute the move
            MovePiece(_selectedPiece, targetRow, targetCol);

            // If we're not in multi-jump mode (meaning the move either wasn't a jump
            // or was a jump but with no additional jumps available)
            if (!_isInMultiJump)
            {
                // Clear highlights
                ClearHighlights();

                // Clear selection
                _selectedPiece = null;

                // Check for end of game
                CheckForGameEnd();
            }
            // Otherwise, we're in multi-jump mode and MovePiece has already set up the next move
        }

        /// <summary>
        /// Handles the <see cref="DragEventArgs"/> when a drag operation enters a square.
        /// </summary>
        /// <remarks>This method checks if the dragged piece can be moved to the target square. If the
        /// move is valid,  the square's appearance is updated to indicate its eligibility as a drop target.</remarks>
        /// <param name="sender">The source of the event, typically a <see cref="Button"/> representing a square.</param>
        /// <param name="e">The event data containing information about the drag operation.</param>
        private void Square_DragEnter(object sender, DragEventArgs e)
        {
            if (_selectedPiece != null)
            {
                var square = sender as Button;

                if (square?.Tag == null)
                {
                    return;
                }

                var position = (Point)square.Tag;
                int targetRow = (int)position.X;
                int targetCol = (int)position.Y;

                // Highlight the square if it's a valid move
                if (IsValidMove(_selectedPiece, targetRow, targetCol))
                {
                    square.Opacity = 0.7;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="UIElement.DragLeave"/> event for a square button.
        /// </summary>
        /// <remarks>This method resets the opacity of the button to its default value when a drag
        /// operation leaves the button's bounds.</remarks>
        /// <param name="sender">The source of the event, expected to be a <see cref="Button"/>.</param>
        /// <param name="e">The event data associated with the drag leave operation.</param>
        private void Square_DragLeave(object sender, DragEventArgs e)
        {
            // Reset opacity
            if (sender is Button square)
            {
                square.Opacity = 1.0;
            }
        }

        /// <summary>
        /// Handles the click event for a game board square, allowing a selected piece to move to the clicked square if
        /// the move is valid.
        /// </summary>
        /// <remarks>This method validates the move based on the current game state, including whether the
        /// game is in progress, whether a piece is selected, and whether the move adheres to the game's rules. If the
        /// move is valid, the piece is moved to the target square, and the game state is updated accordingly.
        /// Multi-jump constraints and game-end conditions are also handled.</remarks>
        /// <param name="sender">The button representing the clicked square.</param>
        /// <param name="e">The event data associated with the click event.</param>
        private void Square_Click(object sender, RoutedEventArgs e)
        {
            // If game not in progress or no piece selected, do nothing
            if (!_gameInProgress || _selectedPiece == null)
            {
                return;
            }

            var square = sender as Button;

            if (square?.Tag == null)
            {
                return;
            }

            var position = (Point)square.Tag;
            int targetRow = (int)position.X;
            int targetCol = (int)position.Y;

            // Validate move
            if (_validMoves == null || !IsValidMove(_selectedPiece, targetRow, targetCol))
            {
                return;
            }

            // If multi-jump constraint
            if (_isInMultiJump && _multiJumpPiece != _selectedPiece)
            {
                return;
            }

            // Perform move
            MovePiece(_selectedPiece, targetRow, targetCol);

            // Finalize if no further jumps
            if (!_isInMultiJump)
            {
                ClearHighlights();
                _selectedPiece = null;
                CheckForGameEnd();
            }

            e.Handled = true;
        }

        /// <summary>
        /// Determines the valid moves for a given checkers piece based on the current game state.
        /// </summary>
        /// <remarks>This method calculates both standard moves (one square diagonally) and jump moves
        /// (capturing an opponent's piece).  Jump moves take precedence over standard moves, and if any jump moves are
        /// available, they must be performed.  If the game is in a multi-jump state, only moves for the piece currently
        /// performing the multi-jump are considered.</remarks>
        /// <param name="piece">The checkers piece for which to calculate valid moves. This parameter can be null, in which case an empty
        /// list is returned.</param>
        /// <returns>A list of <see cref="Point"/> objects representing the valid moves for the specified piece.  If the piece
        /// can perform a jump move, only jump moves are returned as they are mandatory.  If the piece is in a
        /// multi-jump sequence and no further jumps are available, an empty list is returned.</returns>
        private List<Point> GetValidMoves(CheckersPiece? piece)
        {
            var moves = new List<Point>();
            var jumpMoves = new List<Point>();

            try
            {
                if (piece == null || _pieces == null)
                {
                    return moves;
                }

                int row = piece.Row;
                int col = piece.Column;
                var player = piece.Player;
                bool isKing = piece.IsKing;

                // Direction of movement depends on player (unless it's a king)
                int[] rowDirections = isKing ? new[] { -1, 1 } : player == Player.Red ? new[] { -1 } : new[] { 1 };

                foreach (int rowDir in rowDirections)
                {
                    for (int colDir = -1; colDir <= 1; colDir += 2) // -1 and 1 for diagonals
                    {
                        // Jump move (capturing an opponent's piece)
                        int newRow = row + rowDir;
                        int newCol = col + colDir;
                        int jumpRow = row + rowDir * 2;
                        int jumpCol = col + colDir * 2;

                        if (IsValidPosition(newRow, newCol) && IsValidPosition(jumpRow, jumpCol) &&
                            _pieces[newRow, newCol] != null && _pieces[newRow, newCol]!.Player != player &&
                            _pieces[jumpRow, jumpCol] == null)
                        {
                            jumpMoves.Add(new Point(jumpRow, jumpCol));
                        }

                        // Standard move (one square) - only allowed if not in multi-jump and no jumps available
                        if (!_isInMultiJump && IsValidPosition(newRow, newCol) && _pieces[newRow, newCol] == null)
                        {
                            moves.Add(new Point(newRow, newCol));
                        }
                    }
                }

                // If there are jump moves available, they are mandatory
                if (jumpMoves.Count > 0)
                {
                    return jumpMoves;
                }

                // If we're in multi-jump mode and no jumps are available, return empty list
                if (_isInMultiJump && _multiJumpPiece == piece)
                {
                    return new List<Point>();
                }
            }
            catch (Exception ex)
            {
                // Log the error but return an empty list of moves rather than crashing
                System.Diagnostics.Debug.WriteLine($"Error getting valid moves: {ex.Message}");
            }

            return moves;
        }
        
        /// <summary>
        /// Determines whether the specified move for a given checkers piece is valid.
        /// </summary>
        /// <remarks>A move is considered valid if it matches one of the precomputed valid moves stored in
        /// the internal state.</remarks>
        /// <param name="piece">The checkers piece for which the move is being validated.</param>
        /// <param name="targetRow">The target row of the move.</param>
        /// <param name="targetCol">The target column of the move.</param>
        /// <returns><see langword="true"/> if the move to the specified row and column is valid for the given piece; otherwise,
        /// <see langword="false"/>.</returns>
        private bool IsValidMove(CheckersPiece piece, int targetRow, int targetCol)
        {
            if (_validMoves == null)
            {
                return false;
            }

            foreach (var move in _validMoves)
            {
                if ((int)move.X == targetRow && (int)move.Y == targetCol)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Moves a checkers piece to the specified target position on the board.
        /// </summary>
        /// <remarks>This method handles all aspects of moving a piece, including capturing opponent
        /// pieces, promoting to king, updating the game state, and switching turns. If the move is a jump, it checks
        /// for additional jumps and enables multi-jump mode if applicable. In single-player mode, the AI's turn is
        /// triggered after the player's move.</remarks>
        /// <param name="piece">The <see cref="CheckersPiece"/> to move. Cannot be <see langword="null"/>.</param>
        /// <param name="targetRow">The target row index on the board. Must be within the valid board range.</param>
        /// <param name="targetCol">The target column index on the board. Must be within the valid board range.</param>
        private void MovePiece(CheckersPiece? piece, int targetRow, int targetCol)
        {
            try
            {
                if (piece == null || _pieces == null || _boardSquares == null)
                {
                    return;
                }

                // Get source position
                int sourceRow = piece.Row;
                int sourceCol = piece.Column;

                bool isJumpMove = Math.Abs(targetRow - sourceRow) == 2;

                // Check if this is a capture move (jump)
                if (isJumpMove)
                {
                    // Calculate position of captured piece
                    int capturedRow = sourceRow + (targetRow - sourceRow) / 2;
                    int capturedCol = sourceCol + (targetCol - sourceCol) / 2;

                    // Remove the captured piece
                    RemovePiece(capturedRow, capturedCol);
                }

                // Make sure positions are valid before updating
                if (IsValidPosition(sourceRow, sourceCol) && IsValidPosition(targetRow, targetCol))
                {
                    // Update the internal state
                    _pieces[sourceRow, sourceCol] = null;
                    _pieces[targetRow, targetCol] = piece;

                    // Update the piece's properties
                    piece.Row = targetRow;
                    piece.Column = targetCol;

                    // Update the UI if the buttons exist
                    if (_boardSquares[sourceRow, sourceCol] != null && _boardSquares[targetRow, targetCol] != null)
                    {
                        _boardSquares[sourceRow, sourceCol].Content = null;
                        _boardSquares[targetRow, targetCol].Content = piece;
                    }

                    // Accent the square of the last move
                    ClearLastMoveAccent();
                    AccentLastMove(targetRow, targetCol);

                    // Promotion check
                    bool isPromotion = ShouldPromoteToKing(piece, targetRow);
                    if (isPromotion)
                    {
                        piece.IsKing = true;
                    }

                    // Play appropriate sound
                    if (_soundsEnabled)
                    {
                        if (isPromotion)
                        {
                            SoundManager.KingSound.Play();
                        }
                        else if (isJumpMove)
                        {
                            // If AI performing jump, play computer jump sound, otherwise player jump sound
                            if (_gameMode == GameMode.SinglePlayer && piece.Player == Player.Black)
                            {
                                SoundManager.ComputerJumpSound.Play();
                            }
                            else
                            {
                                SoundManager.JumpSound.Play();
                            }
                        }
                        else
                        {
                            SoundManager.MoveSound.Play();
                        }
                    }

                    // If it was a jump move, check if additional jumps are available
                    if (isJumpMove)
                    {
                        // Get potential additional jump moves
                        List<Point> additionalJumps = GetJumpsFromPosition(piece);

                        if (additionalJumps.Count > 0)
                        {
                            // If there are additional jumps, enter multi-jump mode
                            _isInMultiJump = true;
                            _multiJumpPiece = piece;
                            _selectedPiece = piece;

                            // Update valid moves to show only additional jumps
                            _validMoves = additionalJumps;

                            // Highlight the valid moves
                            ClearHighlights();
                            HighlightValidMoves(additionalJumps);

                            // Don't switch players yet
                            return;
                        }
                    }
                    // Reset multi-jump state
                    _isInMultiJump = false;
                    _multiJumpPiece = null;

                    // Switch to the other player's turn
                    _currentPlayer = _currentPlayer == Player.Red ? Player.Black : Player.Red;

                    // Update window title after turn change
                    UpdateTitle();

                    // If in single-player mode and it's the AI's turn, make an AI move
                    if (_gameMode == GameMode.SinglePlayer && _currentPlayer == Player.Black)
                    {
                        // Use Dispatcher to run the AI move after a short delay
                        _mainWindow.Dispatcher.BeginInvoke(new Action(MakeAIMove), System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error moving piece: {ex.Message}");
                MessageBox.Show($"Error moving piece: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a piece from the specified position on the board.
        /// </summary>
        /// <remarks>If the specified position is invalid or there is no piece at the given position, the
        /// method performs no action.</remarks>
        /// <param name="row">The row index of the position from which to remove the piece. Must be a valid board position.</param>
        /// <param name="col">The column index of the position from which to remove the piece. Must be a valid board position.</param>
        private void RemovePiece(int row, int col)
        {
            if (_boardSquares == null || _pieces == null)
            {
                return;
            }

            if (!IsValidPosition(row, col) || _pieces[row, col] == null)
            {
                return;
            }

            // Remove from UI
            _boardSquares[row, col].Content = null;

            // Remove from data structure
            _pieces[row, col] = null;
        }

        /// <summary>
        /// Determines whether the specified piece should be promoted to a king based on its position.
        /// </summary>
        /// <param name="piece">The checkers piece to evaluate. Must not be null.</param>
        /// <param name="row">The row index of the piece's current position, where 0 is the top row and 7 is the bottom row.</param>
        /// <returns><see langword="true"/> if the piece should be promoted to a king; otherwise, <see langword="false"/>. A
        /// piece is promoted to a king if it is not already a king and reaches the opponent's back row (row 0 for red
        /// pieces, row 7 for black pieces).</returns>
        private bool ShouldPromoteToKing(CheckersPiece? piece, int row)
        {
            // Prevent re-promoting an already-king piece
            if (piece.IsKing)
            {
                return false;
            }

            // Red pieces become kings on row 0
            if (piece.Player == Player.Red && row == 0)
            {
                return true;
            }

            // Black pieces become kings on row 7
            if (piece.Player == Player.Black && row == 7)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Highlights the valid moves on the game board by visually indicating the specified positions.
        /// </summary>
        /// <remarks>Each valid move is highlighted by changing the background color of the corresponding
        /// board square. The highlight color alternates based on the square's position to maintain the board's visual
        /// pattern. If the board is not initialized, the method exits without performing any action.</remarks>
        /// <param name="moves">A list of <see cref="Point"/> objects representing the valid move positions to highlight.</param>
        private void HighlightValidMoves(List<Point> moves)
        {
            if (_boardSquares == null)
            {
                return;
            }

            foreach (var move in moves)
            {
                int row = (int)move.X;
                int col = (int)move.Y;

                if (IsValidPosition(row, col))
                {
                    // Add a visual indicator for valid moves
                    _boardSquares[row, col].Background =
                        (row + col) % 2 == 0
                        ? _mainWindow.LightSquareHighlightColor
                        : _mainWindow.DarkSquareHighlightColor;
                }
            }
        }

        /// <summary>
        /// Clears all visual highlights from the chessboard squares, restoring their original colors and opacity.
        /// </summary>
        /// <remarks>This method resets the background color and opacity of each square on the chessboard
        /// to its default state. It assumes an 8x8 board layout and alternates colors based on the square's position.
        /// If the board squares are not initialized, the method exits without performing any actions.</remarks>
        private void ClearHighlights()
        {
            if (_boardSquares == null)
            {
                return;
            }

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    // Reset the square color to its original color
                    _boardSquares[row, col].Opacity = 1.0;  // Ensure square is fully opaque
                    _boardSquares[row, col].Background =
                        (row + col) % 2 == 0
                        ? _mainWindow.LightSquareColor
                        : _mainWindow.DarkSquareColor;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified position is within the bounds of an 8x8 grid.
        /// </summary>
        /// <param name="row">The row index to validate. Must be a non-negative integer less than 8.</param>
        /// <param name="col">The column index to validate. Must be a non-negative integer less than 8.</param>
        /// <returns><see langword="true"/> if the specified position is within the bounds of the grid;  otherwise, <see
        /// langword="false"/>.</returns>
        private bool IsValidPosition(int row, int col)
        {
            return row is >= 0 and < 8 && col is >= 0 and < 8;
        }

        private List<Point> GetJumpsFromPosition(CheckersPiece? piece)
        {
            var jumps = new List<Point>();

            if (piece == null || _pieces == null)
            {
                return jumps;
            }

            int row = piece.Row;
            int col = piece.Column;
            var player = piece.Player;
            bool isKing = piece.IsKing;

            // Direction of movement depends on player (unless it's a king)
            int[] rowDirections = isKing ? new[] { -1, 1 } : player == Player.Red ? new[] { -1 } : new[] { 1 };

            foreach (int rowDir in rowDirections)
            {
                for (int colDir = -1; colDir <= 1; colDir += 2) // -1 and 1 for diagonals
                {
                    // Check for jump moves only
                    int newRow = row + rowDir;
                    int newCol = col + colDir;
                    int jumpRow = row + rowDir * 2;
                    int jumpCol = col + colDir * 2;

                    if (IsValidPosition(newRow, newCol) && IsValidPosition(jumpRow, jumpCol) &&
                        _pieces[newRow, newCol] != null && _pieces[newRow, newCol]!.Player != player &&
                        _pieces[jumpRow, jumpCol] == null)
                    {
                        jumps.Add(new Point(jumpRow, jumpCol));
                    }
                }
            }

            return jumps;
        }

        /// <summary>
        /// Determines whether the game has ended based on the current state of the board and game rules.
        /// </summary>
        /// <remarks>This method checks for various game-ending conditions, including: <list
        /// type="bullet"> <item><description>Whether the game is in multi-jump mode, in which case the check is
        /// deferred.</description></item> <item><description>The "only two kings left" draw
        /// condition.</description></item> <item><description>Whether either player has no playable pieces or valid
        /// moves remaining.</description></item> </list> If a game-ending condition is met, the appropriate action is
        /// taken, such as ending the game in a draw or declaring a winner. Any exceptions encountered during the check
        /// are logged but do not interrupt execution.</remarks>
        private void CheckForGameEnd()
        {
            try
            {
                // If we're in multi-jump mode, don't check for game end yet
                if (_isInMultiJump)
                {
                    return;
                }

                // Check for the "only two kings left" draw condition
                if (CheckForTwoKingsDrawCondition())
                {
                    EndGameInDraw();
                    return;
                }

                bool redHasPlayablePieces = false;
                bool blackHasPlayablePieces = false;

                // Check if each player has pieces and valid moves
                if (_pieces != null)
                {
                    for (int row = 0; row < 8; row++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            var piece = _pieces[row, col];
                            if (piece == null)
                            {
                                continue;
                            }

                            if (piece.Player == Player.Red && GetValidMoves(piece).Count > 0)
                            {
                                redHasPlayablePieces = true;
                            }
                            else if (piece.Player == Player.Black && GetValidMoves(piece).Count > 0)
                            {
                                blackHasPlayablePieces = true;
                            }

                            // Early exit if both players have playable pieces
                            if (redHasPlayablePieces && blackHasPlayablePieces)
                            {
                                return;
                            }
                        }
                    }
                }
                // Game ends if a player has no playable pieces
                if (!redHasPlayablePieces)
                {
                    EndGame(Player.Black);
                }
                else if (!blackHasPlayablePieces)
                {
                    EndGame(Player.Red);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error checking game end: {ex.Message}");
            }
        }

        /// <summary>
        /// Ends the current game and declares the specified player as the winner.
        /// </summary>
        /// <remarks>This method stops the game, plays a game-over sound, triggers a confetti animation, 
        /// and displays a message box announcing the winner.</remarks>
        /// <param name="winner">The player who won the game. Cannot be null.</param>
        private void EndGame(Player winner)
        {
            _gameInProgress = false;

            SoundManager.GameOverSound.Play();
            _mainWindow.StartConfetti();  // Launch confetti animation on win

            // Show game over message
            MessageBox.Show($"Game Over! {winner} wins!");
        }

        /// <summary>
        /// Determines whether the current game state is a draw due to only two kings remaining on the board.
        /// </summary>
        /// <remarks>This method checks if the board contains exactly one red king and one black king,
        /// with no other pieces. If more than two pieces are found, the method returns <see
        /// langword="false"/>.</remarks>
        /// <returns><see langword="true"/> if the board contains exactly one red king and one black king; otherwise, <see
        /// langword="false"/>.</returns>
        private bool CheckForTwoKingsDrawCondition()
        {
            // It's a draw if there is exactly one red king and one black king left
            if (_pieces == null)
            {
                return false;
            }

            int redKings = 0;
            int blackKings = 0;
            int totalPieces = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_pieces[row, col] != null)
                    {
                        totalPieces++;

                        // Count kings by color
                        if (_pieces[row, col]!.IsKing)
                        {
                            if (_pieces[row, col]!.Player == Player.Red)
                            {
                                redKings++;
                            }
                            else
                            {
                                blackKings++;
                            }
                        }

                        // If we find more than 2 pieces, this isn't a draw condition
                        if (totalPieces > 2)
                        {
                            return false;
                        }
                    }
                }
            }

            // It's a draw if there is exactly one red king and one black king
            return redKings == 1 && blackKings == 1;
        }

        /// <summary>
        /// Ends the game and declares a draw when only one red king and one black king remain.
        /// </summary>
        /// <remarks>This method stops the game, displays a message indicating the draw, and optionally
        /// plays a game-over sound if sounds are enabled.</remarks>
        private void EndGameInDraw()
        {
            _gameInProgress = false;

            // Show game over message for draw
            MessageBox.Show("Game Over! It's a draw (1 Red King vs 1 Black King).");

            // Play game over sound
            if (_soundsEnabled)
            {
                SoundManager.GameOverSound.Play();
            }
        }

        /// <summary>
        /// Forfeits the current game, awarding victory to the opponent.
        /// </summary>
        public void Forfeit()
        {
            if (!_gameInProgress)
            {
                return;
            }

            // Clear any highlights and selection
            ClearHighlights();
            _selectedPiece = null;

            // Determine opponent as winner
            var winner = _currentPlayer == Player.Red ? Player.Black : Player.Red;
            EndGame(winner);
        }

        /// <summary>
        /// Executes the AI's move in a single-player game when it is the AI's turn.
        /// </summary>
        /// <remarks>This method is invoked to make the AI perform a move during a single-player game.  It
        /// prioritizes capturing moves (jumps) and moves that promote a piece to a king.  If no valid moves are
        /// available, the AI forfeits its turn. The method also handles  multi-jump scenarios by recursively calling
        /// itself until no further jumps are possible.</remarks>
        private void MakeAIMove()
        {
            try
            {
                if (_gameMode != GameMode.SinglePlayer || _currentPlayer != Player.Black)
                {
                    return;
                }

                // Add a small delay to make the AI move seem more natural
                Thread.Sleep(500);

                // Get all AI pieces with valid moves
                var aiPieces = new List<CheckersPiece?>();
                var allJumpPieces = new List<CheckersPiece?>();

                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        var piece = _pieces?[row, col];
                        if (piece != null && piece.Player == Player.Black)
                        {
                            var moves = GetValidMoves(piece);
                            if (moves.Count > 0)
                            {
                                aiPieces.Add(piece);

                                // Check if any moves are jumps (capturing moves)
                                foreach (var move in moves)
                                {
                                    if (Math.Abs(piece.Row - (int)move.X) == 2)
                                    {
                                        allJumpPieces.Add(piece);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }                // No valid moves, AI loses
                if (aiPieces.Count == 0)
                {
                    return;
                }

                // Prioritize pieces that can capture
                var selectedPieces = allJumpPieces.Count > 0 ? allJumpPieces : aiPieces;

                // New: prioritize pieces that can be promoted to king
                var promotionPieces = selectedPieces.Where(p => GetValidMoves(p)
                                          .Any(m => (int)m.X == 7 && !p.IsKing)).ToList();
                if (promotionPieces.Count > 0)
                {
                    selectedPieces = promotionPieces;
                }

                // Select a random piece that can move
                var aiPiece = selectedPieces[_random.Next(selectedPieces.Count)];
                var validMoves = GetValidMoves(aiPiece);

                // Select the best move for the AI piece
                Point bestMove = SelectBestMove(aiPiece, validMoves);

                // Execute the move
                _selectedPiece = aiPiece;
                MovePiece(aiPiece, (int)bestMove.X, (int)bestMove.Y);

                // If we're not in multi-jump mode (meaning the move either wasn't a jump
                // or was a jump but with no additional jumps available)
                if (!_isInMultiJump)
                {
                    // Clear selection
                    _selectedPiece = null;

                    // Check for end of game
                    CheckForGameEnd();
                }
                // Otherwise, we're in multi-jump mode, so the AI needs to make another move
                else
                {
                    MakeAIMove();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AI move: {ex.Message}");
            }
        }

        /// <summary>
        /// Selects the best move for the AI-controlled checkers piece from a list of valid moves.
        /// </summary>
        /// <remarks>The method prioritizes moves in the following order: 1. Jumps (capturing moves), with
        /// preference for jumps that promote the piece to a king. 2. Moves that promote the piece to a king. 3. Moves
        /// that avoid immediate capture by the opponent. 4. A random valid move if no other criteria are met.</remarks>
        /// <param name="aiPiece">The AI-controlled checkers piece for which the move is being selected. Cannot be null.</param>
        /// <param name="validMoves">A list of valid moves available to the AI piece. Must not be null or empty.</param>
        /// <returns>The <see cref="Point"/> representing the selected move. The move is chosen based on a prioritized strategy:
        /// jumps (capturing moves) are preferred, followed by moves that promote the piece to a king,  then moves that
        /// avoid immediate capture, and finally a random valid move if no other criteria are met.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="validMoves"/> is null or contains no elements.</exception>
        private Point SelectBestMove(CheckersPiece? aiPiece, List<Point> validMoves)
        {
            if (validMoves == null || validMoves.Count == 0)
            {
                throw new InvalidOperationException("No valid moves available for AI.");
            }

            // 1. Jumps (capturing moves)
            var jumpMoves = new List<Point>();

            foreach (var move in validMoves)
            {
                if (Math.Abs(aiPiece.Row - (int)move.X) == 2)
                {
                    jumpMoves.Add(move);
                }
            }

            if (jumpMoves.Count > 0)
            {
                // Prefer jumps that promote to king
                var promoteJumps = jumpMoves.Where(j => (int)j.X == 7 && !aiPiece.IsKing).ToList();
                if (promoteJumps.Count > 0)
                {
                    return promoteJumps.First();
                }

                return jumpMoves[_random.Next(jumpMoves.Count)];
            }

            // 2. Moves that make a king
            var kingMoves = validMoves.Where(m => (int)m.X == 7 && !aiPiece.IsKing).ToList();

            if (kingMoves.Count > 0)
            {
                return kingMoves.First();
            }

            // 3. Moves that avoid immediate capture
            var safeMoves = new List<Point>();

            foreach (var move in validMoves)
            {
                int targetRow = (int)move.X;
                int targetCol = (int)move.Y;
                bool isSafe = true;

                // Simulate move
                int origRow = aiPiece.Row;
                int origCol = aiPiece.Column;
                var displaced = _pieces[targetRow, targetCol];
                _pieces[origRow, origCol] = null;
                _pieces[targetRow, targetCol] = aiPiece;

                // Check potential captures by Red
                for (int r = Math.Max(0, targetRow - 2); r <= Math.Min(7, targetRow + 2) && isSafe; r++)
                {
                    for (int c = Math.Max(0, targetCol - 2); c <= Math.Min(7, targetCol + 2) && isSafe; c++)
                    {
                        var opp = _pieces[r, c];
                        if (opp != null && opp.Player == Player.Red)
                        {
                            var oppJumps = GetJumpsFromPosition(opp);
                            foreach (var j in oppJumps)
                            {
                                int capR = (r + (int)j.X) / 2;
                                int capC = (c + (int)j.Y) / 2;
                                if (capR == targetRow && capC == targetCol)
                                {
                                    isSafe = false;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Restore board
                _pieces[origRow, origCol] = aiPiece;
                _pieces[targetRow, targetCol] = displaced;

                if (isSafe)
                {
                    safeMoves.Add(move);
                }
            }
            if (safeMoves.Count > 0)
            {
                return safeMoves[_random.Next(safeMoves.Count)];
            }

            // 4. Default: random move
            return validMoves[_random.Next(validMoves.Count)];
        }

        /// <summary>
        /// Updates the main window's title to reflect the current player's turn and game mode.
        /// </summary>
        /// <remarks>The title indicates whether it is Player 1's turn, Player 2's turn, or the computer's
        /// turn, depending on the current player and game mode. This method ensures the title update is performed on
        /// the UI thread.</remarks>
        private void UpdateTitle()
        {
            string title;
            if (_currentPlayer == Player.Red)
            {
                title = "Checkers: Player 1's Turn";
            }
            else
            {
                title = _gameMode == GameMode.SinglePlayer
                    ? "Checkers: Computer's Turn"
                    : "Checkers: Player 2's Turn";
            }
            _mainWindow.Dispatcher.Invoke(() => _mainWindow.Title = title);
        }

        /// <summary>
        /// Clears the visual accent applied to the last move on the game board, if any.
        /// </summary>
        /// <remarks>This method removes the border styling from the square that was previously
        /// highlighted as the target of the last move. If no move has been highlighted or the board is uninitialized,
        /// the method performs no action.</remarks>
        private void ClearLastMoveAccent()
        {
            if (_boardSquares == null)
            {
                return;
            }

            if (_lastMoveTarget.HasValue)
            {
                int row = (int)_lastMoveTarget.Value.X;
                int col = (int)_lastMoveTarget.Value.Y;
                var square = _boardSquares[row, col];
                square.BorderBrush = null;
                square.BorderThickness = new Thickness(0);
                _lastMoveTarget = null;
            }
        }

        /// <summary>
        /// Highlights the last move on the game board by applying a visual accent to the specified square.
        /// </summary>
        /// <remarks>This method updates the border appearance of the specified square to indicate the
        /// last move. If the board is not initialized, the method exits without making any changes.</remarks>
        /// <param name="row">The zero-based row index of the square to accent.</param>
        /// <param name="col">The zero-based column index of the square to accent.</param>
        private void AccentLastMove(int row, int col)
        {
            if (_boardSquares == null)
            {
                return;
            }

            var square = _boardSquares[row, col];
            square.BorderBrush = _lastMoveAccentBrush;
            square.BorderThickness = _lastMoveAccentThickness;
            _lastMoveTarget = new Point(row, col);
        }
    }
}