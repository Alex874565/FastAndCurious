using UnityEngine;
using Cinemachine;

public class CameraDragController : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;
    public float rotationSpeed = 90f; // degrees per second
    public float returnSpeed = 2f;

    public float minAngle = -90f;
    public float maxAngle = 90f;

    private float currentAngle = 0f; // Degrees
    private float defaultAngle = 0f;
    private float defaultY = 0.5f;

    void Start()
    {
        if (freeLookCamera != null)
        {
            // Convert m_XAxis.Value to angle in degrees (0-360)
            currentAngle = freeLookCamera.m_XAxis.Value * 360f;
        }
    }

    void Update()
    {
        if (freeLookCamera == null) return;

        bool rotatingLeft = Input.GetKey(KeyCode.Q);
        bool rotatingRight = Input.GetKey(KeyCode.E);

        if (rotatingLeft)
        {
            currentAngle -= rotationSpeed * Time.deltaTime;
        }
        else if (rotatingRight)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
        }
        else
        {
            // Return to default angle smoothly
            currentAngle = Mathf.LerpAngle(currentAngle, defaultAngle, Time.deltaTime * returnSpeed);
            freeLookCamera.m_YAxis.Value = Mathf.Lerp(freeLookCamera.m_YAxis.Value, defaultY, Time.deltaTime * returnSpeed);
        }

        // Clamp horizontal angle
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // Convert angle to m_XAxis.Value (normalized)
        freeLookCamera.m_XAxis.Value = currentAngle;
    }
}
