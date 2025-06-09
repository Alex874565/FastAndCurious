using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class RaceResultsManager : MonoBehaviourPunCallbacks
{
    public static RaceResultsManager Instance;

    private Dictionary<int, float> playerDistances = new Dictionary<int, float>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Call this once per player, when they finish
    public void ReportFinish(int playerID, float distance)
    {
        if (Instance == null)
        {
            Debug.LogError("RaceResultsManager instance not found!");
            return;
        }

        // Call the RPC on this RaceResultsManager's PhotonView, sent to MasterClient only
        Instance.photonView.RPC("RPC_ReportFinish", RpcTarget.MasterClient, playerID, distance);
    }

    private void CheckIfAllPlayersFinished()
    {
        Debug.Log($"Checking if all players finished... Current count: {playerDistances.Count}, Expected: {PhotonNetwork.CurrentRoom.PlayerCount}");
        if (playerDistances.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log("All players finished! Sending results...");
            ShowResultsToAll();
        }
    }


    public void ResetResults()
    {
        playerDistances.Clear();
    }

    private List<int> finishOrder = new List<int>();

    [PunRPC]
    public void RPC_ReportFinish(int playerID, float distance)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!playerDistances.ContainsKey(playerID))
        {
            playerDistances[playerID] = distance;
            finishOrder.Add(playerID); // 👈 Save the order they finished
            Debug.Log($"Player {playerID} finished with distance {distance}");
        }
        else
        {
            Debug.LogWarning($"Player {playerID} already reported finish!");
            return;
        }

        CheckIfAllPlayersFinished();
    }

    private void ShowResultsToAll()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < finishOrder.Count; i++)
        {
            int actorID = finishOrder[i];
            int place = i + 1;

            foreach (var player in FindObjectsOfType<PlayerBehaviour>())
            {
                if (player.photonView.OwnerActorNr == actorID)
                {
                    player.photonView.RPC("RPC_FinishRaceWithPlace", player.photonView.Owner, place);
                    break;
                }
            }
        }
    }

}
