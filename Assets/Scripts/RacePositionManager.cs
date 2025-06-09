using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class RacePositionManager : MonoBehaviour
{
    public static RacePositionManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public int GetPlayerPosition()
    {
        var allProps = PhotonNetwork.CurrentRoom.CustomProperties;
        var distances = new List<(int actorNumber, float distance)>();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            string key = $"distance_{player.ActorNumber}";
            if (allProps.ContainsKey(key) && allProps[key] is float dist)
            {
                distances.Add((player.ActorNumber, dist));
            }
        }

        distances.Sort((a, b) => b.distance.CompareTo(a.distance)); // Descending

        for (int i = 0; i < distances.Count; i++)
        {
            if (distances[i].actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                return i + 1;
            }
        }

        return -1; // Not found
    }

}
