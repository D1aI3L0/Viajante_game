using System.Collections.Generic;
using UnityEngine;

public enum BiomeNames
{
    Ocean,
    Desert,
    StoneDesert,
    Plain,
    Mud,
    Mountain,
    SubBorder,
    None
}


struct Biome
{
    public BiomeNames name;
    public int maxRadius, minRadius;
    public int texture;

    public Biome(BiomeNames name, int minRadius, int maxRadius, int texture)
    {
        this.name = name;
        this.minRadius = minRadius;
        this.maxRadius = maxRadius;
        this.texture = texture;
    }
}

public class HexBiome
{
    HexCell center;
    public HexCell Center
    {
        get
        {
            return center;
        }
    }

    List<HexCell> borderCells;
    public List<HexCell> Border
    {
        get
        {
            return borderCells;
        }
    }
    BiomeNames name;

    public HexBiome(HexCell center, List<HexCell> borderCells)
    {
        this.center = center;
        this.borderCells = borderCells;
        name = center.biomeName;
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

    ~HexBiome()
    {
        if (borderCells != null)
            ListPool<HexCell>.Add(borderCells);
    }
}
