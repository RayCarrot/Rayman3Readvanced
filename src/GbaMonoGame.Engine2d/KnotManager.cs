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
        AlwaysActors = new BaseActor[sceneResource.AlwaysActorsCount];
        Actors = new BaseActor[sceneResource.ActorsCount];
        Captors = new Captor[sceneResource.CaptorsCount];
        PendingAddedAlwaysActors = new List<BaseActor>();
        AddedAlwaysActors = new List<BaseActor>();
        KnotsWidth = sceneResource.KnotsWidth;
        Knots = sceneResource.Knots;

        if (sceneResource.GameObjectCount != sceneResource.AlwaysActorsCount + sceneResource.ActorsCount + sceneResource.CaptorsCount)
            throw new Exception("Invalid game objects count");

        // Create a special knot with every object which we use when loading all objects at once
        _fullKnot = new Knot
        {
            ActorsCount = (byte)ActorsCount,
            CaptorsCount = (byte)CaptorsCount,
            ActorIds = Enumerable.Range(ActorsIndex, ActorsCount).Select(x => (byte)x).ToArray(),
            CaptorIds = Enumerable.Range(CaptorsIndex, CaptorsCount).Select(x => (byte)x).ToArray(),
        };
    }

    #endregion

    #region Private Fields

    private readonly Knot _fullKnot;

    #endregion

    #region Public Properties

    public List<GameObject> GameObjects { get; set; }
    public int GameObjectsCount => GameObjects.Count;
    
    public BaseActor[] AlwaysActors { get; }
    public int AlwaysActorsCount => AlwaysActors.Length;
    public int AlwaysActorsIndex => 0;
    
    public BaseActor[] Actors { get; }
    public int ActorsCount => Actors.Length;
    public int ActorsIndex => AlwaysActorsIndex + AlwaysActorsCount;

    public Captor[] Captors { get; }
    public int CaptorsCount => Captors.Length;
    public int CaptorsIndex => ActorsIndex + ActorsCount;

    // Custom list of always actors - removes the projectile limit
    public List<BaseActor> PendingAddedAlwaysActors { get; }
    public List<BaseActor> AddedAlwaysActors { get; }
    public int AddedAlwaysActorsCount => AddedAlwaysActors.Count;
    public int AddedAlwaysActorsIndex => CaptorsIndex + CaptorsCount;

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
            AlwaysActors[i] = ActorFactory.Create(instanceId, scene, sceneResource.AlwaysActors[i]);
            GameObjects.Add(AlwaysActors[i]);
            instanceId++;
        }

        // Create actors
        for (int i = 0; i < sceneResource.Actors.Length; i++)
        {
            Actors[i] = ActorFactory.Create(instanceId, scene, sceneResource.Actors[i]);
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

    public bool IsInCurrentKnot(Scene2D scene, int instanceId)
    {
        foreach (GameObject gameObject in new ActorCaptorIterator(scene))
        {
            if (gameObject.InstanceId == instanceId)
                return true;
        }

        return false;
    }

    public bool IsInPreviousKnot(Scene2D scene, int instanceId)
    {
        foreach (GameObject gameObject in new ActorCaptorIterator(scene, PreviousKnot))
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
        GameObjects.AddRange(PendingAddedAlwaysActors);
        AddedAlwaysActors.AddRange(PendingAddedAlwaysActors);
        PendingAddedAlwaysActors.Clear();
    }

    public BaseActor CreateProjectile(Scene2D scene, int actorType, bool allowAddWhenNeeded)
    {
        foreach (BaseActor actor in new DisabledActorIterator(scene))
        {
            if (actor.IsProjectile && actor.Type == actorType)
            {
                actor.ProcessMessage(null, Message.ResurrectWakeUp);
                return actor;
            }
        }

        foreach (BaseActor actor in new DisabledAlwaysActorIterator(scene))
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

            int instanceId = GameObjectsCount + PendingAddedAlwaysActors.Count;
            BaseActor projectile = ActorFactory.Create(instanceId, scene, actorResource);

            PendingAddedAlwaysActors.Add(projectile);
            projectile.Init(actorResource);

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