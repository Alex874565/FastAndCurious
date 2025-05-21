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
    }

    public void ReportFinish(int playerID, float distance)
    {
        if (!playerDistances.ContainsKey(playerID))
            playerDistances.Add(playerID, distance);

        CheckIfAllPlayersFinished();
    }

    private void CheckIfAllPlayersFinished()
    {
        if (playerDistances.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            ShowResultsToAll();
        }
    }

    private void ShowResultsToAll()
    {
        // Sort players by distance (descending)
        var sorted = playerDistances.OrderByDescending(p => p.Value).Select(p => p.Key).ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            int actorID = sorted[i];
            photonView.RPC("RPC_SetFinalPlace", RpcTarget.All, actorID, i + 1);
        }
    }

    [PunRPC]
    private void RPC_SetFinalPlace(int actorID, int place)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorID)
        {
            PlayerBehaviour localPlayer = FindObjectOfType<PlayerBehaviour>();
            localPlayer.SetFinalPlace(place);
        }
    }
}
