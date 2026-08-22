using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    private LocalPlayer player1;
    private LocalPlayer player2;
    [SerializeField] private CharacterSelectUI characterSelectUI;
    [SerializeField] private InputActionAsset multiplayerInputActions;

    public void InitializeCharacterSelection(LocalPlayer player, int startingCharacter)
    {
        player.selectedCharacter = startingCharacter;
        player.isCharacterLocked = false;
        if (player.playerNumber == 1)
        {
            player1 = player;
        }
        else if (player.playerNumber == 2)
        {
            player2 = player;
        }

        Debug.Log(
            $"Player {player.playerNumber} started on character {startingCharacter}"
        );
        characterSelectUI.UpdateCursor(player);
    }

    public bool IsCharacterAvailable(LocalPlayer requestingPlayer, int characterIndex)
    {
        if (player1 != null && player1 != requestingPlayer && player1.isCharacterLocked &&
            player1.selectedCharacter == characterIndex)
        {
            return false;
        }

        if (player2 != null && player2 != requestingPlayer && player2.isCharacterLocked &&
            player2.selectedCharacter == characterIndex)
        {
            return false;
        }

        return true;
    }

    public bool SelectCharacter(LocalPlayer player, int characterIndex)
    {
        if (player.isCharacterLocked)
        {
            return false;
        }

        if (!IsCharacterAvailable(player, characterIndex))
        {
            return false;
        }

        player.selectedCharacter = characterIndex;
        characterSelectUI.UpdateCursor(player);
        Debug.Log($"Player {player.playerNumber} selected character {characterIndex}");
        return true;
    }

    public bool ConfirmSelection(LocalPlayer player)
    {
        if (player.isCharacterLocked)
        {
            return false;
        }

        if (!IsCharacterAvailable(player, player.selectedCharacter))
        {
            Debug.Log($"Player {player.playerNumber} cannot confirm " + $"character {player.selectedCharacter}");
            MoveToAvailableCharacter(player);
            return false;
        }

        player.isCharacterLocked = true;
        Debug.Log($"Player {player.playerNumber} locked in " + $"character {player.selectedCharacter}");
        CheckBothPlayersLocked();
        return true;
    }

    private void MoveToAvailableCharacter(LocalPlayer player)
    {
        int otherCharacter = player.selectedCharacter == 0 ? 1 : 0;
        if (IsCharacterAvailable(player, otherCharacter))
        {
            player.selectedCharacter = otherCharacter;
            characterSelectUI.UpdateCursor(player);
            Debug.Log($"Player {player.playerNumber} moved to " + $"character {otherCharacter}");
        }
    }

    private void CheckBothPlayersLocked()
    {
        if (player1 == null || player2 == null)
        {
            return;
        }

        if (!player1.isCharacterLocked || !player2.isCharacterLocked)
        {
            return;
        }

        Debug.Log("Both players selected characters");
        Debug.Log($"Player 1: {player1.selectedCharacter}");
        Debug.Log($"Player 2: {player2.selectedCharacter}");
        SceneManager.LoadScene("GrayBoxed Game");
    }
}