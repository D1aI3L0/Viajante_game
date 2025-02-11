using UnityEngine;

public class NewMapMenu : MonoBehaviour
{
	public HexGrid hexGrid;

	bool generateMaps = true;
	public HexMapGenerator mapGenerator;
	bool xWrapping = true, zWrapping = true;

	public void Open()
	{
		gameObject.SetActive(true);
		HexMapCamera.Locked = true;
	}

	public void Close()
	{
		gameObject.SetActive(false);
		HexMapCamera.Locked = false;
	}

	void CreateMap(int x, int z)
	{
		if (generateMaps)
		{
			mapGenerator.GenerateMap(x, z, xWrapping, zWrapping);
		}
		else
		{
			hexGrid.CreateMap(x, z, xWrapping, zWrapping);
		}
		HexMapCamera.ValidatePosition();
		Close();
	}


	public void CreateSmallMap()
	{
		CreateMap(HexMetrics.chunkSizeX * 4, HexMetrics.chunkSizeZ * 3);
	}

	public void CreateMediumMap()
	{
		CreateMap(HexMetrics.chunkSizeX * 8, HexMetrics.chunkSizeZ * 6);
	}

	public void CreateLargeMap()
	{
		CreateMap(HexMetrics.chunkSizeX * 20, HexMetrics.chunkSizeZ * 12);
	}

	public void ToggleMapGeneration(bool toggle)
	{
		generateMaps = toggle;
	}

	public void ToggleWrapping(bool toggle)
	{
		xWrapping = toggle;
		zWrapping = toggle;
	}
}
