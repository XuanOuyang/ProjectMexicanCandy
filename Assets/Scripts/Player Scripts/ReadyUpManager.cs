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

    private void Start()
    {
        LocalPlayer[] existingPlayers = FindObjectsByType<LocalPlayer>();
        foreach (LocalPlayer player in existingPlayers)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("CharacterSelect");
                playerInput.ActivateInput();
            }

            if (player.playerNumber == 1)
            {
                player.InitializePlayer(1);
                characterSelectManager.InitializeCharacterSelection(player, 0);
                playersReady++;
            }
            else if (player.playerNumber == 2)
            {
                player.InitializePlayer(2);
                characterSelectManager.InitializeCharacterSelection(player, 1);
                playersReady++;
            }
        }
    }

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
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                wasdClaimed = true;
                JoinPlayer("WASD", Keyboard.current);
                return;
            }
        }

        if (!arrowsClaimed && Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
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
                if (gamepad.buttonSouth.wasPressedThisFrame)
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
            Debug.LogWarning(
                $"ReadyUpManager: Cannot join player. Max player count ({playerInputManager.maxPlayerCount}) reached.");
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
        int playerNumber = playersReady;
        int startingCharacter = playerNumber == 1 ? 0 : 1;
        localPlayer.InitializePlayer(playerNumber);
        if (characterSelectManager != null)
        {
            characterSelectManager.InitializeCharacterSelection(localPlayer, startingCharacter);
            characterSelectManager.ConfirmSelection(localPlayer);
        }

        Debug.Log($"Player {playerNumber} joined and is ready");
    }
}