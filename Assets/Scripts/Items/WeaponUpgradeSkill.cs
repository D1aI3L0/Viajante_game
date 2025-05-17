using System;
using System.IO;


[Serializable]
public class WeaponUpgradeSkill : WeaponUpgrade
{
    public bool isFixed;
    public SkillAsset linkedSkill = null;
    //=================================================================================================
    //                                      Сохранение и загрузка
    //=================================================================================================
    public override void Save(BinaryWriter writer)
    {
        base.Save(writer);
        writer.Write(isFixed);
    }

    public void Save(BinaryWriter writer, Weapon weapon)
    {
        Save(writer);
        writer.Write(weapon.GetSkillID(linkedSkill));
    }

    public override void Load(BinaryReader reader)
    {
        base.Load(reader);
        
        isFixed = reader.ReadBoolean();
    }

    public void Load(BinaryReader reader, Weapon weapon)
    {
        Load(reader);

        linkedSkill = weapon.GetSkillByID(reader.ReadInt32());
    }
}