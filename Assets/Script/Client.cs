using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public string clientName;
    public bool isHost = false;

    public bool OpponentDisconnected { get; private set; }

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

        //try
        //{
        socket = new TcpClient(host, port);
        stream = socket.GetStream();
        writer = new StreamWriter(stream);
        reader = new StreamReader(stream);

        socketReady = true;
        //}
        //catch (Exception e)
        //{
        //    Debug.Log("Socket error (in Client ConnectToServer): " + e.Message);
        //}

        return socketReady;
    }

    private void Update()
    {
        if (socketReady)
        {
            if (!IsConnected(socket))
            {
                Debug.Log("Server unexpectedly lost its connection!");

                CloseSocket();

                OpponentDisconnected = true;

                return;
            }

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

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
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
        Debug.Log($"Client {clientName} received: {data}");

        string[] dataPart = data.Split('|');

        switch (dataPart[0])
        {
            case "Server:WhoIsThere":
                for (int i = 1; i < dataPart.Length - 1; i++)
                {
                    UserConnected(dataPart[i], false);
                }

                Send("Client:IAmHere|" + clientName + "|" + (isHost ? 1 : 0).ToString());
                break;
            // Somebody has connected (or myself)
            case "Server:NewConnection":
                UserConnected(dataPart[1], false);
                break;
            case "Server:Move":
                CheckersBoard.Instance.TryMove(
                    int.Parse(dataPart[1]),
                    int.Parse(dataPart[2]),
                    int.Parse(dataPart[3]),
                    int.Parse(dataPart[4]));
                break;
            case "Server:LostConnection":
                Debug.Log($"Client {dataPart[1]} lost connection!");
                OpponentDisconnected = true;
                GameManager.Instance.gameInterrupted = true;
                break;
        }
    }

    private void UserConnected(string name, bool isHost)
    {
        GameClient c = new GameClient();
        c.name = name;

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