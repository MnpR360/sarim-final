
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEngine;



public class UGVMQTT : MonoBehaviour
{

    public ClientM2 clientClass;
    SensorMo sensorMo;

    //these values are taken by M2MqttUnityClient class
    [Header("MQTT broker configuration")]
    public string brokerAddress = "broker.hivemq.com";
    public int brokerPort = 1883;
    public bool isEncrypted = true;
    //--------------------------------------------------

    string topicMReceive;
    public Robot robot1;
    int rID;



    float timeToSend;
    float timeToSend2;
    float waitTime = 1;
    float waitTime2 = 1;
    // Start is called before the first frame 
    void Start()
    {


        sensorMo = GetComponent<SensorMo>();
        BuildRobotRandom();

        timeToSend = Time.time + waitTime;
        timeToSend2 = Time.time + waitTime2;

        clientClass.ConnectToServer();

        StartCoroutine(WaitForConnectionAndSubscribe());





    }

    private void BuildRobotRandom()
    {
        rID = Random.Range(1, 100);// Generate a random ID between 1 and 100
        string name = "Robot" + rID;  // Generate a random name based on the ID
        float battery = Random.Range(0.0f, 1.0f);  // Generate a random battery level between 0 and 1
        sensorMo.SetBattery(battery);
        //Debug.Log(battery);

        robot1 = new Robot(rID.ToString(), name, "StandBy", battery, 0f, 0f, 0f, 0f, 0f, 0f, 80f);

        Debug.Log("sarim-ae/rover/" + rID);
        topicMReceive = "sarim-ae/rover/" + rID;
    }

    private IEnumerator WaitForConnectionAndSubscribe()
    {
        yield return new WaitForSeconds(1.0f);

        // Now that the connection is established, subscribe to robot 1's topic
        clientClass.SubscribeTopics(robot1.GetTopicNameSub());
    }


    //public Robot getRobotByIndex()



    public void Command()
    {
        List<Environment_Struct.Message> command = clientClass.GetEventMsg();


        if (command != null)
        {
            foreach (Environment_Struct.Message msg in command)
            {
                Debug.Log(msg.topic + " " + msg.message + "       topicMReceive: " + topicMReceive);

                // Check if the robot1 instance has been initialized
                if (robot1 != null)
                {
                    if (msg.topic == topicMReceive)
                    {
                        Debug.Log("topic correct");
                        Debug.Log("msgmsgmsgmsg   " + msg);
                        // Deserialize the JSON message into a Mission object
                        Environment_Struct.Mission mission = JsonUtility.FromJson<Environment_Struct.Mission>(msg.message);

                        // Create a list to store selected waypoints
                        List<Environment_Struct.Waypoint> selectedWaypoints = new List<Environment_Struct.Waypoint>();

                        foreach (Environment_Struct.Waypoint waypoint in mission.waypoints)
                        {
                            foreach (Environment_Struct.Robot robot in waypoint.robots)
                            {
                                Debug.Log("topic correct2");
                                if (robot.id == rID.ToString())
                                {
                                    Debug.Log("topic correct In");
                                    // Add the selected waypoint to the list
                                    selectedWaypoints.Add(waypoint);

                                    // You can break out of the inner loop since you've found the desired robot
                                    break;
                                }
                            }
                        }

                        // Create a new Mission object with the selected waypoints
                        Environment_Struct.Mission selectedMission = new Environment_Struct.Mission
                        {
                            id = mission.id,
                            name = mission.name,
                            waypoints = selectedWaypoints.ToArray()
                        };

                        // Convert the selected mission to JSON
                        string selectedMissionJson = JsonUtility.ToJson(selectedMission);

                        // Set the mission for the robot1 instance
                        robot1.SetMission(selectedMission);
                        robot1.SetStatus("OnGoing");


                    }
                }

            }

        }
    }




    // Update is called once per 
    void Update()
    {
        if (Time.time > timeToSend2)
        {
            clientClass.ConnectToServer();
            timeToSend2 = Time.time + waitTime2;
            
            StartCoroutine(WaitForConnectionAndSubscribe());
        }

        if (clientClass != null)
        {
            if (Time.time > timeToSend)
            {
                timeToSend = Time.time + waitTime;


                Vector2 xyCoordinates = new Vector2(transform.position.z, transform.position.x); // Replace with your own x and y coordinates
                Vector2 lonLat = CoordsConverter.ConvertXZToLonLat(xyCoordinates);
                robot1.UpdatePos(lonLat, sensorMo.GetBattery(), sensorMo.GetHeading(), sensorMo.GetPitch(), sensorMo.GetRoll(), sensorMo.GetSpeed(), sensorMo.GetCPUTemp());


                // Debug.Log(robot1.ToJson());


                clientClass.PublishMSG(robot1.GetTopicNamePub(), robot1.ToJson());
                Debug.Log("inPub");



            }

            Command();
        }
        else
            Debug.Log("null pub clientClass");
    }
}
