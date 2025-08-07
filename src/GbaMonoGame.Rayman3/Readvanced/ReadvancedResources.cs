using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class ReadvancedResources
{
    public static ActorModel TimeFreezeItemActorModel { get; } = CreateTimeFreezeItemActorModel();
    public static ActorModel TimeDecreaseActorModel { get; } = CreateTimeDecreaseActorModel();

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
            ViewBox = new EngineBox(AnimOffsetY, AnimOffsetX, AnimOffsetY + AnimHeight, AnimOffsetX + AnimWidth),
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
                // Init_Decrease3
                new Action
                {
                    Box = new EngineBox(0, 0, 0, 0),
                    AnimationIndex = 0,
                    Flags = ActionFlags.None,
                    MechModelType = 1,
                    MechModel = new MechModel() { Params = [] }
                },
                // Init_Decrease5
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

    private static ActorModel CreateTimeDecreaseActorModel()
    {
        const int AnimWidth = 24;
        const int AnimHeight = 16;
        const int AnimOffsetX = -(AnimWidth / 2);
        const int AnimOffsetY = -(AnimHeight / 2);

        // Create the idle animation
        AnimationBuilder idleAnimBuilder = new() { Speed = 1 };
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 0, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 1, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 2, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 3, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 4, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 5, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 6, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 8, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 10, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 12, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 14, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 16, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 19, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 22, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 25, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 28, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 31, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 35, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 40, 0);
        idleAnimBuilder.NewFrame();
        idleAnimBuilder.AddSprite(AnimOffsetX, AnimOffsetY - 45, 0);
        Animation idleAnim = idleAnimBuilder.Build();

        ActorModel model = new()
        {
            ViewBox = new EngineBox(AnimOffsetY - 45, AnimOffsetX, AnimOffsetY + AnimHeight, AnimOffsetX + AnimWidth),
            DetectionBox = new EngineBox(0, 0, 0, 0),
            CheckAgainstMapCollision = false,
            CheckAgainstObjectCollision = false,
            IsSolid = false,
            IsAgainstCaptor = false,
            ReceivesDamage = true,
            HitPoints = 1,
            AttackPoints = 0,
            Actions = [],
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
                ]
            }
        };

        return model;
    }
}