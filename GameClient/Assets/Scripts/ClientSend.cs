using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    /// <summary>Sends a packet to the server via TCP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to the server via UDP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    /// <summary>Lets the server know that the welcome message was received.</summary>
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
            NetworkDebug.instance.packetsSent.Add(_packet);
            NetworkDebug.instance.AddSentByte(_packet.Length());
        }
    }

    /// <summary>Sends player input to the server.</summary>
    /// <param name="_inputs"></param>
    public static void PlayerMovement(bool[] _inputs)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);
            _packet.Write(GameManager.players[Client.instance.myId].camTransform.rotation);

            SendUDPData(_packet);
            NetworkDebug.instance.packetsSent.Add(_packet);
            NetworkDebug.instance.AddSentByte(_packet.Length());
        }
    }

    public static void PlayerShoot(Vector3 _facing)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerShoot))
        {
            _packet.Write(_facing);

            SendTCPData(_packet);
            NetworkDebug.instance.packetsSent.Add(_packet);
            NetworkDebug.instance.AddSentByte(_packet.Length());
        }
    }

    public static void PlayerShootClient(PlayerManager _player)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerShootClient))
        {
            _packet.Write(_player.id);

            SendUDPData(_packet);
            NetworkDebug.instance.packetsSent.Add(_packet);
            NetworkDebug.instance.AddSentByte(_packet.Length());
        }
    }

    public static void PlayerDrawItem(bool _drawn)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerDrawItem))
        {
            //work to do
            _packet.Write(_drawn);

            SendUDPData(_packet);

            NetworkDebug.instance.packetsSent.Add(_packet);
            NetworkDebug.instance.AddSentByte(_packet.Length());
        }
    }

    public static void SendPing()
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendPing))
        {
            NetworkDebug.instance.time = 0;
            SendUDPData(_packet);
            NetworkDebug.instance.ping = true;
        }
    }
    #endregion
}
