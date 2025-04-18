using System;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;


public struct MinMaxInt
{
    public int min, max;

    public MinMaxInt(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}


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
    List<HexCell> connection;

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

        GenerateSettlements();

        if (erode) ErodeLand();
        CheckTerrain();
        SetSubBordersTerrain();

        ConnectSettlements();

        for (int i = 0; i < cellCount; i++)
        {
            grid.GetCell(i).SearchPhase = 0;
            grid.GetCell(i).Distance = 0;
        }

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
            new BiomePattern(BiomeName.Desert, 8, 11),
            new BiomePattern(BiomeName.Plain, 8, 11),
            new BiomePattern(BiomeName.Snow, 8, 11),
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

        for (int i = 0; i < biomesCount; i++)
        {
            if (elevationCaps[i].min <= 0)
                elevationCaps[i].min = waterLevel;
        }

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
        cellsCopy.Clear();
    }


    bool GenerateBiome(int biomeIndex)
    {
        if (connection == null || connection.Count != 0)
            connection = ListPool<HexCell>.Get();

        int centerIndex = UnityEngine.Random.Range(0, cellsCopy.Count - 1);
        HexCell centerCell = cellsCopy[centerIndex];

        if (!CreateBorders(centerCell, biomePatterns[(int)mapType][biomeIndex].minMaxRadius, true, 1, new ConnectionParams(BiomeName.None, ConnectionType.BiomeBorder)))
        {
            connection.Clear();
            return false;
        }
        cellsCopy.Remove(centerCell);
        for (int i = 0; i < connection.Count; i++)
            connection[i].biomeName = biomePatterns[(int)mapType][biomeIndex].name;

        CreateSubBorders(connection);
        if (fill) FillBiome(connection, centerCell, biomeIndex);
        grid.AddBiome(centerCell, connection);
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
    public int waterLevel = 0;
    [Range(1, 14)]
    public int perlinOctaves = 3;
    [Range(-0.5f, 0.5f)]
    public float perlinPersistence = -0.3f;
    [Range(-5f, 5f)]
    public float amplitude = 1f;

    MinMaxInt[] elevationCaps =
    {
        new MinMaxInt(0, 3),
        new MinMaxInt(0, 3),
        new MinMaxInt(0, 3),
        new MinMaxInt(0, 1),
        new MinMaxInt(0, 1),
        new MinMaxInt(0, 3)
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

                int iElevation = (int)(Math.Sin(fElevation * amplitude) * 10);

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

    void CheckTerrain()
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

            if (cell.biomeName < BiomeName.SubBorder)
                cell.TerrainTypeIndex = 1 << (int)cell.biomeName;
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

    void SetSubBordersTerrain()
    {
        for (int i = 0; i < 4; i++)
        {
            List<HexCell> subBorders = grid.GetCells((long)TerrainType.None, waterLevel);
            while (subBorders.Count > 0)
            {
                HexCell cell = subBorders[UnityEngine.Random.Range(0, subBorders.Count - 1)];
                List<long> terrains = cell.GetNeigborsTerrains();
                if (terrains.Count == 1)
                    cell.TerrainTypeIndex = terrains[0];
                else if (terrains.Count == 2)
                    cell.TerrainTypeIndex = terrains[0] | terrains[1];
                subBorders.Remove(cell);
                ListPool<long>.Add(terrains);
            }
            ListPool<HexCell>.Add(subBorders);
        }
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

    [Range(4, 10)]
    public int minSettlementsDistance = 7;


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
            || (checkCell != null && checkCell.GetEdgeType(cell) == HexEdgeType.Cliff);
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
                        int settlementType = UnityEngine.Random.Range(Math.Max(j - 2, (int)SettlementType.Village), Math.Min(j - 1, (int)SettlementType.City));
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
        if (connection == null || connection.Count != 0)
            connection = ListPool<HexCell>.Get();

        int index;
        HexCell center = null;
        int iterations = 0;
        while (iterations++ < cellsCopy.Count)
        {
            index = UnityEngine.Random.Range(0, cellsCopy.Count - 1);
            center = cellsCopy[index];
            if (CheckArea(center, (int)settlementType + 2 + (isCapital ? 1 : 0), checkAreaRules, null, biome.border) && !IsSettlementNearby(center, minSettlementsDistance))
                break;
            center = null;
        }

        if (center != null)
        {
            if (!CreateBorders(center, settlementPatterns[(int)settlementType + (isCapital ? 1 : 0)].minMaxRadius, true, 0, new ConnectionParams(center.biomeName, ConnectionType.SettlementBorder)))
            {
                connection.Clear();
                return false;
            }

            center.SpecialIndex = (int)biome.BiomeName + 1;
            if (settlementType != SettlementType.Village)
                center.Walled = true;
            center.isSettlement = true;
            center.UrbanLevel = (int)settlementType + 1;
            cellsCopy.Remove(center);

            for (int i = 0; i < connection.Count; i++)
            {
                if (settlementType != SettlementType.Village)
                    connection[i].Walled = true;
                connection[i].isSettlement = true;
                connection[i].UrbanLevel = (int)settlementType + 1;
                cellsCopy.Remove(connection[i]);
            }
            FillSettlement(center, settlementType);
            biome.settlementsCount++;
            grid.AddSettlement(center, connection, settlementType, (SettlementNation)biome.BiomeName, isCapital);
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
                if (!neighbor || connection.Contains(neighbor))
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

    bool IsSettlementNearby(HexCell center, int minDistance)
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
        if (connection == null || connection.Count != 0)
            connection = ListPool<HexCell>.Get();

        connectionSkipRules = (cell, connectionParams, checkCell) =>
        {
            return (checkCell != null && checkCell.GetEdgeType(cell) == HexEdgeType.Cliff)
            || cell.IsUnderwater;
        };

        List<HexSettlement> settlements = grid.GetSettlements();

        for (int i = 0; i < settlements.Count; i++)
        {
            while (settlements[i].connectedWith.Count < 2)
            {
                HexSettlement closSett = FindClosestSettlement(settlements[i], settlements);
                if (closSett == null)
                    continue;

                ConnectCells(closSett.center, settlements[i].center, new ConnectionParams(true, ConnectionType.Roads), true);

                for (int j = 0; j < connection.Count - 1; j++)
                {
                    HexCell cell = connection[j];
                    for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                    {
                        HexCell neighbor = cell.GetNeighbor(d);
                        if (neighbor == null)
                            continue;

                        if (neighbor == connection[j + 1])
                        {
                            cell.AddRoad(d);
                            neighbor.AddRoad(d.Opposite());
                            break;
                        }
                    }
                }
                connection.Clear();
                settlements[i].connectedWith.Add(closSett);
                closSett.connectedWith.Add(settlements[i]);
            }
        }
    }


    HexSettlement FindClosestSettlement(HexSettlement settlement, List<HexSettlement> allSettlements)
    {
        HexCoordinates settCoords = settlement.center.coordinates;

        int minDistance = int.MaxValue;
        int closIndex = -1;

        for (int i = 0; i < allSettlements.Count; i++)
        {
            if (allSettlements[i] == settlement || settlement.connectedWith.Contains(allSettlements[i]))
                continue;

            int distance = settCoords.DistanceTo(allSettlements[i].center.coordinates);

            if (minDistance > distance)
            {
                minDistance = distance;
                closIndex = i;
            }
        }

        if (closIndex >= 0)
            return allSettlements[closIndex];
        return null;
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
        public bool useCliffCheck;
        public ConnectionType connectionType;

        public ConnectionParams(BiomeName biome, ConnectionType connectionType, bool useCliffCheck = false)
        {
            searchedBorderBiome = biome;
            this.useCliffCheck = useCliffCheck;
            this.connectionType = connectionType;
        }

        public ConnectionParams(bool useCliffCheck, ConnectionType connectionType, BiomeName biome = BiomeName.None)
        {
            searchedBorderBiome = biome;
            this.useCliffCheck = useCliffCheck;
            this.connectionType = connectionType;
        }
    }

    delegate bool CornerBreakRules(HexCell cell);
    CornerBreakRules cornerBreakRules;
    delegate bool CornerBackRules(HexCell cell, BiomeName biomeName);
    CornerBackRules cornerBackRules;

    enum ConnectionType
    {
        None,
        BiomeBorder,
        SettlementBorder,
        Roads

    }

    bool CreateBorders(HexCell centerCell, MinMaxInt minMaxRadius, bool useSubCorners, int subCornersScale, ConnectionParams connectionParams)
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
                    cornerDistance1 = UnityEngine.Random.Range(0, UnityEngine.Random.Range(minMaxRadius.min, minMaxRadius.max));
                    cornerDistance2 = UnityEngine.Random.Range(0, UnityEngine.Random.Range(minMaxRadius.min, minMaxRadius.max));
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
                    connection.Add(corner1);
                    ConnectCells(corner2, corner1, connectionParams);
                    connection.Add(corner2);
                }
                else
                {
                    ConnectCells(corner1, connection[^1], connectionParams);
                    connection.Add(corner1);
                    ConnectCells(corner2, corner1, connectionParams);
                    connection.Add(corner2);
                }
                if (d == HexDirection.NW)
                {
                    ConnectCells(connection[0], corner2, connectionParams);
                }
            }
            else
            {
                if (d == HexDirection.NE)
                {
                    connection.Add(corner);
                }
                else
                {
                    ConnectCells(corner, connection[^1], connectionParams);
                    connection.Add(corner);
                }
                if (d == HexDirection.NW)
                {
                    ConnectCells(connection[0], corner, connectionParams);
                }
            }
        }

        if (connection.Contains(centerCell))
            return false;

        for (int j = 0; j < connection.Count; j++)
        {
            if (connection.IndexOf(connection[j]) != connection.LastIndexOf(connection[j]))
            {
                if (j < connection.LastIndexOf(connection[j]))
                    connection.RemoveAt(connection.LastIndexOf(connection[j]));
            }
            HexCell cell = connection[j];
            cellsCopy.Remove(cell);
        }

        return true;
    }

    bool ConnectCells(HexCell cell1, HexCell cell2, ConnectionParams connectionParams, bool includeCorners = false)
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
                if (connectionSkipRules(neighbor, connectionParams, connectionParams.useCliffCheck ? current : null))
                {
                    continue;
                }

                int distance = current.Distance;
                var extraDistance = connectionParams.connectionType switch
                {
                    ConnectionType.Roads => (neighbor.HasRoads ? 0 : 4) + (UnityEngine.Random.value > 0.3f ? (int)neighbor.GetEdgeType(current) * 2 : 0),
                    _ => 0,
                };

                distance += extraDistance;

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
            if (includeCorners)
                connection.Add(cell2);
            HexCell currentCell = cell2;
            while (currentCell != cell1)
            {
                currentCell = currentCell.PathFrom;
                if (currentCell == cell1 && !includeCorners)
                    break;
                connection.Add(currentCell);
            }
            return true;
        }
        return false;
    }
}
