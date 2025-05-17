using System;
using System.IO;


[Serializable]
public class Equipment
{
    public Weapon weapon1, weapon2;
    public ArmorCore armorCore = new();
    public Artifact artifact = null;

    public void Initialize()
    {
        armorCore.Initialize();
    }
    //=================================================================================================
    //                                      Сохранение и загрузка
    //=================================================================================================
    public void Save(BinaryWriter writer, PlayerCharacter character)
    {
        writer.Write(character.GetWeaponID(weapon1));
        writer.Write(character.GetWeaponID(weapon2));
        
        armorCore.Save(writer);
    }

    public void Load(BinaryReader reader, PlayerCharacter character)
    {
        int weapon1ID = reader.ReadInt32();
        if(weapon1ID >= 0)
            weapon1 = character.GetWeaponByID(weapon1ID);

        int weapon2ID = reader.ReadInt32();
        if(weapon2ID >= 0)
            weapon2 = character.GetWeaponByID(weapon2ID);

        armorCore.Load(reader);
    }
}