using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class CongratScript : MonoBehaviour
{
    public TextMesh Text;
    public ParticleSystem SparksParticles;

    private List<string> TextToDisplay = new List<string>();
    
    private float RotatingSpeed;
    private float TimeToNextText;

    private int CurrentText;
    
    // Start is called before the first frame update
    void Start()
    {
        TimeToNextText = 0.0f;
        CurrentText = 0;
        
        RotatingSpeed = 50.0f;

        TextToDisplay.Add("Congratulation");
        TextToDisplay.Add("All Errors Fixed");

        Text.text = TextToDisplay[0];
        
        //Instantiate(SparksParticles, new Vector3(0.0f, -0.8f, 0.0f), new Quaternion(-90f, 0f, 0f, 0f));

        if (SparksParticles != null)
        {
            SparksParticles.Play();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TimeToNextText += Time.deltaTime;
        transform.Rotate(0, RotatingSpeed * Time.deltaTime, 0);
        if (TimeToNextText > 1.5f)
        {
            TimeToNextText = 0.0f;
            
            CurrentText++;
            if (CurrentText >= TextToDisplay.Count)
            {
                CurrentText = 0;
                
            }
            Text.text = TextToDisplay[CurrentText];
        }
    }
}