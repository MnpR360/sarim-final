
using Cinemachine;
using TMPro;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    public TextMeshProUGUI currentPosText;
    public TextMeshProUGUI currentDisText;
    public TextMeshProUGUI idText;
    public GameObject uGVsObj;

    CinemachineVirtualCamera[] virtualCameras;

    void Update()
    {
        // Iterate through all ugv objects in the uGVsObj
        foreach (Transform ugvTransform in uGVsObj.transform)
        {
            // Get the ugv components
            Transform ugv = ugvTransform.GetComponent<Transform>();
            Wheels wheels = ugvTransform.GetComponent<Wheels>();
            UGVMQTT uGVMQTT = ugvTransform.GetComponent<UGVMQTT>();

            // Get the CinemachineVirtualCamera component of the ugv
            CinemachineVirtualCamera[] ugvVirtualCameras = ugvTransform.GetComponentsInChildren<CinemachineVirtualCamera>();

            // Check if the ugv is currently being viewed by any of the virtual cameras
            bool isUgvInView = false;
            foreach (CinemachineVirtualCamera virtualCamera in ugvVirtualCameras)
            {
                if (virtualCamera.Priority == 10) // Check if the virtual camera has the highest priority (currently viewed)
                {
                    isUgvInView = true;
                    break;
                }
            }

            // Update UI values based on the ugv in view
            if (isUgvInView)
            {
                Vector2 xyCoordinates = new Vector2(wheels.transform.position.z, wheels.transform.position.x);
                Vector2 lonLat = CoordsConverter.ConvertXZToLonLat(xyCoordinates);

                ChangeCurrentPos(lonLat.x + " " + lonLat.y);
                ChangeCurrentDis(wheels.latLon.x + " " + wheels.latLon.y);
                SetID(uGVMQTT.robot1.ID);

                // Break the loop after updating UI values for the ugv in view
                break;
            }
        }
    }

    public void ChangeCurrentPos(string CP)
    {
        currentPosText.text = CP;
    }

    public void ChangeCurrentDis(string CP)
    {
        currentDisText.text = CP;
    }

    public void SetID(string id)
    {
        idText.text = id;
    }

     // Start is called before the first frame update
    void Start()
    {
        
    }

}
