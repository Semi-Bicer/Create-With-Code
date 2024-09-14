using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerX : MonoBehaviour
{
    public GameObject plane;
    private Vector3 offset = new Vector3(10 , 2, 15);

    // Start is called before the first frame update
    void Start()
    {
        
        transform.Rotate(Vector3.up, -120);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        transform.position = plane.transform.position + offset;

    }
}
