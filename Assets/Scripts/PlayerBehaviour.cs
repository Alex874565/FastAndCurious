using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class PlayerBehaviour : MonoBehaviour
{
    public PhotonView photonView;
    public Rigidbody rb;
    public Animator anim;
    public GameObject playerCamera;
    public CinemachineVirtualCamera playerCinemachine;
    public Transform followPoint;
    public Renderer playerRenderer;
    public TMP_Text playerText;

    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking;
    private bool isPlayerStopped = false;
    private Vector3 savedVelocity;
    private Vector3 savedAngularVelocity;


    // Settings
    [SerializeField] private float motorForce, breakForce, maxSteerAngle;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    //private WheelControl[] wheels;

    private void Awake()
    {
        if (photonView.IsMine)
        {
            playerCamera.SetActive(true);
            gameObject.tag = "Player"; // Set the tag at runtime
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();
        playerText.text = photonView.Owner.NickName;
        playerRenderer.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        if(photonView.IsMine)
        {
            playerCinemachine.Follow = followPoint;
            playerCinemachine.LookAt = followPoint;
            playerText.color = Color.green;
        }
        else
        {
            playerText.color = Color.red;
        }
    }

    private bool hasFinished = false;

    public void FinishRace()
    {
        hasFinished = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine || !CountdownController.raceStarted || hasFinished || isPlayerStopped)
        {
            HandleIdle();
            return;
        }

        CheckInput();
    }


    private void HandleIdle()
    {
        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;
        ApplyBreaking(); // Might keep car still before GO!
    }


    private void CheckInput()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        // Steering Input
        horizontalInput = Input.GetAxis("Horizontal");

        // Acceleration Input
        verticalInput = Input.GetAxis("Vertical");

        // Breaking Input
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentbreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    public void StopCar()
    {
        savedVelocity = rb.velocity;
        savedAngularVelocity = rb.angularVelocity;

        isPlayerStopped = true;

        // Freeze all movement
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void StartCar()
    {
        Debug.Log("StartCar called");

        isPlayerStopped = false;

        rb.constraints = RigidbodyConstraints.None;

        // Restore motion
        rb.velocity = savedVelocity;
        rb.angularVelocity = savedAngularVelocity;
    }

    public IEnumerator CountdownAndStartCar(TMP_Text countdownText, CheckpointSingle checkpoint)
    {
        if (photonView.IsMine)
        {
            countdownText.transform.parent.gameObject.SetActive(true);

            int count = 1;

            // Show 3, 2, 1 with 1-second gaps
            while (count > 1)
            {
                countdownText.text = count.ToString();
                yield return new WaitForSeconds(1f);
                count--;
            }

            // Final second with milliseconds (from 1.00 to 0.00)
            float timer = 1f;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                countdownText.text = timer.ToString("F2"); // format to 2 decimal places
                yield return null;
            }

            // Show "GO!" briefly
            countdownText.text = "GO!";
            yield return new WaitForSeconds(.5f);

            countdownText.transform.parent.gameObject.SetActive(false);

            checkpoint.wrongCheckpoint = true;

            StartCar();
        }
    }


}
