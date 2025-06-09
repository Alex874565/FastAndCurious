using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerDistanceTracker : MonoBehaviourPun
{
    private List<Transform> checkpoints;
    private int currentCheckpointIndex = 0;

    private float progress = 0f;
    private float lastSentProgress = 0f;

    private float sendInterval = 0.2f;
    private float nextSendTime = 0f;

    void Start()
    {
        var cpParent = GameObject.Find("Checkpoints");
        if (cpParent == null)
        {
            Debug.LogError("Checkpoints parent not found!");
            enabled = false;
            return;
        }

        checkpoints = new List<Transform>();
        foreach (Transform child in cpParent.transform)
        {
            checkpoints.Add(child);
        }

        if (checkpoints.Count < 2)
        {
            Debug.LogError("Not enough checkpoints found.");
            enabled = false;
            return;
        }

        // Optional: sort checkpoints by name
        checkpoints = checkpoints.OrderBy(c => c.name).ToList();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        Transform current = checkpoints[currentCheckpointIndex];
        Transform next = checkpoints[(currentCheckpointIndex + 1) % checkpoints.Count];

        float distToNext = Vector3.Distance(transform.position, next.position);
        float segmentLength = Vector3.Distance(current.position, next.position);
        float segmentProgress = 1f - Mathf.Clamp01(distToNext / segmentLength);

        float newProgress = currentCheckpointIndex + segmentProgress;

        // Update checkpoint index if passed
        if (Vector3.Distance(transform.position, next.position) < 5f)
        {
            currentCheckpointIndex = (currentCheckpointIndex + 1) % checkpoints.Count;
        }

        progress = newProgress;

        if (Time.time >= nextSendTime && Mathf.Abs(progress - lastSentProgress) > 0.01f)
        {
            UpdateDistanceProperty(progress);
            lastSentProgress = progress;
            nextSendTime = Time.time + sendInterval;
        }
    }

    private void UpdateDistanceProperty(float value)
    {
        Debug.Log($"Updating distance for player {PhotonNetwork.LocalPlayer.ActorNumber}: {value}");
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        string key = $"distance_{actorNumber}";

        ExitGames.Client.Photon.Hashtable props = new();
        props[key] = value;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}
