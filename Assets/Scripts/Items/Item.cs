using System;
using System.IO;
using UnityEngine;

[Serializable]
public class Item
{
    public Sprite icon;
    //=================================================================================================
    //                                      Сохранение и загрузка
    //=================================================================================================
    public virtual void Save(BinaryWriter writer)
    { 
        
    }

    public virtual void Load(BinaryReader reader)
    {

    }
}
