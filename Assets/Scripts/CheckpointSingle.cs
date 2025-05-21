using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;


public class CheckpointSingle : MonoBehaviour
{
    private TrackCheckpoints trackCheckpoints;
    private MeshRenderer meshRenderer;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject questionCanvas;
    [SerializeField] private Rigidbody carRigidbody;



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
        if (other.TryGetComponent<PlayerBehaviour>(out PlayerBehaviour player))
        {
            trackCheckpoints.PlayerThroughCheckpoint(this, other.transform);

            // Opre?te ma?ina
            carRigidbody.velocity = Vector3.zero;
            carRigidbody.isKinematic = true;

            // Afi?eaz? întrebarea
            questionCanvas.SetActive(true);
            gameManager.StartQuestion(() =>
            {
                questionCanvas.SetActive(false);
                carRigidbody.isKinematic = false;
            });
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