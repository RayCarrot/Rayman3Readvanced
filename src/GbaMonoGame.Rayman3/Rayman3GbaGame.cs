using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Editor;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class Rayman3GbaGame : GbaGame
{
    #region Protected Properties

    protected override string Title => "Rayman 3";

    #endregion

    #region Protected Methods

    protected override Frame CreateInitialFrame() => new TitleScreen(false);
    protected override Frame CreateFatalErrorFrame(Exception exception) => new FrameFatalError(exception);

    protected override void InitEngine()
    {
        Rayman3.InitEngine();
    }

    protected override void InitGame()
    {
        Engine.InitGame(
            font: new FontManager(Rom.Loader.Font8, Rom.Loader.Font16, Rom.Loader.Font32));
        Rayman3.InitGame();
    }

    protected override void UnInitEngine()
    {
        Rayman3.UnInitEngine();
    }

    protected override void UnInitGame()
    {
        Engine.UnInitGame();
        Rayman3.UnInitGame();
    }

    protected override void AddDebugWindowsAndMenus(DebugLayout debugLayout)
    {
        debugLayout.AddWindow(new SceneDebugWindow());
        debugLayout.AddWindow(new GameObjectDebugWindow());
        debugLayout.AddWindow(new PlayfieldDebugWindow());
        debugLayout.AddWindow(new Rayman3DebugWindow());
        debugLayout.AddMenu(new FramesDebugMenu());
        debugLayout.AddMenu(new ExportDebugMenu());
    }

    #endregion
}