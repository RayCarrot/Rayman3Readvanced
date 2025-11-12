using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Engine2d;

// TODO: Do not call Load on dialogs multiple times
public class Scene2D
{
    public Scene2D(int id, Func<Scene2D, CameraActor> createCameraFunc, int layersCount, int actorDrawPriority)
    {
        LayersCount = layersCount;
        ActorDrawPriority = actorDrawPriority;
        Camera = createCameraFunc(this);

        AllowModalDialogs = true;
        AnimationPlayer = new AnimationPlayer(false, SoundEventsManager.ProcessEvent);
        Dialogs = new List<Dialog>(layersCount);
        DialogModalFlags = new List<bool>(layersCount);

        Resource = Rom.LoadResource<Scene2DResource>(id);

        Playfield = TgxPlayfield.Load(Resource.Playfield);
        
        HudRenderContext = new HudRenderContext(RenderContext);

        KnotManager = new KnotManager(Resource);
        KnotManager.LoadGameObjects(this);

        Camera.LinkedObject = MainActor;

        // The game does this ugly hack here to disable background scrolling in Cave of Bad Dreams 1. This was added late during
        // development and the reason is unknown. We'll ignore this if set to fix bugs since this seems unnecessary.
        if (id == 11 && !Engine.ActiveConfig.Tweaks.FixBugs)
            ((TgxPlayfield2D)Playfield).Camera.GetCluster(1).ScrollFactor = Vector2.Zero;

        Camera.SetFirstPosition();
    }

    // Scene2DGameCube
    public Scene2D(GameCubeMap map, Func<Scene2D, CameraActor> createCameraFunc, int layersCount, int actorDrawPriority)
    {
        LayersCount = layersCount;
        ActorDrawPriority = actorDrawPriority;
        Camera = createCameraFunc(this);

        AllowModalDialogs = true;
        AnimationPlayer = new AnimationPlayer(false, SoundEventsManager.ProcessEvent);
        Dialogs = new List<Dialog>(layersCount);
        DialogModalFlags = new List<bool>(layersCount);

        Playfield = TgxPlayfield.Load<TgxPlayfield2D>(map.Playfield);
        HudRenderContext = new HudRenderContext(RenderContext);

        Resource = map.Scene;
        KnotManager = new KnotManager(Resource);
        KnotManager.LoadGameObjects(this);

        Camera.LinkedObject = MainActor;
        Camera.SetFirstPosition();
    }

    public Scene2DResource Resource { get; }
    public CameraActor Camera { get; }
    public RenderContext HudRenderContext { get; }
    public List<Dialog> Dialogs { get; }
    public List<bool> DialogModalFlags { get; }
    public AnimationPlayer AnimationPlayer { get; }
    public TgxPlayfield Playfield { get; }
    public int LayersCount { get; }
    public int ActorDrawPriority { get; }
    public KnotManager KnotManager { get; }
    public int FirstActiveDialogIndex { get; set; }

    public bool ShowDebugBoxes { get; set; }
    
    // Flags
    public bool InDialogModalMode { get; set; }
    public bool AllowModalDialogs { get; set; }
    public bool PendingDialogRefresh { get; set; }
    public bool InitializeNewModalDialog { get; set; }
    public bool ReloadPlayfield { get; set; } // Unused in Rayman 3
    public bool IsMultiplayerPaused { get; set; } // N-Gage only

    public RenderContext RenderContext => Playfield.RenderContext;
    public Vector2 Resolution => RenderContext.Resolution;

    public MovableActor MainActor => (MovableActor)(RSMultiplayer.IsActive ? GetGameObject(RSMultiplayer.MachineId) : GetGameObject(0));

    // If we're playing in a different resolution than the original we can't use
    // the knots (object sectors). Instead we keep all objects active at all times.
    public bool KeepAllObjectsActive => Resolution != Rom.OriginalResolution || Playfield is TgxPlayfieldMode7;

    public void Init()
    {
        ResurrectActors();
        CameraStep();
        ProcessDialogs();
        DrawActors();
    }

    public void UnInit()
    {
        Playfield.UnInit();
    }

    public void Step()
    {
        RefreshDialogs();

        if (Rom.Platform == Platform.GBA)
        {
            if (InDialogModalMode)
            {
                ProcessDialogs();
            }
            else
            {
                ActorBehaviorStep();
                ResurrectActors();
                ActorStep();
                ActorMoveStep();
                CaptorStep();
                CameraStep();
                ProcessDialogs();
                DrawActors();
            }
        }
        else if (Rom.Platform == Platform.NGage)
        {
            if (InDialogModalMode || IsMultiplayerPaused)
            {
                ProcessDialogs();
            }
            else
            {
                ActorBehaviorStep();
                ResurrectActors();
                ActorStep();
                ActorMoveStep();
                CaptorStep();
                CameraStep();
                DrawActors();
                ProcessDialogs();
            }
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        // Custom - add new created always actors at the end of this frame to avoid issues with enumerations
        KnotManager.AddPendingActors();

        // Toggle showing debug boxes
        if (Engine.ActiveConfig.Debug.DebugModeEnabled && InputManager.IsInputJustPressed(Input.Debug_ToggleDisplayBoxes))
            ShowDebugBoxes = !ShowDebugBoxes;

        // Draw debug boxes
        if (!InDialogModalMode && ShowDebugBoxes)
            DrawDebugBoxes();
    }

    public bool AddDialog(Dialog dialog, bool isModal, bool reloadPlayfield)
    {
        // Can't add new dialogs if a refresh is pending
        if (PendingDialogRefresh)
            return false;

        // Modal (for example the pause dialog)
        if (isModal)
        {
            if (!AllowModalDialogs)
                return false;

            DialogModalFlags.Add(true);
            Dialogs.Add(dialog);
            FirstActiveDialogIndex = Dialogs.Count - 1;

            InDialogModalMode = true;
            PendingDialogRefresh = true;
            InitializeNewModalDialog = true;

            if (reloadPlayfield)
                ReloadPlayfield = true;
        }
        // Normal
        else
        {
            DialogModalFlags.Add(false);
            Dialogs.Add(dialog);

            dialog.Load();
            dialog.Init();
        }

        return true;
    }

    public T GetDialog<T>()
        where T : Dialog
    {
        foreach (Dialog dialog in Dialogs)
        {
            if (dialog is T dlg)
                return dlg;
        }

        return null;
    }

    public T GetRequiredDialog<T>()
        where T : Dialog
    {
        foreach (Dialog dialog in Dialogs)
        {
            if (dialog is T dlg)
                return dlg;
        }

        throw new Exception($"Dialog of type {typeof(T)} has not been added to the scene");
    }

    public void ProcessDialogs()
    {
        // Can't process if a refresh is pending
        if (PendingDialogRefresh) 
            return;
        
        for (int i = FirstActiveDialogIndex; i < Dialogs.Count; i++)
        {
            Dialogs[i].Step();
            Dialogs[i].Draw(AnimationPlayer);
        }
    }

    public void RemoveLastDialog()
    {
        // If the last dialog is a modal...
        if (DialogModalFlags.Last())
        {
            // Remove the modal dialog
            Dialogs.RemoveAt(Dialogs.Count - 1);
            DialogModalFlags.RemoveAt(DialogModalFlags.Count - 1);

            // Check if there is another modal dialog and if so have that one be active
            bool endModalMode = true;
            for (int i = Dialogs.Count - 1; i >= 0; i--)
            {
                // Found a modal dialog
                if (DialogModalFlags[i])
                {
                    InDialogModalMode = true;
                    FirstActiveDialogIndex = i;
                    endModalMode = false;
                    break;
                }
            }

            // End modal mode if there were no other modal dialogs found
            if (endModalMode)
            {
                FirstActiveDialogIndex = 0;
                InDialogModalMode = false;
            }

            InitializeNewModalDialog = false;
            PendingDialogRefresh = true;
        }
        // If the last dialog is a normal one we just remove it
        else
        {
            Dialogs.RemoveAt(Dialogs.Count - 1);
            DialogModalFlags.RemoveAt(DialogModalFlags.Count - 1);
        }
    }

    public void RefreshDialogs()
    {
        if (!PendingDialogRefresh) 
            return;

        // Game resets the AnimationPaletteManager and AnimationSpriteManager here

        // Unused in Rayman 3
        if (ReloadPlayfield)
        {
            // The game unloads the playfield here
            throw new InvalidOperationException("Reloading the playfield is currently not supported");
        }

        // If we're exiting modal mode we want to reload animation data
        if (!InDialogModalMode)
        {
            // Unused in Rayman 3
            if (ReloadPlayfield)
            {
                // The game reloads the playfield here
                throw new InvalidOperationException("Reloading the playfield is currently not supported");
            }

            KnotManager.ReloadAnimations();

            for (int i = FirstActiveDialogIndex; i < Dialogs.Count; i++)
                Dialogs[i].Load();
        }
        // A new model dialog has been added which we want to load and initialize
        else if (InitializeNewModalDialog)
        {
            Dialogs[FirstActiveDialogIndex].Load();
            Dialogs[FirstActiveDialogIndex].Init();
        }
        // No new modal dialog has been added, so just reload the animation data for what's there from before
        else
        {
            for (int i = FirstActiveDialogIndex; i < Dialogs.Count; i++)
                Dialogs[i].Load();
        }

        PendingDialogRefresh = false;
    }

    public void ActorBehaviorStep()
    {
        foreach (BaseActor actor in new EnabledAlwaysActorIterator(this))
        {
            actor.DoBehavior();
        }

        foreach (BaseActor actor in new EnabledActorIterator(this))
        {
            actor.DoBehavior();
        }
    }

    public void ResurrectActors()
    {
        Vector2 camPos = Playfield.Camera.Position;
        bool newKnot = KnotManager.UpdateCurrentKnot(Playfield, camPos, KeepAllObjectsActive);

        // Resurrect always actors if immediate
        foreach (BaseActor obj in new DisabledAlwaysActorIterator(this))
        {
            if (obj.ResurrectsImmediately)
                obj.ProcessMessage(null, Message.Resurrect);
        }

        // Resurrect actors and captors if immediate
        foreach (GameObject obj in new DisabledActorCaptorIterator(this))
        {
            if (obj.ResurrectsImmediately)
                obj.ProcessMessage(null, Message.Resurrect);
        }

        // Resurrect actors and captors if later
        if (!KeepAllObjectsActive)
        {
            if (newKnot && KnotManager.PreviousKnot != null)
            {
                foreach (GameObject obj in new DisabledActorCaptorIterator(this, knot: KnotManager.PreviousKnot))
                {
                    if (obj.ResurrectsLater && !KnotManager.IsInCurrentKnot(this, obj.InstanceId))
                        obj.ProcessMessage(null, Message.Resurrect);
                }
            }
        }
        // If we keep all objects active then we can't use the knots to determine if the
        // object should be resurrected. Instead we check if the object is off-screen.
        else if (Playfield is TgxPlayfield2D)
        {
            const float margin = 64;
            Box viewBox = new(Playfield.Camera.Position - new Vector2(margin), Resolution + new Vector2(margin * 2));

            foreach (GameObject obj in new DisabledActorCaptorIterator(this))
            {
                if (obj.ResurrectsLater && !viewBox.Contains(obj.Position))
                    obj.ProcessMessage(null, Message.Resurrect);
            }
        }
    }

    public void ActorStep()
    {
        foreach (BaseActor actor in new EnabledAlwaysActorIterator(this))
        {
            actor.Step();
        }

        foreach (BaseActor actor in new EnabledActorIterator(this))
        {
            actor.Step();
        }
    }

    public void ActorMoveStep()
    {
        foreach (BaseActor actor in new EnabledAlwaysActorIterator(this))
        {
            if (actor is MovableActor movableActor)
                movableActor.Move();
        }

        foreach (BaseActor actor in new EnabledActorIterator(this))
        {
            if (actor is MovableActor movableActor)
                movableActor.Move();
        }
    }

    public void CaptorStep()
    {
        foreach (Captor captor in new EnabledCaptorIterator(this))
        {
            if (captor.TriggerOnMainActorDetection)
            {
                Debug.Assert(MainActor.IsAgainstCaptor, "The main actor is not against captor");

                captor.IsDetected = captor.GetCaptorBox().Intersects(MainActor.GetDetectionBox());
            }
            else
            {
                if (!captor.IsDetected)
                {
                    foreach (BaseActor actor in new EnabledAlwaysActorIterator(this))
                    {
                        // Skip main actor if not in multiplayer
                        if (!RSMultiplayer.IsActive && actor.InstanceId == 0)
                            continue;

                        if (actor.IsAgainstCaptor && actor is ActionActor actionActor)
                        {
                            captor.IsDetected = captor.GetCaptorBox().Intersects(actionActor.GetDetectionBox());
                            break;
                        }
                    }

                    foreach (BaseActor actor in new EnabledActorIterator(this))
                    {
                        if (actor.IsAgainstCaptor && actor is ActionActor actionActor)
                        {
                            captor.IsDetected = captor.GetCaptorBox().Intersects(actionActor.GetDetectionBox());
                            break;
                        }
                    }
                }
            }

            if (captor.IsDetected)
                captor.TriggerEvent();
        }
    }

    public void DrawActors()
    {
        foreach (BaseActor actor in new EnabledAlwaysActorIterator(this))
        {
            actor.Draw(AnimationPlayer, false);
        }

        foreach (BaseActor actor in new EnabledActorIterator(this))
        {
            actor.Draw(AnimationPlayer, false);
        }
    }

    public void CameraStep()
    {
        Camera.Step();
    }

    public void DrawDebugBoxes()
    {
        foreach (BaseActor actor in new EnabledAlwaysActorIterator(this))
        {
            actor.DrawDebugBoxes(AnimationPlayer);
        }

        foreach (GameObject gameObject in new EnabledActorCaptorIterator(this))
        {
            gameObject.DrawDebugBoxes(AnimationPlayer);
        }
    }

    public bool IsDetectedMainActor(ActionActor actor)
    {
        Box mainActorDetectionBox = MainActor.GetDetectionBox();
        Box actionBox = actor.GetActionBox();

        return mainActorDetectionBox.Intersects(actionBox);
    }

    public bool IsDetectedMainActor(ActionActor actor, float addBottom, float addTop, float addLeft, float addRight)
    {
        Box mainActorDetectionBox = MainActor.GetDetectionBox();
        Box actionBox = actor.GetActionBox();

        actionBox.Left += addLeft;
        actionBox.Top += addTop;
        actionBox.Right += addRight;
        actionBox.Bottom += addBottom;

        return mainActorDetectionBox.Intersects(actionBox);
    }

    public bool IsHitMainActor(InteractableActor actor)
    {
        Debug.Assert(MainActor.ReceivesDamage, "The main actor cannot receive damage");

        Box mainActorVulnerabilityBox = MainActor.GetVulnerabilityBox();
        Box attackBox = actor.GetAttackBox();

        return mainActorVulnerabilityBox.Intersects(attackBox);
    }

    public InteractableActor IsHitActor(InteractableActor actor)
    {
        Box attackBox = actor.GetAttackBox();

        foreach (BaseActor actorToCheck in new EnabledAlwaysActorIterator(this))
        {
            // Ignore main actor if not in multiplayer
            if (!RSMultiplayer.IsActive && actorToCheck.InstanceId == 0)
                continue;

            // Check for collision
            if (actorToCheck != actor &&
                actorToCheck.ReceivesDamage &&
                actorToCheck is InteractableActor interactableActor &&
                interactableActor.GetVulnerabilityBox().Intersects(attackBox))
                return interactableActor;
        }

        foreach (BaseActor actorToCheck in new EnabledActorIterator(this))
        {
            // Check for collision
            if (actorToCheck != actor &&
                actorToCheck.ReceivesDamage &&
                actorToCheck is InteractableActor interactableActor &&
                interactableActor.GetVulnerabilityBox().Intersects(attackBox))
                return interactableActor;
        }

        return null;
    }

    public InteractableActor IsHitActorOfType(InteractableActor actor, int type)
    {
        Box attackBox = actor.GetAttackBox();

        foreach (BaseActor actorToCheck in new EnabledAlwaysActorIterator(this))
        {
            // Ignore main actor if not in multiplayer
            if (!RSMultiplayer.IsActive && actorToCheck.InstanceId == 0)
                continue;

            // Check for collision
            if (actorToCheck != actor &&
                actorToCheck.Type == type &&
                actorToCheck.ReceivesDamage &&
                actorToCheck is InteractableActor interactableActor &&
                interactableActor.GetVulnerabilityBox().Intersects(attackBox))
                return interactableActor;
        }

        foreach (BaseActor actorToCheck in new EnabledActorIterator(this))
        {
            // Check for collision
            if (actorToCheck != actor &&
                actorToCheck.Type == type &&
                actorToCheck.ReceivesDamage &&
                actorToCheck is InteractableActor interactableActor &&
                interactableActor.GetVulnerabilityBox().Intersects(attackBox))
                return interactableActor;
        }

        return null;
    }

    public GameObject GetGameObject(int instanceId) => KnotManager.GetGameObject(instanceId);
    public T GetGameObject<T>(int instanceId) where T : GameObject => (T)KnotManager.GetGameObject(instanceId);

    public T CreateProjectile<T>(Enum actorType, bool allowAddWhenNeeded = true)
        where T : BaseActor
    {
        return (T)KnotManager.CreateProjectile(this, (int)(object)actorType, allowAddWhenNeeded);
    }

    public BaseActor CreateProjectile(int actorType, bool allowAddWhenNeeded = true)
    {
        return KnotManager.CreateProjectile(this, actorType, allowAddWhenNeeded);
    }

    public PhysicalType GetPhysicalType(Vector2 position)
    {
        return new PhysicalType(Playfield.GetPhysicalValue((position / Tile.Size).ToPoint()));
    }
}