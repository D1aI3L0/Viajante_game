using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;



public class NewMapGenerator : MonoBehaviour
{
    public HexGrid grid;
    int cellCount;

    public int seed;
    public bool useFixedSeed;

    HexCellPriorityQueue searchFrontier;
    int searchFrontierPhase;

    Biome[] biomes =
    {
        new Biome(BiomeNames.Desert, 8, 17, 0), new Biome(BiomeNames.StoneDesert, 7, 14, 3),
        new Biome(BiomeNames.Plain, 8, 16, 1), new Biome(BiomeNames.Mud, 6, 12, 2), new Biome(BiomeNames.Mountain, 2, 5, 4)
    };
    int biomesCount = 5;

    List<HexCell> cellsCopy;
    int untouchedCells;

    List<HexCell> borders;

    public void GenerateMap(int x, int z, bool xWrapping, bool zWrapping)
    {
        UnityEngine.Random.State originalRandomState = UnityEngine.Random.state;
        if (!useFixedSeed)
        {
            seed = UnityEngine.Random.Range(0, int.MaxValue);
            seed ^= (int)System.DateTime.Now.Ticks;
            seed ^= (int)Time.time;
            seed &= int.MaxValue;
        }
        UnityEngine.Random.InitState(seed);
        cellCount = x * z;
        grid.CreateMap(x, z, xWrapping, zWrapping);
        if (searchFrontier == null)
        {
            searchFrontier = new HexCellPriorityQueue();
        }
        for (int i = 0; i < cellCount; i++)
        {
            grid.GetCell(i).WaterLevel = waterLevel;
        }

        GenerateBiomes();
        GenerateElevationMap();

        for (int i = 0; i < cellCount; i++)
        {
            grid.GetCell(i).SearchHeuristic = 0;
            grid.GetCell(i).SearchPhase = 0;
        }

        for(int i = 0; i < grid.GetBiomesCount; i++)
        {
            grid.GetBiome(i).GetBiomeCells();
        }

        UnityEngine.Random.state = originalRandomState;
    }

    [Range(1f, 0.005f)]
    public float elevationScaling = 0.001f;
    [Range(0, 5)]
    public int minElevation = 0;
    [Range(5, 10)]
    public int maxElevation = 7;
    [Range(0, 3)]
    public int waterLevel = 3;

    void GenerateBiomes()
    {
        cellsCopy = ListPool<HexCell>.Get();
        for (int i = 0; i < cellCount; i++)
        {
            cellsCopy.Add(grid.GetCell(i));
        }
        untouchedCells = cellCount;
        for (int c = 0; c < 3 && untouchedCells > 0; c++)
        {
            for (int i = 0; i < biomesCount && untouchedCells > 0; i++)
            {
                GenerateBiome(i);
            }
        }
        for (int i = 0; i < cellCount; i++)
        {
            HexCell cell = grid.GetCell(i);
            if (cell.biomeName == BiomeNames.None && cell.AllNeighborsAreSubBorders)
            {
                cell.biomeName = BiomeNames.SubBorder;
            }
            if (cell.biomeName != BiomeNames.None)
            {
                if (cell.biomeName == BiomeNames.SubBorder)
                    cell.Elevation = 6;
                else if (cell.Elevation != maxElevation)
                    cell.Elevation = waterLevel + 1;
            }

        }
        ListPool<HexCell>.Add(cellsCopy);
    }

    void GenerateBiome(int biomeIndex)
    {
        int centerIndex = UnityEngine.Random.Range(0, cellsCopy.Count - 1);
        HexCell centerCell = cellsCopy[centerIndex];
        centerCell.Elevation = maxElevation;
        cellsCopy.Remove(centerCell);
        untouchedCells -= 1;

        borders = ListPool<HexCell>.Get();

        CreateBorders(borders, centerCell, biomeIndex);
        FillBiome(borders, centerCell, biomeIndex);
        CreateSubBorders(borders, centerCell.coordinates, 1);
        RefillBiome(borders, centerCell, biomeIndex);
        grid.AddBiome(centerCell, borders);
    }


    void CreateBorders(List<HexCell> borders, HexCell centerCell, int biomeIndex)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            int cornerDistance = UnityEngine.Random.Range(biomes[biomeIndex].minRadius, biomes[biomeIndex].maxRadius);
            HexCell corner = centerCell;
            for (int j = 0; j < cornerDistance; j++)
            {
                if (!corner.GetNeighbor(d))
                    break;
                corner = corner.GetNeighbor(d);
                if (corner.biomeName == BiomeNames.SubBorder)
                    break;
            }
            while (corner.biomeName != BiomeNames.None)
            {
                corner = corner.GetNeighbor(d.Opposite());
            }
            HexCell corner1 = corner;
            HexCell corner2 = corner;

            int cornerDistance1 = UnityEngine.Random.Range(1, cornerDistance/3);
            int cornerDistance2 = UnityEngine.Random.Range(1, cornerDistance/3);

            while (cornerDistance1-- > 0)
            {
                if (!corner1.GetNeighbor(d.Previous()))
                    break;
                corner1 = corner1.GetNeighbor(d.Previous());
                if (corner1.biomeName == BiomeNames.SubBorder)
                    break;
            }
            while (corner1.biomeName != BiomeNames.None)
            {
                corner1 = corner1.GetNeighbor(d.Previous().Opposite());
            }

            while (cornerDistance2-- > 0)
            {
                if (!corner2.GetNeighbor(d.Next()))
                    break;
                corner2 = corner2.GetNeighbor(d.Next());
                if (corner2.biomeName == BiomeNames.SubBorder)
                    break;
            }
            while (corner2.biomeName != BiomeNames.None)
            {
                corner2 = corner2.GetNeighbor(d.Next().Opposite());
            }
            if (!corner1)
            {
                Debug.Log("!1");
                return;
            }
            if (!corner2)
            {
                Debug.Log("!2");
                return;
            }

            if (d == HexDirection.NE)
            {
                borders.Add(corner1);
                ConnectCorners(borders, corner2, corner1);
                borders.Add(corner2);
            }
            else
            {
                ConnectCorners(borders, corner1, borders[borders.Count - 1]);
                borders.Add(corner1);
                ConnectCorners(borders, corner2, corner1);
                borders.Add(corner2);
            }
            if (d == HexDirection.NW)
            {
                ConnectCorners(borders, borders[0], corner2);
            }

            // if (d == HexDirection.NE)
            // {
            //     borders.Add(corner);
            // }
            // else
            // {
            //     ConnectCorners(borders, corner, borders[borders.Count - 1]);
            //     borders.Add(corner);
            // }
            // if (d == HexDirection.NW)
            // {
            //     ConnectCorners(borders, borders[0], corner);
            // }
        }

        for (int j = 0; j < borders.Count; j++)
        {
            HexCell cell = borders[j];
            cell.biomeName = biomes[biomeIndex].name;
            cell.TerrainTypeIndex = biomes[biomeIndex].texture;
            cellsCopy.Remove(cell);
            untouchedCells -= 1;
        }
    }

    void ConnectCorners(List<HexCell> borders, HexCell corner1, HexCell corner2)
    {
        searchFrontierPhase += 2;

        searchFrontier.Clear();

        corner1.SearchPhase = searchFrontierPhase;
        corner1.Distance = 0;
        searchFrontier.Enqueue(corner1);

        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if (current == corner2)
            {
                break;
            }

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase || neighbor.biomeName != BiomeNames.None)
                {
                    continue;
                }

                int distance = current.Distance;

                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(corner2.coordinates);
                    searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }

        HexCell currentBorder = corner2;
        while (currentBorder != corner1)
        {
            currentBorder = currentBorder.PathFrom;
            if (currentBorder == corner1)
                break;
            borders.Add(currentBorder);
        }
    }

    void CreateSubBorders(List<HexCell> borders, HexCoordinates centerCoords, int subBorderSize)
    {
        searchFrontierPhase += 1;
        foreach (HexCell border in borders)
        {
            HexCoordinates borderCoords = border.coordinates;
            int distance = borderCoords.DistanceTo(centerCoords);

            searchFrontier.Clear();
            border.SearchPhase = searchFrontierPhase;
            searchFrontier.Enqueue(border);

            while (searchFrontier.Count > 0 && untouchedCells > 0)
            {
                HexCell current = searchFrontier.Dequeue();
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    HexCell neighbor = current.GetNeighbor(d);
                    if (!neighbor || neighbor.biomeName != BiomeNames.None || borders.Contains(neighbor))
                    {
                        continue;
                    }
                    if (neighbor.coordinates.DistanceTo(borderCoords) <= subBorderSize && neighbor.SearchPhase < searchFrontierPhase)
                    {
                        if (neighbor.biomeName != BiomeNames.SubBorder)
                        {
                            neighbor.biomeName = BiomeNames.SubBorder;
                            cellsCopy.Remove(neighbor);
                            untouchedCells -= 1;
                        }
                        neighbor.SearchPhase = searchFrontierPhase;
                        searchFrontier.Enqueue(neighbor);
                    }
                }
            }
        }
    }

    void FillBiome(List<HexCell> borders, HexCell centerCell, int biomeIndex)
    {
        searchFrontierPhase += 1;
        searchFrontier.Clear();

        centerCell.SearchPhase = searchFrontierPhase;
        searchFrontier.Enqueue(centerCell);

        List<HexCell> biomeCells = ListPool<HexCell>.Get();
        biomeCells.Add(centerCell);

        while (searchFrontier.Count > 0 && untouchedCells > 0)
        {
            HexCell current = searchFrontier.Dequeue();

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (!neighbor || neighbor.biomeName != BiomeNames.None)
                {
                    continue;
                }
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    if (!borders.Contains(neighbor))
                    {
                        biomeCells.Add(neighbor);
                    }
                    neighbor.SearchPhase = searchFrontierPhase;
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }

        foreach (HexCell cell in biomeCells)
        {
            cell.biomeName = biomes[biomeIndex].name;
            cell.TerrainTypeIndex = biomes[biomeIndex].texture;
            cellsCopy.Remove(cell);
            untouchedCells -= 1;
        }
    }

    void RefillBiome(List<HexCell> borders, HexCell centerCell, int biomeIndex)
    {
        searchFrontierPhase += 1;
        searchFrontier.Clear();

        centerCell.SearchPhase = searchFrontierPhase;
        searchFrontier.Enqueue(centerCell);

        List<HexCell> biomeCells = ListPool<HexCell>.Get();

        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (!neighbor)
                {
                    continue;
                }
                if (neighbor.biomeName == BiomeNames.SubBorder && (!borders.Contains(current) || neighbor.AllNeighborsAreBiome) && neighbor.SearchPhase < searchFrontierPhase)
                {
                    biomeCells.Add(neighbor);
                    neighbor.SearchPhase = searchFrontierPhase;
                    searchFrontier.Enqueue(neighbor);
                }
                if ((neighbor.biomeName == biomes[biomeIndex].name || neighbor.biomeName == BiomeNames.None) && neighbor.SearchPhase < searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    biomeCells.Add(neighbor);
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }

        foreach (HexCell cell in biomeCells)
        {
            cell.biomeName = biomes[biomeIndex].name;
            cell.TerrainTypeIndex = biomes[biomeIndex].texture;
            if (cellsCopy.Contains(cell))
            {
                cellsCopy.Remove(cell);
                untouchedCells -= 1;
            }
        }
    }

    void GenerateElevationMap()
    {
        for (int i = 0; i < cellCount; i++)
        {
            HexCell cell = grid.GetCell(i);
            float fElevation = SampleNoise(cell.Position * elevationScaling, 0.005f)[0];

            int iElevation = (int)(Math.Sin(fElevation) * 10);
            if (iElevation > maxElevation)
                iElevation = maxElevation;
            else if (iElevation < minElevation)
                iElevation = minElevation;

            if(cell.biomeName != BiomeNames.None)
                cell.Elevation = iElevation;
        }
    }

    Vector4 SampleNoise(Vector3 position, float noiseScale)
    {
        Vector4 sample = HexMetrics.generatorNoiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
        return sample;
    }
}
