
using Cinemachine;
using UnityEngine;
using static Environment_Struct;

public class Wheels : MonoBehaviour
{
    private CinemachineVirtualCamera foundVC;


    // Wheel colliders for left and right wheels
    public WheelCollider[] leftWheelColliders;
    public WheelCollider[] rightWheelColliders;

    // Visual wheels game objects
    public GameObject[] leftVisualWheels;
    public GameObject[] rightVisualWheels;
    public GameObject[] fans;


    public Vector2 latLon;
    public Vector2 destination;
    Vector2[] destFromHome = new Vector2[2];

    // Vehicle parameters
    public float motorForce = 1000f;
    public float speedLimit = 10.0f;
    public float brakeTorque = 1000f; // Adjust as needed for the braking strength
    public float maxSteerAngle = 30f;
    public float differentialFactor = 0.5f; // Adjust this value to control differential steering
    public float rotatePower = 2f;

    public float fanSpeed = 10f;


    // Flag to control user input
    public bool userInputEnabled = true;
    private bool isSpaceKeyPressed = false;
    private float lastKeyPressTime;

    public float delay = 0.5f; // Adjust the delay as needed


    float acceleration;

    float threshhold = 1.2f;


    bool autoControllFromHome = false;
    public bool exitHome = false;

    private void Start()
    {
        FindChildWithVirtualCamera(transform);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isSpaceKeyPressed || (Time.time - lastKeyPressTime) >= delay)
            {
                //SetMission To home
                //GetComponent<UGVMQTT>().robot1.SetMission(CreateMission.GenerateRandomMissionForRobot(GetComponent<UGVMQTT>().robot1.GetID()));

                isSpaceKeyPressed = true;
                userInputEnabled = !userInputEnabled;
                lastKeyPressTime = Time.time;
                //Debug.Log("hello");
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isSpaceKeyPressed = false;
        }
    }
    private void FixedUpdate()
    {


        float forceInLocalX = CalculateGravityForceInLocalX();
       // Debug.Log("Force in local X-axis: " + forceInLocalX);

        ApplyOpposingForce(forceInLocalX);

        RotateFans(fanSpeed);




        // Read user input if enabled
        if (userInputEnabled)
        {
            float verticalInput = Input.GetAxis("Vertical"); // W/S or Up/Down arrow keys
            float horizontalInput = -Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys

            // Apply motor force only if user input is enabled
            if (userInputEnabled && foundVC.Priority == 10)
            {
                ApplyMotorForce(verticalInput, horizontalInput);
            }

        }
        else
        {
            Robot robot1 = GetComponent<UGVMQTT>().robot1;

            if (robot1.currentMission.id != null)
            {

                // CoordsConverter converter = GetComponent<CoordsConverter>();
                int totalWaypoints = robot1.currentMission.waypoints.Length;
                int reversedWaypointIndex = totalWaypoints - 1 - robot1.index;

                if (reversedWaypointIndex >= 0 && reversedWaypointIndex < totalWaypoints)
                {
                    latLon = new Vector2(robot1.currentMission.waypoints[reversedWaypointIndex].lat, robot1.currentMission.waypoints[reversedWaypointIndex].lng);

                    if (!autoControllFromHome)
                    {
                        destination = CoordsConverter.ConvertLonLatToXZ(latLon);
                        Debug.Log("destination  " + destination);
                    }

                    if (!exitHome)
                    {
                        if (DistReached())
                        {
                            if (autoControllFromHome)
                            {
                                if (destination != destFromHome[1])
                                    destination = destFromHome[1];
                                else
                                    autoControllFromHome = false;
                            }
                            else
                            {
                                robot1.index++;
                            }
                        }
                    }
                    else
                    {
                        ApplyMotorForce(1, 0);
                    }


                }
                else
                {
                    HandleBraking();
                    robot1.SetStatus("StandBy");
                    robot1.currentMission = new Environment_Struct.Mission();
                    robot1.index = 0;
                }

            }
            else
            {
                //robot1.SetMission();
                HandleBraking();
            }
        }
        // Update visual wheels
        UpdateVisualWheels();
    }

    void FindChildWithVirtualCamera(Transform parent)
    {
        foreach (Transform child in parent)
        {
            CinemachineVirtualCamera vc = child.GetComponent<CinemachineVirtualCamera>();
            if (vc != null)
            {
                foundVC = vc; // Assign the found VC component
                return; // Stop searching once a child with VC is found
            }

            // If not found in this child, recursively search in its children
            FindChildWithVirtualCamera(child);
        }
    }

    bool DistReached()
    {
        if (Vector2.Distance(new Vector2(transform.position.z, transform.position.x), destination) > threshhold)

        {

            // Debug.Log(robot1.index);

            float angleIWantToGo = CalculateYawAngle(transform.position, new Vector3(destination.y, 0, destination.x)); // Replace with your desired angle
            float currentAngle = transform.eulerAngles.y;
            float angleSign = Mathf.Sign(angleIWantToGo - currentAngle);
            float angleDiffAbs = Mathf.Abs(angleIWantToGo - currentAngle);

            if (angleSign > 0 && angleDiffAbs < 180 && angleDiffAbs > 1f)
            {
                ApplyMotorForce(1, -1);
                Debug.Log("Right+");
            }
            else if (angleSign > 0 && angleDiffAbs > 180 && angleDiffAbs > 1f)
            {
                ApplyMotorForce(1, 1);
                Debug.Log("Left+");
            }
            else if (angleSign < 0 && angleDiffAbs < 180 && angleDiffAbs > 1f)
            {
                ApplyMotorForce(1, 1);
                Debug.Log("Left-");
            }
            else if (angleSign < 0 && angleDiffAbs > 180 && angleDiffAbs > 1f)
            {
                ApplyMotorForce(1, -1);
                Debug.Log("Right-");
            }
            else
                ApplyMotorForce(1, 0);
            return false;
        }
        //HandleBraking();
        return true;
    }
    public void SetAutoControllFromHome(bool autoCHome)
    {
        autoControllFromHome = autoCHome;
    }
    public void SetDestination(Vector2 []dests)
    { 
        destFromHome = new Vector2[2];

        destFromHome[0] = dests[0];
        destFromHome[1] = dests[1];

        destination = destFromHome[0];

        Debug.Log("destination  " + destination);
    }
    public float GetAcceleration()
    {
        return acceleration;
    }

    private void ApplyOpposingForce(float force)
    {
        // Calculate the opposing force
        Vector3 opposingForce = -transform.right * force; // Assuming the local x-axis is the car's right direction

        // Apply the opposing force to cancel the gravity force
        GetComponent<Rigidbody>().AddForce(opposingForce, ForceMode.Force);
    }

    private float CalculateGravityForceInLocalX()
    {
        // Get the global gravity vector
        Vector3 globalGravity = Physics.gravity;

        // Transform the global gravity vector into local space of the car
        Vector3 localGravity = transform.InverseTransformDirection(globalGravity);

        // Extract the force applied by gravity in the local x-axis
        float forceInLocalX = localGravity.x * GetComponent<Rigidbody>().mass;

        return forceInLocalX;
    }

    void HandleBraking()
    {
        //Debug.Log("BRAKE");
        float brakeTorque = 1000f; // Adjust as needed for the braking strength
        for (int i = 0; i < leftWheelColliders.Length; i++)
        {
            leftWheelColliders[i].brakeTorque = brakeTorque; // Apply brake torque
            rightWheelColliders[i].brakeTorque = brakeTorque; // Apply brake torque
        }
    }

    void RotateFans(float speed)
    {
        foreach (GameObject fan in fans)
        {
            fan.transform.Rotate(Vector3.forward, speed);
        }
    }

    // Calculate the yaw angle (y-rotation) needed to face point B from point A
    float CalculateYawAngle(Vector3 pointA, Vector3 pointB)
    {
        Vector3 directionToB = (pointB - pointA).normalized;
        float angle = Mathf.Atan2(directionToB.x, directionToB.z) * Mathf.Rad2Deg;
        if (angle < 0)
            return angle + 360;

        return angle;
    }

    // Apply motor force based on speed limit
    void ApplyMotorForce(float verticalInput, float horizontalInput)
    {
        for (int i = 0; i < leftWheelColliders.Length; i++)
        {
            leftWheelColliders[i].brakeTorque = 0; // Apply brake torque
            rightWheelColliders[i].brakeTorque = 0; // Apply brake torque
        }

        float currentSpeed = leftWheelColliders[0].rpm * (leftWheelColliders[0].radius * 2 * Mathf.PI) * 60 / 1000;

        if (Mathf.Abs(currentSpeed) < speedLimit)
        {
            // Calculate acceleration based on current speed and speed limit
            acceleration = Mathf.Lerp(motorForce, 0f, currentSpeed / speedLimit);

            for (int i = 0; i < leftWheelColliders.Length; i++)
            {
                // Adjust motor force based on terrain friction
                float adjustedMotorForce = verticalInput * acceleration * Time.deltaTime;
                leftWheelColliders[i].motorTorque = adjustedMotorForce;
                rightWheelColliders[i].motorTorque = adjustedMotorForce;
            }
        }

        // Differential control for rotation
        float rotationFactor = horizontalInput * rotatePower;

        for (int i = 0; i < leftWheelColliders.Length; i++)
        {
            if (rotationFactor > 0)
            {
                leftWheelColliders[i].motorTorque -= rotationFactor * motorForce * Time.deltaTime;
                rightWheelColliders[i].motorTorque += rotationFactor * motorForce * Time.deltaTime;
            }
            else if (rotationFactor < 0)
            {
                leftWheelColliders[i].motorTorque += Mathf.Abs(rotationFactor) * motorForce * Time.deltaTime;
                rightWheelColliders[i].motorTorque -= Mathf.Abs(rotationFactor) * motorForce * Time.deltaTime;
            }
        }
    }

    // Update visual wheels' positions and rotations
    void UpdateVisualWheels()
    {
        for (int i = 0; i < leftVisualWheels.Length; i++)
        {
            WheelCollider wheelCollider = leftWheelColliders[i];
            GameObject visualWheel = leftVisualWheels[i];

            Vector3 wheelPosition;
            Quaternion wheelRotation;
            wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);
            visualWheel.transform.position = wheelPosition;
            visualWheel.transform.rotation = wheelRotation;
        }

        for (int i = 0; i < rightVisualWheels.Length; i++)
        {
            WheelCollider wheelCollider = rightWheelColliders[i];
            GameObject visualWheel = rightVisualWheels[i];

            Vector3 wheelPosition;
            Quaternion wheelRotation;
            wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);
            visualWheel.transform.position = wheelPosition;
            visualWheel.transform.rotation = wheelRotation;
        }
    }
}
