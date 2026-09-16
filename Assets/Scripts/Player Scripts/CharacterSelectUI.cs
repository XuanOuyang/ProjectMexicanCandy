using UnityEngine;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private RectTransform character1Button;
    [SerializeField] private RectTransform character2Button;
    [SerializeField] private RectTransform p1Cursor;
    [SerializeField] private RectTransform p2Cursor;
    [SerializeField] private RectTransform p1SelectionOutline;
    [SerializeField] private RectTransform p2SelectionOutline;

    public void UpdateCursor(LocalPlayer player)
    {
        RectTransform cursor = player.playerNumber == 1 ? p1Cursor : p2Cursor;
        RectTransform outline = player.playerNumber == 1 ? p1SelectionOutline : p2SelectionOutline;
        RectTransform target = player.selectedCharacter == 0 ? character1Button : character2Button;
        Vector2 offset = player.playerNumber == 1 ? new Vector2(0, 100f) : new Vector2(0, -100f);
        cursor.position = target.position;
        cursor.anchoredPosition += offset;
        outline.position = target.position;
        cursor.gameObject.SetActive(true);
        outline.gameObject.SetActive(true);
    }
}