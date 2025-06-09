using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Player1Name : MonoBehaviourPunCallbacks
{
    [Header("UI Reference")]
    public Text playerText; // leagă acest Text în Inspector

    [Header("Settings")]
    public int playerSlotIndex = 0; // 0 = Player 1, 1 = Player 2 etc.
    public string emptySlotPlaceholder = "Waiting for player...";

    void Start()
    {
        UpdateSlot();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateSlot();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateSlot();
    }

    public override void OnJoinedRoom()
    {
        UpdateSlot();
    }

    private void UpdateSlot()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            playerText.text = emptySlotPlaceholder;
            return;
        }

        Player[] players = PhotonNetwork.PlayerList;

        if (playerSlotIndex < players.Length)
        {
            playerText.text = $"Player {playerSlotIndex + 1}: {players[playerSlotIndex].NickName}";
        }
        else
        {
            playerText.text = emptySlotPlaceholder;
        }
    }
}
