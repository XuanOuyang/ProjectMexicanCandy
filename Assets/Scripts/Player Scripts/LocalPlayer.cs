using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayer : MonoBehaviour
{
    public int playerNumber;
    public int selectedCharacter;
    public bool isCharacterLocked;
    private CharacterSelectManager characterSelectManager;

    public void InitializePlayer(int number)
    {
        playerNumber = number;
        characterSelectManager = FindAnyObjectByType<CharacterSelectManager>();
        Debug.Log($"Player {playerNumber} initialized");
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (!context.performed || isCharacterLocked)
        {
            return;
        }

        Debug.Log($"P{playerNumber} received Navigate!");

        float horizontal = context.ReadValue<Vector2>().x;
        if (horizontal > 0)
        {
            characterSelectManager.SelectCharacter(this, 1);
        }
        else if (horizontal < 0)
        {
            characterSelectManager.SelectCharacter(this, 0);
        }
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        characterSelectManager.ConfirmSelection(this);
    }
}