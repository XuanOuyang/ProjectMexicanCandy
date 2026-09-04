using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;

public class PlayerInputManager : MonoBehaviour
{
    [Header("Gameplay Characters")] [SerializeField]
    private PlayerMovementInput player1Movement;

    [SerializeField] private CandyShootingInput player1Shooting;

    [SerializeField] private PlayerMovementInput player2Movement;
    [SerializeField] private CandyShootingInput player2Shooting;

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
            Debug.LogError($"P{player.playerNumber} has no PlayerInput");
            return;
        }

        Debug.Log($"Setting up P{player.playerNumber} " + $"with existing scheme {playerInput.currentControlScheme}");
        playerInput.SwitchCurrentActionMap("Gameplay");
        InputAction moveAction = playerInput.actions.FindAction("Move");
        InputAction shootAction = playerInput.actions.FindAction("Shoot");
        if (moveAction == null)
        {
            Debug.LogError($"P{player.playerNumber} has no MoveAction");
            return;
        }

        if (shootAction == null)
        {
            Debug.LogError($"P{player.playerNumber} has no ShootAction");
            return;
        }

        if (player.playerNumber == 1)
        {
            SetupGameplayInput(playerInput, moveAction, shootAction, player1Movement, player1Shooting);
        }
        else if (player.playerNumber == 2)
        {
            SetupGameplayInput(playerInput, moveAction, shootAction, player2Movement, player2Shooting);
        }

        Debug.Log($"P{player.playerNumber} is now using Gameplay Input");
    }

    private void SetupGameplayInput(PlayerInput playerInput, InputAction moveAction, InputAction shootAction,
        PlayerMovementInput movement, CandyShootingInput shooting)
    {
        if (movement == null)
        {
            Debug.LogError("PlayerMovementInput reference is missing");
            return;
        }

        if (shooting == null)
        {
            Debug.LogError("PlayerMovementInput reference is missing");
            return;
        }

        moveAction.performed += movement.OnMove;
        moveAction.canceled += movement.OnMove;
        shootAction.started += shooting.OnShoot;
        shootAction.canceled += shooting.OnShoot;
        shooting.InitializeInput(playerInput);
    }
}