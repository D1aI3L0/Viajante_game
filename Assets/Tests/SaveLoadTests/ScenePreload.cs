using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ScenePreload
{
    public static HexGrid grid;
    private static TaskCompletionSource<bool> sceneLoaded = new TaskCompletionSource<bool>();

    [SetUp]
    public static async Task Setup()
    {
        if (grid != null)
            return;

        GameParameters myData = Resources.Load<GameParameters>("GameParameters");
        Assert.IsNotNull(myData, "ScriptableObject не найден!");

        myData.gameName = "gg";
        myData.isNewGame = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("GlobalMapScene");

        await sceneLoaded.Task;

        grid = Object.FindFirstObjectByType<HexGrid>();
        Assert.IsNotNull(grid, "HexGrid не был найден в сцене!");
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GlobalMapScene")
        {
            sceneLoaded.SetResult(true);
        }
    }
}