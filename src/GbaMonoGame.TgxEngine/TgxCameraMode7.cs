using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

// TODO: Implement
public class TgxCameraMode7 : TgxCamera
{
    public TgxCameraMode7(RenderContext renderContext) : base(renderContext)
    {
        Horizon = 62;
        field_0xb49 = 235;
        Zoom = 38;
        IsDirty = true;
        
        RotscaleAffineMatrixes = new AffineMatrix[220];
        for (int i = 0; i < RotscaleAffineMatrixes.Length; i++)
            RotscaleAffineMatrixes[i] = AffineMatrix.Identity;

        Scales = new float[256];
        for (int i = 0; i < 160; i++)
            Scales[i] = 2000000000;
        for (int i = 161; i < 255; i++)
            Scales[i] = Mode7ScaleTable[i - Horizon] * Zoom;
    }

    private static float[] Mode7ScaleTable { get; } =
    [
        12f, // 0x000C0000
        10.909088f, // 0x000AE8BA
        10f, // 0x000A0000
        9.230759f, // 0x00093B13
        8.571426f, // 0x00089249
        8f, // 0x00080000
        7.5f, // 0x00078000
        7.0588226f, // 0x00070F0F
        6.6666565f, // 0x0006AAAA
        6.3157806f, // 0x000650D7
        6f, // 0x00060000
        5.714279f, // 0x0005B6DB
        5.454544f, // 0x0005745D
        5.2173767f, // 0x000537A6
        5f, // 0x00050000
        4.799988f, // 0x0004CCCC
        4.6153717f, // 0x00049D89
        4.4444427f, // 0x000471C7
        4.2857056f, // 0x00044924
        4.137924f, // 0x0004234F
        4f, // 0x00040000
        3.8709564f, // 0x0003DEF7
        3.75f, // 0x0003C000
        3.6363525f, // 0x0003A2E8
        3.5294037f, // 0x00038787
        3.4285583f, // 0x00036DB6
        3.3333282f, // 0x00035555
        3.2432404f, // 0x00033E45
        3.1578827f, // 0x0003286B
        3.0769196f, // 0x000313B1
        3f, // 0x00030000
        2.9268188f, // 0x0002ED44
        2.857132f, // 0x0002DB6D
        2.7906952f, // 0x0002CA6B
        2.7272644f, // 0x0002BA2E
        2.6666565f, // 0x0002AAAA
        2.6086884f, // 0x00029BD3
        2.5531769f, // 0x00028D9D
        2.5f, // 0x00028000
        2.4489746f, // 0x000272F0
        2.399994f, // 0x00026666
        2.3529358f, // 0x00025A5A
        2.3076782f, // 0x00024EC4
        2.264145f, // 0x0002439F
        2.2222137f, // 0x000238E3
        2.1818085f, // 0x00022E8B
        2.1428528f, // 0x00022492
        2.1052551f, // 0x00021AF2
        2.0689545f, // 0x000211A7
        2.0338898f, // 0x000208AD
        2f, // 0x00020000
        1.9672089f, // 0x0001F79B
        1.9354706f, // 0x0001EF7B
        1.9047546f, // 0x0001E79E
        1.875f, // 0x0001E000
        1.8461456f, // 0x0001D89D
        1.8181763f, // 0x0001D174
        1.7910309f, // 0x0001CA81
        1.7646942f, // 0x0001C3C3
        1.7391205f, // 0x0001BD37
        1.7142792f, // 0x0001B6DB
        1.6901398f, // 0x0001B0AD
        1.6666565f, // 0x0001AAAA
        1.6438293f, // 0x0001A4D2
        1.6216125f, // 0x00019F22
        1.5999908f, // 0x00019999
        1.5789337f, // 0x00019435
        1.5584412f, // 0x00018EF6
        1.5384521f, // 0x000189D8
        1.5189819f, // 0x000184DC
        1.5f, // 0x00018000
        1.4814758f, // 0x00017B42
        1.4634094f, // 0x000176A2
        1.4457703f, // 0x0001721E
        1.4285583f, // 0x00016DB6
        1.4117584f, // 0x00016969
        1.39534f, // 0x00016535
        1.379303f, // 0x0001611A
        1.3636322f, // 0x00015D17
        1.3483124f, // 0x0001592B
        1.3333282f, // 0x00015555
        1.3186798f, // 0x00015195
        1.3043365f, // 0x00014DE9
        1.2903137f, // 0x00014A52
        1.2765808f, // 0x000146CE
        1.2631531f, // 0x0001435E
        1.25f, // 0x00014000
        1.2371063f, // 0x00013CB3
        1.2244873f, // 0x00013978
        1.2121124f, // 0x0001364D
        1.199997f, // 0x00013333
        1.1881104f, // 0x00013028
        1.1764679f, // 0x00012D2D
        1.1650391f, // 0x00012A40
        1.1538391f, // 0x00012762
        1.1428528f, // 0x00012492
        1.1320648f, // 0x000121CF
        1.1214905f, // 0x00011F1A
        1.1110992f, // 0x00011C71
        1.1009064f, // 0x000119D5
        1.0908966f, // 0x00011745
        1.08107f, // 0x000114C1
        1.0714264f, // 0x00011249
        1.0619354f, // 0x00010FDB
        1.0526276f, // 0x00010D79
        1.0434723f, // 0x00010B21
        1.0344696f, // 0x000108D3
        1.0256348f, // 0x00010690
        1.0169373f, // 0x00010456
        1.0083923f, // 0x00010226
        1f, // 0x00010000
        0.99172974f, // 0x0000FDE2
        0.9835968f, // 0x0000FBCD
        0.9756012f, // 0x0000F9C1
        0.96772766f, // 0x0000F7BD
        0.95999146f, // 0x0000F5C2
        0.9523773f, // 0x0000F3CF
        0.94487f, // 0x0000F1E3
        0.9375f, // 0x0000F000
        0.93022156f, // 0x0000EE23
        0.9230652f, // 0x0000EC4E
        0.9160156f, // 0x0000EA80
        0.90908813f, // 0x0000E8BA
        0.9022522f, // 0x0000E6FA
        0.8955078f, // 0x0000E540
        0.8888855f, // 0x0000E38E
        0.8823395f, // 0x0000E1E1
        0.87590027f, // 0x0000E03B
        0.8695526f, // 0x0000DE9B
        0.8632965f, // 0x0000DD01
        0.85713196f, // 0x0000DB6D
        0.85105896f, // 0x0000D9DF
        0.84506226f, // 0x0000D856
        0.8391571f, // 0x0000D6D3
        0.83332825f, // 0x0000D555
        0.8275757f, // 0x0000D3DC
        0.8219147f, // 0x0000D269
        0.8163147f, // 0x0000D0FA
        0.8108063f, // 0x0000CF91
        0.8053589f, // 0x0000CE2C
        0.7999878f, // 0x0000CCCC
        0.794693f, // 0x0000CB71
        0.7894592f, // 0x0000CA1A
        0.78430176f, // 0x0000C8C8
        0.7792206f, // 0x0000C77B
        0.7741852f, // 0x0000C631
        0.7692261f, // 0x0000C4EC
        0.764328f, // 0x0000C3AB
        0.75949097f, // 0x0000C26E
        0.75471497f, // 0x0000C135
        0.75f, // 0x0000C000
        0.7453308f, // 0x0000BECE
        0.7407379f, // 0x0000BDA1
        0.7361908f, // 0x0000BC77
        0.7317047f, // 0x0000BB51
        0.7272644f, // 0x0000BA2E
        0.72288513f, // 0x0000B90F
        0.71855164f, // 0x0000B7F3
        0.7142792f, // 0x0000B6DB
        0.7100525f, // 0x0000B5C6
        0.7058716f, // 0x0000B4B4
        0.7017517f, // 0x0000B3A6
        0.69766235f, // 0x0000B29A
        0.69363403f, // 0x0000B192
        0.6896515f, // 0x0000B08D
        0.68569946f, // 0x0000AF8A
        0.6818085f, // 0x0000AE8B
        0.67796326f, // 0x0000AD8F
        0.67414856f, // 0x0000AC95
        0.67037964f, // 0x0000AB9E
        0.6666565f, // 0x0000AAAA
        0.6629791f, // 0x0000A9B9
        0.6593323f, // 0x0000A8CA
        0.6557312f, // 0x0000A7DE
        0.65216064f, // 0x0000A6F4
        0.64863586f, // 0x0000A60D
        0.64515686f, // 0x0000A529
        0.6417084f, // 0x0000A447
        0.6382904f, // 0x0000A367
        0.6349182f, // 0x0000A28A
        0.63157654f, // 0x0000A1AF
        0.6282654f, // 0x0000A0D6
        0.625f, // 0x0000A000
        0.6217499f, // 0x00009F2B
        0.61854553f, // 0x00009E59
        0.6153717f, // 0x00009D89
        0.61224365f, // 0x00009CBC
        0.60913086f, // 0x00009BF0
        0.6060486f, // 0x00009B26
        0.6030121f, // 0x00009A5F
        0.59999084f, // 0x00009999
        0.5970001f, // 0x000098D5
        0.5940552f, // 0x00009814
        0.5911255f, // 0x00009754
        0.5882263f, // 0x00009696
        0.58535767f, // 0x000095DA
        0.58251953f, // 0x00009520
        0.57969666f, // 0x00009467
        0.57691956f, // 0x000093B1
        0.5741577f, // 0x000092FC
        0.5714264f, // 0x00009249
        0.5687103f, // 0x00009197
        0.5660248f, // 0x000090E7
        0.56336975f, // 0x00009039
        0.56074524f, // 0x00008F8D
        0.558136f, // 0x00008EE2
        0.555542f, // 0x00008E38
        0.5529938f, // 0x00008D91
        0.55044556f, // 0x00008CEA
        0.5479431f, // 0x00008C46
        0.5454407f, // 0x00008BA2
        0.542984f, // 0x00008B01
        0.54052734f, // 0x00008A60
        0.53811646f, // 0x000089C2
        0.53570557f, // 0x00008924
        0.5333252f, // 0x00008888
        0.5309601f, // 0x000087ED
        0.5286255f, // 0x00008754
        0.52630615f, // 0x000086BC
        0.52401733f // 0x00008626
    ];

    public AffineMatrix[] RotscaleAffineMatrixes { get; }
    public float[] Scales { get; }
    public override Vector2 Position { get; set; }
    public int Horizon { get; set; }
    public int field_0xb49 { get; set; }
    public int Zoom { get; set; }
    public float MaxDist { get; set; }
    public byte Direction { get; set; }
    public bool IsDirty { get; set; }

    public void Update()
    {
        IsDirty = false;

        float addCos = MathHelpers.Cos256(Direction + field_0xb49);
        float addSin = MathHelpers.Sin256(Direction + field_0xb49);
        float subCos = MathHelpers.Cos256(Direction - field_0xb49);
        float subSin = MathHelpers.Sin256(Direction - field_0xb49);

        float scale = 0;
        float x = 0;
        float y = 0;
        for (int i = 160; i > Horizon; i--)
        {
            float prevX = x;
            float prevY = y;

            // Get the scale for the current scanline
            scale = Mode7ScaleTable[i - Horizon] * Zoom;
            
            // Save the scale for sprite positioning
            Scales[i] = scale;
            
            // Calculate unknown x and y values
            x = scale * addCos;
            y = scale * addSin;
            float unkX = scale * subCos - x;
            float unkY = scale * subSin - y;

            // Calculate the affine matrix
            RotscaleAffineMatrixes[i] = new AffineMatrix(
                pa: unkX / 4096f + unkX / 256f,
                pb: prevX - x,
                pc: unkY / 4096f + unkY / 256f,
                pd: prevY - y);
        }

        MaxDist = scale;

        // Screen position
        Vector2 bgPos = Position + new Vector2(x, y);
    }
}