using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset = new Vector3(0, 5, -10);
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        


    }

    void LateUpdate() //
    {
        //transform.position is the camera's current position
        transform.position = player.transform.position + offset;
        // to eliminate the stuttering effect we need to update camera a bit late
        
    }
}
