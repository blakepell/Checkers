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
using Argus.Memory;
using Checkers.Common;

namespace Checkers.Managers
{
    /// <summary>
    /// Represents the available sound effects.
    /// </summary>
    public enum SoundEffect
    {
        Move,
        Jump,
        King,
        ComputerJump,
        GameOver
    }

    /// <summary>
    /// Provides functionality to manage and play sound effects in the application.
    /// </summary>
    /// <remarks>The <see cref="SoundManager"/> class is a static utility for playing predefined sound
    /// effects,  such as move, jump, and game over sounds. It uses the <see cref="SoundPlayer"/> class to load  and
    /// play audio files located in the application's "Assets/Audio" directory.</remarks>
    public static class SoundManager
    {
        private static readonly SoundPlayer MoveSound;
        private static readonly SoundPlayer JumpSound;
        private static readonly SoundPlayer KingSound;
        private static readonly SoundPlayer ComputerJumpSound;
        private static readonly SoundPlayer GameOverSound;

        static SoundManager()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            MoveSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "Move.wav"));
            JumpSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "Jump.wav"));
            KingSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "King.wav"));
            ComputerJumpSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "ComputerJump.wav"));
            GameOverSound = new SoundPlayer(Path.Combine(basePath, "Assets", "Audio", "GameOver.wav"));
        }

        /// <summary>
        /// Plays the specified sound effect.
        /// </summary>
        /// <param name="effect">The sound effect to play.</param>
        public static void Play(SoundEffect effect)
        {
            var appSettings = AppServices.GetRequiredService<AppSettings>();

            if (!appSettings.SoundEnabled)
            {
                return;
            }

            switch (effect)
            {
                case SoundEffect.Move:
                    MoveSound.Play();
                    break;
                case SoundEffect.Jump:
                    JumpSound.Play();
                    break;
                case SoundEffect.King:
                    KingSound.Play();
                    break;
                case SoundEffect.ComputerJump:
                    ComputerJumpSound.Play();
                    break;
                case SoundEffect.GameOver:
                    GameOverSound.Play();
                    break;
            }
        }
    }
}