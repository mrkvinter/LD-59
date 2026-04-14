using Game.Core.Contexts;
using KvinterGames;

namespace Code.Game.Core
{
    public class G : BaseServiceLocator<G>
    {
        public static AudioService AudioService => Resolve<AudioService>();
    }
}