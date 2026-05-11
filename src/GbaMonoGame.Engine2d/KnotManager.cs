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
        GameObjects = new List<GameObject>(sceneResource.GameObjectCount);
        GameObjectTypes = new List<GameObjectType>(sceneResource.GameObjectCount);

        PendingAddedGameObjects = [];
        PendingAddedGameObjectTypes = [];
        KnotsWidth = sceneResource.KnotsWidth;
        Knots = sceneResource.Knots;

        if (sceneResource.GameObjectCount != sceneResource.AlwaysActorsCount + sceneResource.ActorsCount + sceneResource.CaptorsCount)
            throw new Exception("Invalid game objects count");
    }

    #endregion

    #region Public Properties

    public List<GameObject> GameObjects { get; }
    public List<GameObjectType> GameObjectTypes { get; }

    public int GameObjectsCount { get; private set; }
    public int AlwaysActorsCount { get; private set; }
    public int ActorsCount { get; private set; }
    public int CaptorsCount { get; private set; }
    public int AddedGameObjectsCount { get; private set; }

    // Custom pending objects, allow us to add new ones (such as more projectiles)
    public List<GameObject> PendingAddedGameObjects { get; }
    public List<GameObjectType> PendingAddedGameObjectTypes { get; }

    public Knot[] Knots { get; }
    public byte KnotsWidth { get; }

    public Knot CurrentKnot { get; private set; }
    public Knot PreviousKnot { get; private set; }

    #endregion

    #region Public Methods

    // The game does this in the constructor, but we need the object instance to be created before doing this in case an
    // object has to access the main actor of the scene
    public void LoadGameObjects(Scene2D scene)
    {
        Scene2DResource sceneResource = scene.Resource;
        int instanceId = 0;

        // Get the counts
        GameObjectsCount = sceneResource.GameObjectCount;
        AlwaysActorsCount = sceneResource.AlwaysActorsCount;
        ActorsCount = sceneResource.ActorsCount;
        CaptorsCount = sceneResource.CaptorsCount;

        // Create always actors
        foreach (ActorResource alwaysActor in sceneResource.AlwaysActors)
        {
            GameObjects.Add(ActorFactory.Create(instanceId, scene, alwaysActor));
            GameObjectTypes.Add(GameObjectType.AlwaysActor);
            instanceId++;
        }

        // Create actors
        foreach (ActorResource actor in sceneResource.Actors)
        {
            GameObjects.Add(ActorFactory.Create(instanceId, scene, actor));
            GameObjectTypes.Add(GameObjectType.Actor);
            instanceId++;
        }

        // Create captors
        foreach (CaptorResource captor in sceneResource.Captors)
        {
            GameObjects.Add(new Captor(instanceId, scene, captor));
            GameObjectTypes.Add(GameObjectType.Captor);
            instanceId++;
        }

        // Initialize the actors
        for (int i = 0; i < GameObjects.Count; i++)
        {
            if (GameObjectTypes[i] == GameObjectType.AlwaysActor)
                ((BaseActor)GameObjects[i]).Init(sceneResource.AlwaysActors[i]);
            else if (GameObjectTypes[i] == GameObjectType.Actor)
                ((BaseActor)GameObjects[i]).Init(sceneResource.Actors[i - sceneResource.AlwaysActorsCount]);
        }
    }

    public GameObject GetGameObject(int instanceId)
    {
        return GameObjects[instanceId];
    }

    public GameObjectType GetGameObjectType(int instanceId)
    {
        return GameObjectTypes[instanceId];
    }

    public bool UpdateCurrentKnot(TgxPlayfield playfield, Vector2 camPos)
    {
        // We always have to use the original resolution for the knots since they're pre-calculated with that
        Vector2 res = Rom.OriginalResolution;

        // Mode7 is centered
        if (playfield is TgxPlayfieldMode7)
            camPos -= res / 2;

        TgxGameLayer physicalLayer = playfield.PhysicalLayer;

        if (physicalLayer.PixelWidth - res.X <= camPos.X)
            camPos.X = physicalLayer.PixelWidth - res.X - 1;

        if (physicalLayer.PixelHeight - res.Y <= camPos.Y)
            camPos.Y = physicalLayer.PixelHeight - res.Y - 1;

        int knotX = (int)(camPos.X / res.X);
        int knotY = (int)(camPos.Y / res.Y);

        // The original game doesn't do this, but since we support higher resolutions we make sure we don't go out of bounds
        knotX = Math.Clamp(knotX, 0, KnotsWidth);
        knotY = Math.Clamp(knotY, 0, Knots.Length / KnotsWidth);

        Knot knot = Knots[knotX + knotY * KnotsWidth];

        if (knot == CurrentKnot)
            return false;

        PreviousKnot = CurrentKnot;
        CurrentKnot = knot;

        // NOTE: At this point the game loads tiles into VRAM for objects which are set to load dynamically. This is irrelevant here.

        return true;
    }

    public bool IsInCurrentKnot(Scene2D scene, int instanceId)
    {
        foreach (GameObject gameObject in scene.Iterate<GameObject>(IteratorFlags.Actor | IteratorFlags.Captor, IteratorKnot.Current))
        {
            if (gameObject.InstanceId == instanceId)
                return true;
        }

        return false;
    }

    public bool IsInPreviousKnot(Scene2D scene, int instanceId)
    {
        foreach (GameObject gameObject in scene.Iterate<GameObject>(IteratorFlags.Actor | IteratorFlags.Captor, IteratorKnot.Previous))
        {
            if (gameObject.InstanceId == instanceId)
                return true;
        }

        return false;
    }

    public void ReloadAnimations()
    {
        // Don't need to do anything here. The original game re-allocates data in VRAM here, usually after game has been paused.
    }

    public void AddPendingActors()
    {
        if (PendingAddedGameObjects.Count == 0)
            return;

        GameObjects.AddRange(PendingAddedGameObjects);
        GameObjectsCount += PendingAddedGameObjects.Count;
        AddedGameObjectsCount += PendingAddedGameObjects.Count;
        PendingAddedGameObjects.Clear();
        
        GameObjectTypes.AddRange(PendingAddedGameObjectTypes);
        foreach (GameObjectType type in PendingAddedGameObjectTypes)
        {
            switch (type)
            {
                case GameObjectType.AlwaysActor:
                    AlwaysActorsCount++;
                    break;
                
                case GameObjectType.Actor:
                    ActorsCount++;
                    break;
                
                case GameObjectType.Captor:
                    CaptorsCount++;
                    break;
            }
        }
        PendingAddedGameObjectTypes.Clear();
    }

    public BaseActor AddActor(Scene2D scene, ActorResource actorResource, GameObjectType type)
    {
        // Get the next instance id
        int instanceId = GameObjectsCount + PendingAddedGameObjects.Count;
        
        // Create the actor
        BaseActor actor = ActorFactory.Create(instanceId, scene, actorResource);

        // Add to pending list of actors
        PendingAddedGameObjects.Add(actor);
        PendingAddedGameObjectTypes.Add(type);

        // Initialize
        actor.Init(actorResource);

        return actor;
    }

    public BaseActor CreateProjectile(Scene2D scene, int actorType, bool allowAddWhenNeeded)
    {
        foreach (BaseActor actor in scene.Iterate<BaseActor>(IteratorFlags.Actor | IteratorFlags.Disabled))
        {
            if (actor.IsProjectile && actor.Type == actorType)
            {
                actor.ProcessMessage(null, Message.ResurrectWakeUp);
                return actor;
            }
        }

        foreach (BaseActor actor in scene.Iterate<BaseActor>(IteratorFlags.AlwaysActor | IteratorFlags.Disabled))
        {
            if (actor.IsProjectile && actor.Type == actorType)
            {
                actor.ProcessMessage(null, Message.ResurrectWakeUp);
                return actor;
            }
        }

        if (allowAddWhenNeeded && Engine.ActiveConfig.Tweaks.AddProjectilesWhenNeeded)
        {
            // Custom code to remove the limit of only spawning already allocated projectiles. This is needed if the game runs
            // at a higher resolution as it might need more projectiles to be active at the same time due to more actors being
            // on screen at the same time. Otherwise an enemy might fire a shot which doesn't spawn.

            Scene2DResource sceneResource = scene.Resource;
            ActorResource actorResource = sceneResource.AlwaysActors.FirstOrDefault(x => x.Type == actorType && x.IsProjectile);

            if (actorResource == null)
                return null;

            BaseActor projectile = AddActor(scene, actorResource, GameObjectType.AlwaysActor);

            projectile.ProcessMessage(null, Message.ResurrectWakeUp);

            Logger.Info("Added a new projectile with instance id {0} and type {1}", projectile.InstanceId, projectile.Type);

            return projectile;
        }
        else
        {
            return null;
        }
    }

    #endregion
}