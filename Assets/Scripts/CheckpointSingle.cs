using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Unity.Properties;
using UnityEngine;


public class CheckpointSingle : MonoBehaviour
{
    private TrackCheckpoints trackCheckpoints;
    private MeshRenderer meshRenderer;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject questionCanvas;

    [HideInInspector] public bool wrongCheckpoint;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        Hide();
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerBehaviour = other.GetComponent<PlayerBehaviour>();
        if (playerBehaviour && playerBehaviour.photonView.IsMine)
        {
            trackCheckpoints.PlayerThroughCheckpoint(this, other.transform);

            if (!CompareTag("NoQuestion") && !wrongCheckpoint)
            {
                playerBehaviour.StopCar();

                questionCanvas.SetActive(true);
                gameManager.StartQuestion(() =>
                {
                    questionCanvas.SetActive(false);
                    playerBehaviour.StartCoroutine(playerBehaviour.CountdownAndStartCar(trackCheckpoints.countdownText, this));
                });
            }
        }
    }


    public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints)
    {
        this.trackCheckpoints = trackCheckpoints;
    }

    public void Show()
    {
        meshRenderer.enabled = true;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
    }
}