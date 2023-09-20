using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SensorMo : MonoBehaviour
{

    private Vector3 previousPosition;
    private float speedKmh;
    private float battry = 1f;
    float idealPowerConsumbtionFactor = 10f;
    float powerConsumbtionFactor = 0.000005f * 10;
    private float currentTemperature = 100f; // Starting temperature

    Wheels wheels;
    // Start is called before the first frame update
    void Start()
    {
        wheels = GetComponent<Wheels>();
        previousPosition = transform.position;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // Calculate and update the speed based on position change
        CalculateSpeedKmh();

        //Debug.Log(battry);
        // Update the previous position
        previousPosition = transform.position;
    }

    public void SetBattery(float intialBattry)
    {
        battry = intialBattry;
    }

    public void AddBatteryP(float powerAdd)
    {
        battry += powerAdd;
    }
    public float GetBattery()
    {
        float acceleration = wheels.GetAcceleration();
        battry -= powerConsumbtionFactor * (acceleration * Time.deltaTime + idealPowerConsumbtionFactor * Time.deltaTime);
        if (battry < 0f || battry > 1f) { battry = 1f; }
        //Debug.Log("battry: " + battry);
        return battry;
    }
    private void CalculateSpeedKmh()
    {
        // Calculate the time difference between frames
        float deltaTime = Time.fixedDeltaTime;

        // Calculate the position change since the last frame
        Vector3 positionChange = transform.position - previousPosition;

        // Calculate the distance traveled
        float distanceTraveled = positionChange.magnitude;

        // Calculate the speed in m/s
        float speedMps = distanceTraveled / deltaTime;

        // Convert the speed from m/s to km/h
        speedKmh = speedMps * 3.6f;
    }

    public float GetSpeed()
    { return speedKmh; }

    public float GetHeading()
    {
        return transform.eulerAngles.y;
    }
    public float GetPitch()
    {
        if (transform.eulerAngles.x > 180)
            return (360 - transform.eulerAngles.x);

        return transform.eulerAngles.x;
    }
    public float GetRoll()
    {
        if (transform.eulerAngles.z > 180)
            return (360 - transform.eulerAngles.z);
        return transform.eulerAngles.z;
    }

    public float GetCPUTemp()
    {
        // Calculate a small random change
        float randomChange = Random.Range(-1f, 1f);

        // Apply the change to the current temperature
        currentTemperature += randomChange;

        // Ensure the temperature stays within the range of 80 to 120
        currentTemperature = Mathf.Clamp(currentTemperature, 80f, 120f);

        return currentTemperature;
    }
}
