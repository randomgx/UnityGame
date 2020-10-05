using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkDebug : MonoBehaviour
{
    public static NetworkDebug instance;

    public bool ping;
    public float time;

    public TextMeshProUGUI pingtxt;
    public TextMeshProUGUI packetup;
    public TextMeshProUGUI packetdown;
    public TextMeshProUGUI byteup;
    public TextMeshProUGUI bytedown;

    public List<Packet> packetsReceived = new List<Packet>();
    public List<Packet> packetsSent = new List<Packet>();

    int sentLength;
    int receivedLength;

    private void Awake()
    {
        if (instance == null)
        {
            InvokeRepeating("ResetPacketStats", 1, 1);
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Update()
    {
        if(ping)
        {
            time += Time.deltaTime;
        }
    }

    public void AddSentByte(int b)
    {
        sentLength += b;
    }

    public void AddReceivedByte(int b)
    {
        receivedLength += b;
    }

    private void ResetPacketStats()
    {
        packetup.text = string.Format("packets up/s: {0}", packetsSent.Count);
        packetdown.text = string.Format("packets dn/s: {0}", packetsReceived.Count);
        byteup.text = string.Format("bytes up/s: {0}", sentLength);
        bytedown.text = string.Format("bytes dn/s: {0}", receivedLength);

        sentLength = 0;
        receivedLength = 0;
        packetsReceived.Clear();
        packetsSent.Clear();
    }
}
