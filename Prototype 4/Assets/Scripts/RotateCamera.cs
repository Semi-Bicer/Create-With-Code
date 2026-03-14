using UnityEngine;

public class RotateCamera : MonoBehaviour
{
<<<<<<< Updated upstream
    
    public float rotationSpeed;
=======
>>>>>>> Stashed changes
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
<<<<<<< Updated upstream
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);
=======
        
>>>>>>> Stashed changes
    }
}
