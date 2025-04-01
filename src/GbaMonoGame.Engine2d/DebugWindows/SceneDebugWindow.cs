using System;
using System.Linq;
using GbaMonoGame.TgxEngine;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GbaMonoGame.Engine2d;

public class SceneDebugWindow : DebugWindow
{
    public override string Name => "Scene";
    public GameObject HighlightedGameObject { get; set; }
    public GameObject SelectedGameObject { get; set; }

    private void DrawBox(GfxRenderer renderer, TgxPlayfield playfield, Box box, Color color)
    {
        if (box == Box.Empty)
            return;

        box = box.Offset(-playfield.Camera.Position);
        renderer.DrawRectangle(box.ToRectangle(), color);
    }

    private Box GetObjBox(GameObject obj)
    {
        if (obj is BaseActor actor)
            return actor.GetViewBox();
        else if (obj is Captor captor)
            return captor.GetCaptorBox();
        else
            throw new Exception("Unsupported object type");
    }

    private void UpdateMouseDetection(Scene2D scene)
    {
        if (!InputManager.IsMouseOnScreen(scene.RenderContext))
            return;
        
        HighlightedGameObject = null;

        Vector2 mousePos = InputManager.GetMousePosition(scene.RenderContext);

        foreach (GameObject gameObject in new EnabledAlwaysActorIterator(scene).Concat(new EnabledActorCaptorIterator(scene)))
        {
            Box box = GetObjBox(gameObject).Offset(-scene.Playfield.Camera.Position);

            if (box.Contains(mousePos))
            {
                HighlightedGameObject = gameObject;
                break;
            }
        }

        if (InputManager.GetMouseState().LeftButton == ButtonState.Pressed)
        {
            SelectedGameObject = HighlightedGameObject;
        }
    }

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        if (Frame.Current is not IHasScene { Scene: { } scene2D }) 
            return;

        if (scene2D.Playfield is TgxPlayfield2D)
            UpdateMouseDetection(scene2D);

        ImGui.SeparatorText("General");

        ImGui.Text($"Keep all objects active: {scene2D.KeepAllObjectsActive}");

        if (ImGui.Button("Deselect object"))
            SelectedGameObject = null;

        ImGui.SeparatorText("Always actors");

        ImGui.Text($"Count: {scene2D.KnotManager.AlwaysActorsCount}");

        if (ImGui.BeginListBox("##_alwaysActors", new System.Numerics.Vector2(300, 150)))
        {
            foreach (BaseActor actor in scene2D.KnotManager.AlwaysActors)
            {
                bool isSelected = SelectedGameObject == actor;
                if (ImGui.Selectable($"{actor.InstanceId}. {ObjectFactory.GetActorTypeName(actor.Type)}", isSelected))
                    SelectedGameObject = actor;
            }

            ImGui.EndListBox();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.SeparatorText("Actors");

        ImGui.Text($"Count: {scene2D.KnotManager.ActorsCount}");

        if (ImGui.BeginListBox("##_actors", new System.Numerics.Vector2(300, 300)))
        {
            foreach (BaseActor actor in scene2D.KnotManager.Actors)
            {
                bool isSelected = SelectedGameObject == actor;
                if (ImGui.Selectable($"{actor.InstanceId}. {ObjectFactory.GetActorTypeName(actor.Type)}", isSelected))
                    SelectedGameObject = actor;
            }

            ImGui.EndListBox();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.SeparatorText("Captors");

        ImGui.Text($"Count: {scene2D.KnotManager.CaptorsCount}");

        if (scene2D.KnotManager.CaptorsCount > 0 && ImGui.BeginListBox("##_captors", new System.Numerics.Vector2(300, 80)))
        {
            foreach (Captor captor in scene2D.KnotManager.Captors)
            {
                bool isSelected = SelectedGameObject == captor;
                if (ImGui.Selectable($"{captor.InstanceId}. Captor", isSelected))
                    SelectedGameObject = captor;
            }

            ImGui.EndListBox();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.SeparatorText("Added projectiles");

        ImGui.Text($"Count: {scene2D.KnotManager.AddedProjectiles.Count}");

        if (scene2D.KnotManager.AddedProjectiles.Count > 0 && ImGui.BeginListBox("##_addedProjectiles", new System.Numerics.Vector2(300, 80)))
        {
            foreach (BaseActor actor in scene2D.KnotManager.AddedProjectiles)
            {
                bool isSelected = SelectedGameObject == actor;
                if (ImGui.Selectable($"{actor.InstanceId}. {ObjectFactory.GetActorTypeName(actor.Type)}", isSelected))
                    SelectedGameObject = actor;
            }

            ImGui.EndListBox();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.SeparatorText("Knots");

        ImGui.Text("Count: 0");
        ImGui.Text("TODO: Implement");

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.SeparatorText("Camera");

        scene2D.Camera.DrawDebugLayout(debugLayout, textureManager);
    }

    public override void DrawGame(GfxRenderer renderer)
    {
        if (Frame.Current is not IHasScene { Scene: { Playfield: TgxPlayfield2D } scene2D }) 
            return;

        renderer.BeginRender(new RenderOptions()
        {
            RenderContext = scene2D.RenderContext,
        });

        if (HighlightedGameObject != null)
            DrawBox(renderer, scene2D.Playfield, GetObjBox(HighlightedGameObject), Color.Orange);

        if (SelectedGameObject != null)
            DrawBox(renderer, scene2D.Playfield, GetObjBox(SelectedGameObject), Color.Red);
    }
}