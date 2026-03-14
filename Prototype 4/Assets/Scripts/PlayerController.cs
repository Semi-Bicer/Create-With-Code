using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5;
    private Rigidbody rb;
    private bool hasPowerup = false;
    private float powerupStrength = 15.0f;
    public GameObject powerupIndicator;
    private Vector3 indicatorPosition = new Vector3(0, -0.5f, 0);

    private GameObject focalPoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("FocalPoint");
    }

    // Update is called once per frame
    void Update()
    {
        float v = Input.GetAxis("Vertical");
        rb.AddForce(focalPoint.transform.forward * v * speed);
        powerupIndicator.transform.position = transform.position + indicatorPosition;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Powerup")
        {
            hasPowerup = true;
            powerupIndicator.SetActive(true);
            Destroy(other.gameObject);
            StartCoroutine(PowerupCountdownRoutine());
        }
    }

    private IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(6.0f);
        hasPowerup = false;
        powerupIndicator.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Rigidbody enemyRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 knockbackDirection = (collision.gameObject.transform.position - transform.position);

            Debug.Log("Collided with " + collision.gameObject.name + " with powerup set to " + hasPowerup);
            enemyRigidbody.AddForce(knockbackDirection * powerupStrength, ForceMode.Impulse);
        }
    }
}
