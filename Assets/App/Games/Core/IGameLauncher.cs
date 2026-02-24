using Cysharp.Threading.Tasks;
using Miyo.Data;

namespace Miyo.Games
{
    public interface IGameLauncher
    {
        bool IsGameActive { get; }
        UniTask LaunchAsync(GameDefinition game);
    }
}
