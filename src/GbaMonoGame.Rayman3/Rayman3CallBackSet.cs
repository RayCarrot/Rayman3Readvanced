using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class Rayman3CallBackSet : CallBackSet
{
    public override Vector2 GetObjectPosition(object obj)
    {
        if (obj is not GameObject gameObject)
            return Vector2.Zero;

        if (Engine.Settings.Local.Sound.ForceSoundPanning)
        {
            // NOTE: The game doesn't do this, but it also never has audio panning or roll-off for Mode7 objects.
            if (obj is Mode7Actor mode7Actor)
            {
                Vector3 screenPos = ((TgxPlayfieldMode7)mode7Actor.Scene.Playfield).Camera.Project(new Vector3(mode7Actor.Position, 0));
                return new Vector2(screenPos.X, 0);
            }

            // NOTE: The game doesn't do this, but it also never has audio panning or roll-off for the main actor. We add this
            //       to disable this explicitly for the main actor since it'd be annoying hearing Rayman's sounds louder in one
            //       ear than the other (since he is never in the exact center of the screen, and often makes sounds).
            if (gameObject is BaseActor actor && actor.IsLinkedCameraObject())
                return GetMikePosition(obj);
        }

        return new Vector2(gameObject.Position.X, 0);
    }

    public override Vector2 GetMikePosition(object obj)
    {
        if (obj is not GameObject gameObject)
            return Vector2.Zero;

        if (Engine.Settings.Local.Sound.ForceSoundPanning)
        {
            // NOTE: The game doesn't do this, but it also never has audio panning or roll-off for Mode7 objects.
            if (obj is Mode7Actor mode7Actor)
                return new Vector2(mode7Actor.Scene.Resolution.X / 2, 0);
        }

        TgxCamera cam = gameObject.Scene.Playfield.Camera;
        return new Vector2(cam.Position.X + gameObject.Scene.Resolution.X / 2, 0);
    }

    public override int GetSwitchIndex()
    {
        return 0;
    }
}