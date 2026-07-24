using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [Header("Assign your two players here")]
    public PlayerInput player1;
    public PlayerInput player2;

    private int joinedPlayers = 0;

    // Track which schemes have been claimed so they can't be picked twice
    private bool wasdClaimed = false;
    private bool arrowsClaimed = false;
    private bool gamepadClaimed = false;

    void Start()
    {
        // Turn off the PlayerInput components so they can't move until they "join"
        if (player1 != null) player1.enabled = false;
        if (player2 != null) player2.enabled = false;
    }

    void Update()
    {
        // Stop checking if both players have already joined
        if (joinedPlayers >= 2) return;

        // 1. Check if someone pressed SPACEBAR to claim WASD
        if (!wasdClaimed && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            AssignControlSchemeToNextPlayer("WASD", Keyboard.current);
            wasdClaimed = true;
        }
        
        // 2. Check if someone pressed ENTER to claim ARROWS
        else if (!arrowsClaimed && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            AssignControlSchemeToNextPlayer("Arrows", Keyboard.current);
            arrowsClaimed = true;
        }
        
        // 3. Check if someone pressed the 'A' / 'South' button to claim GAMEPAD
        else if (!gamepadClaimed && Gamepad.current != null)
        {
            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    AssignControlSchemeToNextPlayer("Gamepad", gamepad);
                    gamepadClaimed = true;
                    break; // Break out of the loop so we only assign one gamepad per frame
                }
            }
        }
    }

    void AssignControlSchemeToNextPlayer(string schemeName, InputDevice device)
    {
        PlayerInput targetPlayer = (joinedPlayers == 0) ? player1 : player2;

        targetPlayer.enabled = true; 
        targetPlayer.SwitchCurrentControlScheme(schemeName, device);

        Debug.Log($"Player {joinedPlayers + 1} joined using {schemeName}!");
        
        joinedPlayers++;
    }
}