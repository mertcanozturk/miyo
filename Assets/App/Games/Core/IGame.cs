using System;

namespace Miyo.Games
{
    public interface IGame
    {
        void StartGame(string childName);
        void ExitGame();
        event Action GameExited;
    }
}
