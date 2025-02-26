using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum BiomeNames
{
    Desert,
    Plain,
    Mud,
    StoneDesert,
    Snow,
    SubBorder,
    None
}

public enum SubBiomeNames
{
    Lake,
    Oasis,
    None
}

struct BiomePattern
{
    public BiomeNames name;
    public int maxRadius, minRadius;

    public BiomePattern(BiomeNames name, int minRadius, int maxRadius)
    {
        this.name = name;
        this.minRadius = minRadius;
        this.maxRadius = maxRadius;
    }
}

struct MinMaxElevation
{
    public int min, max;

    public MinMaxElevation(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}


public class HexBiome : MonoBehaviour
{
    HexCell center;
    public HexCell Center
    {
        get
        {
            return center;
        }
        set
        {
            center = value;
        }
    }

    public List<HexCell> borderCells;
    public List<HexCell> Border
    {
        get
        {
            return borderCells;
        }
        set
        {
            borderCells = value;
        }
    }

    public HexBiome(HexCell center, List<HexCell> borderCells)
    {
        this.center = center;
        this.borderCells = borderCells;
    }

    public List<HexCell> GetBiomeCells()
    {
        List<HexCell> cells = ListPool<HexCell>.Get();
        HexCellPriorityQueue searchFrontier = new HexCellPriorityQueue();
        int searchFrontierPhase = 1;

        cells.Add(center);
        center.SearchPhase = searchFrontierPhase;
        searchFrontier.Enqueue(center);

        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (!neighbor || neighbor.biomeName == BiomeNames.SubBorder)
                {
                    continue;
                }
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    cells.Add(neighbor);
                    neighbor.SearchPhase = searchFrontierPhase;
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }

        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].SearchPhase = 0;
        }
        return cells;
    }

    public static List<HexCell> GetBiomeCells(HexCell center)
    {
        List<HexCell> cells = ListPool<HexCell>.Get();
        HexCellPriorityQueue searchFrontier = new HexCellPriorityQueue();
        int searchFrontierPhase = 1;

        cells.Add(center);
        center.SearchPhase = searchFrontierPhase;
        searchFrontier.Enqueue(center);

        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (!neighbor || neighbor.biomeName == BiomeNames.SubBorder)
                {
                    continue;
                }
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    cells.Add(neighbor);
                    neighbor.SearchPhase = searchFrontierPhase;
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }

        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].SearchPhase = 0;
        }
        return cells;
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(center.Index);

        writer.Write(borderCells.Count);
        for(int i = 0; i < borderCells.Count; i++)
        {
            writer.Write(borderCells[i].Index);
        }
    }

    ~HexBiome()
    {
        if (borderCells != null)
            ListPool<HexCell>.Add(borderCells);
    }
}
