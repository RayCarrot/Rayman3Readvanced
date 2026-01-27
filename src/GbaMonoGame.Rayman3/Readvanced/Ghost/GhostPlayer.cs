using System.Collections.Generic;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public class GhostPlayer
{
    public GhostPlayer(Scene2D scene, GhostFrame[] frames)
    {
        Scene = scene;
        _frames = frames;
    }

    private readonly Dictionary<int, Ghost> _ghostActors = [];
    private readonly GhostFrame[] _frames;
    private int _frameIndex;

    public Scene2D Scene { get; }

    private Ghost CreateGhostActor(BaseActor baseActor)
    {
        Ghost ghost = (Ghost)Scene.KnotManager.AddAlwaysActor(Scene, new ActorResource
        {
            Pos = default,
            IsEnabled = true,
            IsAwake = true,
            IsAnimatedObjectDynamic = false,
            IsProjectile = false,
            ResurrectsImmediately = false,
            ResurrectsLater = false,
            Type = (byte)ReadvancedActorType.Ghost,
            Idx_ActorModel = 0xFF,
            Links = [0xFF, 0xFF, 0xFF, 0xFF],
            Model = baseActor.ActorModel,
        });
        ghost.AnimatedObject.IsSoundEnabled = false;
        ghost.AnimatedObject.BlendMode = BlendMode.AlphaBlend;
        ghost.AnimatedObject.Alpha = 0.6f;
        return ghost;
    }

    public void Step()
    {
        if (_frameIndex < _frames.Length)
        {
            GhostFrame frame = _frames[_frameIndex];

            foreach (Ghost ghostActor in _ghostActors.Values)
                ghostActor.ProcessMessage(this, Message.Destroy);

            foreach (GhostActorFrame ghostActorFrame in frame.Actors)
            {
                if (!_ghostActors.TryGetValue(ghostActorFrame.InstanceId, out Ghost ghostActor))
                {
                    ghostActor = CreateGhostActor(Scene.GetGameObject<BaseActor>(ghostActorFrame.InstanceId));
                    _ghostActors.Add(ghostActorFrame.InstanceId, ghostActor);
                }

                ghostActor.ProcessMessage(this, Message.ResurrectWakeUp);
                ghostActor.ApplyFrame(ghostActorFrame);
            }

            _frameIndex++;
        }
        else if (_frameIndex == _frames.Length)
        {
            foreach (Ghost ghostActor in _ghostActors.Values)
                ghostActor.ProcessMessage(this, Message.Destroy);

            _frameIndex++;
        }
    }
}