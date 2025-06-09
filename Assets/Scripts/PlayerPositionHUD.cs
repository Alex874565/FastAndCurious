using Photon.Pun;
using UnityEngine;
using TMPro;

public class PlayerPositionHUD : MonoBehaviourPun
{
    private TMP_Text positionText;
    private PlayerDistanceTracker tracker;

    void Start()
    {
        if (photonView.IsMine)
        {
            tracker = gameObject.GetComponent<PlayerDistanceTracker>();

            // Find the HUD text object in the scene at runtime
            positionText = GameObject.Find("PositionText").GetComponent<TMP_Text>();
        }
        else
        {
            enabled = false; // Disable script for other players
        }
    }

    void Update()
    {
        if (tracker != null && positionText != null && RacePositionManager.Instance != null)
        {
            int place = RacePositionManager.Instance.GetPlayerPosition(tracker);
            Debug.Log($"Player place: {place}");
            positionText.text = $"Locul: {place}";

        }
    }
}
