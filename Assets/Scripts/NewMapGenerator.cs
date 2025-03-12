using System;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;


public class NewMapGenerator : MonoBehaviour
{
    public HexGrid grid;
    int cellCount;

    public int seed;
    public bool useFixedSeed;

    public enum MapType
    {
        Standart,
        Desert,
        Plain,
        Snow,
        Swamp,
        HighDesert
    }
    public MapType mapType = MapType.Standart;

    HexCellPriorityQueue searchFrontier;
    int searchFrontierPhase;

    List<HexCell> cellsCopy;
    List<HexCell> borders;

    public bool fill = true;


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
        grid.SetBiomesCells();

        for (int i = 0; i < cellCount; i++)
            grid.GetCell(i).SearchPhase = 0;

        GenerateSettlements();
        ConnectSettlements();

        if (erode) ErodeLand();
        SetTerrain();

        UnityEngine.Random.state = originalRandomState;
    }
    //============================================================================================================
    //                                              Биомы 
    //============================================================================================================
    static readonly int biomesCount = 5;
    readonly BiomePattern[][] biomePatterns =
    {
        new BiomePattern[]
        {
            new BiomePattern(BiomeName.Desert, 9, 12),
            new BiomePattern(BiomeName.Plain, 9, 12),
            new BiomePattern(BiomeName.Snow, 9, 12),
            new BiomePattern(BiomeName.Mud, 4, 8),
            new BiomePattern(BiomeName.StoneDesert, 7, 11)
        },
        new BiomePattern[]
        {
            new BiomePattern(BiomeName.Desert, 11, 18),
            new BiomePattern(BiomeName.Plain, 7, 12),
            new BiomePattern(BiomeName.Snow, 6, 9),
            new BiomePattern(BiomeName.Mud, 4, 8),
            new BiomePattern(BiomeName.StoneDesert, 9, 14)
        },
        new BiomePattern[]
        {
            new BiomePattern(BiomeName.Desert, 8, 17),
            new BiomePattern(BiomeName.Plain, 14, 19),
            new BiomePattern(BiomeName.Snow, 6, 9),
            new BiomePattern(BiomeName.Mud, 10, 17),
            new BiomePattern(BiomeName.StoneDesert, 7, 14)
        },
        new BiomePattern[]
        {
            new BiomePattern(BiomeName.Desert, 6, 10),
            new BiomePattern(BiomeName.Plain, 8, 11),
            new BiomePattern(BiomeName.Snow, 9, 15),
            new BiomePattern(BiomeName.Mud, 4, 8),
            new BiomePattern(BiomeName.StoneDesert, 7, 13)
        },
        new BiomePattern[]
        {
            new BiomePattern(BiomeName.Desert, 9, 14),
            new BiomePattern(BiomeName.Plain, 8, 16),
            new BiomePattern(BiomeName.Snow, 6, 10),
            new BiomePattern(BiomeName.Mud, 4, 8),
            new BiomePattern(BiomeName.StoneDesert, 11, 18)
        },
        new BiomePattern[]
        {
            new BiomePattern(BiomeName.Desert, 8, 17),
            new BiomePattern(BiomeName.Plain, 7, 12),
            new BiomePattern(BiomeName.Snow, 3, 6),
            new BiomePattern(BiomeName.Mud, 14, 19),
            new BiomePattern(BiomeName.StoneDesert, 5, 11)
        }
    };

    public float[] biomeWeights;
    public int[] biomesMaxCount;

    [Range(1, 100)]
    public int landPersentage = 50;
    [Range(3, 10)]
    public int maxSubCornersScale = 3;

    void GenerateBiomes()
    {
        connectionSkipRules = (cell, connectionParams, checkCell) => { return cell.biomeName != BiomeName.None; };
        cornerBreakRules = (cell) => { return cell.biomeName == BiomeName.SubBorder; };
        cornerBackRules = (cell, biome) => { return cell.biomeName != biome; };

        int[] curBiomesCount = new int[biomesCount];
        int totalBiomesCount = 0, totalBiomesMaxCount = 0;
        biomeWeights = new float[biomesCount];
        for (int i = 0; i < biomesCount; i++)
            totalBiomesMaxCount += biomesMaxCount[i];

        for (int i = 0; i < biomesCount; i++)
            biomeWeights[i] = biomesMaxCount[i] / (float)totalBiomesMaxCount;

        cellsCopy = ListPool<HexCell>.Get();
        for (int i = 0; i < cellCount; i++)
        {
            cellsCopy.Add(grid.GetCell(i));
        }

        int itCount = 0;
        int coveredCells = cellsCopy.Count - cellsCopy.Count * landPersentage / 100;

        while (cellsCopy.Count > coveredCells && totalBiomesCount < totalBiomesMaxCount && itCount < 10000)
        {
            itCount++;
            int b = 0;
            float w = UnityEngine.Random.value;
            while (b < biomesCount)
            {
                if ((w -= biomeWeights[b]) <= 0)
                    break;
                b++;
            }

            if (b >= biomesCount)
                continue;
            if (curBiomesCount[b] >= biomesMaxCount[b])
                continue;

            if (GenerateBiome(b))
            {
                curBiomesCount[b] += 1;
                totalBiomesCount += 1;
            }
        }
        for (int i = 0; i < cellCount; i++)
        {
            HexCell cell = grid.GetCell(i);
            if (cell.biomeName == BiomeName.None && cell.AllNeighborsAreSubBorders)
            {
                cell.biomeName = BiomeName.SubBorder;
            }
        }
        cellsCopy.Clear();
    }


    bool GenerateBiome(int biomeIndex)
    {
        if (borders == null || borders.Count != 0)
            borders = ListPool<HexCell>.Get();

        int centerIndex = UnityEngine.Random.Range(0, cellsCopy.Count - 1);
        HexCell centerCell = cellsCopy[centerIndex];

        if (!CreateBorders(centerCell, biomePatterns[(int)mapType][biomeIndex].minMaxRadius, true, 1, connectionSkipRules, new ConnectionParams(BiomeName.None), cornerBreakRules, cornerBackRules))
        {
            borders.Clear();
            return false;
        }
        cellsCopy.Remove(centerCell);
        for (int i = 0; i < borders.Count; i++)
            borders[i].biomeName = biomePatterns[(int)mapType][biomeIndex].name;
        CreateSubBorders(borders);
        if (fill) FillBiome(borders, centerCell, biomeIndex);
        grid.AddBiome(centerCell, borders);
        return true;
    }


    void CreateSubBorders(List<HexCell> borders)
    {
        foreach (HexCell border in borders)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = border.GetNeighbor(d);
                if (!neighbor || borders.Contains(neighbor))
                {
                    continue;
                }

                if (neighbor.biomeName != BiomeName.SubBorder)
                {
                    neighbor.biomeName = BiomeName.SubBorder;
                    cellsCopy.Remove(neighbor);
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
                if (!neighbor)
                {
                    continue;
                }
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    if (neighbor.biomeName == BiomeName.SubBorder)
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
                            {
                                if (++b >= 4)
                                {
                                    biomeCells.Add(neighbor);
                                    neighbor.SearchPhase = searchFrontierPhase;
                                    searchFrontier.Enqueue(neighbor);
                                    break;
                                }
                            }
                        }
                    }
                    else if (neighbor.biomeName == BiomeName.None || borders.Contains(neighbor))
                    {
                        if (neighbor.biomeName == BiomeName.None)
                        {
                            biomeCells.Add(neighbor);
                        }
                        neighbor.SearchPhase = searchFrontierPhase;
                        searchFrontier.Enqueue(neighbor);
                    }
                }
            }
        }

        foreach (HexCell cell in biomeCells)
        {
            cell.biomeName = biomePatterns[(int)mapType][biomeIndex].name;
            if (cellsCopy.Contains(cell))
            {
                cellsCopy.Remove(cell);
            }
        }

        ListPool<HexCell>.Add(biomeCells);
    }
    //============================================================================================================
    //                                               Высоты
    //============================================================================================================
    public bool elevation = true;
    [Range(1f, 0.001f)]
    public float elevationScaling = 0.003f;
    [Range(-3, 2)]
    public int minElevation = -3;
    [Range(5, 10)]
    public int maxElevation = 7;
    [Range(-3, 3)]
    public int waterLevel = -3;
    [Range(1, 14)]
    public int perlinOctaves = 3;
    [Range(-0.5f, 0.5f)]
    public float perlinPersistence = -0.3f;

    readonly MinMaxInt[] elevationCaps =
    {
        new MinMaxInt(0, 2),
        new MinMaxInt(0, 2),
        new MinMaxInt(0, 2),
        new MinMaxInt(0, 1),
        new MinMaxInt(0, 1),
        new MinMaxInt(0, 2)
    };

    public enum PerlinChoise
    {
        Integrated,
        Personal,
        PersonalWithOctaves
    }

    public PerlinChoise perlinType = PerlinChoise.PersonalWithOctaves;

    void GenerateElevationMap()
    {
        if (elevation)
        {
            Perlin perlin = new Perlin();
            Perlin2D perlin2D = new Perlin2D(seed);
            perlin.SetSeed(seed);
            for (int i = 0; i < cellCount; i++)
            {
                HexCell cell = grid.GetCell(i);

                Vector3 pos = cell.Position * elevationScaling;
                float fElevation;
                switch (perlinType)
                {
                    default:
                        fElevation = perlin.Noise(pos.x, pos.z);
                        break;
                    case PerlinChoise.Personal:
                        fElevation = perlin2D.Noise(pos.x, pos.z);
                        break;
                    case PerlinChoise.PersonalWithOctaves:
                        fElevation = perlin2D.Noise(pos.x, pos.z, perlinOctaves, perlinPersistence);
                        break;
                }

                int iElevation = (int)(Math.Sin(fElevation) * 10);

                if (cell.biomeName == BiomeName.None && iElevation >= waterLevel)
                {
                    iElevation = waterLevel - 1;
                }
                else if (cell.biomeName != BiomeName.None)
                {
                    if (iElevation > elevationCaps[(int)cell.biomeName].max)
                        iElevation = elevationCaps[(int)cell.biomeName].max;
                    else if (iElevation < elevationCaps[(int)cell.biomeName].min)
                        iElevation = elevationCaps[(int)cell.biomeName].min;
                }

                cell.Elevation = iElevation;
            }
        }
        else
        {
            for (int i = 0; i < cellCount; i++)
            {
                HexCell cell = grid.GetCell(i);
                if (cell.biomeName != BiomeName.None)
                    cell.Elevation = waterLevel;
                else
                    cell.Elevation = waterLevel - 1;
            }
        }
    }
    //============================================================================================================
    //                                               Эрозия
    //============================================================================================================
    public bool erode = true;
    [Range(0, 100)]
    public int erosionPercentage = 50;


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

            if (cell.Elevation != waterLevel)
                cell.Elevation -= 1;
            targetCell.Elevation += 1;
            if (targetCell.biomeName == BiomeName.None && !targetCell.IsUnderwater)
                targetCell.biomeName = BiomeName.SubBorder;

            if (!IsErodible(cell))
            {
                erodibleCells[index] = erodibleCells[^1];
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
    //============================================================================================================
    //                                               Рельеф
    //============================================================================================================
    public bool checkWater = false;
    [Range(1, 50)]
    public int minWaterCount = 15;

    void SetTerrain()
    {
        for (int i = 0; i < cellCount; i++)
        {
            HexCell cell = grid.GetCell(i);
            if (cell.biomeName == BiomeName.None && !cell.IsUnderwater)
            {
                cell.Elevation = waterLevel - 1;
            }
            else if (cell.biomeName < BiomeName.SubBorder && cell.IsUnderwater)
            {
                cell.Elevation = waterLevel;
            }
            if (checkWater && cell.IsUnderwater)
            {
                CheckWater(cell);
            }

            if (cell.biomeName != BiomeName.None)
                cell.TerrainTypeIndex = (int)cell.biomeName;
        }
    }

    void CheckWater(HexCell cell)
    {
        searchFrontierPhase += 1;
        searchFrontier.Clear();

        cell.SearchPhase = searchFrontierPhase;
        searchFrontier.Enqueue(cell);

        List<HexCell> waterCells = ListPool<HexCell>.Get();
        waterCells.Add(cell);

        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();

            for (HexDirection d = HexDirection.NE; d < HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (!neighbor)
                {
                    continue;
                }

                if (neighbor.IsUnderwater && cell.biomeName == BiomeName.None && neighbor.SearchPhase < searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    waterCells.Add(neighbor);
                    if (waterCells.Count >= minWaterCount)
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
            waterCell.biomeName = BiomeName.SubBorder;
            cell.Elevation = waterLevel;
        }

        ListPool<HexCell>.Add(waterCells);
    }
    //============================================================================================================
    //                                                 Поселения
    //============================================================================================================
    SettlementPattern[] settlementPatterns =
    {
        new SettlementPattern(SettlementType.Village, 1, 1),
        new SettlementPattern(SettlementType.Town, 1, 2),
        new SettlementPattern(SettlementType.City, 1, 3),
        new SettlementPattern(SettlementType.City, 2, 3),
    };

    [Range(1.5f, 3f)]
    public float minSettlementsDistanceScale = 2.5f;


    void GenerateSettlements()
    {
        checkAreaRules = (cell1, cell2, deviation) =>
        {
            return cell1.biomeName != cell2.biomeName
            || cell1.isSettlement;
        };
        connectionSkipRules = (cell, connectionParams, checkCell) =>
        {
            return cell.biomeName != connectionParams.searchedBorderBiome
            || cell.isSettlement
            || (checkCell != null && cell.Elevation - checkCell.Elevation > connectionParams.minMaxElevationDeviation.max)
            || (checkCell != null && cell.Elevation - checkCell.Elevation < connectionParams.minMaxElevationDeviation.min);
        };
        cornerBreakRules = (cell) => { return cell.biomeName == BiomeName.SubBorder || cell.isSettlement; };
        cornerBackRules = (cell, biome) => { return cell.biomeName != biome || cell.isSettlement; };

        for (int i = 0; i < 3; i++)
        {
            bool isFirst = true;
            List<HexBiome> biomes;
            for (int j = (int)BiomeSize.Large; j > (int)BiomeSize.ToSmall; j--)
            {
                biomes = grid.GetBiomes((BiomeName)i, (BiomeSize)j);

                if (isFirst)
                {
                    foreach (HexBiome biome in biomes)
                    {
                        ListPool<HexCell>.Add(cellsCopy);
                        cellsCopy = biome.GetBiomeCells(false);
                        if (GenerateSettlement(biome, SettlementType.City, true))
                        {
                            isFirst = false;
                            break;
                        }
                    }
                }

                while (biomes.Count != 0)
                {
                    ListPool<HexCell>.Add(cellsCopy);
                    cellsCopy = biomes[0].GetBiomeCells(false);
                    for (int d = 0; d < j && biomes[0].settlementsCount < j; d++)
                    {
                        int settlementType = UnityEngine.Random.Range(Math.Max(j - d - 1, (int)SettlementType.Village), Math.Min(j - 1, (int)SettlementType.City));
                        GenerateSettlement(biomes[0], (SettlementType)settlementType);
                    }
                    biomes.RemoveAt(0);
                }
                ListPool<HexBiome>.Add(biomes);
            }
        }
    }

    bool GenerateSettlement(HexBiome biome, SettlementType settlementType, bool isCapital = false)
    {
        Debug.Log((int)settlementType);
        if (borders == null || borders.Count != 0)
            borders = ListPool<HexCell>.Get();

        int index;
        HexCell center = null;
        int iterations = 0;
        while (iterations++ < cellsCopy.Count)
        {
            index = UnityEngine.Random.Range(0, cellsCopy.Count - 1);
            center = cellsCopy[index];
            if (CheckArea(center, (int)settlementType + 2 + (isCapital ? 1 : 0), checkAreaRules, null, biome.border) && !FindClosestSettlement(center, (int)(((int)settlementType + 1 + (isCapital ? 1 : 0)) * minSettlementsDistanceScale)))
                break;
            center = null;
        }

        if (center != null)
        {
            if (!CreateBorders(center, settlementPatterns[(int)settlementType + (isCapital ? 1 : 0)].minMaxRadius, false, 0, connectionSkipRules, new ConnectionParams(center.biomeName), cornerBreakRules, cornerBackRules))
            {
                borders.Clear();
                return false;
            }

            center.SpecialIndex = (int)biome.BiomeName + 1;
            if (settlementType != SettlementType.Village)
                center.Walled = true;
            center.isSettlement = true;
            center.UrbanLevel = (int)settlementType + 1;
            cellsCopy.Remove(center);

            for (int i = 0; i < borders.Count; i++)
            {
                if (settlementType != SettlementType.Village)
                    borders[i].Walled = true;
                borders[i].isSettlement = true;
                borders[i].UrbanLevel = (int)settlementType + 1;
                cellsCopy.Remove(borders[i]);
            }
            FillSettlement(center, settlementType);
            biome.settlementsCount++;
            grid.AddSettlement(center, borders, (SettlementNation)biome.BiomeName, settlementType, isCapital);
            return true;
        }
        else
            return false;
    }

    void FillSettlement(HexCell center, SettlementType settlementType)
    {
        searchFrontierPhase += 1;
        searchFrontier.Clear();

        center.SearchPhase = searchFrontierPhase;
        searchFrontier.Enqueue(center);

        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (!neighbor || borders.Contains(neighbor))
                {
                    continue;
                }
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    if (settlementType != SettlementType.Village)
                        neighbor.Walled = true;
                    neighbor.isSettlement = true;
                    neighbor.UrbanLevel = (int)settlementType + 1;
                    cellsCopy.Remove(neighbor);
                    neighbor.SearchPhase = searchFrontierPhase;
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }
    }

    bool FindClosestSettlement(HexCell center, int minDistance)
    {
        searchFrontierPhase += 1;
        searchFrontier.Clear();

        HexCoordinates centerCoords = center.coordinates;

        center.SearchPhase = searchFrontierPhase;
        searchFrontier.Enqueue(center);

        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (!neighbor || centerCoords.DistanceTo(neighbor.coordinates) > minDistance || neighbor.biomeName != center.biomeName)
                {
                    continue;
                }
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    if (neighbor.isSettlement)
                        return true;
                    neighbor.SearchPhase = searchFrontierPhase;
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }
        return false;
    }

    void ConnectSettlements()
    {

    }
    //============================================================================================================
    //                                                 Универсальные функции
    //============================================================================================================
    delegate bool CheckAreaRules(HexCell cell1, HexCell cell2, MinMaxInt? minMaxElevationVediation);
    CheckAreaRules checkAreaRules;

    bool CheckArea(HexCell center, int radius, CheckAreaRules rules, MinMaxInt? minMaxElevationDeviation, List<HexCell> border = null)
    {
        searchFrontierPhase += 1;
        searchFrontier.Clear();

        HexCoordinates centerCoords = center.coordinates;

        center.SearchPhase = searchFrontierPhase;
        searchFrontier.Enqueue(center);

        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (!neighbor || centerCoords.DistanceTo(neighbor.coordinates) > radius)
                {
                    continue;
                }
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    if (rules(neighbor, center, minMaxElevationDeviation))
                        return false;
                    if (border != null && border.Contains(neighbor))
                        return false;
                    neighbor.SearchPhase = searchFrontierPhase;
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }
        return true;
    }


    delegate bool ConnectionSkipRules(HexCell cell, ConnectionParams connectionParams, HexCell checkCell = null);
    ConnectionSkipRules connectionSkipRules;
    struct ConnectionParams
    {
        public BiomeName searchedBorderBiome;
        public MinMaxInt minMaxElevationDeviation;
        public bool useElevationCheck;

        public ConnectionParams(BiomeName biome, int minElevationDeviation = 0, int maxElevationDeviation = 0)
        {
            searchedBorderBiome = biome;
            minMaxElevationDeviation = new(minElevationDeviation, maxElevationDeviation);
            if (minElevationDeviation != 0 || maxElevationDeviation != 0)
                useElevationCheck = true;
            else
                useElevationCheck = false;
        }
    }

    delegate bool CornerBreakRules(HexCell cell);
    CornerBreakRules cornerBreakRules;
    delegate bool CornerBackRules(HexCell cell, BiomeName biomeName);
    CornerBackRules cornerBackRules;

    bool CreateBorders(HexCell centerCell, MinMaxInt minMaxRadius, bool useSubCorners, int subCornersScale, ConnectionSkipRules connectionSkipRules, ConnectionParams connectionParams, CornerBreakRules cornerBreakRules, CornerBackRules cornerBackRules)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            int cornerDistance = UnityEngine.Random.Range(minMaxRadius.min, minMaxRadius.max);
            HexCell corner = centerCell;
            for (int j = 0; j < cornerDistance; j++)
            {
                if (!corner.GetNeighbor(d))
                    break;
                corner = corner.GetNeighbor(d);
                if (cornerBreakRules(corner))
                    break;
            }
            while (cornerBackRules(corner, connectionParams.searchedBorderBiome))
            {
                corner = corner.GetNeighbor(d.Opposite());
            }

            if (useSubCorners)
            {
                HexCell corner1, corner2;
                corner1 = corner2 = corner;

                int cornerDistance1, cornerDistance2;
                if (subCornersScale > 0)
                {
                    cornerDistance1 = UnityEngine.Random.Range(1, UnityEngine.Random.Range(1, maxSubCornersScale));
                    cornerDistance2 = UnityEngine.Random.Range(1, UnityEngine.Random.Range(1, maxSubCornersScale));
                }
                else
                {
                    cornerDistance1 = UnityEngine.Random.Range(1, minMaxRadius.max);
                    cornerDistance2 = UnityEngine.Random.Range(1, minMaxRadius.max);
                }

                for (int j = 0; j < cornerDistance1; j++)
                {
                    if (!corner1.GetNeighbor(d.Previous()))
                        break;
                    corner1 = corner1.GetNeighbor(d.Previous());
                    if (cornerBreakRules(corner1))
                        break;
                }
                while (cornerBackRules(corner1, connectionParams.searchedBorderBiome))
                {
                    corner1 = corner1.GetNeighbor(d.Previous().Opposite());
                }

                for (int j = 0; j < cornerDistance2; j++)
                {
                    if (!corner2.GetNeighbor(d.Next()))
                        break;
                    corner2 = corner2.GetNeighbor(d.Next());
                    if (cornerBreakRules(corner2))
                        break;
                }
                while (cornerBackRules(corner2, connectionParams.searchedBorderBiome))
                {
                    corner2 = corner2.GetNeighbor(d.Next().Opposite());
                }

                if (d == HexDirection.NE)
                {
                    borders.Add(corner1);
                    ConnectCells(corner2, corner1, connectionSkipRules, connectionParams);
                    borders.Add(corner2);
                }
                else
                {
                    ConnectCells(corner1, borders[^1], connectionSkipRules, connectionParams);
                    borders.Add(corner1);
                    ConnectCells(corner2, corner1, connectionSkipRules, connectionParams);
                    borders.Add(corner2);
                }
                if (d == HexDirection.NW)
                {
                    ConnectCells(borders[0], corner2, connectionSkipRules, connectionParams);
                }
            }
            else
            {
                if (d == HexDirection.NE)
                {
                    borders.Add(corner);
                }
                else
                {
                    ConnectCells(corner, borders[^1], connectionSkipRules, connectionParams);
                    borders.Add(corner);
                }
                if (d == HexDirection.NW)
                {
                    ConnectCells(borders[0], corner, connectionSkipRules, connectionParams);
                }
            }
        }

        if (borders.Contains(centerCell))
            return false;

        for (int j = 0; j < borders.Count; j++)
        {
            if (borders.IndexOf(borders[j]) != borders.LastIndexOf(borders[j]))
            {
                if (j < borders.LastIndexOf(borders[j]))
                    borders.RemoveAt(borders.LastIndexOf(borders[j]));
            }
            HexCell cell = borders[j];
            cellsCopy.Remove(cell);
        }

        return true;
    }

    void ConnectCells(HexCell cell1, HexCell cell2, ConnectionSkipRules connectionSkipRules, ConnectionParams connectionParams)
    {
        searchFrontierPhase += 2;

        searchFrontier.Clear();

        cell1.SearchPhase = searchFrontierPhase;
        cell1.Distance = 0;
        searchFrontier.Enqueue(cell1);

        bool isFind = false;
        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if (current == cell2)
            {
                isFind = true;
                break;
            }

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)
                {
                    continue;
                }
                if (connectionSkipRules(neighbor, connectionParams, connectionParams.useElevationCheck ? current : null))
                {
                    continue;
                }

                int distance = current.Distance;

                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(cell2.coordinates);
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

        if (isFind)
        {
            HexCell currentBorder = cell2;
            while (currentBorder != cell1)
            {
                currentBorder = currentBorder.PathFrom;
                if (currentBorder == cell1)
                    break;
                borders.Add(currentBorder);
            }
            return;
        }
    }
}
