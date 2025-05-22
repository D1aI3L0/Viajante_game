using Mirror;
using UnityEngine;

public class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] private GameObject playerCharacterPrefab;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName = "Player";

    [SyncVar(hook = nameof(OnReadyChanged))]
    public bool isReady = false;

    // Вызывается на клиенте при изменении имени
    private void OnNameChanged(string oldName, string newName)
    {
        Debug.Log($"Player name changed: {newName}");
    }

    // Вызывается на клиенте при изменении состояния готовности
    private void OnReadyChanged(bool oldReady, bool newReady)
    {
        Debug.Log($"Player ready: {newReady}");
    }


    // Команда для установки готовности (вызывается клиентом, выполняется на сервере)
    [Command]
    public void CmdSetReady(bool ready)
    {
        isReady = ready;
        BattleManagerMP.Instance.ServerCheckPlayersReady();
    }

    // Только для локального игрока (вызывается в UI)
    public void LocalSetReady(bool ready)
    {
        if (isLocalPlayer)
        {
            CmdSetReady(ready);
        }
    }

    [Server]
    public void SpawnPlayerCharacter(BattleCell spawnCell)
    {
        if (playerCharacterPrefab == null)
        {
            Debug.LogError("PlayerCharacterPrefab не назначен!");
            return;
        }

        // Создаём экземпляр персонажа на сервере
        GameObject playerInstance = Instantiate(
            playerCharacterPrefab,
            spawnCell.transform.position,
            Quaternion.identity
        );

        // Спавн на всех клиентах
        NetworkServer.Spawn(playerInstance, connectionToClient);

        // Настройка клетки
        BattleEntityMP entity = playerInstance.GetComponent<BattleEntityMP>();
        spawnCell.SetOccupant(entity);
        entity.currentCell = spawnCell;
    }
}