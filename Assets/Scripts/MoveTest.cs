using UnityEngine;

public class MoveTest : MonoBehaviour
{
    float forceMagnitude = 0; // Magnitude of the force to be applied
    private Rigidbody rb;
    private Vector3 rotationAxis = new Vector3(0f, 1f, 0f);  //Vector3.zero;
    private float rotationAmount = 0f; //0f;


    public void SetTwist(Vector3 linear, Vector3 angular)
    {
        forceMagnitude = linear.x;

        rotationAxis = Vector3.up;
        rotationAmount = angular.z;
        Debug.Log("TwistSet");
    }  

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        MoveUGV();
        RotateUGV();
    }
   
    private void MoveUGV()
    {
        // Apply a constant force in the forward direction of the GameObject
        Vector3 forceDirection = new Vector3(0f, 0f, 1f);
        Vector3 force = forceDirection * forceMagnitude;
        rb.AddRelativeForce(force);
    }

    private void RotateUGV()
    {
        transform.Rotate(rotationAxis, rotationAmount);
    }
}
