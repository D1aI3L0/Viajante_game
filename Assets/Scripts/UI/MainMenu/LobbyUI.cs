using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button btnHost;
    [SerializeField] private Button btnJoin;
    [SerializeField] private Button btnReady;

    private void Start()
    {
        btnHost.onClick.AddListener(() => NetworkManager.singleton.StartHost());
        btnJoin.onClick.AddListener(() => NetworkManager.singleton.StartClient());
        btnReady.onClick.AddListener(() => 
        {
            NetworkPlayerController localPlayer = NetworkClient.localPlayer.GetComponent<NetworkPlayerController>();
            localPlayer.LocalSetReady(true);
        });
    }
}