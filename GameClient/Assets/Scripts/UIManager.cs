using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject connectMenu;
    public GameObject lobbyMenu;
    public GameObject inGame;

    public InputField usernameField;

    public Text teamText;

    public int gameState;

    //Prelobby
    public GameObject preLobby;

    public Text topText;
    public Text countdownText;

    public bool startTimer;
    public float timeRemaining;

    public enum MenuScreen
    {
        UI_CONNECT,
        UI_LOBBY,
        UI_INGAME,
        UI_NONE
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            OnScreenChange(MenuScreen.UI_CONNECT);
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Update()
    {
        if(startTimer)
        {
            if(timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                countdownText.text = timeRemaining.ToString("##");
            }
        }
    }

    /// <summary>Change displaying screen on UI.</summary>
    /// <param name="screen">New screen to display.</param>
    public void OnScreenChange(MenuScreen screen)
    {
        switch (screen)
        {
            case MenuScreen.UI_CONNECT:
                connectMenu.SetActive(true);
                break;
            case MenuScreen.UI_LOBBY:
                lobbyMenu.SetActive(true);
                break;
            case MenuScreen.UI_INGAME:
                inGame.SetActive(true);
                break;
            case MenuScreen.UI_NONE:
                lobbyMenu.SetActive(false);
                connectMenu.SetActive(false);
                break;
        }
    }

    public void HandleRoundState(int _state)
    {
        switch(_state)
        {
            case 0:
                topText.text = "Waiting for more players";
                gameState = 0;
                break;
            case 1:
                topText.text = "Round starts in:";
                gameState = 1;
                teamText.enabled = true;
                break;
            case 2:
                gameState = 2;
                preLobby.SetActive(false);
                break;
            case 3:
                gameState = 3;
                preLobby.SetActive(true);
                break;
            case 4:
                gameState = 4;
                teamText.text = "";
                topText.text = "Restarting round...";
                GameManager.players[Client.instance.myId].carryingWeapon = false;
                break;
        }
    }

    public void RoundEnd(int _winner)
    {
        switch(_winner)
        {
            case 0:
                topText.text = "Winner: Bystanders";
                break;
            case 1:
                topText.text = "Winner: Murderers";
                break;
        }
    }

    public void HandleMurdererState()
    {
        teamText.text = "Murderer";
    }

    public void HandleDetectiveState()
    {
        teamText.text = "Detective";
    }

    public void HandleBystanderState()
    {
        teamText.text = "Bystander";
    }

    /// <summary>Attempts to connect to the server.</summary>
    public void ConnectToServer()
    {
        connectMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
        OnScreenChange(MenuScreen.UI_INGAME);
    }
}
