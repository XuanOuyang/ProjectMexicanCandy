using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public PlayerInput player1;
    public PlayerInput player2;

    void Start()
    {
        // Force Player 1 to use WASD on the current keyboard
        if (player1 != null)
        {
            player1.SwitchCurrentControlScheme("WASD", Keyboard.current);
        }

        // Force Player 2 to use Arrows on the SAME keyboard
        if (player2 != null)
        {
            player2.SwitchCurrentControlScheme("Arrows", Keyboard.current);
        }
    }
}