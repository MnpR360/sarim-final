using UnityEngine;
using static Environment_Struct;

public static class CreateMission
{
    
    public static Mission GenerateRandomMissionForRobot(string robotId)
    {
        

        Mission randomMission = new Mission();

        // Generate random id and name
        randomMission.id = Random.Range(100, 200).ToString();
        randomMission.name = "Random Mission " + randomMission.id;

        // Reference lat and lon
        float referenceLat = 25.0513380f;
        float referenceLon = 55.3779827f;

        // Generate random waypoints
        int numWaypoints = 1; //Random.Range(1, 5);
        randomMission.waypoints = new Waypoint[numWaypoints];

        for (int i = 0; i < numWaypoints; i++)
        {
            Waypoint waypoint = new Waypoint();

            // Generate random offsets for latitude and longitude
            float latOffset = Random.Range(-0.0045f, 0.0045f); // Approx. 500m in degrees
            float lonOffset = Random.Range(-0.0045f, 0.0045f); // Approx. 500m in degrees

            waypoint.lat = 25.0548f;//referenceLat + latOffset;
            waypoint.lng = 55.38237f;///referenceLon + lonOffset;

            // Generate a single robot for this waypoint
            Environment_Struct.Robot robot = new Environment_Struct.Robot();
            robot.id = robotId;
            robot.name = "Robot" + robot.id;

            waypoint.robots = new Environment_Struct.Robot[] { robot };

            randomMission.waypoints[i] = waypoint;
        }

        return randomMission;
    }


}
