using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Engine2d;

public class KnotManager
{
    #region Constructor

    public KnotManager(Scene2DResource sceneResource)
    {
        GameObjectsCount = sceneResource.GameObjectCount;
        AlwaysActorsCount = sceneResource.AlwaysActorsCount;
        ActorsCount = sceneResource.ActorsCount;
        CaptorsCount = sceneResource.CaptorsCount;
        KnotsWidth = sceneResource.KnotsWidth;
        GameObjects = new GameObject[GameObjectsCount];
        Knots = sceneResource.Knots;

        // Create a special knot with every object which we use when loading all objects at once
        _fullKnot = new Knot
        {
            ActorsCount = (byte)ActorsCount,
            CaptorsCount = (byte)CaptorsCount,
            ActorIds = Enumerable.Range(AlwaysActorsCount, ActorsCount).Select(x => (byte)x).ToArray(),
            CaptorIds = Enumerable.Range(AlwaysActorsCount + ActorsCount, CaptorsCount).Select(x => (byte)x).ToArray(),
        };
    }

    #endregion

    #region Private Fields

    private readonly Knot _fullKnot;

    #endregion

    #region Public Properties

    // Total
    public int GameObjectsCount { get; }

    // Object types
    public int AlwaysActorsCount { get; }
    public int ActorsCount { get; }
    public int CaptorsCount { get; }

    public GameObject[] GameObjects { get; }
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
        // Create always actors
        for (int i = 0; i < sceneResource.AlwaysActors.Length; i++)
            GameObjects[i] = ObjectFactory.Create(i, scene, sceneResource.AlwaysActors[i]);

        // Create actors
        for (int i = 0; i < sceneResource.Actors.Length; i++)
            GameObjects[AlwaysActorsCount + i] = ObjectFactory.Create(AlwaysActorsCount + i, scene, sceneResource.Actors[i]);

        // Create captors
        for (int i = 0; i < sceneResource.Captors.Length; i++)
            GameObjects[AlwaysActorsCount + ActorsCount + i] = new Captor(AlwaysActorsCount + ActorsCount + i, scene, sceneResource.Captors[i]);

        // Initialize always actors
        for (int i = 0; i < AlwaysActorsCount; i++)
            ((BaseActor)GameObjects[i]).Init(sceneResource.AlwaysActors[i]);

        // Initialize actors
        for (int i = 0; i < ActorsCount; i++)
            ((BaseActor)GameObjects[AlwaysActorsCount + i]).Init(sceneResource.Actors[i]);
    }

    public IEnumerable<BaseActor> EnumerateAlwaysActors(bool isEnabled)
    {
        return GameObjects.Take(AlwaysActorsCount).Where(x => x.IsEnabled == isEnabled).Cast<BaseActor>();
    }

    public IEnumerable<BaseActor> EnumerateActors(bool isEnabled, Knot knot = null)
    {
        knot ??= CurrentKnot;
        return knot.ActorIds.Select(x => GameObjects[x]).Where(x => x.IsEnabled == isEnabled).Cast<BaseActor>();
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
        return knot.CaptorIds.Select(x => GameObjects[x]).Where(x => x.IsEnabled == isEnabled).Cast<Captor>();
    }

    public GameObject GetGameObject(int instanceId) => GameObjects[instanceId];

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