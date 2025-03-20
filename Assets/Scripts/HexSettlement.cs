using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SettlementType
{
    Village,
    Town,
    City
}

public enum SettlementNation
{
    Desert,
    Plain,
    Snow
}

public struct SettlementPattern
{
    public SettlementType settlementType;
    public MinMaxInt minMaxRadius;

    public SettlementPattern(SettlementType settlementType, int minRadius, int maxRadius)
    {
        this.settlementType = settlementType;
        minMaxRadius = new MinMaxInt(minRadius, maxRadius);
    }
}

public class HexSettlement : MonoBehaviour
{
    public int index;
    public bool isCapital;
    public SettlementType type;
    public SettlementNation nation;
    public HexCell center;
    public List<HexCell> border;
    public List<HexSettlement> connectedWith;

    public HexSettlement(HexCell center, List<HexCell> border, SettlementType type, SettlementNation nation, bool isCapital, int index)
    {
        this.index = index;
        this.nation = nation;
        this.type = type;
        this.center = center;
        this.border = border;
        this.isCapital = isCapital;
        connectedWith = ListPool<HexSettlement>.Get();
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(index);
        writer.Write(center.Index);

        writer.Write(border.Count);
        for (int i = 0; i < border.Count; i++)
        {
            writer.Write(border[i].Index);
        }

        writer.Write((byte)type);
        writer.Write((byte)nation);
        writer.Write(isCapital);
        writer.Write(connectedWith.Count);
        for (int i = 0; i < connectedWith.Count; i++)
        {
            writer.Write(connectedWith[i].index);
        }
    }
    
    ~HexSettlement()
    {
        if (border != null)
            ListPool<HexCell>.Add(border);
    }
}
