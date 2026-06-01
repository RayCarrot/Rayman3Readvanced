namespace GbaMonoGame.Rayman3.J2ME;

// Replaces javax.microedition.lcdui.Font
public class Font
{
    public Font(FontSize size)
    {
        Size = size;
    }

    public FontSize Size { get; }

    public static Font getFont(int face, int style, int size)
    {
        // Ignore the face, style and size and hard-code to the 16px font
        return new Font(FontSize.Font16);
    }

    public int getHeight()
    {
        return Engine.Font.GetFontHeight(Size);
    }

    public int stringWidth(string str)
    {
        return Engine.Font.GetStringWidth(Size, str);
    }
}