using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Desktop builds have no window chrome in fullscreen, so Escape (or the Android back button, which the Input System
 * maps to Escape) closes the game. Does nothing in the editor.
 */
public class QuitOnEscape : MonoBehaviour
{
    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Application.Quit();
    }
}
