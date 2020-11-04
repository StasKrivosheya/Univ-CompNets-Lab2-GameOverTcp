using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public string clientName;
    public bool isHost = false;

    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public List<GameClient> players = new List<GameClient>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

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
        Debug.Log("Client: " + data);
        
        string[] dataPart = data.Split('|');

        switch (dataPart[0])
        {
            case "SWHO":
                for (int i = 1; i < dataPart.Length - 1; i++)
                {
                    UserConnected(dataPart[i], false);
                }

                Send("CWHO|" + clientName + "|" + (isHost ? 1 : 0).ToString());
                break;
            // Somebody has connected (myself)
            case "SCNN":
                UserConnected(dataPart[1], false); //
                break;
            case "SMOV":
                CheckersBoard.Instance.TryMove(
                    int.Parse(dataPart[1]),
                    int.Parse(dataPart[2]),
                    int.Parse(dataPart[3]),
                    int.Parse(dataPart[4]));
                break;
        }
    }

    private void UserConnected(string name, bool isHost)
    {
        GameClient c = new GameClient();
        c.name = name;
        //c.isHost = isHost;

        players.Add(c);

        if (players.Count == 2)
        {
            GameManager.Instance.StartGame();
        }
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