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

using System.IO;
using System.Media;

namespace Checkers.Managers
{
    /// <summary>
    /// Provides functionality for managing and playing sound effects in the application.
    /// </summary>
    /// <remarks>The <see cref="SoundManager"/> class initializes and provides access to preloaded sound
    /// effects  that can be used throughout the application. All sound effects are loaded from the "Assets/Audio" 
    /// directory relative to the application's base directory.   This class is static and cannot be instantiated. Use
    /// the exposed static members to access the  available sound effects.</remarks>
    public static class SoundManager
    {
        public static readonly SoundPlayer MoveSound;
        public static readonly SoundPlayer JumpSound;
        public static readonly SoundPlayer KingSound;
        public static readonly SoundPlayer ComputerJumpSound;
        public static readonly SoundPlayer GameOverSound;

        static SoundManager()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            JumpSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "Jump.wav"));
            KingSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "King.wav"));
            MoveSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "Move.wav"));
            ComputerJumpSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "ComputerJump.wav"));
            GameOverSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "GameOver.wav"));
        }
    }
}
