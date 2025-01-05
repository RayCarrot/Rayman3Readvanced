namespace GbaMonoGame;

public class QuitGameMenu : Menu
{
    public override void Update(MenuManager menu)
    {
        menu.SetColumns(1);
        menu.SetHorizontalAlignment(MenuManager.HorizontalAlignment.Center);

        menu.Text("Are you sure you want to quit the game? Any unsaved progress will be lost!");

        if (menu.Button("No"))
            menu.GoBack();

        if (menu.Button("Yes"))
            Engine.GbaGame.Exit();
    }
}