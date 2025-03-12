using System;
using System.Collections.Generic;
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
    public bool isCapital;
    public SettlementType type;
    public SettlementNation nation;
    public HexCell center;
    public List<HexCell> border;

    public HexSettlement(HexCell center, List<HexCell> border, SettlementType type, SettlementNation nation, bool isCapital)
    {
        this.nation = nation;
        this.type = type;
        this.center = center;
        this.border = border;
        this.isCapital = isCapital;
    }
}
