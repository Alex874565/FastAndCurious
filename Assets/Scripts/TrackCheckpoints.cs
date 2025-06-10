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
    [SerializeField] private GameObject waitingCanvas;

    private List<Transform> carTransformList = new List<Transform>();
    private List<int> nextCheckpointSingleIndexList = new List<int>();
    //private List<CheckpointSingle> checkpointSingleList;
    [SerializeField] private List<CheckpointSingle> checkpointSingleList;

    public TMP_Text countdownText;

    private int totalLaps;  // How many laps are needed to finish

    private List<int> currentLapList = new List<int>(); // One entry per player

    [SerializeField] private TMP_Text lapText;


    private void Awake()
    {
        //checkpointSingleList = new List<CheckpointSingle>();

        foreach (CheckpointSingle checkpointSingleTransform in checkpointSingleList)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
            checkpointSingle.SetTrackCheckpoints(this);
        }

        totalLaps = GameMenuController.Laps;
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
                PlayerDistanceTracker distanceTracker = player.GetComponent<PlayerDistanceTracker>();

                carTransformList.Add(player.transform);
                nextCheckpointSingleIndexList.Add(0);

                currentLapList.Add(0); // Start at lap 0

                PlayerBehaviour pb = player.GetComponent<PlayerBehaviour>();
                if (pb != null)
                {
                    pb.SetEndRaceUI(endRaceCanvas, placeText, resultText, waitingCanvas);
                }

                lapText.text = $"Lap: 1/{totalLaps}";
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
            if (checkpointSingle.CompareTag("Finish"))
            {
                currentLapList[carIndex]++;

                Debug.Log($"Player {carTransform.name} completed lap {currentLapList[carIndex]}");
                
                if (carTransform.GetComponent<PhotonView>().IsMine)
                {
                    lapText.text = $"Lap: {currentLapList[carIndex] + 1}/{totalLaps}";
                }

                if (currentLapList[carIndex] < totalLaps)
                {
                    foreach (var checkpoint in checkpointSingleList)
                    {
                        checkpoint.ResetCheckpoint();
                    }
                }

                if (currentLapList[carIndex] >= totalLaps)
                {
                    Debug.Log("Player finished the race!");

                    PlayerBehaviour pb = carTransform.GetComponent<PlayerBehaviour>();
                    if (pb != null && pb.photonView.IsMine)
                    {
                        waitingCanvas.SetActive(true);
                        RaceResultsManager.Instance.ReportFinish(pb.photonView.Owner.ActorNumber, pb.GetDistanceTraveled());
                    }
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
