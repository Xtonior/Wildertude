//WILDERTUDE - WILDERNESS + VICISSITUDE

using XGE3D;
using XGE3D.Core;
using XGE3D.Core.SceneSystem;

namespace Wildertude
{
    internal class Program
    {
        public static SceneData scene = new SceneData("main scene");

        static void Main(string[] args)
        {
            GameEngine.SetCurrentScene(scene);
            Game mainScript = new Game();
            GameEngine.Start(mainScript);
        }
    }
}
