using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Media;
using System.Windows.Media;

namespace Checkers
{
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
        private readonly SoundPlayer _moveSound;
        private readonly SoundPlayer _jumpSound;
        private readonly SoundPlayer _kingSound;
        private readonly SoundPlayer _computerJumpSound;
        private readonly SoundPlayer _gameOverSound;
        // Last move accenting
        private Point? _lastMoveTarget;
        private readonly Brush _lastMoveAccentBrush = Brushes.Gold;
        private readonly Thickness _lastMoveAccentThickness = new Thickness(3);

        public bool SoundsEnabled { get => _soundsEnabled; set => _soundsEnabled = value; }

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
            _jumpSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "Jump.wav"));
            _kingSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "King.wav"));
            _moveSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "Move.wav"));
            _computerJumpSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "ComputerJump.wav"));
            _gameOverSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "GameOver.wav"));
            _soundsEnabled = true;
        }
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

        private void Square_DragLeave(object sender, DragEventArgs e)
        {
            // Reset opacity
            if (sender is Button square)
            {
                square.Opacity = 1.0;
            }
        }

        // Handler for click-to-move
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
                        int jumpRow = row + (rowDir * 2);
                        int jumpCol = col + (colDir * 2);

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
        private void MovePiece(CheckersPiece piece, int targetRow, int targetCol)
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
                    int capturedRow = sourceRow + ((targetRow - sourceRow) / 2);
                    int capturedCol = sourceCol + ((targetCol - sourceCol) / 2);

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
                            _kingSound.Play();
                        }
                        else if (isJumpMove)
                        {
                            // If AI performing jump, play computer jump sound, otherwise player jump sound
                            if (_gameMode == GameMode.SinglePlayer && piece.Player == Player.Black)
                            {
                                _computerJumpSound.Play();
                            }
                            else
                            {
                                _jumpSound.Play();
                            }
                        }
                        else
                        {
                            _moveSound.Play();
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

        private bool ShouldPromoteToKing(CheckersPiece piece, int row)
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

        private bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        private List<Point> GetJumpsFromPosition(CheckersPiece piece)
        {
            var jumps = new List<Point>();

            if (piece == null || _pieces == null)
            {
                return jumps;
            }

            int row = piece.Row;
            int col = piece.Column;
            Player player = piece.Player;
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
                    int jumpRow = row + (rowDir * 2);
                    int jumpCol = col + (colDir * 2);

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

        private void EndGame(Player winner)
        {
            _gameInProgress = false;

            _gameOverSound.Play();
            _mainWindow.StartConfetti();  // Launch confetti animation on win

            // Show game over message
            MessageBox.Show($"Game Over! {winner} wins!");
        }
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
            return (redKings == 1) && (blackKings == 1);
        }
        private void EndGameInDraw()
        {
            _gameInProgress = false;

            // Show game over message for draw
            MessageBox.Show("Game Over! It's a draw (1 Red King vs 1 Black King).");

            // Play game over sound
            if (_soundsEnabled)
            {
                _gameOverSound.Play();
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

        // AI methods for single player mode
        private void MakeAIMove()
        {
            try
            {
                if (_gameMode != GameMode.SinglePlayer || _currentPlayer != Player.Black)
                {
                    return;
                }

                // Add a small delay to make the AI move seem more natural
                System.Threading.Thread.Sleep(500);

                // Get all AI pieces with valid moves
                var aiPieces = new List<CheckersPiece>();
                var allJumpPieces = new List<CheckersPiece>();

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
        private Point SelectBestMove(CheckersPiece aiPiece, List<Point> validMoves)
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

        // Helper to update the window title based on current player
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