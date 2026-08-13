using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Candy Shooting")] [SerializeField]
    private CandyShootingInput candyShooting;

    [Header("Card Holders")] [SerializeField]
    private RectTransform[] cardHolders = new RectTransform[3];

    [Header("Card Holder Images")] [SerializeField]
    private Image[] cardHolderImages = new Image[3];

    [Header("Card Images")] [SerializeField]
    private Image[] cardImages = new Image[3];

    [Header("Selected Appearance")] [SerializeField]
    private float selectedScale = 1.15f;

    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    private void OnEnable()
    {
        if (candyShooting != null)
        {
            candyShooting.OnCandySelected += UpdateSelection;
        }
    }

    private void Start()
    {
        UpdateSelection(candyShooting != null ? candyShooting.SelectedIndex : 0);
    }

    private void OnDisable()
    {
        if (candyShooting != null)
        {
            candyShooting.OnCandySelected -= UpdateSelection;
        }
    }

    private void UpdateSelection(int selectedIndex)
    {
        for (int i = 0; i < cardHolders.Length; i++)
        {
            bool isSelected = i == selectedIndex;
            Color color = isSelected ? selectedColor : unselectedColor;
            if (cardHolders[i] != null)
            {
                cardHolders[i].localScale = Vector3.one * (isSelected ? selectedScale : 1f);
                if (isSelected)
                {
                    cardHolders[i].SetAsLastSibling();
                }
            }

            if (cardHolderImages[i] != null)
            {
                cardHolderImages[i].color = color;
            }

            if (cardImages[i] != null)
            {
                cardImages[i].color = color;
            }
        }
    }
}