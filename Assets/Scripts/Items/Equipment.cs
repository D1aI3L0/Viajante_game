using System;


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
}