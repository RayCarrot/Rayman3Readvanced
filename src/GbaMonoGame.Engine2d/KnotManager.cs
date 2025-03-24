using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Engine2d;

// TODO: Duplicate all always actors with projectile flag
public class KnotManager
{
    #region Constructor

    public KnotManager(Scene2DResource sceneResource)
    {
        GameObjects = new GameObject[sceneResource.GameObjectCount];
        AlwaysActors = new BaseActor[sceneResource.AlwaysActorsCount];
        Actors = new BaseActor[sceneResource.ActorsCount];
        Captors = new Captor[sceneResource.CaptorsCount];
        KnotsWidth = sceneResource.KnotsWidth;
        Knots = sceneResource.Knots;

        if (GameObjects.Length != AlwaysActors.Length + Actors.Length + Captors.Length)
            throw new Exception("Invalid game objects count");

        // Create a special knot with every object which we use when loading all objects at once
        _fullKnot = new Knot
        {
            ActorsCount = (byte)Actors.Length,
            CaptorsCount = (byte)Captors.Length,
            ActorIds = Enumerable.Range(AlwaysActors.Length, Actors.Length).Select(x => (byte)x).ToArray(),
            CaptorIds = Enumerable.Range(AlwaysActors.Length + Actors.Length, Captors.Length).Select(x => (byte)x).ToArray(),
        };
    }

    #endregion

    #region Private Fields

    private readonly Knot _fullKnot;

    #endregion

    #region Public Properties

    public GameObject[] GameObjects { get; set; }
    public BaseActor[] AlwaysActors { get; }
    public BaseActor[] Actors { get; }
    public Captor[] Captors { get; }

    public Knot[] Knots { get; }
    public byte KnotsWidth { get; }

    public Knot CurrentKnot { get; set; }
    public Knot PreviousKnot { get; set; }

    #endregion

    #region Public Methods

    // The game does this in the constructor, but we need the object instance to be created before doing this in case an
    // object has to access the main actor of the scene
    public void LoadGameObjects(Scene2D scene, Scene2DResource sceneResource)
    {
        int instanceId = 0;

        // Create always actors
        for (int i = 0; i < sceneResource.AlwaysActors.Length; i++)
        {
            AlwaysActors[i] = ObjectFactory.Create(instanceId, scene, sceneResource.AlwaysActors[i]);
            GameObjects[instanceId] = AlwaysActors[i];
            instanceId++;
        }

        // Create actors
        for (int i = 0; i < sceneResource.Actors.Length; i++)
        {
            Actors[i] = ObjectFactory.Create(instanceId, scene, sceneResource.Actors[i]);
            GameObjects[instanceId] = Actors[i];
            instanceId++;
        }

        // Create captors
        for (int i = 0; i < sceneResource.Captors.Length; i++)
        {
            Captors[i] = new Captor(instanceId, scene, sceneResource.Captors[i]);
            GameObjects[instanceId] = Captors[i];
            instanceId++;
        }

        // Initialize always actors
        for (int i = 0; i < AlwaysActors.Length; i++)
            AlwaysActors[i].Init(sceneResource.AlwaysActors[i]);

        // Initialize actors
        for (int i = 0; i < Actors.Length; i++)
            Actors[i].Init(sceneResource.Actors[i]);
    }

    public IEnumerable<BaseActor> EnumerateAlwaysActors(bool isEnabled)
    {
        return AlwaysActors.Where(x => x.IsEnabled == isEnabled);
    }

    public IEnumerable<BaseActor> EnumerateActors(bool isEnabled, Knot knot = null)
    {
        knot ??= CurrentKnot;
        return knot.ActorIds.Select(x => GetGameObject(x)).Where(x => x.IsEnabled == isEnabled).Cast<BaseActor>();
    }

    public IEnumerable<BaseActor> EnumerateAllActors(bool isEnabled, Knot knot = null)
    {
        return EnumerateAlwaysActors(isEnabled).Concat(EnumerateActors(isEnabled, knot));
    }

    public IEnumerable<GameObject> EnumerateAllGameObjects(bool isEnabled, Knot knot = null)
    {
        return EnumerateAlwaysActors(isEnabled).
            Concat(EnumerateActors(isEnabled, knot)).
            Cast<GameObject>().
            Concat(EnumerateCaptors(isEnabled, knot));
    }

    public IEnumerable<Captor> EnumerateCaptors(bool isEnabled, Knot knot = null)
    {
        knot ??= CurrentKnot;
        return knot.CaptorIds.Select(x => GetGameObject(x)).Where(x => x.IsEnabled == isEnabled).Cast<Captor>();
    }

    public GameObject GetGameObject(int instanceId)
    {
        return GameObjects[instanceId];
    }

    public bool UpdateCurrentKnot(TgxPlayfield playfield, Vector2 camPos, bool keepObjectsActive)
    {
        Knot knot;

        if (keepObjectsActive)
        {
            knot = _fullKnot;
        }
        else
        {
            if (playfield is TgxPlayfieldMode7)
                camPos -= Rom.OriginalResolution / 2;

            TgxGameLayer physicalLayer = playfield.PhysicalLayer;
            Vector2 res = playfield.RenderContext.Resolution;

            if (physicalLayer.PixelWidth - res.X <= camPos.X)
                camPos.X = physicalLayer.PixelWidth - res.X - 1;

            if (physicalLayer.PixelHeight - res.Y <= camPos.Y)
                camPos.Y = physicalLayer.PixelHeight - res.Y - 1;

            int knotX = (int)(camPos.X / Rom.OriginalResolution.X);
            int knotY = (int)(camPos.Y / Rom.OriginalResolution.Y);
            knot = Knots[knotX + knotY * KnotsWidth];
        }

        if (knot == CurrentKnot)
            return false;

        PreviousKnot = CurrentKnot;
        CurrentKnot = knot;

        // NOTE: At this point the game loads tiles into VRAM for objects which are set to load dynamically. This is irrelevant here.

        return true;
    }

    public bool IsInCurrentKnot(GameObject gameObject)
    {
        if (gameObject is BaseActor)
            return CurrentKnot.ActorIds.All(x => x != gameObject.InstanceId);
        else if (gameObject is Captor)
            return CurrentKnot.CaptorIds.All(x => x != gameObject.InstanceId);
        else
            throw new Exception($"Unsupported game object type {gameObject}");
    }

    public bool IsInPreviousKnot(GameObject gameObject)
    {
        if (gameObject is BaseActor)
            return PreviousKnot.ActorIds.All(x => x != gameObject.InstanceId);
        else if (gameObject is Captor)
            return PreviousKnot.CaptorIds.All(x => x != gameObject.InstanceId);
        else
            throw new Exception($"Unsupported game object type {gameObject}");
    }

    public void ReloadAnimations()
    {
        // Don't need to do anything here. The original game re-allocates data in VRAM here, usually after game has been paused.
    }

    public BaseActor CreateProjectile(int actorType)
    {
        BaseActor actor = EnumerateAllActors(isEnabled: false).FirstOrDefault(x => x.Type == actorType && x.IsProjectile);
        actor?.ProcessMessage(null, Message.ResurrectWakeUp);
        return actor;
    }

    #endregion
}