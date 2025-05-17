using System;
using System.IO;


[Serializable]
public class WeaponUpgradeRune : WeaponUpgrade
{
    public Rune linkedRune = null;
    public bool isBurned;
    //=================================================================================================
    //                                      IUpgradable
    //=================================================================================================
    public override void Save(BinaryWriter writer)
    {
        base.Save(writer);

        writer.Write(isBurned);
        writer.Write(linkedRune != null);
        //if(linkedRune != null) linkedRune.Save(writer);
    }

    public override void Load(BinaryReader reader)
    {
        base.Load(reader);
        
        isBurned = reader.ReadBoolean();
        if(reader.ReadBoolean())
        {
            //linkedRune = new();
            //linkedRune.Load(reader);
        }
    }
}