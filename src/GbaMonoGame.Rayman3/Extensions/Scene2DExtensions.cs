using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public static class Scene2DExtensions
{
    extension(Scene2D scene)
    {
        public T CreateProjectile<T>(ActorType actorType, bool allowAddWhenNeeded = true)
            where T : BaseActor
        {
            return (T)scene.CreateProjectile((int)actorType, allowAddWhenNeeded);
        }

        public T CreateProjectile<T>(ReadvancedActorType actorType, bool allowAddWhenNeeded = true)
            where T : BaseActor
        {
            return (T)scene.CreateProjectile((int)actorType, allowAddWhenNeeded);
        }
    }
}