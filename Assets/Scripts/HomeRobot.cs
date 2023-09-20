using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeRobot : MonoBehaviour
{
    
    public List<Transform> entranceAndExitPoints;
    Wheels wheels;

    private Vector3 FindNearestPoint(Vector3 colliderPosition)
    { 
        Vector3 nearestPoint = Vector3.zero;
        float nearestDistance = float.MaxValue;

        foreach (Transform point in entranceAndExitPoints)
        {
            float distance = Vector3.Distance(colliderPosition, point.position);

            // Compare distances and update nearest point
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = point.position;
            }
        }

        return nearestPoint;
    }

    private void Start()
    {
        Debug.Log(FindNearestPoint(this.transform.position));

    }

    private void OnTriggerEnter(Collider other)
    {
        if (Vector2.Distance(new Vector2(transform.position.z, transform.position.x), other.GetComponentInParent<Wheels>().destination) > 10)
        {
            return;
        }
        Vector3 colliderPosition = other.transform.position;
        Vector3 NearPoint = FindNearestPoint(colliderPosition);
        Vector2[] dest = new Vector2[2];
        dest[0] = new Vector2(NearPoint.z, NearPoint.x);
        dest[1] = new Vector2(transform.position.z, transform.position.x);

        wheels = other.GetComponentInParent<Wheels>();
        wheels.SetDestination(dest);
        wheels.SetAutoControllFromHome(true);

        Debug.Log("home " + CoordsConverter.ConvertXZToLonLat(transform.position).x + " " + CoordsConverter.ConvertXZToLonLat(transform.position).y);
    }

    private void OnTriggerStay(Collider other)
    {
        if (Vector2.Distance(new Vector2(transform.position.z, transform.position.x), other.GetComponentInParent<Wheels>().destination) > 10)
        {
            wheels.exitHome = true;
            StartCoroutine(WaitToExit());
        }
    }

    private IEnumerator WaitToExit()
    {
        yield return new WaitForSeconds(5.0f); // Wait for 5 second
        wheels.exitHome = false;

    }
}
