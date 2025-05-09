using UnityEngine;

public class NewMapMenu : MonoBehaviour
{
	public static NewMapMenu Instance;
	public GameObject UIContainer;
	public HexGrid hexGrid;
	private bool xWrapping = true, zWrapping = false;

    private void Awake()
    {
        Instance = this;
		enabled = false;
		UIContainer.SetActive(false);
    }

    public void Show()
	{
		enabled = true;
		UIContainer.SetActive(true);
		HexMapCamera.Locked = true;
	}

	public void Hide()
	{
		enabled = false;
		UIContainer.SetActive(false);
		HexMapCamera.Locked = false;
	}

	private void CreateMap(int x, int z)
	{
		NewMapGenerator.Instance.GenerateMap(x, z, xWrapping, zWrapping);
		HexMapCamera.ValidatePosition();
		Hide();
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

	public void ToggleXWrapping(bool toggle)
	{
		xWrapping = toggle;
	}

	public void ToggleZWrapping(bool toggle)
	{
		zWrapping = toggle;
	}
}
