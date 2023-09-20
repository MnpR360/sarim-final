using UnityEngine;
using static Cinemachine.CinemachineOrbitalTransposer;

public class Robot
{
    public string ID;
    public string Name;
    public string Status;
    public float Battery;
    public float Latitude;
    public float Longitude;
    public float Heading;
    public float PitchX;
    public float RollZ;
    public float Speed;
    public float CPUTemp;

    public int index = 0;


    public Environment_Struct.Mission currentMission;

    public Robot(string id, string name, string status, float battery,
             float latitude, float longitude, float heading, float pitchX, float rollZ, float speed, float cPUTemp)
    {
        ID = id;
        Name = name;
        Status = status;
        Battery = battery;
        Latitude = latitude;
        Longitude = longitude;
        Heading = heading;
        PitchX = pitchX;
        RollZ = rollZ;
        Speed = speed;
        CPUTemp = cPUTemp;
    }



    // Getter and Setter Methods for Properties

    public string GetID()
    {
        return ID;
    }

    public void SetID(string newID)
    {
        ID = newID;
    }

    public string GetName()
    {
        return Name;
    }

    public void SetName(string newName)
    {
        Name = newName;
    }

    public string GetStatus()
    {
        return Status;
    }

    public void SetStatus(string newStatus)
    {
        Status = newStatus;
    }

    public float GetBattery()
    {
        return Battery;
    }

    public void SetBattery(float newBattery)
    {
        Battery = newBattery;
    }

    public float GetLatitude()
    {
        return Latitude;
    }

    public void SetLatitude(float newLatitude)
    {
        Latitude = newLatitude;
    }

    public float GetLongitude()
    {
        return Longitude;
    }

    public void SetLongitude(float newLongitude)
    {
        Longitude = newLongitude;
    }

    public float GetHeading()
    {
        return Heading;
    }

    public void SetHeading(float newHeading)
    {
        Heading = newHeading;
    }

    public void SetPitchX(float pitchX)
    {
        PitchX = pitchX;
    }

    public void SetRollZ(float rollZ)
    {
        RollZ = rollZ;
    }

    public void SetSpeed(float speed)
    {
        Speed = speed;
    }

    public void SetCpuTemp(float cpuTemp)
    {
        CPUTemp = cpuTemp;
    }
    public string GetTopicNamePub()
    {
        return "sarim-ae/rover/robot-status";
    }

    public string GetTopicNameSub()
    {
        return "sarim-ae/rover/" + ID;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void UpdatePos(Vector2 lonLat, float battery, float heading, float pitchX, float rollZ, float speed, float cpuTemp)
    {
        this.SetLongitude(lonLat.y);
        this.SetLatitude(lonLat.x);
        this.SetBattery(battery);
        this.SetHeading(heading);
        this.SetPitchX(pitchX);
        this.SetRollZ(rollZ);
        this.SetSpeed(speed);
        this.SetCpuTemp(cpuTemp);
    }



    public void SetMission(Environment_Struct.Mission missionData)
    {
        currentMission = missionData;

        index = 0;


        // check if current mission is not null 

        // if null assing the current missionData to current mission 

        // if not null add on waypoints to the current list of waypoints
    }

}
