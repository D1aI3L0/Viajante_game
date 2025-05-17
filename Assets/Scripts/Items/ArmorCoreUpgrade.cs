using System;
using System.IO;
using UnityEngine;

[Serializable]
public enum SurvivalStatType
{
    Balanced,
    Health,
    Defence,
    Evasion
}

[Serializable]
public class ArmorCoreUpgrade : Upgrade
{
    public bool isBurned;
    public SurvivalStatType statType;
    public int bonusValue;
    //=================================================================================================
    //                                      Сохранение и загрузка
    //=================================================================================================
    public override void Save(BinaryWriter writer)
    {
        base.Save(writer);
        writer.Write(isBurned);
        writer.Write((int)statType);
        writer.Write(bonusValue);
    }

    public override void Load(BinaryReader reader)
    {
        base.Load(reader);

        isBurned = reader.ReadBoolean();
        statType = (SurvivalStatType)reader.ReadInt32();
        bonusValue = reader.ReadInt32();
    }
}