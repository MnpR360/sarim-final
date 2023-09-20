using UnityEngine;
using Cinemachine;

public class CamController : MonoBehaviour
{
    private CinemachineVirtualCamera[] virtualCameras;
    private int currentCameraIndex = 0;
    private bool isDPressed = false;
    private bool isAPressed = false;

    private const string xboxDPadHorizontalAxis = "XboxDPadHorizontalAxis";

    float timeToPressAllowed = 0f;
    float timeDiff = 1f;

    private void Start()
    {
        virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>();

        // Set the initial priorities
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            virtualCameras[i].Priority = i == 0 ? 10 : 1;
        }
    }

    private void Update()
    {
        // Handle D-pad input for camera switching or keyboard input
        if (Input.GetAxis(xboxDPadHorizontalAxis) > 0 || Input.GetKeyDown(KeyCode.X))
        {
            if (!isDPressed)
            {
                isDPressed = true;
                CycleToNextCamera();
            }
        }
        else if (Input.GetAxis(xboxDPadHorizontalAxis) < 0 || Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Working Left");
            if (!isAPressed)
            {
                isAPressed = true;
                CycleToPreviousCamera();
            }
        }

        // Handle key release
        if ((!Input.GetKeyUp(KeyCode.X) || Input.GetAxis(xboxDPadHorizontalAxis) == 0) && !(Input.GetKeyUp(KeyCode.Z)) &&  Time.time > timeToPressAllowed)
        {
            Debug.Log("false");
            isDPressed = false;
            isAPressed = false;
            timeToPressAllowed = Time.time + timeDiff; 
        }
    }

    private void CycleToNextCamera()
    {
        virtualCameras[currentCameraIndex].Priority = 1;

        currentCameraIndex = (currentCameraIndex + 1) % virtualCameras.Length;

        virtualCameras[currentCameraIndex].Priority = 10;
    }

    private void CycleToPreviousCamera()
    {
        virtualCameras[currentCameraIndex].Priority = 1;

        currentCameraIndex = (currentCameraIndex - 1 + virtualCameras.Length) % virtualCameras.Length;

        virtualCameras[currentCameraIndex].Priority = 10;
    }
}
