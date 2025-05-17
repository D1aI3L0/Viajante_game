using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class SaveLoadMenu : MonoBehaviour
{
	public static SaveLoadMenu Instance;
	private const int mapFileVersion = 9;

	public GameObject UIContainer;
	public HexGrid hexGrid;
	public TMP_InputField nameInput;
	private bool saveMode;
	public TMP_Text menuLabel, actionButtonLabel;

	public RectTransform listContent;

	public SaveLoadItem itemPrefab;

	private void Awake()
	{
		Instance = this;
		Close();
	}

	public void Open(bool saveMode)
	{
		this.saveMode = saveMode;

		if (saveMode)
		{
			menuLabel.text = "Save Map";
			actionButtonLabel.text = "Save";
		}
		else
		{
			menuLabel.text = "Load Map";
			actionButtonLabel.text = "Load";
		}

		FillList();
		enabled = true;
		UIContainer.SetActive(true);
		HexMapCamera.Locked = true;
	}

	public void Close()
	{
		enabled = false;
		UIContainer.SetActive(false);
		HexMapCamera.Locked = false;
	}

	private string GetSelectedPath()
	{
		string mapName = nameInput.text;
		if (mapName.Length == 0)
		{
			return null;
		}
		return System.IO.Path.Combine(Application.persistentDataPath, mapName + ".map");
	}

	public void SaveGame(string name)
	{
		Save(System.IO.Path.Combine(Application.persistentDataPath, name + ".map"));
	}

	private void Save(string path)
	{
		using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
		{
			writer.Write(mapFileVersion);
			hexGrid.Save(writer);
			GlobalMapGameManager.Instance.Save(writer);
			RecruitingController.Instance.Save(writer);
			EnemiesController.Instance.Save(writer);
		}
	}

	public void LoadGame(string name)
	{
		Load(System.IO.Path.Combine(Application.persistentDataPath, name + ".map"));
	}

	private void Load(string path)
	{
		if (!File.Exists(path))
		{
			Debug.LogError("File does not exist " + path);
			return;
		}
		using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
		{
			int header = reader.ReadInt32();
			if (header <= mapFileVersion)
			{
				hexGrid.Load(reader, header);
				GlobalMapGameManager.Instance.Load(reader);
				RecruitingController.Instance.Load(reader);
				EnemiesController.Instance.Load(reader);
				HexMapCamera.ValidatePosition();
			}
			else
			{
				Debug.LogWarning("Unknown map format " + header);
			}
		}
	}

	public void Action()
	{
		string path = GetSelectedPath();
		if (path == null)
		{
			return;
		}
		if (saveMode)
		{
			Save(path);
		}
		else
		{
			Load(path);
		}
		Close();
	}

	public void SelectItem(string name)
	{
		nameInput.text = name;
	}

	private void FillList()
	{
		for (int i = 0; i < listContent.childCount; i++)
		{
			Destroy(listContent.GetChild(i).gameObject);
		}

		string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.map");
		Array.Sort(paths);
		for (int i = 0; i < paths.Length; i++)
		{
			SaveLoadItem item = Instantiate(itemPrefab);
			item.Initialize(this);
			item.MapName = System.IO.Path.GetFileNameWithoutExtension(paths[i]);
			item.transform.SetParent(listContent, false);
		}
	}

	public void Delete()
	{
		string path = GetSelectedPath();
		if (path == null)
		{
			return;
		}
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		nameInput.text = "";
		FillList();
	}
}