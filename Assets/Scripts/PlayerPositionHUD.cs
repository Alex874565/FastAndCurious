using Photon.Pun;
using UnityEngine;
using TMPro;

public class PlayerPositionHUD : MonoBehaviourPun
{
    public TMP_Text positionText;
    private PlayerDistanceTracker tracker;

    void Start()
    {
        if (photonView.IsMine)
        {
            tracker = GetComponentInParent<PlayerDistanceTracker>();
        }
        else
        {
            gameObject.SetActive(false); // ascunzi HUD-ul altor juc?tori
        }
    }

    void Update()
    {
        if (tracker != null)
        {
            int place = RacePositionManager.Instance.GetPlayerPosition(tracker);
            positionText.text = $"Locul: {place}";
        }
    }
}
