using UnityEngine;

public class SetTransform : MonoBehaviour
{
    [SerializeField] Transform followCam;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = followCam.position + new Vector3(0, 0, 0);
        //transform.localEulerAngles = Vector3.zero;

        transform.eulerAngles = followCam.eulerAngles + new Vector3(0, 0, 0);
    }
}
