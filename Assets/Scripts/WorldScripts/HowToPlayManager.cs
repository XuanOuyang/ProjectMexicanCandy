using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HowToPlayManager : MonoBehaviour
{
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject keyboardControlsPanel;
    [SerializeField] private GameObject playstationControlsPanel;
    [SerializeField] private GameObject xboxControlsPanel;
    [SerializeField] private UnityEngine.InputSystem.UI.InputSystemUIInputModule uiInputModule;

    private int currentSlide = 0;
    private bool canAdvance = false;

    public void OpenHowToPlay()
    {
        currentSlide = 0;
        canAdvance = false;
        uiInputModule.enabled = false;
        howToPlayPanel.SetActive(true);
        keyboardControlsPanel.SetActive(false);
        playstationControlsPanel.SetActive(false);
        xboxControlsPanel.SetActive(false);
        StartCoroutine(EnableNextInputFrame());
    }

    private IEnumerator EnableNextInputFrame()
    {
        yield return null;
        canAdvance = true;
    }

    private void Update()
    {
        if (!canAdvance || !IsAnyPanelOpen())
        {
            return;
        }

        bool pressed = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                       (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                       (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
        if (pressed)
        {
            NextSlide();
        }
    }

    private void NextSlide()
    {
        switch (currentSlide)
        {
            case 0:
                howToPlayPanel.SetActive(false);
                keyboardControlsPanel.SetActive(true);
                currentSlide = 1;
                break;
            case 1:
                keyboardControlsPanel.SetActive(false);
                playstationControlsPanel.SetActive(true);
                currentSlide = 2;
                break;
            case 2:
                playstationControlsPanel.SetActive(false);
                xboxControlsPanel.SetActive(true);
                currentSlide = 3;
                break;
            case 3:
                xboxControlsPanel.SetActive(false);
                currentSlide = 0;
                uiInputModule.enabled = true;
                break;
        }
    }

    private bool IsAnyPanelOpen()
    {
        return howToPlayPanel.activeSelf || keyboardControlsPanel.activeSelf || playstationControlsPanel.activeSelf ||
               xboxControlsPanel.activeSelf;
    }
}