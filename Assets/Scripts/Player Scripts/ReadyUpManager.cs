using UnityEngine;
using UnityEngine.InputSystem;

public class ReadyUpManager : MonoBehaviour
{
    [SerializeField] private CharacterSelectManager characterSelectManager;
    private int playersReady = 0;

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playersReady++;
        LocalPlayer localPlayer = playerInput.GetComponent<LocalPlayer>();
        localPlayer.controlScheme = playerInput.currentControlScheme;
        if (playerInput.devices.Count > 0)
        {
            localPlayer.inputDevice = playerInput.devices[0];
        }
        //playerInput.SwitchCurrentActionMap("CharacterSelect");
        Debug.Log($"Player {playersReady} joined using " + $"{localPlayer.controlScheme}");
        if (playersReady == 1)
        {
            Debug.Log("Player 1 Ready!");
            localPlayer.InitializePlayer(1);
            characterSelectManager.InitializeCharacterSelection(localPlayer, 0);
        }
        else if (playersReady == 2)
        {
            Debug.Log("Player 2 Ready!");
            localPlayer.InitializePlayer(2);
            characterSelectManager.InitializeCharacterSelection(localPlayer, 1);
            Debug.Log("Both players ready!");
        }
    }
}