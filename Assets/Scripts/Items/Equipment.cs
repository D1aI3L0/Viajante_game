using System;


[Serializable]
public class Equipment
{
    public Weapon weapon1 = new(), weapon2 = new();
    public ArmorCore armorCore = new();
    public Artifact artifact;

    public void Initialize()
    {
        armorCore.Initialize();
        weapon1.Initialize();
        weapon2.Initialize();
    }
}