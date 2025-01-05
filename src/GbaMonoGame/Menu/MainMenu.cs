namespace GbaMonoGame;

public class MainMenu : Menu
{
    public override void Update(MenuManager menu)
    {
        menu.SetColumns(1);
        menu.SetHorizontalAlignment(MenuManager.HorizontalAlignment.Center);

        menu.Text("Paused");
        menu.Spacing();

        menu.SetColumns(1);
        menu.SetHorizontalAlignment(MenuManager.HorizontalAlignment.Center);

        if (menu.Button("Resume"))
            menu.Close();

        if (Engine.GbaGame.CanSkipCutscene)
        {
            if (menu.Button("Skip cutscene"))
            {
                Engine.GbaGame.SkipCutscene();
                menu.Close();
            }
        }

        if (menu.Button("Options"))
            menu.ChangeMenu(new OptionsMenu());

        if (menu.Button("Bonus"))
            menu.ChangeMenu(new BonusMenu());

        if (menu.Button("Quit game"))
            menu.ChangeMenu(new QuitGameMenu());
    }
}