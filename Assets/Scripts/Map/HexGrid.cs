using TMPro;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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

	public bool skipAutoinitialization = false;

	void Awake()
	{
		if (skipAutoinitialization)
			return;
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.InitializeHashGrid(seed);
		Squad.playerSquadPrefab = playerSquadPrefab;
		Squad.enemySquadPrefab = enemySquadPrefab;
		Base.basePrefab = basePrefab;
		CreateMap(cellCountX, cellCountZ, xWrapping, zWrapping);
	}

	void OnEnable()
	{
		if (skipAutoinitialization)
			return;
		if (!HexMetrics.noiseSource)
		{
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
			Squad.playerSquadPrefab = playerSquadPrefab;
			Squad.enemySquadPrefab = enemySquadPrefab;
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
		{
			biomes = ListPool<HexBiome>.Get();
		}
		else
		{
			biomes.Clear();
		}

		settlementsEmpty = new GameObject("Settlements").transform;
		settlementsEmpty.SetParent(transform);
		if (settlements == null)
		{
			settlements = ListPool<HexSettlement>.Get();
		}
		else
		{
			settlements.Clear();
		}

		if (squads.Count > 0)
		{
			foreach (Squad squad in squads)
			{
				squad.Die();
			}
		}
		squads.Clear();

		RemoveBase();

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

		@base.Save(writer);

		writer.Write(squads.Count);
		for (int i = 0; i < squads.Count; i++)
		{
			writer.Write((int)squads[i].squadType);
			squads[i].Save(writer);
		}
	}

	public void Load(BinaryReader reader, int header)
	{
		ClearPath();
		ClearSquads();

		int x = 40, z = 24;
		x = reader.ReadInt32();
		z = reader.ReadInt32();

		bool xWrapping = reader.ReadBoolean();
		bool zWrapping = reader.ReadBoolean();

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


		@base = Instantiate(Base.basePrefab);
		@base.Load(reader, this);
		
        Base.Instance = @base;

		int squadsCount = reader.ReadInt32();
		for (int i = 0; i < squadsCount; i++)
		{
			SquadType squadType = (SquadType)reader.ReadInt32();
			Squad newSquad = Instantiate(squadType == SquadType.Player ? Squad.playerSquadPrefab : Squad.enemySquadPrefab);
			newSquad.squadType = squadType;
			newSquad.Load(reader, this);
			squads.Add(newSquad);
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
		if (searchFrontierPhase > 100000)
			ResetSearchPhase();

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

	public void ResetSearchPhase()
	{
		for (int i = 0; i < cellsCount; i++)
		{
			cells[i].SearchPhase = 0;
			cells[i].Distance = 0;
			cells[i].SearchHeuristic = 0;
		}

		searchFrontierPhase = 0;
	}
	//============================================================================================================
	//                                               Юниты 
	//============================================================================================================
	public Squad playerSquadPrefab, enemySquadPrefab;
	public Base basePrefab;
	private List<Squad> squads = new();
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
		for (int i = 0; i < squads.Count; i++)
		{
			squads[i].Die();
		}
		squads.Clear();
	}

	public Base GetBase()
	{
		return @base;
	}

	public void AddPlayerSquad(HexCell location, float orientation, List<PlayerCharacter> characters, Base @base)
	{
		Squad squad = Instantiate(Squad.playerSquadPrefab);
		squads.Add(squad);
		squad.Initialize(@base, characters);
		squad.ResetStamina();
		squad.Grid = this;
		squad.Location = location;
		squad.Orientation = orientation;
	}

	public Squad AddEnemySquad(HexCell location, float orientation, List<EnemyCharacter> characters)
	{
		Squad squad = Instantiate(Squad.enemySquadPrefab);
		squads.Add(squad);
		squad.Initialize(characters);
		squad.Grid = this;
		squad.Location = location;
		squad.Orientation = orientation;
		return squad;
	}

	public void AddBase(HexCell location, float orientation)
	{
		if (@base)
		{
			@base.Location = location;
			@base.Orientation = orientation;
		}
		else
		{
			Base _base = Instantiate(Base.basePrefab);
			_base.Initialise();
			@base = _base;
			@base.ResetStamina();
			@base.Grid = this;
			@base.Location = location;
			@base.Orientation = orientation;
		}
	}

	public List<Squad> GetSquads(SquadType squadType)
	{
		List<Squad> squads = new();
		foreach (Unit unit in this.squads)
		{
			if (unit is Squad squad && squad.squadType == squadType)
				squads.Add(squad);
		}
		return squads;
	}

	public void RemoveSquad(Squad squad)
	{
		squads.Remove(squad);
	}

	public void RemoveBase()
	{
		if (@base)
		{
			@base.Die();
			@base = null;
		}
	}

	public void MakeChildOfChunk(Transform child, int columnIndex, int lineIndex)
	{
		child.SetParent(chunks[lineIndex * chunkCountX + columnIndex].transform, false);
	}

	public void ResetUnitsStamina()
	{
		for (int i = 0; i < squads.Count; i++)
		{
			squads[i].ResetStamina();
		}

		if (@base)
			@base.ResetStamina();
	}

	public HexCell GetAvailableEnemyCell(int minRadius, int maxRadius)
	{
		searchFrontier.Clear();

		List<HexCell> possiblePositions = ListPool<HexCell>.Get();
		List<Unit> checkUnits = new();
		checkUnits.AddRange(GetSquads(SquadType.Player));
		if (@base) checkUnits.Add(@base);

		foreach (Unit unit in checkUnits)
		{
			searchFrontierPhase += 1;
			if (searchFrontierPhase > 100000)
				ResetSearchPhase();

			unit.Location.SearchPhase = searchFrontierPhase;

			searchFrontier.Enqueue(unit.Location);

			while (searchFrontier.Count > 0)
			{
				HexCell current = searchFrontier.Dequeue();
				current.SearchPhase += 1;

				for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
				{
					HexCell neighbor = current.GetNeighbor(d);
					if (neighbor == null || neighbor.SearchPhase >= searchFrontierPhase)
						continue;

					if (neighbor.coordinates.DistanceTo(unit.Location.coordinates) > maxRadius)
						continue;

					if (neighbor.isSettlement || neighbor.HasRiver || neighbor.HasRoads || neighbor.HasUnit || neighbor.UrbanLevel > 0 || neighbor.FarmLevel > 0 || neighbor.Walled || neighbor.IsUnderwater || !neighbor.Explorable)
						continue;

					neighbor.SearchPhase = searchFrontierPhase;
					if (neighbor.coordinates.DistanceTo(unit.Location.coordinates) >= minRadius)
						possiblePositions.Add(neighbor);
					searchFrontier.Enqueue(neighbor);
				}
			}
		}

		HexCell selectedCell = null;
		if (possiblePositions.Count > 0)
			selectedCell = possiblePositions[UnityEngine.Random.Range(0, possiblePositions.Count)];
		ListPool<HexCell>.Add(possiblePositions);

		return selectedCell;
	}

	public HexCell GetAvailableBaseLocation()
	{
		List<HexCell> possiblePositions = ListPool<HexCell>.Get();

		foreach (HexCell cell in cells)
		{
			if (cell.isSettlement || cell.HasRiver || cell.HasUnit || cell.UrbanLevel > 0 || cell.FarmLevel > 0 || cell.Walled || cell.IsUnderwater || !cell.Explorable)
				continue;

			possiblePositions.Add(cell);
		}

		HexCell selectedCell = null;
		if (possiblePositions.Count > 0)
			selectedCell = possiblePositions[UnityEngine.Random.Range(0, possiblePositions.Count)];
		ListPool<HexCell>.Add(possiblePositions);

		return selectedCell;
	}

	public int GetSquadID(Squad squad)
	{
		for (int i = 0; i < squads.Count; i++)
		{
			if (squads[i] == squad)
				return i;
		}
		return -1;
	}

	public Squad GetSquadByID(int id)
	{
		if (id < 0 || id > squads.Count)
			return null;

		return squads[id];
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
		for (int i = 0; i < squads.Count; i++)
		{
			Unit unit = squads[i];
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
}