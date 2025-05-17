using Mirror;
using NUnit.Framework;
using UnityEngine;

namespace NetworkTests
{
    public static class SyncTestsSetup
    {
        private static GameObject networkManagerGO;
        public static NetworkManager networkManager;

        public static void Setup()
        {
            if (networkManager != null)
                TearDown();

            // Создание NetworkManager
            networkManagerGO = new GameObject("NetworkManager");
            networkManagerGO.SetActive(false);
            networkManagerGO.AddComponent<NetworkManager>();
            networkManagerGO.AddComponent<NetworkManagerHUD>();
            networkManager = networkManagerGO.GetComponent<NetworkManager>();
            var telepathyTransport = networkManagerGO.AddComponent<TelepathyTransport>();

            Transport.active = telepathyTransport;
            networkManager.transport = telepathyTransport;

            // Назначаем PlayerPrefab в NetworkManager
            networkManager.playerPrefab = Resources.Load<GameObject>("NetworkAllyMapPref");

            networkManagerGO.SetActive(true);
            networkManager.StartHost();
        }

        public static void TearDown()
        {
            if (networkManager == null)
                return;
            networkManager.StopHost();
            Object.DestroyImmediate(networkManagerGO);
        }
    }
}