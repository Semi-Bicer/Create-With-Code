using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneController : MonoBehaviour
{
    public Camera gameCamera;
    public Text playerGreetingText;
    public Text shapeInfoText;

    private InputAction m_ClickAction;

    private void Awake()
    {
        m_ClickAction = new InputAction("Click", InputActionType.Button, "<Mouse>/leftButton");
    }

    private void OnEnable()
    {
        m_ClickAction.Enable();
    }

    private void OnDisable()
    {
        m_ClickAction.Disable();
    }

    private void Start()
    {
        UpdatePlayerGreeting();
    }

    private void Update()
    {
        if (m_ClickAction.WasPressedThisFrame())
        {
            HandleMouseSelection();
        }
    }

    // ABSTRACTION
    private void UpdatePlayerGreeting()
    {
        string playerName = DataManager.Instance != null ? DataManager.Instance.PlayerName : "Player";
        if (playerGreetingText != null)
        {
            playerGreetingText.text = $"Welcome, {playerName}! Click any shape to test OOP:";
        }
    }

    // ABSTRACTION
    public void HandleMouseSelection()
    {
        if (gameCamera == null || Mouse.current == null)
            return;

        Ray ray = gameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Shape clickedShape = hit.collider.GetComponentInParent<Shape>();
            if (clickedShape != null && shapeInfoText != null)
            {
                shapeInfoText.text = clickedShape.HandleSelection();
            }
        }
    }

    // ABSTRACTION
    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}

