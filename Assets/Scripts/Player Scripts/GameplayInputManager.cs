using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class GameplayInputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset multiplayerInput;

    private void Start()
    {
        LocalPlayer[] players = FindObjectsByType<LocalPlayer>();
        foreach (LocalPlayer player in players)
        {
            SetupPlayer(player);
        }
    }

    private void SetupPlayer(LocalPlayer player)
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError($"Player {player.playerNumber} has no PlayerInput!");
            return;
        }
        Debug.Log($"Setting up P{player.playerNumber} " + $"with {player.controlScheme}");
        //playerInput.SwitchCurrentControlScheme(player.controlScheme, player.inputDevice);
        playerInput.SwitchCurrentActionMap("Gameplay");
        Debug.Log($"P{player.playerNumber} is now using Gameplay input");
    }
}