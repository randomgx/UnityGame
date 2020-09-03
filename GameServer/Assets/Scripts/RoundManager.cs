using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;

    public int currentPlayers;
    public int minimumPlayers = 3;

    private bool countdownStarted;
    public float countdownTime = 30;
    [HideInInspector]
    public float countdownCurrentTimer;
    private int murdererPlayer;
    private int detectivePlayer;

    public int murderersAlive;
    public int bystandersAlive;

    public enum RoundState
    {
        WAITING,
        COUNTDOWN,
        PLAYING,
        END,
        RESTART
    }

    public RoundState rState;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            countdownCurrentTimer = countdownTime;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Update()
    {
        if(rState == RoundState.COUNTDOWN && countdownStarted)
        {
            if(countdownCurrentTimer > 0)
            {
                countdownCurrentTimer -= Time.deltaTime;
            }
        }
    }

    public void OnChangeRoundState(RoundState state)
    {
        switch(state)
        {
            case RoundState.WAITING:
                rState = RoundState.WAITING;
                ServerSend.RoundState((int)rState);

                if (GetPlayerCount() >= minimumPlayers)
                {
                    OnChangeRoundState(RoundState.COUNTDOWN);
                }

                break;
            case RoundState.COUNTDOWN:
                rState = RoundState.COUNTDOWN;
                if (!countdownStarted)
                {
                    StartCoroutine(Countdown());
                }
                break;
            case RoundState.PLAYING:
                Debug.Log("Starting round!");
                rState = RoundState.PLAYING;
                ServerSend.RoundState((int)rState);
                break;
            case RoundState.END:
                Debug.Log("Round ended. Restarting soon.");
                rState = RoundState.END;
                StartCoroutine(End());
                break;
            case RoundState.RESTART:
                Debug.Log("Restarting round...");
                rState = RoundState.RESTART;
                StartCoroutine(Restart());
                break;
        }
    }

    public int GetPlayerCount()
    {
        int p = 0;
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                p++;
            }
        }
        return p;
    }

    public void SelectTeams()
    {
        murdererPlayer = Random.Range(1, GetPlayerCount() + 1);
        detectivePlayer = Random.Range(1, GetPlayerCount() + 1);

        if(murdererPlayer == detectivePlayer)
        {
            SelectTeams();
            Debug.Log("Murderer and Detective was the same id. Running again...");
        }
        else
        {
            Debug.Log("Found a murderer! Id " + (murdererPlayer));
            Debug.Log("Found a detective! Id " + (detectivePlayer));
        }
    }

    /// <summary>
    /// Calls the end of the round.
    /// </summary>
    /// <param name="team">Winner team.</param>
    public void EndRound(Player.Team team)
    {
        if(team == Player.Team.Murderer)
        {
            OnChangeRoundState(RoundState.END);
            ServerSend.RoundState((int)RoundState.END);
            ServerSend.RoundEnd((int)team);
        }
        else if(team == Player.Team.Bystander)
        {
            OnChangeRoundState(RoundState.END);
            ServerSend.RoundState((int)RoundState.END);
            ServerSend.RoundEnd((int)team);
        }
    }

    IEnumerator Countdown()
    {
        Debug.Log("Starting countdown sequence. Selecting teams in 20 seconds");
        countdownStarted = true;
        ServerSend.RoundState((int)rState);

        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.RoundCountdown(_client.id, countdownCurrentTimer);
            }
        }

        yield return new WaitForSeconds((countdownTime/3)*2);

        SelectTeams();

        if (Server.clients[murdererPlayer].player != null)
        {
            Server.clients[murdererPlayer].player.team = Player.Team.Murderer;
            Server.clients[murdererPlayer].player.hitRange = 1.5f;
            Server.clients[murdererPlayer].player.carryingWeapon = true;
            murderersAlive++;
            ServerSend.RoundMurderer(murdererPlayer); //need to change that later, maybe we're selecting an id that is not on the server anymore
        }
        else
        {
            Debug.Log("Murderer player id is not connected.");
            SelectTeams();
        }

        if (Server.clients[detectivePlayer].player != null && Server.clients[detectivePlayer].player.team != Player.Team.Murderer)
        {
            Server.clients[detectivePlayer].player.team = Player.Team.Detective;
            Server.clients[detectivePlayer].player.hitRange = 30f;
            Server.clients[detectivePlayer].player.carryingWeapon = true;
            bystandersAlive++;
            ServerSend.RoundDetective(detectivePlayer); //need to change that later, maybe we're selecting an id that is not on the server anymore
        }
        else
        {
            Debug.Log("Detective player id is not connected.");
            SelectTeams();
        }

        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null && _client.player.team == Player.Team.Bystander)
            {
                bystandersAlive++;
                _client.player.hitRange = 30f;
                ServerSend.RoundBystander(_client.id);
            }
        }

        yield return new WaitForSeconds(countdownTime / 3);

        OnChangeRoundState(RoundState.PLAYING);
    }

    IEnumerator End()
    {
        yield return new WaitForSeconds(5);

        OnChangeRoundState(RoundState.RESTART);
    }

    IEnumerator Restart()
    {
        countdownStarted = false;
        countdownCurrentTimer = countdownTime;
        bystandersAlive = 0;
        murderersAlive = 0;

        ServerSend.RoundState((int)rState);

        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.player.health == 0)
                {
                    StartCoroutine(_client.player.Respawn());
                }
                _client.player.team = Player.Team.Bystander;
                _client.player.carryingWeapon = false;

                ServerSend.DrawedWeapon(_client.id, 0, 0);
                ServerSend.DrawedWeapon(_client.id, 0, 1);
            }
        }

        yield return new WaitForSeconds(1f);

        OnChangeRoundState(RoundState.WAITING);

    }
}
