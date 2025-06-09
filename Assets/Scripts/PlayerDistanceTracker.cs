using Photon.Pun;
using UnityEngine;

public class PlayerDistanceTracker : MonoBehaviourPun
{
    public float DistanceTravelled { get; private set; }
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        RacePositionManager.Instance.RegisterPlayer(this); // te înregistrezi
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            DistanceTravelled = Vector3.Distance(startPosition, transform.position);
        }
    }

    void OnDestroy()
    {
        if (RacePositionManager.Instance != null)
            RacePositionManager.Instance.UnregisterPlayer(this);
    }
}
