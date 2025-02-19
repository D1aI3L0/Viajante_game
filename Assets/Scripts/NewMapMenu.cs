using UnityEngine;

public class NewMapMenu : MonoBehaviour
{
	enum Generator
	{
		None,
		GuideGen,
		NewGen
	}

	public HexGrid hexGrid;

	Generator generator = Generator.None;
	public HexMapGenerator guideGenerator;
	public NewMapGenerator newGenerator;
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
		if (generator == Generator.GuideGen)
		{
			guideGenerator.GenerateMap(x, z, xWrapping, zWrapping);
		}
		else if (generator == Generator.NewGen)
		{
			newGenerator.GenerateMap(x, z, xWrapping, false);
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
		CreateMap(HexMetrics.chunkSizeX * 20, HexMetrics.chunkSizeZ * 16);
	}

	public void ToggleMapGeneration(int toggle)
	{
		generator = (Generator)toggle;
	}

	public void ToggleXWrapping(bool toggle)
	{
		xWrapping = toggle;
	}

	public void ToggleZWrapping(bool toggle)
	{
		zWrapping = toggle;
	}
}
