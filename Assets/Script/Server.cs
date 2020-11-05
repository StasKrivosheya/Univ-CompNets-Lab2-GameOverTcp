using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    public int port = 6321;

    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    private TcpListener server;
    private bool serverStarted;

    public void Init()
    {
        DontDestroyOnLoad(gameObject);

        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error (In Server Init): " + e.Message);
        }
    }

    private void Update()
    {
        if (!serverStarted)
        {
            return;
        }

        foreach (ServerClient client in clients)
        {
            // is the client still connected
            if (!IsConnected(client.tcp))
            {
                client.tcp.Close();
                disconnectList.Add(client);
                continue;
            }
            else
            {
                NetworkStream ns = client.tcp.GetStream();

                if (ns.DataAvailable)
                {
                    StreamReader reader = new StreamReader(ns, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(client, data);
                    }
                }
            }
        }

        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            // Tell another player (actually, host) somebody has disconnected
            string msg = "SLSTCNN|" + clients.Find(sc => sc.Equals(disconnectList[i])).clientName;

            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);

            Broadcast(msg, clients);
        }
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;

        string allUsers = "";
        foreach (ServerClient i in clients)
        {
            allUsers += i.clientName + '|';
        }

        try
        {
            ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
            clients.Add(sc);

            StartListening();
            Broadcast("SWHO|" + allUsers, clients[clients.Count - 1]);
        }
        catch (Exception e)
        {
            Debug.Log("Server AcceptTcpClient error: " + e.Message);
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

    //Server Send
    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient c in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error: " + e.Message);
            }
        }
    }
    private void Broadcast(string data, ServerClient c)
    {
        List<ServerClient> sc = new List<ServerClient> { c };
        Broadcast(data, sc);
    }

    // Server Read
    private void OnIncomingData(ServerClient c, string data)
    {
        Debug.Log("Server: " + data);

        string[] dataPart = data.Split('|');

        switch (dataPart[0])
        {
            case "CWHO":
                c.clientName = dataPart[1];
                c.isHost = dataPart[2] == "1";
                // Server ConNNection
                Broadcast("SCNN|" + c.clientName, clients);
                break;
            case "CMOV":
                Broadcast("SMOV|" +
                          dataPart[1] + "|" +
                          dataPart[2] + "|" +
                          dataPart[3] + "|" +
                          dataPart[4],
                    clients);
                Broadcast(data, clients);
                break;
        }
    }

    public void StopListener()
    {
        server.Stop();
    }
}

public class ServerClient
{
    public string clientName;
    public TcpClient tcp;
    public bool isHost;

    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
}
