public class Trait
{
    public TraitData Data { get; }
    private int remainingDuration;

    public Trait(TraitData data)
    {
        Data = data;
        remainingDuration = data.durationTurns;
    }

    public void OnTurnEnd()
    {
        if (!Data.isPermanent)
        {
            remainingDuration--;
        }
    }

    public bool IsExpired => !Data.isPermanent && remainingDuration <= 0;
}