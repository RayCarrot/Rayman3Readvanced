using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BinarySerializer.Ubisoft.GbaEngine;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GbaMonoGame.Rayman3;

public class GenerateDebugMenu : DebugMenu
{
    public override string Name => "Generate";

    private void WriteJson<T>(T obj, string filePath)
    {
        string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter(), new ByteArrayHexConverter() }
        });
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, json);
    }

    private void GenerateActorsCsv()
    {
        ActorModel[] actorModels = new ActorModel[256];

        for (int i = 0; i < GameInfo.Levels.Length; i++)
        {
            Scene2DResource scene = Rom.LoadResource<Scene2DResource>(i);

            // NOTE: Some actors types have multiple models, but they all appear the same
            foreach (Actor actor in scene.Actors.Concat(scene.AlwaysActors))
            {
                ActorModel model = actor.Model;
                actorModels[actor.Type] = model;
            }
        }

        StringBuilder sb = new();

        void addValue(string value) => sb.Append($"{value},");

        // Header
        addValue("Type Id");
        addValue("Type Name");
        addValue("Hit Points");
        addValue("Attack Points");
        addValue("Receives damage");
        addValue("Map Collision");
        addValue("Object Collision");
        addValue("Is Solid");
        addValue("Is Against Captor");
        addValue("Actions");
        addValue("Animations");
        sb.AppendLine();

        for (int i = 0; i < actorModels.Length; i++)
        {
            ActorModel model = actorModels[i];

            if (model == null)
                continue;

            addValue($"{i}");
            addValue(Enum.IsDefined(typeof(ActorType), i) ? $"{(ActorType)i}" : "");
            addValue(model.HitPoints != 0 ? model.HitPoints.ToString() : "");
            addValue(model.AttackPoints != 0 ? model.AttackPoints.ToString() : "");
            addValue(model.ReceivesDamage ? "✔️" : "");
            addValue(model.CheckAgainstMapCollision ? $"{model.MapCollisionType}" : "");
            addValue(model.CheckAgainstObjectCollision ? "✔️" : "");
            addValue(model.IsSolid ? "✔️" : "");
            addValue(model.IsAgainstCaptor ? "✔️" : "");
            addValue($"{model.Actions.Length}");
            addValue($"{model.AnimatedObject.Animations.Length}");
            sb.AppendLine();
        }

        File.WriteAllText("actors.csv", sb.ToString());
    }

    private void GenerateGameData()
    {
        List<ActorModel>[] actorModels = new List<ActorModel>[256];
        List<AnimatedObjectResource> animatedObjects = new();

        for (int i = 0; i < GameInfo.Levels.Length; i++)
        {
            Scene2DResource scene = Rom.LoadResource<Scene2DResource>(i);

            foreach (Actor actor in scene.Actors.Concat(scene.AlwaysActors))
            {
                ActorModel model = actor.Model;

                string animatedObjectOutputFile = exportAnimatedObject(model.AnimatedObject, $"{(ActorType)actor.Type}");

                actorModels[actor.Type] ??= new List<ActorModel>();

                if (actorModels[actor.Type].Contains(model))
                    continue;

                actorModels[actor.Type].Add(model);

                string actorOutputFile = $"Actors/{(ActorType)actor.Type}_{actorModels[actor.Type].Count}.json";
                writeJson(new
                {
                    ViewBox = new Box(model.ViewBox),
                    DetectionBox = new Box(model.DetectionBox),
                    AnimatedObject = animatedObjectOutputFile,
                    model.MapCollisionType,
                    model.CheckAgainstMapCollision,
                    model.CheckAgainstObjectCollision,
                    model.IsSolid,
                    model.IsAgainstCaptor,
                    model.ReceivesDamage,
                    model.HitPoints,
                    model.AttackPoints,
                    Actions = model.Actions.Select(x => new
                    {
                        Box = new Box(x.Box),
                        x.AnimationIndex,
                        x.Flags,
                        x.MechModelType,
                        MechModelParams = x.MechModel?.Params.Select(p => p.AsFloat)
                    })
                }, actorOutputFile);
            }
        }

        // TODO: Generate remaining data (playfields, localization, story acts, animated objects etc.). Data with properties should
        //       be exported to .json and raw data, like graphics and maps, should be .dat files (with a .json for the header).
        //       Once done we can use this to load this as the game data, allowing easier edits. We can also compare data between
        //       prototypes, or even import data from prototypes into final version (like the snail actor).

        string exportAnimatedObject(AnimatedObjectResource animatedObject, string name)
        {
            string animatedObjectOutputFile = $"Animation/{name}.json";

            if (animatedObjects.Contains(animatedObject))
                return animatedObjectOutputFile;

            animatedObjects.Add(animatedObject);

            writeJson(new
            {
                animatedObject.Is8Bit,
                animatedObject.IsDynamic,
                Animations = animatedObject.Animations.Select(x => new
                {
                    x.Speed,
                    x.DoNotRepeat,
                    x.ChannelsPerFrame,
                    x.Channels
                }),
            }, animatedObjectOutputFile);

            return animatedObjectOutputFile;
        }

        void writeJson<T>(T obj, string filePath) => WriteJson(obj, Path.Combine("GameData", filePath));
    }

    private void GenerateCreditsWheelMesh()
    {
        const string exportDir = "Models";
        const string name = "CreditsWheel";
        const float textureSize = 64;

        Directory.CreateDirectory(exportDir);

        // Load resources
        AnimActorResource animActorResource = Rom.LoadResource<AnimActorResource>(GameResource.CreditsWheelAnimActor);
        TextureTable textureTable = Rom.LoadResource<TextureTable>(GameResource.CreditsWheelTextureTable);
        PaletteTable paletteTable = Rom.LoadResource<PaletteTable>(GameResource.CreditsWheelPaletteTable);

        GeometryObject geometryObject = animActorResource.GeometryTable.GeometryObjects[0];

        using StreamWriter objWriter = new(Path.Combine(exportDir, $"{name}.obj"));
        using StreamWriter mtlWriter = new(Path.Combine(exportDir, $"{name}.mtl"));

        objWriter.WriteLine($"mtllib {name}.mtl");

        // Vertices
        foreach (Vector3 vertex in geometryObject.Vertices)
            objWriter.WriteLine(String.Format("v {0} {1} {2}", 
                vertex.X.AsFloat.ToString(CultureInfo.InvariantCulture),
                vertex.Y.AsFloat.ToString(CultureInfo.InvariantCulture),
                vertex.Z.AsFloat.ToString(CultureInfo.InvariantCulture)));

        objWriter.WriteLine();

        // UVs
        foreach (UV uv in geometryObject.TriangleUVs.SelectMany(x => x.UVs))
            objWriter.WriteLine(String.Format("vt {0} {1}",
                (uv.U / (textureSize - 1)).ToString(CultureInfo.InvariantCulture),
                (uv.V / (textureSize - 1)).ToString(CultureInfo.InvariantCulture)));

        objWriter.WriteLine();

        HashSet<string> writtenMaterials = new();

        // Triangles
        writeGroup("Side1", 0, 8, -1);
        for (int i = 0; i < 16; i += 2)
            writeGroup($"Icon{i / 2}", 8 + i, 2, i / 2);
        writeGroup("Side2", 24, 8, -1);

        void writeGroup(string groupName, int triangleStartIndex, int trianglesCount, int textureIndex)
        {
            string materialName = textureIndex == -1 ? "Untextured" : $"Texture{textureIndex}";

            objWriter.WriteLine($"g group_{groupName}");
            objWriter.WriteLine($"usemtl mtl_{materialName}");

            for (int i = triangleStartIndex; i < triangleStartIndex + trianglesCount; i++)
            {
                Triangle triangle = geometryObject.Triangles[i];

                objWriter.WriteLine("f {2}/{5} {1}/{4} {0}/{3}",
                    triangle.Vertices[0] + 1,
                    triangle.Vertices[1] + 1,
                    triangle.Vertices[2] + 1,
                    triangle.UVsOffset / 2 + 0 + 1,
                    triangle.UVsOffset / 2 + 1 + 1,
                    triangle.UVsOffset / 2 + 2 + 1);
            }

            objWriter.WriteLine();

            if (!writtenMaterials.Add(materialName))
                return;

            mtlWriter.WriteLine($"newmtl mtl_{materialName}");
            mtlWriter.WriteLine("Ka 0.00000 0.00000 0.00000");
            mtlWriter.WriteLine("Kd 0.50000 0.50000 0.50000");
            mtlWriter.WriteLine("Ks 0.00000 0.00000 0.00000");
            mtlWriter.WriteLine("d 1.00000");
            mtlWriter.WriteLine("illum 0");

            if (textureIndex != -1)
            {
                Texture2D tex = Engine.TextureCache.GetOrCreateObject(
                    pointer: textureTable.Offset,
                    id: textureIndex,
                    data: (Texture: textureTable.Textures[textureIndex].Value, Palette: paletteTable.Palettes[0].Value),
                    createObjFunc: static data =>
                    {
                        Palette palette = Engine.PaletteCache.GetOrCreateObject(
                            pointer: data.Palette.Offset,
                            id: 0,
                            data: data.Palette,
                            createObjFunc: static paletteData => new Palette(paletteData));

                        return new BitmapTexture2D(data.Texture.Width, data.Texture.Height, data.Texture.ImgData, palette);
                    });

                string texFilePath = $"{name}_{materialName}.png";

                using Stream texStream = File.Create(Path.Combine(exportDir, texFilePath));
                tex.SaveAsPng(texStream, tex.Width, tex.Height);

                mtlWriter.WriteLine($"map_Kd {texFilePath}");
            }

            mtlWriter.WriteLine();
        }
    }

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        if (ImGui.MenuItem("Actors CSV"))
            GenerateActorsCsv();

        if (ImGui.MenuItem("Game data"))
            GenerateGameData();

        if (ImGui.MenuItem("Credits wheel mesh"))
            GenerateCreditsWheelMesh();
    }
}