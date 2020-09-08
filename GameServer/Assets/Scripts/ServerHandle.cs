using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }

        // Send all players to the new player
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != _fromClient)
                {
                    ServerSend.SpawnPlayer(_fromClient, _client.player);
                }
            }
        }

        if (RoundManager.instance.rState == RoundManager.RoundState.WAITING || RoundManager.instance.rState == RoundManager.RoundState.COUNTDOWN && RoundManager.instance.countdownCurrentTimer > (RoundManager.instance.countdownTime / 3))
        {
            Server.clients[_fromClient].SendIntoGame(_username);
        }
        else
        {
            WaitingPlayers _player = new WaitingPlayers();
            _player.id = _fromClient;
            _player.username = _username;
            RoundManager.instance.waitingPlayers.Add(_player);
        }
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    public static void PlayerShoot(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.Shoot(_shootDirection);
    }

    public static void PlayerDrawWeapon(int _fromClient, Packet _packet)
    {
        int _showing = _packet.ReadInt();

        Server.clients[_fromClient].player.DrawWeapon(_showing);
    }
}
