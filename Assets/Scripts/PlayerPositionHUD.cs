using Photon.Pun;
using UnityEngine;
using TMPro;

public class PlayerPositionHUD : MonoBehaviourPun
{
    private TMP_Text positionText;

    void Start()
    {
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }

        GameObject textObj = GameObject.Find("PositionText");
        if (textObj != null)
            positionText = textObj.GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (positionText == null)
        {
            positionText = GameObject.Find("PositionText")?.GetComponent<TMP_Text>();
            if (positionText == null) return;
        }

        if (RacePositionManager.Instance != null)
        {
            int position = RacePositionManager.Instance.GetPlayerPosition();
            positionText.text = $"Locul: {position}";
        }
    }

}
