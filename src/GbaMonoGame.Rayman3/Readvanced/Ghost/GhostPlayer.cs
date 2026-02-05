using System.Collections.Generic;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public class GhostPlayer
{
    public GhostPlayer(Scene2D scene, GhostMapData ghostMapData)
    {
        Scene = scene;
        _frames = ghostMapData.Frames;
        _isMode7 = ghostMapData.IsMode7;
    }

    private const float GhostAlpha = 0.6f;

    private readonly Dictionary<int, BaseActor> _ghostActors = [];
    private readonly GhostFrame[] _frames;
    private int _frameIndex;
    private readonly bool _isMode7;

    public Scene2D Scene { get; }

    private BaseActor CreateGhostActor(BaseActor baseActor)
    {
        BaseActor ghost = Scene.KnotManager.AddAlwaysActor(Scene, new ActorResource
        {
            Pos = default,
            IsEnabled = true,
            IsAwake = true,
            IsAnimatedObjectDynamic = false,
            IsProjectile = false,
            ResurrectsImmediately = false,
            ResurrectsLater = false,
            Type = (byte)(_isMode7 ? ReadvancedActorType.GhostMode7 : ReadvancedActorType.Ghost),
            Idx_ActorModel = 0xFF,
            Links = [0xFF, 0xFF, 0xFF, 0xFF],
            Model = baseActor.ActorModel,
        });
        ghost.AnimatedObject.IsSoundEnabled = false;

        if (ghost is GhostMode7 ghostMode7)
        {
            ghostMode7.Alpha = GhostAlpha;
        }
        else
        {
            ghost.AnimatedObject.BlendMode = BlendMode.AlphaBlend;
            ghost.AnimatedObject.Alpha = GhostAlpha;
        }

        return ghost;
    }

    public void Step()
    {
        if (_frameIndex < _frames.Length)
        {
            GhostFrame frame = _frames[_frameIndex];

            foreach (BaseActor ghostActor in _ghostActors.Values)
                ghostActor.ProcessMessage(this, Message.Destroy);

            foreach (GhostActorFrame ghostActorFrame in frame.Actors)
            {
                if (!_ghostActors.TryGetValue(ghostActorFrame.InstanceId, out BaseActor ghostActor))
                {
                    ghostActor = CreateGhostActor(Scene.GetGameObject<BaseActor>(ghostActorFrame.InstanceId));
                    _ghostActors.Add(ghostActorFrame.InstanceId, ghostActor);
                }

                ghostActor.ProcessMessage(this, Message.ResurrectWakeUp);

                if (ghostActor is Ghost ghost)
                    ghost.ApplyFrame(ghostActorFrame);
                else if (ghostActor is GhostMode7 ghostMode7)
                    ghostMode7.ApplyFrame(ghostActorFrame);
            }

            _frameIndex++;
        }
        else if (_frameIndex == _frames.Length)
        {
            foreach (BaseActor ghostActor in _ghostActors.Values)
                ghostActor.ProcessMessage(this, Message.Destroy);

            _frameIndex++;
        }
    }
}