﻿using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public bool ConnectToServer(string host, int port)
    {
        if (socketReady)
        {
            return false;
        }

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }

        return socketReady;
    }

    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();

                if (data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }

    // Sending messages to the server
    public void Send(string data)
    {
        if (!socketReady)
        {
            return;
        }

        writer.WriteLine(data);
        writer.Flush();
    }

    // Read messages from the server
    private void OnIncomingData(string data)
    {
        Debug.Log(data);
    }

    // inherited from MonoBehaviour
    private void OnApplicationQuit()
    {
        CloseSocket();
    }
    // This is also called when the object is destroyed and can be used for any cleanup code
    private void OnDisable()
    {
        CloseSocket();
    }

    private void CloseSocket()
    {
        if (!socketReady)
        {
            return;
        }

        writer.Close();
        reader.Close();
        socket.Close();

        socketReady = false;
    }
}

public class GameClient
{
    public string name;
    public bool isHost;
}