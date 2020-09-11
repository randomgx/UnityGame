using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        // Now that we have the client's id, connect UDP
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt(true);
        Vector3 _position = _packet.ReadVector3();

        float i = 0;
        i = Time.time * 40;

        if(GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            //GameManager.players[_id].SetDesiredPosition(new TransformUpdate((int)i, _position));
            _player.SetDesiredPosition(new TransformUpdate((int)i, _position));
        }

        /*int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.position = _position;
        }*/
    }

    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.rotation = _rotation;
        }
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();

        if(GameManager.players[_id].gameObject != null)
        {
            Destroy(GameManager.players[_id].gameObject);
            GameManager.players.Remove(_id);
        }
    }

    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.players[_id].SetHealth(_health);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    public static void CreateItemSpawner(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        Vector3 _spawnerPosition = _packet.ReadVector3();
        bool _hasItem = _packet.ReadBool();

        GameManager.instance.CreateItemSpawner(_spawnerId, _spawnerPosition, _hasItem);
    }

    public static void ItemSpawned(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();

        GameManager.itemSpawners[_spawnerId].ItemSpawned();
    }

    public static void ItemPickedUp(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        int _byPlayer = _packet.ReadInt();

        GameManager.itemSpawners[_spawnerId].ItemPickedUp();
        GameManager.players[_byPlayer].itemCount++;
    }

    public static void SpawnProjectile(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        int _thrownByPlayer = _packet.ReadInt();

        GameManager.instance.SpawnProjectile(_projectileId, _position);
        GameManager.players[_thrownByPlayer].itemCount--;
    }

    public static void ProjectilePosition(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.projectiles.TryGetValue(_projectileId, out ProjectileManager _projectile))
        {
            _projectile.transform.position = _position;
        }
    }

    public static void ProjectileExploded(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.projectiles[_projectileId].Explode(_position);
    }

    public static void RoundState(Packet _packet)
    {
        int _state = _packet.ReadInt();

        UIManager.instance.HandleRoundState(_state);
    }

    public static void RoundMurderer(Packet _packet)
    {
        int _murderer = _packet.ReadInt();

        if(_murderer == Client.instance.myId)
        {
            UIManager.instance.HandleMurdererState();
            GameManager.players[_murderer].team = 1;
            GameManager.players[_murderer].GetWeapon();
        }
    }

    public static void RoundDetective(Packet _packet)
    {
        int _detective = _packet.ReadInt();

        if (_detective == Client.instance.myId)
        {
            UIManager.instance.HandleDetectiveState();
            GameManager.players[_detective].team = 2;
            GameManager.players[_detective].GetWeapon();
        }
    }

    public static void RoundBystander(Packet _packet)
    {
        int _id = _packet.ReadInt();

        if (_id == Client.instance.myId)
        {
            UIManager.instance.HandleBystanderState();
            GameManager.players[_id].team = 0;
        }
    }

    public static void RoundCountdown(Packet _packet)
    {
        float _time = _packet.ReadFloat();

        UIManager.instance.timeRemaining = _time;
        UIManager.instance.startTimer = true;
    }

    public static void RoundEnd(Packet _packet)
    {
        int _winner = _packet.ReadInt();

        UIManager.instance.RoundEnd(_winner);
        if (GameManager.players[Client.instance.myId] != null)
        {
            GameManager.players[Client.instance.myId].carryingWeapon = false;
        }
    }

    public static void DrawedWeapon(Packet _packet)
    {
        int _id = _packet.ReadInt();
        int _showing = _packet.ReadInt();
        int _weapon = _packet.ReadInt();

        GameManager.players[_id].DrawWeapon(_showing, _weapon);
    }
}
