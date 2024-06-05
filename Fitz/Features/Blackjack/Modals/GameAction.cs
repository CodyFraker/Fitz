using System;

namespace Fitz.Features.Blackjack.Modals
{
    /// <summary>
    /// Adapted from https://github.com/koistya/Blackjack/blob/master/Blackjack/GameAction.cs
    /// </summary>
    [Flags]
    public enum GameAction : byte
    {
        None = 1,
        Deal = 2,
        Stand = 4,
        Hit = 8
    }
}