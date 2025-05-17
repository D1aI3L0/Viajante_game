using System.Collections.Generic;
using System.IO;

public class Rune : Item
{
    public int attackBonus;
    public int critBonus;
    public int accuracyBonus;
    public int SEmultiplier;

    public List<HexDirection> runeConnections = new();
    //=================================================================================================
    //                                      Сохранение и загрузка
    //=================================================================================================
    public override void Save(BinaryWriter writer)
    {
        base.Save(writer);
        writer.Write(attackBonus);
        writer.Write(critBonus);
        writer.Write(accuracyBonus);
        writer.Write(SEmultiplier);

        writer.Write(runeConnections.Count);
        foreach(HexDirection direction in runeConnections)
        {
            writer.Write((int)direction);
        }
    }

    public override void Load(BinaryReader reader)
    {
        base.Load(reader);
        attackBonus = reader.ReadInt32();
        critBonus = reader.ReadInt32();
        accuracyBonus = reader.ReadInt32();
        SEmultiplier = reader.ReadInt32();

        int connectionsCount = reader.ReadInt32();
        for(int i = 0; i < connectionsCount; i++)
        {
            runeConnections.Add((HexDirection)reader.ReadInt32());
        }
    }
}