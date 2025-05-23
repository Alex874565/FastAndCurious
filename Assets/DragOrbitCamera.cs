using UnityEngine;
using Cinemachine;

public class CameraDragController : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;
    public float dragSensitivity = 2f;
    public float returnSpeed = 2f;

    private bool isDragging = false;

    // Define default position behind car
    private float defaultX = 0f;
    private float defaultY = 0.5f;

    void Update()
    {
        if (freeLookCamera == null) return;

        // Begin dragging with right mouse
        if (Input.GetMouseButtonDown(1))
            isDragging = true;

        if (Input.GetMouseButtonUp(1))
            isDragging = false;

        if (isDragging)
        {
            float deltaX = Input.GetAxis("Mouse X");
            float deltaY = Input.GetAxis("Mouse Y");

            freeLookCamera.m_XAxis.Value += deltaX * dragSensitivity;
            freeLookCamera.m_YAxis.Value -= deltaY * dragSensitivity;
        }
        else
        {
            // Smoothly return to default values
            freeLookCamera.m_XAxis.Value = Mathf.LerpAngle(freeLookCamera.m_XAxis.Value, defaultX, Time.deltaTime * returnSpeed);
            freeLookCamera.m_YAxis.Value = Mathf.Lerp(freeLookCamera.m_YAxis.Value, defaultY, Time.deltaTime * returnSpeed);
        }
    }
}
