using System;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
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

    BiomePattern[] biomes =
    {
        new BiomePattern(BiomeNames.Desert, 8, 17), new BiomePattern(BiomeNames.Plain, 8, 16), new BiomePattern(BiomeNames.Mud, 4, 8),
        new BiomePattern(BiomeNames.StoneDesert, 7, 14), new BiomePattern(BiomeNames.Mountain, 2, 5)
    };
    int biomesCount = 5;

    MinMaxElevation[] elevationCaps =
    {
        new MinMaxElevation(3, 5), new MinMaxElevation(3, 6), new MinMaxElevation(3, 4),
        new MinMaxElevation(5, 7), new MinMaxElevation(7, 10)
    };

    [Range(1, 6)]
    public int iterationsCount = 3;
    [Range(2, 5)]
    public int subCornersScale = 3;

    List<HexCell> cellsCopy;

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
        ErodeLand();
        SetTerrain();

        for (int i = 0; i < cellCount; i++)
        {
            grid.GetCell(i).SearchHeuristic = 0;
            grid.GetCell(i).SearchPhase = 0;
        }

        for (int i = 0; i < grid.GetBiomesCount; i++)
        {
            grid.GetBiome(i).GetBiomeCells();
        }

        UnityEngine.Random.state = originalRandomState;
    }

    [Range(1f, 0.001f)]
    public float elevationScaling = 0.003f;
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
        for (int c = 0; c < iterationsCount && cellsCopy.Count > 0; c++)
        {
            for (int i = 0; i < biomesCount && cellsCopy.Count > 0; i++)
            {
                if (!GenerateBiome(i)) i--;
            }
        }
        for (int i = 0; i < cellCount; i++)
        {
            HexCell cell = grid.GetCell(i);
            if (cell.biomeName == BiomeNames.None && cell.AllNeighborsAreSubBorders)
            {
                cell.biomeName = BiomeNames.SubBorder;
            }
        }
        ListPool<HexCell>.Add(cellsCopy);
    }

    bool GenerateBiome(int biomeIndex)
    {
        int centerIndex = UnityEngine.Random.Range(0, cellsCopy.Count - 1);
        HexCell centerCell = cellsCopy[centerIndex];
        cellsCopy.Remove(centerCell);

        borders = ListPool<HexCell>.Get();

        if (!CreateBorders(borders, centerCell, biomeIndex))
        {
            cellsCopy.Add(centerCell);
            ListPool<HexCell>.Add(borders);
            return false;
        }
        //centerCell.Elevation = maxElevation;
        FillBiome(borders, centerCell, biomeIndex);
        CreateSubBorders(borders, centerCell.coordinates, 1);
        RefillBiome(borders, centerCell, biomeIndex);
        grid.AddBiome(centerCell, borders);
        return true;
    }


    bool CreateBorders(List<HexCell> borders, HexCell centerCell, int biomeIndex)
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

            int cornerDistance1 = UnityEngine.Random.Range(1, cornerDistance / subCornersScale);
            int cornerDistance2 = UnityEngine.Random.Range(1, cornerDistance / subCornersScale);

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

        if (borders.Contains(centerCell))
            return false;

        for (int j = 0; j < borders.Count; j++)
        {
            HexCell cell = borders[j];
            cell.biomeName = biomes[biomeIndex].name;
            cellsCopy.Remove(cell);
        }

        return true;
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

            while (searchFrontier.Count > 0 && cellsCopy.Count > 0)
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

        while (searchFrontier.Count > 0 && cellsCopy.Count > 0)
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
            cellsCopy.Remove(cell);
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
            if (cellsCopy.Contains(cell))
            {
                cellsCopy.Remove(cell);
            }
        }
    }

    void GenerateElevationMap()
    {
        for (int i = 0; i < cellCount; i++)
        {
            HexCell cell = grid.GetCell(i);
            if (cell.biomeName == BiomeNames.SubBorder)
            {
                cell.Elevation = waterLevel;
                continue;
            }
            Vector3 pos = cell.Position * elevationScaling;
            float fElevation = Mathf.PerlinNoise(pos.x, pos.z);

            int iElevation = (int)(Math.Sin(fElevation) * 10);

            if (cell.biomeName == BiomeNames.None && iElevation >= waterLevel)
            {
                iElevation = waterLevel - 1;
            }
            else if (cell.biomeName != BiomeNames.None)
            {
                if (iElevation > elevationCaps[(int)cell.biomeName].max)
                    iElevation = elevationCaps[(int)cell.biomeName].max;
                else if (iElevation < elevationCaps[(int)cell.biomeName].min)
                    iElevation = elevationCaps[(int)cell.biomeName].max;
            }

            cell.Elevation = iElevation;
        }
    }

    [Range(0, 100)]
    public int erosionPercentage = 50;
    [Range(0f, 1f)]
    public float mountainErosionChance = 0.6f;

    void ErodeLand()
    {
        List<HexCell> erodibleCells = ListPool<HexCell>.Get();
        for (int i = 0; i < cellCount; i++)
        {
            HexCell cell = grid.GetCell(i);
            if (IsErodible(cell))
            {
                erodibleCells.Add(cell);
            }
        }

        int targetErodibleCount = (int)(erodibleCells.Count * (100 - erosionPercentage) * 0.01f);

        while (erodibleCells.Count > targetErodibleCount)
        {
            int index = UnityEngine.Random.Range(0, erodibleCells.Count);
            HexCell cell = erodibleCells[index];
            HexCell targetCell = GetErosionTarget(cell);

            cell.Elevation -= 1;
            targetCell.Elevation += 1;

            if (!IsErodible(cell))
            {
                erodibleCells[index] = erodibleCells[erodibleCells.Count - 1];
                erodibleCells.RemoveAt(erodibleCells.Count - 1);
            }

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = cell.GetNeighbor(d);
                if (neighbor && neighbor && neighbor.Elevation == cell.Elevation + 2 && !erodibleCells.Contains(neighbor))
                {
                    erodibleCells.Add(neighbor);
                }
            }

            if (IsErodible(targetCell) && !erodibleCells.Contains(targetCell))
            {
                erodibleCells.Add(targetCell);
            }

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = targetCell.GetNeighbor(d);
                if (neighbor && neighbor != cell && !IsErodible(neighbor) && neighbor.Elevation == targetCell.Elevation + 1)
                {
                    erodibleCells.Remove(neighbor);
                }
            }
        }

        ListPool<HexCell>.Add(erodibleCells);
    }

    bool IsErodible(HexCell cell)
    {
        int erodibleElevation = cell.Elevation - 2;
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            HexCell neighbor = cell.GetNeighbor(d);
            if (neighbor && neighbor.Elevation <= erodibleElevation)
            {
                if (neighbor.biomeName == BiomeNames.Mountain && UnityEngine.Random.value < mountainErosionChance)
                    return false;
                return true;
            }
        }
        return false;
    }

    HexCell GetErosionTarget(HexCell cell)
    {
        List<HexCell> candidates = ListPool<HexCell>.Get();
        int erodibleElevation = cell.Elevation - 2;
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            HexCell neighbor = cell.GetNeighbor(d);
            if (neighbor && neighbor.Elevation <= erodibleElevation)
            {
                candidates.Add(neighbor);
            }
        }
        HexCell target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        ListPool<HexCell>.Add(candidates);
        return target;
    }

    void SetTerrain()
    {
        for (int i = 0; i < cellCount; i++)
        {
            HexCell cell = grid.GetCell(i);
            if ((cell.biomeName == BiomeNames.None && !cell.IsUnderwater) || (cell.IsUnderwater && cell.AllNeighborsAreSubBorders))
                cell.biomeName = BiomeNames.SubBorder;

            if (cell.biomeName == BiomeNames.SubBorder)
            {
                cell.TerrainTypeIndex = 5;
                if(cell.IsUnderwater)
                    cell.Elevation = waterLevel;
            }

            if(cell.biomeName != BiomeNames.None)
            cell.TerrainTypeIndex = (int)cell.biomeName;
        }
    }



    Vector4 SampleNoise(Vector3 position, float noiseScale)
    {
        Vector4 sample = HexMetrics.generatorNoiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
        return sample;
    }



























    List<HexCell> seeds;
    List<List<HexCell>> voronoiCells;

    void Generate()
    {
        cellsCopy = ListPool<HexCell>.Get();
        seeds = ListPool<HexCell>.Get();
        voronoiCells = ListPool<List<HexCell>>.Get();
        for (int i = 0; i < cellCount; i++)
        {
            cellsCopy.Add(grid.GetCell(i));
        }
        GenerateSeeds();
        GenerateVoronoiDiagram();
        SetBiomes();
        ListPool<HexCell>.Add(seeds);
        ListPool<HexCell>.Add(cellsCopy);
    }

    private void GenerateSeeds()
    {
        for (int i = 0; i < iterationsCount * biomesCount; i++)
        {
            HexCell cell = grid.GetCell(UnityEngine.Random.Range(0, cellsCopy.Count - 1));
            cell.biomeName = biomes[i % biomesCount].name;
            cell.Elevation = maxElevation;
            cellsCopy.Remove(cell);
            seeds.Add(cell);
        }
    }

    private void GenerateVoronoiDiagram()
    {
        for (int i = 0; i < seeds.Count; i++)
        {
            List<HexCell> cell = ListPool<HexCell>.Get();
            voronoiCells.Add(cell);
        }

        for (int x = 0; x < cellCount; x++)
        {
            HexCell cell = grid.GetCell(x);
            int closestSeedIndex = FindClosestSeedIndex(cell);
            voronoiCells[closestSeedIndex].Add(cell);
        }
    }

    private int FindClosestSeedIndex(HexCell cell)
    {
        int closestIndex = 0;
        int closestDistance = cell.coordinates.DistanceTo(seeds[0].coordinates);

        for (int i = 1; i < seeds.Count; i++)
        {
            int distance = cell.coordinates.DistanceTo(seeds[i].coordinates);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    void SetBiomes()
    {
        for (int i = 0; i < seeds.Count; i++)
        {
            HexCell centerCell = seeds[i];
            for (int j = 0; j < voronoiCells[i].Count; j++)
            {
                HexCell cell = voronoiCells[i][j];
                cell.biomeName = centerCell.biomeName;
                cell.TerrainTypeIndex = centerCell.TerrainTypeIndex;
                if (cell.Elevation != maxElevation)
                {
                    cell.Elevation = 4;
                }
            }
        }
    }

}
