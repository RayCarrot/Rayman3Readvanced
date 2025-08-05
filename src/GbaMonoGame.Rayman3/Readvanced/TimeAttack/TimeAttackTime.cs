using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public readonly struct TimeAttackTime(TimeAttackTimeType type, int time)
{
    public TimeAttackTimeType Type { get; } = type;
    public int Time { get; } = time;

    public Texture2D LoadIcon(bool filledIn)
    {
        if (!filledIn && Type != TimeAttackTimeType.Record)
            return Engine.FrameContentManager.Load<Texture2D>(Assets.BlankStarSmallTexture);

        return Engine.FrameContentManager.Load<Texture2D>(Type switch
        {
            TimeAttackTimeType.Bronze => Assets.BronzeStarSmallTexture,
            TimeAttackTimeType.Silver => Assets.SilverStarSmallTexture,
            TimeAttackTimeType.Gold => Assets.GoldStarSmallTexture,
            TimeAttackTimeType.Record => Assets.PersonalRecordIconTexture,
            _ => null
        });
    }

    public Texture2D LoadBigIcon(bool filledIn)
    {
        if (!filledIn && Type != TimeAttackTimeType.Record)
            return Engine.FrameContentManager.Load<Texture2D>(Assets.BlankStarBigTexture);

        return Engine.FrameContentManager.Load<Texture2D>(Type switch
        {
            TimeAttackTimeType.Bronze => Assets.BronzeStarBigTexture,
            TimeAttackTimeType.Silver => Assets.SilverStarBigTexture,
            TimeAttackTimeType.Gold => Assets.GoldStarBigTexture,
            _ => null
        });
    }

    public string ToTimeString()
    {
        // Get the minutes value
        int minutes = Time / (60 * 60);

        // Get the seconds value
        int minutesRemainingTime = Time % (60 * 60);
        int seconds = minutesRemainingTime / 60;

        // Get the centiseconds value
        int secondsRemainingTime = minutesRemainingTime % 60;
        int centiSeconds = secondsRemainingTime * 100 / 60;

        int minutesDigit1 = minutes / 10;
        int secondsDigit1 = seconds / 10;
        int centisecondsDigit1 = centiSeconds / 10;

        int minutesDigit2 = minutes + minutesDigit1 * -10;
        if (minutesDigit2 > 9)
            minutesDigit2 = 9;

        int secondsDigit2 = seconds + secondsDigit1 * -10;
        if (secondsDigit2 > 9)
            secondsDigit2 = 9;

        int centisecondsDigit2 = centiSeconds + centisecondsDigit1 * -10;
        if (centisecondsDigit2 > 9)
            centisecondsDigit2 = 9;

        if (centisecondsDigit1 > 10)
            centisecondsDigit1 = 9;

        return $"{minutesDigit1}{minutesDigit2}:{secondsDigit1}{secondsDigit2}.{centisecondsDigit1}{centisecondsDigit2}";
    }
}