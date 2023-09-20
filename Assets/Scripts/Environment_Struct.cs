using System;
using System.Collections.Generic;
using UnityEngine;

public static class Environment_Struct
{
    public struct Message
    {
        public string topic;
        public string message;

    }


    // {"id":"1692083605194","waypoints":[{"lat":37.77071576764137,"lng":-103.05822873741388},{"lat":37.55851437122679,"lng":-101.1835034403205},{"lat":38.18811487561486,"lng":-100.694611838758}]}
    [System.Serializable]
    public struct Mission
    {
        public string id;
        public string name;
        public Waypoint[] waypoints;
    }

    [System.Serializable]
    public struct Waypoint
    {
        public float lat;
        public float lng;
        public Robot[] robots;
    }


    [System.Serializable]
    public struct Robot
    {
        public string id;
        public string name;
    }






}
