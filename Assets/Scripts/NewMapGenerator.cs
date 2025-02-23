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

    // Проблемные сиды
    // 100209442 - HighDesert

    HexCellPriorityQueue searchFrontier;
    int searchFrontierPhase;

    BiomePattern[][] biomes =
    {
        new BiomePattern[]
        {
            new BiomePattern(BiomeNames.Desert, 8, 17), new BiomePattern(BiomeNames.Plain, 8, 16), new BiomePattern(BiomeNames.Mud, 4, 8),
            new BiomePattern(BiomeNames.StoneDesert, 7, 14), new BiomePattern(BiomeNames.Mountain, 3, 6)
        },
        new BiomePattern[]
        {
            new BiomePattern(BiomeNames.Desert, 8, 17), new BiomePattern(BiomeNames.Plain, 14, 19), new BiomePattern(BiomeNames.Mud, 10, 17),
            new BiomePattern(BiomeNames.StoneDesert, 7, 14), new BiomePattern(BiomeNames.Mountain, 6, 9)
        },
        new BiomePattern[]
        {
            new BiomePattern(BiomeNames.Desert, 8, 17), new BiomePattern(BiomeNames.Plain, 7, 12), new BiomePattern(BiomeNames.Mud, 14, 19),
            new BiomePattern(BiomeNames.StoneDesert, 5, 11), new BiomePattern(BiomeNames.Mountain, 3, 6)
        },
        new BiomePattern[]
        {
            new BiomePattern(BiomeNames.Desert, 6, 10), new BiomePattern(BiomeNames.Plain, 8, 11), new BiomePattern(BiomeNames.Mud, 4, 8),
            new BiomePattern(BiomeNames.StoneDesert, 7, 13), new BiomePattern(BiomeNames.Mountain, 9, 15)
        },
        new BiomePattern[]
        {
            new BiomePattern(BiomeNames.Desert, 11, 18), new BiomePattern(BiomeNames.Plain, 7, 12), new BiomePattern(BiomeNames.Mud, 4, 8),
            new BiomePattern(BiomeNames.StoneDesert, 9, 14), new BiomePattern(BiomeNames.Mountain, 6, 9)
        },
        new BiomePattern[]
        {
            new BiomePattern(BiomeNames.Desert, 9, 14), new BiomePattern(BiomeNames.Plain, 8, 16), new BiomePattern(BiomeNames.Mud, 4, 8),
            new BiomePattern(BiomeNames.StoneDesert, 11, 18), new BiomePattern(BiomeNames.Mountain, 6, 10)
        }
    };
    int biomesCount = 5;
    MinMaxElevation[] elevationCaps =
    {
        new MinMaxElevation(3, 5), new MinMaxElevation(3, 6), new MinMaxElevation(3, 4),
        new MinMaxElevation(5, 7), new MinMaxElevation(7, 13)
    };

    float[][] eachBiomeWeight =
    {
        new float[] {0.25f, 0.25f, 0.2f, 0.2f, 0.1f},
        new float[] {0, 0.6f, 0.15f, 0.1f, 0.15f},
        new float[] {0, 0.15f, 0.65f, 0.1f, 0.1f},
        new float[] {0.1f, 0.15f, 0, 0.15f, 0.6f},
        new float[] {0.65f, 0.1f, 0, 0.15f, 0.1f},
        new float[] {0.15f, 0.05f, 0, 0.6f, 0.2f},
    };

    public enum MapType
    {
        Standart,
        Plain,
        Swamp,
        Mountain,
        Desert,
        HighDesert
    }

    public MapType mapType = MapType.Standart;

    [Range(1, 100)]
    public int landPersentage = 50;
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

        int coveredCells = cellsCopy.Count - cellsCopy.Count * landPersentage / 100;

        while (cellsCopy.Count > coveredCells)
        {
            int b;
            float w = UnityEngine.Random.value;
            for (b = 0; b < biomesCount; b++)
            {
                w -= eachBiomeWeight[(int)mapType][b];
                if (w <= 0)
                    break;
            }
            GenerateBiome(b);
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
            int cornerDistance = UnityEngine.Random.Range(biomes[(int)mapType][biomeIndex].minRadius, biomes[(int)mapType][biomeIndex].maxRadius);
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
            HexCell corner1, corner2;
            corner1 = corner2 = corner;

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
        }

        if (borders.Contains(centerCell))
            return false;

        for (int j = 0; j < borders.Count; j++)
        {
            HexCell cell = borders[j];
            cell.biomeName = biomes[(int)mapType][biomeIndex].name;
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

        while (searchFrontier.Count > 0)
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
            cell.biomeName = biomes[(int)mapType][biomeIndex].name;
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
        biomeCells.Add(centerCell);

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
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    if (neighbor.biomeName == BiomeNames.SubBorder)
                    {
                        int b = 0;
                        for (HexDirection sd = HexDirection.NE; sd <= HexDirection.NW; sd++)
                        {
                            HexCell neighborNeighbor = neighbor.GetNeighbor(sd);
                            if (biomeCells.Contains(neighborNeighbor))
                            {
                                biomeCells.Add(neighbor);
                                neighbor.SearchPhase = searchFrontierPhase;
                                searchFrontier.Enqueue(neighbor);
                                break;
                            }
                            if (borders.Contains(neighborNeighbor))
                                b++;
                            if (b >= 4)
                            {
                                biomeCells.Add(neighbor);
                                neighbor.SearchPhase = searchFrontierPhase;
                                searchFrontier.Enqueue(neighbor);
                                break;
                            }
                        }
                    }
                    else if (neighbor.biomeName == BiomeNames.None || borders.Contains(neighbor))
                    {
                        if (neighbor.biomeName == BiomeNames.None)
                        {
                            biomeCells.Add(neighbor);
                        }
                        neighbor.SearchPhase = searchFrontierPhase;
                        searchFrontier.Enqueue(neighbor);
                    }
                }
            }

            // if (bn >= 4)
            // {
            //     Debug.Log("Wrong subborder at: " + current.coordinates.X + " " + current.coordinates.Z);
            //     current.biomeName = BiomeNames.None;
            //     current.SearchPhase -= 1;
            //     biomeCells.Add(current);
            //     // for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            //     // {
            //     //     current.GetNeighbor(d).SearchPhase -= 1;
            //     //     //searchFrontier.Enqueue( current.GetNeighbor(d));
            //     // }
            // }
        }

        foreach (HexCell cell in biomeCells)
        {
            cell.biomeName = biomes[(int)mapType][biomeIndex].name;
            if (cellsCopy.Contains(cell))
            {
                cellsCopy.Remove(cell);
            }
        }

        ListPool<HexCell>.Add(biomeCells);
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

            // Vector4 vElevation = SampleNoise(pos);
            // int noiseSource = UnityEngine.Random.Range(0,4);
            // int iElevation = (int)(Math.Sin(vElevation[noiseSource]) * 10);

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
                if (neighbor.biomeName == BiomeNames.Mountain && 1 - UnityEngine.Random.value < mountainErosionChance)
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
            if (cell.biomeName == BiomeNames.None && !cell.IsUnderwater)
                cell.biomeName = BiomeNames.SubBorder;

            if (checkWater && cell.IsUnderwater)
                CheckWater(cell);

            if (cell.biomeName == BiomeNames.SubBorder)
            {
                if (cell.IsUnderwater)
                    cell.Elevation = waterLevel;
            }

            if (cell.biomeName != BiomeNames.None)
                cell.TerrainTypeIndex = (int)cell.biomeName;
        }
    }

    public bool checkWater = false;

    void CheckWater(HexCell cell)
    {
        searchFrontierPhase += 1;
        searchFrontier.Clear();

        cell.SearchPhase = searchFrontierPhase;
        searchFrontier.Enqueue(cell);

        List<HexCell> waterCells = ListPool<HexCell>.Get();
        waterCells.Add(cell);

        while (searchFrontier.Count > 0 && waterCells.Count < 15)
        {
            HexCell current = searchFrontier.Dequeue();

            for (HexDirection d = HexDirection.NE; d < HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (!neighbor)
                {
                    continue;
                }

                if (neighbor.IsUnderwater && cell.biomeName == BiomeNames.None && neighbor.SearchPhase < searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    waterCells.Add(neighbor);
                    if (waterCells.Count >= 15)
                    {
                        ListPool<HexCell>.Add(waterCells);
                        return;
                    }
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }

        foreach (HexCell waterCell in waterCells)
        {
            waterCell.biomeName = BiomeNames.SubBorder;
        }
    }


    Vector4 SampleNoise(Vector3 position)
    {
        Vector4 sample = HexMetrics.generatorNoiseSource.GetPixelBilinear(position.x, position.z);
        return sample;
    }
}
