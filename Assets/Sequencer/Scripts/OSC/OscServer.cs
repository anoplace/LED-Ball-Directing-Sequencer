using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Osc;

public class OscServer : MonoBehaviour
{
    public int listenPort = 6666;
    public string OSCPath = "/ball";
    UdpClient udpClient;
    IPEndPoint endPoint;
    Parser osc = new Parser();

    void Start()
    {
        endPoint = new IPEndPoint(IPAddress.Any, listenPort);
        udpClient = new UdpClient(endPoint);
    }

    void Update()
    {
        while (udpClient.Available > 0)
        {
            osc.FeedData(udpClient.Receive(ref endPoint));
        }


        while (osc.MessageCount > 0)
        {
            var msg = osc.PopMessage();

            if (msg.path.StartsWith(OSCPath))
            {
                Debug.Log(msg.path + ", " + msg.data[0]);
            }
        }

    }
}