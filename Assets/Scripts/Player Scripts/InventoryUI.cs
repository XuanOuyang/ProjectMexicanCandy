using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Candy Shooting")] 
    [SerializeField] private CandyShootingInput candyShooting;

    [Header("Card Holders")] 
    [SerializeField] private RectTransform[] cardHolders = new RectTransform[3];

    [Header("Card Holder Images")] 
    [SerializeField] private Image[] cardHolderImages = new Image[3];

    [Header("Card Images")] 
    [SerializeField] private Image[] cardImages = new Image[3];

    [Header("Single Selected Ammo Display")] 
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Vector2 textOffset = new Vector2(0f, -40f); // Offset relative to card holder center

    [Header("Selected Appearance")] 
    [SerializeField] private float selectedScale = 1.15f;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    private void OnEnable()
    {
        if (candyShooting != null)
        {
            candyShooting.OnCandySelected += UpdateSelection;
            candyShooting.OnAmmoChanged += HandleAmmoChanged;
        }
    }

    private void Start()
    {
        int initialIndex = candyShooting != null ? candyShooting.SelectedIndex : 0;
        UpdateSelection(initialIndex);
        RefreshAmmoDisplay();
    }

    private void OnDisable()
    {
        if (candyShooting != null)
        {
            candyShooting.OnCandySelected -= UpdateSelection;
            candyShooting.OnAmmoChanged -= HandleAmmoChanged;
        }
    }

    private void HandleAmmoChanged(int slotIndex, int newAmmoCount)
    {
        if (candyShooting != null && slotIndex == candyShooting.SelectedIndex)
        {
            RefreshAmmoDisplay();
        }
    }

    public void RefreshAmmoDisplay()
    {
        if (ammoText != null && candyShooting != null)
        {
            ammoText.text = candyShooting.CurrentAmmo.ToString();
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

                    if (ammoText != null)
                    {
                        ammoText.transform.SetParent(cardHolders[i], false);
                        ammoText.transform.SetAsLastSibling();

                        RectTransform textRect = ammoText.rectTransform;
                        
                        // Offset slightly inwards/upwards so it doesn't clip the screen edge
                        textRect.anchoredPosition = new Vector2(30f, 20f); // Adjust numbers to taste!
                        textRect.localPosition = new Vector3(30f, 20f, 0f);
                    }
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

        RefreshAmmoDisplay();
    }
}