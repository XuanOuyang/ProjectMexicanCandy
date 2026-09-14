using UnityEngine;
using UnityEngine.InputSystem;

public class HowToPlayPanel : MonoBehaviour
{
    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            Close();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Close();
        }

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            Close();
        }
    }
}