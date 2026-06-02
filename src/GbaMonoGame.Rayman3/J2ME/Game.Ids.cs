namespace GbaMonoGame.Rayman3.J2ME;

public partial class Game
{
    // TODO: Add resource IDs

    public const int TEXT_BANK_INDEX_GAME = 0x2D;
    public readonly int STRING_ID_NEW_GAME = StringId.Create(0x0, TEXT_BANK_INDEX_GAME); // New Game
    public readonly int STRING_ID_CONTINUE = StringId.Create(0x9, TEXT_BANK_INDEX_GAME); // Continue
    public readonly int STRING_ID_ABOUT = StringId.Create(0x12, TEXT_BANK_INDEX_GAME); // About
    public readonly int STRING_ID_HELP = StringId.Create(0x18, TEXT_BANK_INDEX_GAME); // Help
    public readonly int STRING_ID_HELP_TITLE = StringId.Create(0x1D, TEXT_BANK_INDEX_GAME); // HELP
    public readonly int STRING_ID_RESUME = StringId.Create(0x22, TEXT_BANK_INDEX_GAME); // Resume
    public readonly int STRING_ID_RESTART = StringId.Create(0x29, TEXT_BANK_INDEX_GAME); // Restart
    public readonly int STRING_ID_MUSIC_HIGH = StringId.Create(0x31, TEXT_BANK_INDEX_GAME); // Music : High
    public readonly int STRING_ID_MUSIC_MEDIUM = StringId.Create(0x3E, TEXT_BANK_INDEX_GAME); // Music : Medium
    public readonly int STRING_ID_MUSIC_LOW = StringId.Create(0x4D, TEXT_BANK_INDEX_GAME); // Music : Low
    public readonly int STRING_ID_MUSIC_OFF = StringId.Create(0x59, TEXT_BANK_INDEX_GAME); // Music : Off
    public readonly int STRING_ID_MAIN_MENU = StringId.Create(0x65, TEXT_BANK_INDEX_GAME); // Main Menu
    public readonly int STRING_ID_EXIT = StringId.Create(0x6F, TEXT_BANK_INDEX_GAME); // Exit
    public readonly int STRING_ID_ENTER_NOW = StringId.Create(0x74, TEXT_BANK_INDEX_GAME); // Enter Now!
    public readonly int STRING_ID_LEVEL = StringId.Create(0x7F, TEXT_BANK_INDEX_GAME); // Level 
    public readonly int STRING_ID_UNKNOWN_STATUS = StringId.Create(0x86, TEXT_BANK_INDEX_GAME); //  ?/?
    public readonly int STRING_ID_LOADING = StringId.Create(0x8B, TEXT_BANK_INDEX_GAME); // Loading
    public readonly int STRING_ID_LEVEL_DONE = StringId.Create(0x93, TEXT_BANK_INDEX_GAME); // LEVEL DONE
    public readonly int STRING_ID_GAME_OVER = StringId.Create(0x9E, TEXT_BANK_INDEX_GAME); // GAME OVER
    public readonly int STRING_ID_VICTORY = StringId.Create(0xA8, TEXT_BANK_INDEX_GAME); // VICTORY !!!
    public readonly int STRING_ID_ENABLE_SOUND = StringId.Create(0xB4, TEXT_BANK_INDEX_GAME); // ENABLE SOUND?
    public readonly int STRING_ID_EMPTY = StringId.Create(0xC2, TEXT_BANK_INDEX_GAME); // 
    public readonly int STRING_ID_YES = StringId.Create(0xC3, TEXT_BANK_INDEX_GAME); // yes
    public readonly int STRING_ID_NO = StringId.Create(0xC7, TEXT_BANK_INDEX_GAME); // no
    public readonly int STRING_ID_EXIT_QUESTION = StringId.Create(0xCA, TEXT_BANK_INDEX_GAME); // Exit?
    public readonly int STRING_ID_RESTART_QUESTION = StringId.Create(0xD0, TEXT_BANK_INDEX_GAME); // Restart?
    public readonly int STRING_ID_TO_MAIN_MENU_QUESTION = StringId.Create(0xD9, TEXT_BANK_INDEX_GAME); // To Main menu?

    public const int TEXT_BANK_INDEX_CREDITS_UNUSED = 0x2E;

    public const int TEXT_BANK_INDEX_CREDITS = 0x2F;
    public readonly int STRING_ID_SUPPORT_MAIL = StringId.Create(0x46, TEXT_BANK_INDEX_CREDITS); // support mail
    public readonly int STRING_ID_VERSION_NUMBER = StringId.Create(0x9B, TEXT_BANK_INDEX_CREDITS); // Version Number

    public const int TEXT_BANK_INDEX_HELP = 0x30;
}