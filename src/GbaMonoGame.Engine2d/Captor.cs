using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Engine2d;

public class Captor : GameObject
{
    public Captor(int instanceId, Scene2D scene, CaptorResource captorResource) : base(instanceId, scene, captorResource)
    {
        TriggeredCount = 0;

        TriggerOnMainActorDetection = captorResource.TriggerOnMainActorDetection;
        IsDetected = captorResource.IsDetected;
        
        Events = captorResource.Events.Events;
        OriginalEventsToTrigger = captorResource.EventsCount;
        EventsToTrigger = captorResource.EventsCount;

        Debug.Assert(EventsToTrigger > 0, "The captor has no event");

        _captorBox = new Box(captorResource.BoxMinX, captorResource.BoxMinY, captorResource.BoxMaxX, captorResource.BoxMaxY);

        _debugCaptorBoxAObject = new DebugBoxAObject()
        {
            Size = _captorBox.Size,
            Color = DebugBoxColor.CaptorBox,
            IsFilled = true,
            RenderContext = Scene.RenderContext
        };
    }

    private readonly Box _captorBox;
    private readonly DebugBoxAObject _debugCaptorBoxAObject;

    // Flags
    public bool TriggerOnMainActorDetection { get; set; }
    public bool IsDetected { get; set; }

    public CaptorEvent[] Events { get; }
    public int OriginalEventsToTrigger { get; set; }
    public int EventsToTrigger { get; set; }
    public int TriggeredCount { get; set; }

    public Box GetCaptorBox() => _captorBox;

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // Intercept messages
        switch (message)
        {
            case Message.Captor_Trigger:
                IsDetected = true;
                TriggerEvent();
                return true;
        }

        return base.ProcessMessageImpl(sender, message, param);
    }

    public void TriggerEvent()
    {
        Debug.Assert(IsDetected, "The captor has not been detected");
        Debug.Assert(EventsToTrigger > 0, "The captor has no event to trigger");

        foreach (CaptorEvent evt in Events)
        {
            Message msg = (Message)evt.MessageId;

            switch (msg)
            {
                case Message.Captor_Trigger_Sound:
                    SoundEventsManager.ProcessEvent((short)evt.Param);
                    Logger.Info("Triggering captor event with sound event {0}", evt.Param);
                    break;

                case Message.Captor_Trigger_None:
                    // Do nothing
                    break;
                
                case Message.Captor_Trigger_SendMessageWithParam:
                default:
                    Scene.GetGameObject(evt.Param & 0xFF).ProcessMessage(this, msg, evt.Param >> 8);
                    Logger.Info("Triggering captor event with message {0}, to object {1} with param {2}", msg, evt.Param & 0xFF, evt.Param >> 8);
                    break;

                case Message.Captor_Trigger_SendMessageWithCaptorParam:
                    Scene.GetGameObject(evt.Param & 0xFF).ProcessMessage(this, msg, this);
                    Logger.Info("Triggering captor event with message {0} to object {1}", msg, evt.Param & 0xFF);
                    break;
            }

            EventsToTrigger--;
        }

        TriggeredCount++;

        if (EventsToTrigger == 0)
        {
            ProcessMessage(this, Message.Destroy);
            TriggeredCount = 0;
            EventsToTrigger = OriginalEventsToTrigger;
            IsDetected = false;
        }
    }

    public override void DrawDebugBoxes(AnimationPlayer animationPlayer)
    {
        base.DrawDebugBoxes(animationPlayer);

        if (Scene.Camera.IsDebugBoxFramed(_debugCaptorBoxAObject, _captorBox.Position))
        {
            _debugCaptorBoxAObject.Size = _captorBox.Size;
            animationPlayer.PlayFront(_debugCaptorBoxAObject);
        }
    }
}