using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RacePositionManager : MonoBehaviour
{
    public static RacePositionManager Instance;
    private List<PlayerDistanceTracker> players = new List<PlayerDistanceTracker>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterPlayer(PlayerDistanceTracker tracker)
    {
        if (!players.Contains(tracker))
            players.Add(tracker);
    }

    public void UnregisterPlayer(PlayerDistanceTracker tracker)
    {
        players.Remove(tracker);
    }

    public int GetPlayerPosition(PlayerDistanceTracker myTracker)
    {
        var sorted = players.OrderByDescending(p => p.DistanceTravelled).ToList();
        return sorted.IndexOf(myTracker) + 1;
    }
}
