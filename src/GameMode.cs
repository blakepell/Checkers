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

namespace Checkers
{
    /// <summary>
    /// Represents the available game modes for a game.
    /// </summary>
    /// <remarks>
    /// Use <see cref="GameMode.SinglePlayer"/> for single-player games and  <see
    /// cref="GameMode.TwoPlayer"/> for games involving two players.
    /// </remarks>
    public enum GameMode
    {
        /// <summary>
        /// Represents a game mode where a single player participates.
        /// </summary>
        SinglePlayer,
        /// <summary>
        /// Represents a game mode or scenario designed for two human players (on the same device/tablet).
        /// </summary>
        TwoPlayer
    }
}
