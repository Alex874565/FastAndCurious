using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RacePositionManager : MonoBehaviour
{
    public static RacePositionManager Instance;
    private List<PlayerDistanceTracker> players = new List<PlayerDistanceTracker>();

    [SerializeField] TMPro.TMP_Text positionText;

    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Instance != null) {
            if (positionText != null)
            {
                if (players.Count > 0)
                {
                    var myTracker = players.FirstOrDefault(p => p.photonView.IsMine);
                    if (myTracker != null)
                    {
                        int position = GetPlayerPosition(myTracker);
                        positionText.text = $"Position: {position}";
                    }
                }
                else
                {
                    positionText.text = "No players registered.";
                }
            }
        }
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
