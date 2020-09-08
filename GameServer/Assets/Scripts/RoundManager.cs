using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class WaitingPlayers
{
    public int id;
    public string username;
}

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;

    public List<WaitingPlayers> waitingPlayers = new List<WaitingPlayers>();

    public int currentPlayers;
    public int minimumPlayers = 3;

    private bool countdownStarted;
    public float countdownTime = 30;
    public float countdownCurrentTimer;

    private int murdererPlayer;
    private int detectivePlayer;

    public int murderersQuantity;
    public int detectivesQuantity;

    public List<int> murdererPlayers = new List<int>();
    public List<int> detectivePlayers = new List<int>();
    public List<int> availPlayersId = new List<int>();

    public int murderersAlive;
    public int detectivesAlive;
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

        if(currentPlayers < minimumPlayers && rState == RoundState.PLAYING || currentPlayers < minimumPlayers && rState == RoundState.COUNTDOWN)
        {
            //if there's no detective/murder left
            StopAllCoroutines();
            OnChangeRoundState(RoundState.RESTART);
        }
    }

    public void OnChangeRoundState(RoundState state)
    {
        switch(state)
        {
            case RoundState.WAITING:
                rState = RoundState.WAITING;

                if (waitingPlayers.Count > 0)
                {
                    foreach (WaitingPlayers _player in waitingPlayers)
                    {
                        if(Server.clients[_player.id].player == null)
                        {
                            Server.clients[_player.id].SendIntoGame(_player.username);
                        }
                    }
                }

                waitingPlayers.Clear();

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

    public void UpdateAvailablePlayersIds()
    {
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null && _client.player.team == Player.Team.Bystander)
            {
                availPlayersId.Add(_client.id);
            }
        }
    }

    public void SelectTeams()
    {
        for(int i = 0; murdererPlayers.Count < murderersQuantity; i++)
        {
            UpdateAvailablePlayersIds();
            murderersAlive++;
            int _id = Random.Range(1, availPlayersId.Count+1);
            murdererPlayers.Add(availPlayersId[_id-1]);

            if (Server.clients[murdererPlayers[murdererPlayers.Count - 1]].player != null)
            {
                murdererPlayer = Server.clients[murdererPlayers[murdererPlayers.Count - 1]].id;
                Server.clients[murdererPlayer].player.team = Player.Team.Murderer;
                Server.clients[murdererPlayer].player.hitRange = 2f;
                Server.clients[murdererPlayer].player.carryingWeapon = true;
                availPlayersId.Clear();
                ServerSend.RoundMurderer(murdererPlayer);
            }
            else
            {
                Debug.Log("Murderer player id is not connected.");
            }
        }

        for (int i = 0; detectivePlayers.Count < detectivesQuantity; i++)
        {
            UpdateAvailablePlayersIds();
            detectivesAlive++;
            int _id = Random.Range(1, availPlayersId.Count+1);
            detectivePlayers.Add(availPlayersId[_id-1]);

            if (Server.clients[detectivePlayers[detectivePlayers.Count - 1]].player != null)
            {
                detectivePlayer = Server.clients[detectivePlayers[detectivePlayers.Count - 1]].id;
                Server.clients[detectivePlayer].player.team = Player.Team.Detective;
                Server.clients[detectivePlayer].player.hitRange = 30f;
                Server.clients[detectivePlayer].player.carryingWeapon = true;
                availPlayersId.Clear();
                ServerSend.RoundDetective(detectivePlayer);
            }
            else
            {
                Debug.Log("Murderer player id is not connected.");
            }
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
                ServerSend.RoundCountdown(_client.id, countdownCurrentTimer+1);
            }
        }

        yield return new WaitForSeconds((countdownTime/3)*2);

        SelectTeams();
        UpdateAvailablePlayersIds();

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
        detectivesAlive = 0;
        murdererPlayers.Clear();
        detectivePlayers.Clear();
        availPlayersId.Clear();

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

                _client.player.controller.enabled = false;
                _client.player.transform.position = new Vector3(0f, 25f, 0f);
                _client.player.controller.enabled = true;

                ServerSend.DrawedWeapon(_client.id, 0, 0);
                ServerSend.DrawedWeapon(_client.id, 0, 1);
            }
        }

        yield return new WaitForSeconds(1f);

        OnChangeRoundState(RoundState.WAITING);

    }
}
