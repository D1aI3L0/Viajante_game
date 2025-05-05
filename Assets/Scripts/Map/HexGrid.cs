using TMPro;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{
	public int cellCountX = 20, cellCountZ = 15;
	private int cellsCount;

	private int chunkCountX, chunkCountZ;

	public HexCell cellPrefab;
	private HexCell[] cells;

	public TMP_Text cellLabelPrefab;

	public HexGridChunk chunkPrefab;
	private HexGridChunk[] chunks;

	public Texture2D noiseSource;
	public int seed;

	public bool xWrapping, zWrapping;

	private Transform chunksEmpty, biomesEmpty, settlementsEmpty;

	public HexBiome biomePrefab;
	private List<HexBiome> biomes;

	public HexSettlement settlementPrefab;
	private List<HexSettlement> settlements;

	void Awake()
	{
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.InitializeHashGrid(seed);
		Squad.squadPrefab = squadPrefab;
		Base.basePrefab = basePrefab;
		CreateMap(cellCountX, cellCountZ, xWrapping, zWrapping);
	}

	void OnEnable()
	{
		if (!HexMetrics.noiseSource)
		{
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
			Squad.squadPrefab = squadPrefab;
			Base.basePrefab = basePrefab;
			HexMetrics.wrapSizeX = xWrapping ? cellCountX : 0;
			HexMetrics.wrapSizeZ = zWrapping ? cellCountZ : 0;
			ResetVisibility();
		}
	}
	//============================================================================================================
	//                                          Создание карты 
	//============================================================================================================
	public bool CreateMap(int x, int z, bool xWrapping, bool zWrapping)
	{
		if (x <= 0 || x % HexMetrics.chunkSizeX != 0 || z <= 0 || z % HexMetrics.chunkSizeZ != 0 || z % 2 != 0)
		{
			Debug.LogError("Unsupported map size.");
			return false;
		}

		ClearPath();
		ClearSquads();

		if (chunksEmpty != null)
			Destroy(chunksEmpty.gameObject);
		if (biomesEmpty != null)
			Destroy(biomesEmpty.gameObject);
		if (settlementsEmpty != null)
			Destroy(settlementsEmpty.gameObject);

		cellCountX = x;
		cellCountZ = z;
		this.xWrapping = xWrapping;
		this.zWrapping = zWrapping;
		currentCenterColumnIndex = -1;
		currentCenterLineIndex = -1;
		HexMetrics.wrapSizeX = xWrapping ? cellCountX : 0;
		HexMetrics.wrapSizeZ = zWrapping ? cellCountZ : 0;
		chunkCountX = cellCountX / HexMetrics.chunkSizeX;
		chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;
		CreateChunks();
		CreateCells();

		biomesEmpty = new GameObject("Biomes").transform;
		biomesEmpty.SetParent(transform);
		if (biomes == null)
			biomes = ListPool<HexBiome>.Get();
		else
			biomes.Clear();

		settlementsEmpty = new GameObject("Settlements").transform;
		settlementsEmpty.SetParent(transform);
		if (settlements == null)
			settlements = ListPool<HexSettlement>.Get();
		else
			settlements.Clear();

		return true;
	}

	private void CreateChunks()
	{
		chunksEmpty = new GameObject("Chunks").transform;
		chunksEmpty.SetParent(transform);
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];
		for (int z = 0, i = 0; z < chunkCountZ; z++)
		{
			for (int x = 0; x < chunkCountX; x++)
			{
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(chunksEmpty);
			}
		}
	}

	private void CreateCells()
	{
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++)
		{
			for (int x = 0; x < cellCountX; x++)
			{
				CreateCell(x, z, i++);
			}
		}
	}

	private void CreateCell(int x, int z, int i)
	{
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * HexMetrics.innerDiameter;
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.Index = i;
		cell.ColumnIndex = x / HexMetrics.chunkSizeX;
		cell.LineIndex = z / HexMetrics.chunkSizeZ;

		if (xWrapping)
		{
			if (!zWrapping)
				cell.Explorable = z > 0 && z < cellCountZ - 1;
			else
				cell.Explorable = true;
		}
		else if (zWrapping)
		{
			cell.Explorable = x > 0 && x < cellCountX - 1;
		}
		else
		{
			cell.Explorable =
				x > 0 && z > 0 && x < cellCountX - 1 && z < cellCountZ - 1;
		}

		if (x > 0)
		{
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
			if (xWrapping && x == cellCountX - 1)
			{
				cell.SetNeighbor(HexDirection.E, cells[i - x]);
			}
		}
		if (z > 0)
		{
			if ((z & 1) == 0)
			{
				cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
				if (x > 0)
				{
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
				}
				else if (xWrapping)
				{
					cell.SetNeighbor(HexDirection.SW, cells[i - 1]);
				}
			}
			else
			{
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1)
				{
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
				else if (xWrapping)
				{
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX * 2 + 1]);
				}
				if (zWrapping && z == cellCountZ - 1)
				{
					cell.SetNeighbor(HexDirection.NW, cells[x]);
					if (x == cellCountX - 1)
					{
						if (xWrapping)
							cell.SetNeighbor(HexDirection.NE, cells[0]);
					}
					else
					{
						cell.SetNeighbor(HexDirection.NE, cells[x + 1]);
					}
				}
			}
		}

		TMP_Text label = Instantiate<TMP_Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		cell.uiRect = label.rectTransform;

		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);
	}

	private void AddCellToChunk(int x, int z, HexCell cell)
	{
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}

	public HexCell GetCell(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);

		if (HexMetrics.WrappingZ)
		{
			if (position.z < -1f * HexMetrics.outerRadius)
				position.z += HexMetrics.outerDiametr * cellCountZ;
			if (position.z > HexMetrics.outerDiametr * cellCountZ + HexMetrics.outerRadius)
				position.z -= HexMetrics.outerDiametr * cellCountZ;
		}
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		return GetCell(coordinates);
	}

	public HexCell GetCell(HexCoordinates coordinates)
	{
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ)
		{
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX)
		{
			return null;
		}
		return cells[x + z * cellCountX];
	}

	public HexCell GetCell(Ray ray)
	{
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			return GetCell(hit.point);
		}
		return null;
	}

	public HexCell GetCell(int xOffset, int zOffset)
	{
		return cells[xOffset + zOffset * cellCountX];
	}

	public HexCell GetCell(int cellIndex)
	{
		return cells[cellIndex];
	}

	public void ShowUI(bool visible)
	{
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].ShowUI(visible);
		}
	}
	//============================================================================================================
	//                                                 Биомы 
	//============================================================================================================
	public void AddBiome(HexCell center, List<HexCell> borders)
	{
		HexBiome biome = Instantiate(biomePrefab);
		biome.transform.SetParent(biomesEmpty);
		biome.transform.localPosition = center.Position;
		biome.center = center;
		biome.border = borders;
		biomes.Add(biome);
	}

	public HexBiome GetBiome(HexCell center)
	{
		foreach (HexBiome biome in biomes)
		{
			if (biome.center == center)
				return biome;
		}
		return null;
	}

	public HexBiome GetBiome(int index)
	{
		if (index > biomes.Count - 1)
			return null;
		return biomes[index];
	}

	public List<HexBiome> GetBiomes(BiomeName name)
	{
		List<HexBiome> searchedBiomes = ListPool<HexBiome>.Get();
		foreach (HexBiome biome in biomes)
		{
			if (biome.center.biomeName == name)
				searchedBiomes.Add(biome);
		}
		return searchedBiomes;
	}

	public List<HexBiome> GetBiomes(BiomeName name, BiomeSize size)
	{
		List<HexBiome> searchedBiomes = ListPool<HexBiome>.Get();
		foreach (HexBiome biome in biomes)
		{
			if (biome.BiomeName == name && biome.size == size)
				searchedBiomes.Add(biome);
		}
		return searchedBiomes;
	}

	public int GetBiomesCount()
	{
		return biomes.Count;
	}

	public void SetBiomesCells()
	{
		for (int i = 0; i < biomes.Count; i++)
		{
			biomes[i].SetBiomeCell();
		}
	}

	public List<HexCell> GetCells(long terrainType, int elevationMin)
	{
		List<HexCell> hexCells = ListPool<HexCell>.Get();
		for (int i = 0; i < cells.Length; i++)
		{
			if (cells[i].TerrainTypeIndex == terrainType && cells[i].Elevation >= elevationMin)
				hexCells.Add(cells[i]);
		}
		return hexCells;
	}
	//============================================================================================================
	//                                                 Биомы 
	//============================================================================================================
	public void AddSettlement(HexCell center, List<HexCell> borders, SettlementType settlementType, SettlementNation settlementNation, bool isCapital, int? index = null)
	{
		HexSettlement settlement = Instantiate(settlementPrefab);
		settlement.transform.SetParent(settlementsEmpty);
		settlement.transform.localPosition = center.Position;

		if (index != null)
			settlement.index = (int)index;
		else
			settlement.index = settlements.Count;
		settlement.center = center;
		settlement.border = borders;
		settlement.nation = settlementNation;
		settlement.type = settlementType;
		settlement.isCapital = isCapital;
		settlements.Add(settlement);
	}

	public List<HexSettlement> GetSettlements()
	{
		return settlements;
	}

	public HexSettlement GetSettlement(HexCell center)
	{
		foreach (HexSettlement settlement in settlements)
		{
			if (settlement.center == center)
				return settlement;
		}
		return null;
	}

	public HexSettlement GetSettlement(int index)
	{
		if (index > settlements.Count - 1 || index < 0)
			return null;
		return settlements[index];
	}
	//============================================================================================================
	//                                       Сохранение и загрузка 
	//============================================================================================================
	public void Save(BinaryWriter writer)
	{
		writer.Write(cellCountX);
		writer.Write(cellCountZ);
		writer.Write(xWrapping);
		writer.Write(zWrapping);

		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].Save(writer);
		}

		writer.Write(units.Count);
		for (int i = 0; i < units.Count; i++)
		{
			units[i].Save(writer);
		}

		writer.Write(biomes.Count);
		for (int i = 0; i < biomes.Count; i++)
		{
			biomes[i].Save(writer);
		}

		writer.Write(settlements.Count);
		for (int i = 0; i < settlements.Count; i++)
		{
			settlements[i].Save(writer);
		}
	}

	public void Load(BinaryReader reader, int header)
	{
		ClearPath();
		ClearSquads();

		int x = 40, z = 24;
		if (header >= 1)
		{
			x = reader.ReadInt32();
			z = reader.ReadInt32();
		}

		bool xWrapping = header >= 5 ? reader.ReadBoolean() : false;
		bool zWrapping = header >= 6 ? reader.ReadBoolean() : false;

		if (!CreateMap(x, z, xWrapping, zWrapping))
		{
			return;
		}

		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].Load(reader, header);
		}
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].Refresh();
		}

		if (header >= 2)
		{
			int unitCount = reader.ReadInt32();
			for (int i = 0; i < unitCount; i++)
			{
				Unit.Load(reader, this);
			}
		}

		if (header >= 7)
		{
			int biomesCount = reader.ReadInt32();
			for (int i = 0; i < biomesCount; i++)
			{
				HexCell center = GetCell(reader.ReadInt32());
				int borderSize = reader.ReadInt32();
				List<HexCell> border = ListPool<HexCell>.Get();
				for (int j = 0; j < borderSize; j++)
				{
					border.Add(GetCell(reader.ReadInt32()));
				}
				AddBiome(center, border);
			}
		}

		if (header >= 8)
		{
			int settlementsCount = reader.ReadInt32();
			List<List<int>> connections = new();
			for (int i = 0; i < settlementsCount; i++)
			{
				int index = reader.ReadInt32();
				HexCell center = GetCell(reader.ReadInt32());
				int borderSize = reader.ReadInt32();
				List<HexCell> border = ListPool<HexCell>.Get();
				for (int j = 0; j < borderSize; j++)
				{
					border.Add(GetCell(reader.ReadInt32()));
				}
				AddSettlement(center, border, (SettlementType)reader.ReadByte(), (SettlementNation)reader.ReadByte(), reader.ReadBoolean(), index);
				int connectedWithCount = reader.ReadInt32();
				connections.Add(new List<int>());
				for (int j = 0; j < connectedWithCount; j++)
				{
					connections[i].Add(reader.ReadInt32());
				}
			}

			for (int i = 0; i < settlementsCount; i++)
			{
				for (int j = 0; j < connections[i].Count; j++)
				{
					settlements[i].connectedWith.Add(settlements[connections[i][j]]);
				}
			}
		}
	}
	//============================================================================================================
	//                                            Поиск пути 
	//============================================================================================================
	private HexCellPriorityQueue searchFrontier = new();
	private int searchFrontierPhase;

	private HexCell currentPathFrom, currentPathTo;
	private bool currentPathExists;

	public void FindPath(HexCell fromCell, HexCell toCell, Unit unit)
	{
		ClearPath();
		currentPathExists = PathSearch(fromCell, toCell, unit);
		currentPathFrom = fromCell;
		currentPathTo = toCell;

		if (currentPathExists)
		{
			ShowPath();
		}
	}

	private bool PathSearch(HexCell fromCell, HexCell toCell, Unit unit)
	{
		searchFrontierPhase += 2;

		searchFrontier.Clear();

		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);

		while (searchFrontier.Count > 0)
		{
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;

			if (current == toCell)
			{
				return true;
			}

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				HexCell neighbor = current.GetNeighbor(d);
				if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)
					continue;

				if (neighbor != toCell && !unit.IsValidMove(neighbor))
					continue;

				int moveCost = unit.GetMoveCost(current, d);
				if (moveCost < 0)
					continue;

				int distance = current.Distance + moveCost;
				if (distance > unit.Stamina)
					continue;

				if (neighbor.SearchPhase < searchFrontierPhase)
				{
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
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
		return false;
	}

	private void ShowPath()
	{
		if (currentPathExists)
		{
			HexCell current = currentPathTo;
			while (current != currentPathFrom)
			{
				current.SetLabel(current.Distance.ToString());
				current.EnableHighlight(Color.white);
				current = current.PathFrom;
			}
			currentPathFrom.EnableHighlight(Color.blue);
			currentPathTo.EnableHighlight(Color.red);
		}
	}

	public void ClearPath()
	{
		if (currentPathExists)
		{
			HexCell current = currentPathTo;
			while (current != currentPathFrom)
			{
				current.SetLabel(null);
				current.DisableHighlight();
				current = current.PathFrom;
			}
			current.DisableHighlight();
			currentPathExists = false;
		}
		currentPathFrom = currentPathTo = null;
	}

	public List<HexCell> GetPath()
	{
		if (!currentPathExists)
		{
			return null;
		}
		List<HexCell> path = ListPool<HexCell>.Get();
		for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
		{
			path.Add(c);
		}
		path.Add(currentPathFrom);
		path.Reverse();
		return path;
	}
	//============================================================================================================
	//                                               Юниты 
	//============================================================================================================
	public Squad squadPrefab;
	public Base basePrefab;
	private List<Unit> units = new List<Unit>();
	private Base @base;

	public bool HasPath
	{
		get
		{
			return currentPathExists;
		}
	}

	private void ClearSquads()
	{
		for (int i = 0; i < units.Count; i++)
		{
			units[i].Die();
		}
		units.Clear();
	}

	public void AddPlayerSquad(Squad squad, HexCell location, float orientation, List<PlayerCharacter> characters, Base @base)
	{
		units.Add(squad);
		squad.squadType = SquadType.Player;
		squad.ResetStamina();
		squad.Grid = this;
		squad.Location = location;
		squad.Orientation = orientation;
		squad.Initialize(@base, characters);
	}

	public void AddEnemySquad(Squad squad, HexCell location, float orientation, List<EnemyCharacter> characters)
	{
		units.Add(squad);
		squad.squadType = SquadType.Enemy;
		squad.Grid = this;
		squad.Location = location;
		squad.Orientation = orientation;
		squad.Initialize(characters);
	}

	public void AddBase(Base _base, HexCell location, float orientation)
	{
		RemoveBase();
		@base = _base;
		@base.ResetStamina();
		@base.Grid = this;
		@base.Location = location;
		@base.Orientation = orientation;
		@base.Initialise();
	}

	public void RemoveSquad(Squad squad)
	{
		units.Remove(squad);
		squad.Die();
	}

	public void RemoveBase()
	{
		if (@base)
			@base.Die();
	}

	public void MakeChildOfChunk(Transform child, int columnIndex, int lineIndex)
	{
		child.SetParent(chunks[lineIndex * chunkCountX + columnIndex].transform, false);
	}

	public void ResetUnitsStamina()
	{
		for (int i = 0; i < units.Count; i++)
		{
			units[i].ResetStamina();
		}
		if (@base) @base.ResetStamina();
	}
	//============================================================================================================
	//                                          Область видимости 
	//============================================================================================================
	private List<HexCell> GetVisibleCells(HexCell fromCell, int range)
	{
		List<HexCell> visibleCells = ListPool<HexCell>.Get();

		searchFrontierPhase += 2;
		if (searchFrontier == null)
		{
			searchFrontier = new HexCellPriorityQueue();
		}
		else
		{
			searchFrontier.Clear();
		}

		range += fromCell.ViewElevation;
		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		HexCoordinates fromCoordinates = fromCell.coordinates;
		while (searchFrontier.Count > 0)
		{
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;
			visibleCells.Add(current);
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
			{
				HexCell neighbor = current.GetNeighbor(d);
				if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase || !neighbor.Explorable)
				{
					continue;
				}

				int distance = current.Distance + 1;
				if (distance + neighbor.ViewElevation > range || distance > fromCoordinates.DistanceTo(neighbor.coordinates))
				{
					continue;
				}

				if (neighbor.SearchPhase < searchFrontierPhase)
				{
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.SearchHeuristic = 0;
					searchFrontier.Enqueue(neighbor);
				}
				else if (distance < neighbor.Distance)
				{
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
		return visibleCells;
	}

	public void IncreaseVisibility(HexCell fromCell, int range)
	{
		List<HexCell> cells = GetVisibleCells(fromCell, range);
		for (int i = 0; i < cells.Count; i++)
		{
			cells[i].IncreaseVisibility();
		}
		ListPool<HexCell>.Add(cells);
	}

	public void DecreaseVisibility(HexCell fromCell, int range)
	{
		List<HexCell> cells = GetVisibleCells(fromCell, range);
		for (int i = 0; i < cells.Count; i++)
		{
			cells[i].DecreaseVisibility();
		}
		ListPool<HexCell>.Add(cells);
	}

	public void ResetVisibility()
	{
		for (int i = 0; i < cells.Length; i++)
		{
			cells[i].ResetVisibility();
		}
		for (int i = 0; i < units.Count; i++)
		{
			Unit unit = units[i];
			IncreaseVisibility(unit.Location, unit.VisionRange);
		}
	}
	//============================================================================================================
	//                                        Сворачивание карты 
	//============================================================================================================
	private int currentCenterColumnIndex = -1;
	private int currentCenterLineIndex = -1;

	public void CenterMapX(float xPosition)
	{
		int centerColumnIndex = (int)(xPosition / (HexMetrics.innerDiameter * HexMetrics.chunkSizeX));

		if (centerColumnIndex == currentCenterColumnIndex)
		{
			return;
		}
		currentCenterColumnIndex = centerColumnIndex;

		int minColumnIndex = centerColumnIndex - chunkCountX / 2;
		int maxColumnIndex = centerColumnIndex + chunkCountX / 2;

		Vector3 position;
		for (int i = 0; i < chunkCountX; i++)
		{
			if (i < minColumnIndex)
			{
				position.x = chunkCountX * (HexMetrics.innerDiameter * HexMetrics.chunkSizeX);
			}
			else if (i > maxColumnIndex)
			{
				position.x = chunkCountX * -(HexMetrics.innerDiameter * HexMetrics.chunkSizeX);
			}
			else
			{
				position.x = 0f;
			}
			for (int j = 0; j < chunkCountZ; j++)
			{
				Vector3 chunkPosition = chunks[chunkCountX * j + i].transform.localPosition;
				chunkPosition.x = position.x;
				chunks[chunkCountX * j + i].transform.localPosition = chunkPosition;
			}
		}
		CenterAll();
	}

	public void CenterMapZ(float zPosition)
	{
		int centerLineIndex = (int)(zPosition / (HexMetrics.outerDiametr * HexMetrics.chunkSizeZ));

		if (centerLineIndex == currentCenterLineIndex)
		{
			return;
		}
		currentCenterLineIndex = centerLineIndex;

		int minLineIndex = centerLineIndex - chunkCountZ / 2;
		int maxLinenIndex = centerLineIndex + chunkCountZ / 2;

		Vector3 position;
		for (int i = 0; i < chunkCountZ; i++)
		{
			if (i < minLineIndex)
			{
				position.z = chunkCountZ * (HexMetrics.outerDiametr * HexMetrics.chunkSizeZ);
			}
			else if (i > maxLinenIndex)
			{
				position.z = chunkCountZ * -(HexMetrics.outerDiametr * HexMetrics.chunkSizeZ);
			}
			else
			{
				position.z = 0f;
			}
			for (int j = 0; j < chunkCountX; j++)
			{
				Vector3 chunkPosition = chunks[chunkCountX * i + j].transform.localPosition;
				chunkPosition.z = position.z;
				chunks[chunkCountX * i + j].transform.localPosition = chunkPosition;
			}
		}
		CenterAll();
	}

	public void CenterAll()
	{
		foreach (HexBiome biome in biomes)
		{
			biome.transform.localPosition = biome.center.Position;
		}
		foreach (HexSettlement settlement in settlements)
		{
			settlement.transform.localPosition = settlement.center.Position;
		}
	}
	//============================================================================================================
}