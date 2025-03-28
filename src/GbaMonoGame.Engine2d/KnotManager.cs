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
        GameObjects = new List<GameObject>(sceneResource.GameObjectCount);
        AlwaysActors = new BaseActor[sceneResource.AlwaysActorsCount];
        Actors = new BaseActor[sceneResource.ActorsCount];
        Captors = new Captor[sceneResource.CaptorsCount];
        PendingAddedProjectiles = new List<BaseActor>();
        AddedProjectiles = new List<BaseActor>();
        KnotsWidth = sceneResource.KnotsWidth;
        Knots = sceneResource.Knots;

        if (sceneResource.GameObjectCount != sceneResource.AlwaysActorsCount + sceneResource.ActorsCount + sceneResource.CaptorsCount)
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

    public List<GameObject> GameObjects { get; set; }
    public BaseActor[] AlwaysActors { get; }
    public BaseActor[] Actors { get; }
    public Captor[] Captors { get; }

    // Custom list of always actors - removes the projectile limit
    public List<BaseActor> PendingAddedProjectiles { get; }
    public List<BaseActor> AddedProjectiles { get; }

    public Knot[] Knots { get; }
    public byte KnotsWidth { get; }

    public Knot CurrentKnot { get; set; }
    public Knot PreviousKnot { get; set; }

    #endregion

    #region Public Methods

    // The game does this in the constructor, but we need the object instance to be created before doing this in case an
    // object has to access the main actor of the scene
    public void LoadGameObjects(Scene2D scene)
    {
        Scene2DResource sceneResource = scene.Resource;
        int instanceId = 0;

        // Create always actors
        for (int i = 0; i < sceneResource.AlwaysActors.Length; i++)
        {
            AlwaysActors[i] = ObjectFactory.Create(instanceId, scene, sceneResource.AlwaysActors[i]);
            GameObjects.Add(AlwaysActors[i]);
            instanceId++;
        }

        // Create actors
        for (int i = 0; i < sceneResource.Actors.Length; i++)
        {
            Actors[i] = ObjectFactory.Create(instanceId, scene, sceneResource.Actors[i]);
            GameObjects.Add(Actors[i]);
            instanceId++;
        }

        // Create captors
        for (int i = 0; i < sceneResource.Captors.Length; i++)
        {
            Captors[i] = new Captor(instanceId, scene, sceneResource.Captors[i]);
            GameObjects.Add(Captors[i]);
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
        return AlwaysActors.Concat(AddedProjectiles).Where(x => x.IsEnabled == isEnabled);
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

    public void AddPendingProjectiles()
    {
        GameObjects.AddRange(PendingAddedProjectiles);
        AddedProjectiles.AddRange(PendingAddedProjectiles);
        PendingAddedProjectiles.Clear();
    }

    public BaseActor CreateProjectile(Scene2D scene, int actorType)
    {
        BaseActor actor = EnumerateAllActors(isEnabled: false).FirstOrDefault(x => x.Type == actorType && x.IsProjectile);

        if (actor != null)
        {
            actor.ProcessMessage(null, Message.ResurrectWakeUp);
            return actor;
        }
        else if (Engine.Config.AddProjectilesWhenNeeded)
        {
            // Custom code to remove the limit of only spawning already allocated projectiles. This is needed if the game runs
            // at a higher resolution as it might need more projectiles to be active at the same time due to more actors being
            // on screen at the same time. Otherwise an enemy might fire a shot which doesn't spawn.

            Scene2DResource sceneResource = scene.Resource;
            ActorResource actorResource = sceneResource.AlwaysActors.FirstOrDefault(x => x.Type == actorType && x.IsProjectile);

            if (actorResource == null)
                return null;

            int instanceId = GameObjects.Count + PendingAddedProjectiles.Count;
            actor = ObjectFactory.Create(instanceId, scene, actorResource);

            PendingAddedProjectiles.Add(actor);
            actor.Init(actorResource);

            actor.ProcessMessage(null, Message.ResurrectWakeUp);

            Logger.Info("Added a new projectile with instance id {0} and type {1}", actor.InstanceId, actor.Type);

            return actor;
        }
        else
        {
            return null;
        }
    }

    #endregion
}