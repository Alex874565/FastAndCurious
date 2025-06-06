using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;

public class TrackCheckpoints : MonoBehaviour
{
    public event EventHandler OnPlayerCorrectCheckpoint;
    public event EventHandler OnPlayerWrongCheckpoint;

    [SerializeField] private string playerTag = "Player";

    [SerializeField] private GameObject endRaceCanvas;
    [SerializeField] private TMP_Text placeText;
    [SerializeField] private TMP_Text resultText;

    private List<Transform> carTransformList = new List<Transform>();
    private List<int> nextCheckpointSingleIndexList = new List<int>();
    //private List<CheckpointSingle> checkpointSingleList;
    [SerializeField] private List<CheckpointSingle> checkpointSingleList;

    public TMP_Text countdownText;


    private void Awake()
    {
        Transform checkpointsTransform = transform.Find("Checkpoints");
        //checkpointSingleList = new List<CheckpointSingle>();

        foreach (Transform checkpointSingleTransform in checkpointsTransform)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
            checkpointSingle.SetTrackCheckpoints(this);
            checkpointSingleList.Add(checkpointSingle);
        }
    }

    private void Start()
    {
        Invoke(nameof(RegisterAllPlayers), 1f); // Delay registration by 1 second
    }

    private void RegisterAllPlayers()
    {
        carTransformList.Clear();
        nextCheckpointSingleIndexList.Clear();

        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        foreach (GameObject player in players)
        {
            PhotonView view = player.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                carTransformList.Add(player.transform);
                nextCheckpointSingleIndexList.Add(0);
            }
        }

        if (carTransformList.Count == 0)
        {
            Debug.LogWarning("No player cars found with tag '" + playerTag + "'. Retrying...");
            Invoke(nameof(RegisterAllPlayers), 1f); // Keep retrying until found
        }
    }

    public void PlayerThroughCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform)
{
    int carIndex = carTransformList.IndexOf(carTransform);
    if (carIndex == -1)
    {
        Debug.LogWarning("Car not found in carTransformList: " + carTransform.name);
        return;
    }

    int nextCheckpointSingleIndex = nextCheckpointSingleIndexList[carIndex];
    int checkpointIndex = checkpointSingleList.IndexOf(checkpointSingle);

    if (checkpointIndex == nextCheckpointSingleIndex)
    {
        Debug.Log("Correct checkpoint");
        checkpointSingle.Hide();

        // Check if it's the final checkpoint
        if (nextCheckpointSingleIndex == checkpointSingleList.Count - 1)
        {
            Debug.Log("Player finished the race!");

            // STOP THE CAR
            PlayerBehaviour pb = carTransform.GetComponent<PlayerBehaviour>();
            if (pb != null)
            {
                pb.FinishRace(endRaceCanvas, placeText, resultText);
            }
        }

        nextCheckpointSingleIndexList[carIndex] = (nextCheckpointSingleIndex + 1) % checkpointSingleList.Count;
        OnPlayerCorrectCheckpoint?.Invoke(this, EventArgs.Empty);
    }
    else
    {
        Debug.Log("Wrong checkpoint");
        checkpointSingleList[nextCheckpointSingleIndex].Show();
        OnPlayerWrongCheckpoint?.Invoke(this, EventArgs.Empty);
    }
}

}
