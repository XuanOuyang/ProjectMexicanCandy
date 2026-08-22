using UnityEngine;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private RectTransform character1Button;
    [SerializeField] private RectTransform character2Button;
    [SerializeField] private RectTransform p1Cursor;
    [SerializeField] private RectTransform p2Cursor;

    public void UpdateCursor(LocalPlayer player)
    {
        RectTransform cursor = player.playerNumber == 1 ? p1Cursor : p2Cursor;
        RectTransform target = player.selectedCharacter == 0 ? character1Button : character2Button;
        Vector3 offset = player.playerNumber == 1 ? new Vector3(0, 50f, 0) : new Vector3(0, -50f, 0);
        cursor.position = target.position + offset;
        cursor.gameObject.SetActive(true);
    }
}