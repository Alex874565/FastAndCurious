using UnityEngine;
using Cinemachine;

public class CameraLookBehind : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook freeLookCam;
    [SerializeField] private float recenterSpeed = 5f;
    [SerializeField] private float lookBehindValue = 0.5f; // 180 degrees on X axis normalized (0-1)

    private bool lookBehind = false;

    void Update()
    {
        // Check if space is pressed or released
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            lookBehind = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            lookBehind = false;
        }

        if (lookBehind)
        {
            // Snap instantly or smoothly move to behind front of car
            freeLookCam.m_XAxis.Value = Mathf.Lerp(freeLookCam.m_XAxis.Value, lookBehindValue, Time.deltaTime * recenterSpeed);
        }
        else
        {
            // Smoothly go back behind the car (value 0)
            freeLookCam.m_XAxis.Value = Mathf.Lerp(freeLookCam.m_XAxis.Value, 0f, Time.deltaTime * recenterSpeed);
        }
    }
}
