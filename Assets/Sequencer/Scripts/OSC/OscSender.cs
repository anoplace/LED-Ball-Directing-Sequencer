using UnityEngine;
using System;
using System.Net.Sockets;
using Osc;

/// <summary>
/// Osc sender.
/// </summary>
public class OscSender : MonoBehaviour
{

	public string ip = "localhost";
    public int
        port = 6666;

    private UdpClient
        mUdpClient;

    void Awake ()
	{
		mUdpClient = new UdpClient ();
		try {
			mUdpClient.Connect (ip, port);
		} catch (Exception e) {
			mUdpClient = null;
			Debug.LogError (e);
		}
	}

	void OnDisable ()
	{
		if (mUdpClient != null)
			mUdpClient.Close ();
	}

	public void Send (MessageEncoder encoder)
	{
		if (mUdpClient == null) {
			Debug.LogError ("Not Connected");
			return;
		}

        byte[] data = encoder.Encode();
        try{
            mUdpClient.Send(data, data.Length);
        }
        catch (System.Exception e)
        {
//             Debug.LogError(e);
        }
	}

}

