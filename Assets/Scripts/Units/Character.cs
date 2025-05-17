using System;
using System.IO;



[Serializable]
public class Character
{
    public string characterName;
    public int level = 1;
    public CharacterStats currentCharacterStats = new(), baseCharacterStats = new();
    //=================================================================================================
    //                                      Сохранение и загрузка
    //=================================================================================================
    public virtual void Save(BinaryWriter writer)
    {
        writer.Write(level);
    }
    
    public virtual void Load(BinaryReader reader)
    {
        level = reader.ReadInt32();
    }
}