using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class ReadvancedResources
{
    public static ActorModel TimeFreezeItemActorModel { get; } = CreateTimeFreezeItemActorModel();

    private static ActorModel CreateTimeFreezeItemActorModel()
    {
        const int AnimWidth = 32;
        const int AnimHeight = 32;
        const int AnimOffsetX = -(AnimWidth / 2);
        const int AnimOffsetY = -(AnimHeight / 2);

        ChannelBox vulnerabilityBox = new(AnimOffsetX + AnimWidth, AnimOffsetX, AnimOffsetY, AnimOffsetY + AnimHeight);

        // Create the idle animation
        AnimationBuilder idleAnimBuilder = new() { Speed = 4 };
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 0);
        idleAnimBuilder.AddVulnerabilityBox(vulnerabilityBox);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 0); // Play first frame twice to add a slight delay between wing flaps
        idleAnimBuilder.AddVulnerabilityBox(vulnerabilityBox);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 1);
        idleAnimBuilder.AddVulnerabilityBox(vulnerabilityBox);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 2);
        idleAnimBuilder.AddVulnerabilityBox(vulnerabilityBox);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 3);
        idleAnimBuilder.AddVulnerabilityBox(vulnerabilityBox);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 4);
        idleAnimBuilder.AddVulnerabilityBox(vulnerabilityBox);
        Animation idleAnim = idleAnimBuilder.Build();

        // Create the death animation (same as idle, but without delay, faster speed and no vulnerability boxes)
        AnimationBuilder deathAnimBuilder = new() { Speed = 2 };
        deathAnimBuilder.NewFrame();
        deathAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 0);
        deathAnimBuilder.NewFrame();
        deathAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 1);
        deathAnimBuilder.NewFrame();
        deathAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 2);
        deathAnimBuilder.NewFrame();
        deathAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 3);
        deathAnimBuilder.NewFrame();
        deathAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY, 4);
        Animation deathAnim = deathAnimBuilder.Build();

        ActorModel model = new()
        {
            ViewBox = new EngineBox(AnimOffsetX, AnimOffsetY, AnimOffsetX + AnimWidth, AnimOffsetY + AnimHeight),
            DetectionBox = new EngineBox(0, 0, 0, 0),
            CheckAgainstMapCollision = false,
            CheckAgainstObjectCollision = false,
            IsSolid = false,
            IsAgainstCaptor = false,
            ReceivesDamage = true,
            HitPoints = 1,
            AttackPoints = 0,
            Actions =
            [
                // Init_Blue
                new Action
                {
                    Box = new EngineBox(0, 0, 0, 0),
                    AnimationIndex = 0,
                    Flags = ActionFlags.None,
                    MechModelType = 1,
                    MechModel = new MechModel() { Params = [] }
                },
                // Init_Orange
                new Action
                {
                    Box = new EngineBox(0, 0, 0, 0),
                    AnimationIndex = 0,
                    Flags = ActionFlags.None,
                    MechModelType = 1,
                    MechModel = new MechModel() { Params = [] }
                },
                // Idle
                new Action
                {
                    Box = new EngineBox(0, 0, 0, 0),
                    AnimationIndex = 0,
                    Flags = ActionFlags.None,
                    MechModelType = 1,
                    MechModel = new MechModel() { Params = [] }
                },
                // Dying
                new Action
                {
                    Box = new EngineBox(0, 0, 0, 0),
                    AnimationIndex = 1,
                    Flags = ActionFlags.None,
                    MechModelType = 1,
                    MechModel = new MechModel() { Params = [] }
                },
            ],
            AnimatedObject = new AnimatedObjectResource
            {
                PalettesCount = 0,
                IsDynamic = false,
                AnimationsCount = 1,
                Palettes = new SpritePalettesResource
                {
                    Palettes = []
                },
                SpriteTable = null,
                Animations =
                [
                    idleAnim,
                    deathAnim,
                ]
            }
        };

        return model;
    }
}