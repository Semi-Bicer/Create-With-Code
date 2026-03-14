using JetBrains.Annotations;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [NotNull] public GameObject camera1;
    [NotNull] public GameObject camera2;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera1.SetActive(true);
        camera2.SetActive(false);
        //to be ensure main camera is active at start
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if ( Input.GetKeyDown( KeyCode.C ) )
        {
            ToggleCamera();
        }
    }

    private void ToggleCamera()
    {
        camera1.SetActive(!camera1.activeSelf); // activeSelf: current state
        camera2.SetActive(!camera2.activeSelf);
    }
}
