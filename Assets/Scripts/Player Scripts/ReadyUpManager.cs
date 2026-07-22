using UnityEngine;
using UnityEngine.InputSystem;

public class ReadyUpManager : MonoBehaviour
{
    private int playersReady = 0;

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playersReady++;
        if (playersReady == 1)
        {
            Debug.Log("Player 1 Ready!");
            playerInput.GetComponent<LocalPlayer>()
                .InitializePlayer(1);
        }
        else if (playersReady == 2)
        {
            Debug.Log("Player 2 Ready!");
            playerInput.GetComponent<LocalPlayer>()
                .InitializePlayer(2);
            Debug.Log("Both players ready!");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}