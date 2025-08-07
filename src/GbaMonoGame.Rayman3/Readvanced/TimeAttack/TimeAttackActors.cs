using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class TimeAttackActors
{
    public static IReadOnlyCollection<ActorResource> GetTimeAttackActors(MapId mapId)
    {
        List<ActorResource> actors = new();

        // TODO: Fill out for each map
        // Get the time freeze items
        ActorResource[] timeFreezeItems = mapId switch
        {
            MapId.WoodLight_M1 => WoodLight_M1,
            _ => []
        };
        actors.AddRange(timeFreezeItems);

        // Add max 5 projectile actors
        int projectilesCount = Math.Min(timeFreezeItems.Length, 5);

        // TODO: Update model to have a bigger viewbox
        // Load Power1 scene to get the sparkles model from it
        Scene2DResource sceneResource = Rom.LoadResource<Scene2DResource>((int)MapId.Power1);
        ActorModel sparklesModel = sceneResource.AlwaysActors.First(x => (ActorType)x.Type == ActorType.ChainedSparkles).Model;

        for (int i = 0; i < projectilesCount; i++)
        {
            // Add sparkles
            actors.Add(new ActorResource
            {
                Pos = new BinarySerializer.Ubisoft.GbaEngine.Vector2(0, 0),
                IsEnabled = false,
                IsAwake = false,
                IsAnimatedObjectDynamic = false,
                IsProjectile = true,
                ResurrectsImmediately = false,
                ResurrectsLater = false,
                Type = (byte)ReadvancedActorType.TimeFreezeItemSparkles,
                Model = sparklesModel,
            });

            // Add time decrease
            actors.Add(new ActorResource
            {
                Pos = new BinarySerializer.Ubisoft.GbaEngine.Vector2(0, 0),
                IsEnabled = false,
                IsAwake = false,
                IsAnimatedObjectDynamic = false,
                IsProjectile = true,
                ResurrectsImmediately = false,
                ResurrectsLater = false,
                Type = (byte)ReadvancedActorType.TimeDecrease,
                Model = ReadvancedResources.TimeDecreaseActorModel,
            });
        }

        return actors;
    }

    private static ActorResource[] WoodLight_M1 =>
    [
        new()
        {
            Pos = new BinarySerializer.Ubisoft.GbaEngine.Vector2(300, 197),
            IsEnabled = true,
            IsAwake = true,
            IsAnimatedObjectDynamic = false,
            IsProjectile = false,
            ResurrectsImmediately = false,
            ResurrectsLater = false,
            Type = (byte)ReadvancedActorType.TimeFreezeItem,
            Idx_ActorModel = 0xFF,
            FirstActionId = (byte)TimeFreezeItem.Action.Init_Decrease3,
            Links = [0xFF, 0xFF, 0xFF, 0xFF],
            Model = ReadvancedResources.TimeFreezeItemActorModel,
        },
        new()
        {
            Pos = new BinarySerializer.Ubisoft.GbaEngine.Vector2(998, 158),
            IsEnabled = true,
            IsAwake = true,
            IsAnimatedObjectDynamic = false,
            IsProjectile = false,
            ResurrectsImmediately = false,
            ResurrectsLater = false,
            Type = (byte)ReadvancedActorType.TimeFreezeItem,
            Idx_ActorModel = 0xFF,
            FirstActionId = (byte)TimeFreezeItem.Action.Init_Decrease3,
            Links = [0xFF, 0xFF, 0xFF, 0xFF],
            Model = ReadvancedResources.TimeFreezeItemActorModel,
        },
        new()
        {
            Pos = new BinarySerializer.Ubisoft.GbaEngine.Vector2(1637, 252),
            IsEnabled = true,
            IsAwake = true,
            IsAnimatedObjectDynamic = false,
            IsProjectile = false,
            ResurrectsImmediately = false,
            ResurrectsLater = false,
            Type = (byte)ReadvancedActorType.TimeFreezeItem,
            Idx_ActorModel = 0xFF,
            FirstActionId = (byte)TimeFreezeItem.Action.Init_Decrease3,
            Links = [0xFF, 0xFF, 0xFF, 0xFF],
            Model = ReadvancedResources.TimeFreezeItemActorModel,
        },
        new()
        {
            Pos = new BinarySerializer.Ubisoft.GbaEngine.Vector2(3046, 44),
            IsEnabled = true,
            IsAwake = true,
            IsAnimatedObjectDynamic = false,
            IsProjectile = false,
            ResurrectsImmediately = false,
            ResurrectsLater = false,
            Type = (byte)ReadvancedActorType.TimeFreezeItem,
            Idx_ActorModel = 0xFF,
            FirstActionId = (byte)TimeFreezeItem.Action.Init_Decrease3,
            Links = [0xFF, 0xFF, 0xFF, 0xFF],
            Model = ReadvancedResources.TimeFreezeItemActorModel,
        },
    ];
}