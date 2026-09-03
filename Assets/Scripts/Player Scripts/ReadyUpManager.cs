using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReadyUpManager : MonoBehaviour
{
    [SerializeField] private CharacterSelectManager characterSelectManager;
    [SerializeField] private UnityEngine.InputSystem.PlayerInputManager playerInputManager;

    private int playersReady = 0;

    private bool wasdClaimed = false;
    private bool arrowsClaimed = false;
    private bool gamepadClaimed = false;

    private void Update()
    {
        if (playersReady >= 2)
        {
            return;
        }

        if (!wasdClaimed && Keyboard.current != null)
        {
            if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame ||
                Keyboard.current.sKey.wasPressedThisFrame
                || Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                wasdClaimed = true;
                JoinPlayer("WASD", Keyboard.current);
                return;
            }
        }

        if (!arrowsClaimed && Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                Keyboard.current.rightArrowKey.wasPressedThisFrame
                || Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
            {
                arrowsClaimed = true;
                JoinPlayer("Arrows", Keyboard.current);
                return;
            }
        }

        if (!gamepadClaimed)
        {
            foreach (Gamepad gamepad in Gamepad.all)
            {
                bool anyGamepadInput =
                    gamepad.buttonSouth.wasPressedThisFrame ||
                    gamepad.buttonNorth.wasPressedThisFrame ||
                    gamepad.buttonEast.wasPressedThisFrame ||
                    gamepad.buttonWest.wasPressedThisFrame ||
                    gamepad.startButton.wasPressedThisFrame ||
                    gamepad.selectButton.wasPressedThisFrame ||
                    gamepad.dpad.up.wasPressedThisFrame ||
                    gamepad.dpad.down.wasPressedThisFrame ||
                    gamepad.dpad.left.wasPressedThisFrame ||
                    gamepad.dpad.right.wasPressedThisFrame ||
                    gamepad.leftStick.up.wasPressedThisFrame ||
                    gamepad.leftStick.down.wasPressedThisFrame ||
                    gamepad.leftStick.left.wasPressedThisFrame ||
                    gamepad.leftStick.right.wasPressedThisFrame ||
                    gamepad.rightStick.up.wasPressedThisFrame ||
                    gamepad.rightStick.down.wasPressedThisFrame ||
                    gamepad.rightStick.left.wasPressedThisFrame ||
                    gamepad.rightStick.right.wasPressedThisFrame;
                if (anyGamepadInput)
                {
                    gamepadClaimed = true;
                    JoinPlayer("Gamepad", gamepad);
                    return;
                }
            }
        }
    }

    private void JoinPlayer(string controlScheme, InputDevice device)
    {
        PlayerInput playerInput = playerInputManager.JoinPlayer(pairWithDevice: device);
        if (playerInput == null)
        {
            Debug.LogError("ReadyUpManager: Failed to create PlayerInput");
            return;
        }

        playerInput.SwitchCurrentControlScheme(controlScheme, device);
        LocalPlayer localPlayer = playerInput.GetComponent<LocalPlayer>();
        if (localPlayer == null)
        {
            Debug.LogError("ReadyUpManager: Joined PlayerInput has no LocalPlayer Component");
            return;
        }

        localPlayer.controlScheme = controlScheme;
        localPlayer.inputDevice = device;
        Debug.Log($"Player {playersReady + 1} joined using {controlScheme}");
        OnPlayerJoined(playerInput);
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playersReady++;
        LocalPlayer localPlayer = playerInput.GetComponent<LocalPlayer>();
        if (localPlayer == null)
        {
            Debug.LogError("ReadyUpManager: Joined PlayerInput has no LocalPlayer Component");
            return;
        }

        Debug.Log($"Player {playersReady} Ready!");
        if (playersReady == 1)
        {
            localPlayer.InitializePlayer(1);
            characterSelectManager.InitializeCharacterSelection(localPlayer, 0);
        }
        else if (playersReady == 2)
        {
            localPlayer.InitializePlayer(2);
            characterSelectManager.InitializeCharacterSelection(localPlayer, 1);
            Debug.Log("Both players ready!");
        }
    }
}