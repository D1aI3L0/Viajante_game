using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public enum BiomeName
{
    Desert,
    Plain,
    Snow,
    Mud,
    StoneDesert,
    SubBorder,
    None
}

public enum SubBiomeName
{
    Lake,
    Oasis,
    None
}

public enum BiomeSize
{
    ToSmall,
    Small,
    Medium,
    Large
}

struct BiomePattern
{
    public BiomeName name;
    public MinMaxInt minMaxRadius;

    public BiomePattern(BiomeName name, int minRadius, int maxRadius)
    {
        this.name = name;
        minMaxRadius = new MinMaxInt(minRadius, maxRadius);
    }
}


public struct MinMaxInt
{
    public int min, max;

    public MinMaxInt(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}


public class HexBiome : MonoBehaviour
{
    static readonly int[][] biomeSizes =
    {
        new []{80, 200, 300},
        new []{80, 200, 300},
        new []{80, 200, 300},
        new []{90, 200, 360},
        new []{85, 170, 340}
    };

    public HexCell center;
    public List<HexCell> border;
    public List<HexCell> biomeCells;
    public BiomeSize size;

    public int settlementsCount = 0;

    public BiomeName BiomeName
    {
        get
        {
            return center.biomeName;
        }
    }


    public HexBiome(HexCell center, List<HexCell> borderCells)
    {
        this.center = center;
        border = borderCells;
    }


    public void SetBiomeCell()
    {
        biomeCells = GetBiomeCells();
        size = BiomeSize.ToSmall;
        int biomeType = (int)center.biomeName;
        for (int i = 1; i <= (int)BiomeSize.Large; i++)
        {
            if (biomeCells.Count >= biomeSizes[biomeType][i - 1])
                size = (BiomeSize)i;
            else
                break;
        }
    }


    public List<HexCell> GetBiomeCells(bool includeBorders = true)
    {
        List<HexCell> cells = ListPool<HexCell>.Get();
        HexCellPriorityQueue searchFrontier = new HexCellPriorityQueue();
        int searchFrontierPhase = center.SearchPhase + 1;

        cells.Add(center);
        center.SearchPhase = searchFrontierPhase;
        searchFrontier.Enqueue(center);

        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (!neighbor || neighbor.biomeName == BiomeName.SubBorder)
                {
                    continue;
                }
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    if (!includeBorders || (includeBorders && !border.Contains(neighbor)))
                        cells.Add(neighbor);
                    neighbor.SearchPhase = searchFrontierPhase;
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }
        return cells;
    }


    public void Save(BinaryWriter writer)
    {
        writer.Write(center.Index);

        writer.Write(border.Count);
        for (int i = 0; i < border.Count; i++)
        {
            writer.Write(border[i].Index);
        }
    }


    ~HexBiome()
    {
        if (border != null)
            ListPool<HexCell>.Add(border);
    }
}
