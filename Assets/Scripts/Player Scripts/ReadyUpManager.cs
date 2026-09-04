using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReadyUpManager : MonoBehaviour
{
    [SerializeField] private CharacterSelectManager characterSelectManager;
    [SerializeField] private UnityEngine.InputSystem.PlayerInputManager playerInputManager;
    [SerializeField] private GameObject pressToJoinText;

    private int playersReady = 0;

    private bool wasdClaimed = false;
    private bool arrowsClaimed = false;
    private bool gamepadClaimed = false;

    // Track joined PlayerInput instances to prevent duplicate processing
    private readonly HashSet<PlayerInput> joinedPlayers = new HashSet<PlayerInput>();

    private void Awake()
    {
        if (playerInputManager != null)
        {
            // Enable joining via method call instead of setting property
            playerInputManager.EnableJoining();
        }
    }

    private void Update()
    {
        if (playersReady >= 2 || (playerInputManager != null && playerInputManager.playerCount >= 2))
        {
            return;
        }

        if (!wasdClaimed && Keyboard.current != null)
        {
            if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame ||
                Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame || 
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                wasdClaimed = true;
                JoinPlayer("WASD", Keyboard.current);
                return;
            }
        }

        if (!arrowsClaimed && Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame || 
                Keyboard.current.enterKey.wasPressedThisFrame)
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
        if (playerInputManager == null)
        {
            Debug.LogError("ReadyUpManager: PlayerInputManager reference is missing!");
            return;
        }

        if (playerInputManager.playerCount >= playerInputManager.maxPlayerCount)
        {
            Debug.LogWarning($"ReadyUpManager: Cannot join player. Max player count ({playerInputManager.maxPlayerCount}) reached.");
            return;
        }

        PlayerInput playerInput = playerInputManager.JoinPlayer(
            playerIndex: playersReady, 
            splitScreenIndex: -1, 
            controlScheme: controlScheme, 
            pairWithDevice: device
        );

        if (playerInput == null)
        {
            Debug.LogError("ReadyUpManager: Failed to create PlayerInput");
            return;
        }

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
        if (playerInput == null) return;

        // Prevent processing the same PlayerInput instance multiple times
        if (joinedPlayers.Contains(playerInput))
        {
            return;
        }

        LocalPlayer localPlayer = playerInput.GetComponent<LocalPlayer>();
        if (localPlayer == null)
        {
            Debug.LogError("ReadyUpManager: Joined PlayerInput has no LocalPlayer Component");
            return;
        }

        joinedPlayers.Add(playerInput);
        playersReady++;

        Debug.Log($"Player {playersReady} Ready!");
        if (playersReady == 1)
        {
            localPlayer.InitializePlayer(1);
            if (characterSelectManager != null)
                characterSelectManager.InitializeCharacterSelection(localPlayer, 0);
        }
        else if (playersReady == 2)
        {
            localPlayer.InitializePlayer(2);
            if (characterSelectManager != null)
                characterSelectManager.InitializeCharacterSelection(localPlayer, 1);
            
            if (pressToJoinText != null)
                pressToJoinText.SetActive(false);
                
            Debug.Log("Both players ready!");
        }
    }
}