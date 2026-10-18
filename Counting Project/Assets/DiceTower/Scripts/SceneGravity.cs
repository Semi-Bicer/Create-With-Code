using UnityEngine;

/*
 * Overrides Physics.gravity while this scene is loaded and restores the project value afterwards, so the dice scene
 * can fall faster without touching other scenes in the project. The dice are 1.4 units across, so real gravity makes
 * them look like they fall in slow motion.
 */
public class SceneGravity : MonoBehaviour
{
    [SerializeField] Vector3 gravity = new Vector3(0f, -28f, 0f);

    Vector3 previous;

    void OnEnable()
    {
        previous = Physics.gravity;
        Physics.gravity = gravity;
    }

    void OnDisable()
    {
        Physics.gravity = previous;
    }
}
