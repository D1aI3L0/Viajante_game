using System;
using UnityEngine;

[Serializable]
public class Weapon : Item
{
    public AttackStats attackStats; 
    public float attackBonus, accuracyBonus, critBonus;
}
