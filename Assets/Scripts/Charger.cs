using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        other.GetComponentInParent<SensorMo>().AddBatteryP(Time.deltaTime * 0.1f);
        Debug.Log("trigger");
    }
}
