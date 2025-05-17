using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadItem : MonoBehaviour
{
    [SerializeField] private Button button;

	public string MapName
	{
		get
		{
			return mapName;
		}
		set
		{
			mapName = value;
			transform.GetChild(0).GetComponent<TMP_Text>().text = value;
		}
	}

	public void Initialize(SaveLoadMenu saveLoadMenu)
	{
		button.onClick.AddListener(() => saveLoadMenu.SelectItem(mapName)); 
	}

	public void Initialize(MainMenuUIController mainMenuUI)
	{
		button.onClick.AddListener(() => mainMenuUI.SelectGame(mapName));
	}

	private string mapName;
}
