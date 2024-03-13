using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server2 : MonoBehaviour
{
    [SerializeField] private int port = 7777;
    private List<Socket> clients = new List<Socket>();
    private bool isRunning = false;
    private Socket listener;
    private List<int> globalIDs = new List<int>();

    private ScenicSpotSelectionManager scenicSpotSelectionManager;

    private void Start()
    {
        scenicSpotSelectionManager = ScenicSpotSelectionManager.Instance;
    }

    public void StartServer()
    {
        if (isRunning) return;

        isRunning = true;
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(localEndPoint);
        listener.Listen(10);

        Thread acceptThread = new Thread(() =>
        {
            while (isRunning)
            {
                Socket client = listener.Accept();
                lock (clients)
                {
                    clients.Add(client);
                    Debug.Log($"Client connected: {client.RemoteEndPoint.ToString()}");
                }
                BroadcastIDsToClient(client); //给新加入的客户端同步
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        });
        acceptThread.Start();

        Debug.Log("Server started...");
    }

    public void UpdateGlobalIDs(List<int> newIDs)
    {
        lock (globalIDs)
        {
            globalIDs = newIDs;
        }
        BroadcastIDs();
    }

    private void HandleClient(Socket client)
    {
        try
        {
            while (isRunning)
            {
                byte[] bytes = new byte[1024];
                int bytesRec = client.Receive(bytes);
                string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                // Process received data...
                ReceiveData(data);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Client error: {e.Message}");
        }
        finally
        {
            client.Close();
            lock (clients)
            {
                clients.Remove(client);
            }
        }
    }

    private void ReceiveData(string data)
    {
        data = data.Replace("<EOF>", "");

        lock (globalIDs)
        {
            if (string.IsNullOrEmpty(data))
            {
                globalIDs = new List<int>();
            }
            else
            {
                globalIDs = data.Split(',').Select(int.Parse).ToList();
            }
        }

        MainThreadDispatcher.ExecuteOnMainThread(() => {
            scenicSpotSelectionManager.UpdateSelectedScenicSpotIDList(globalIDs);
        });
        BroadcastIDs();
    }

    private void BroadcastIDs()
    {
        byte[] msg = Encoding.ASCII.GetBytes(string.Join(",", globalIDs) + "<EOF>");
        lock (clients)
        {
            foreach (Socket client in clients)
            {
                if (client.Connected)
                {
                    client.Send(msg);
                }
            }
        }
        Debug.Log($"Broadcasted IDs: {string.Join(", ", globalIDs)}");
    }

    private void BroadcastIDsToClient(Socket client)
    {
        if (client.Connected)
        {
            byte[] msg = Encoding.ASCII.GetBytes(string.Join(",", globalIDs) + "<EOF>");
            client.Send(msg);
            Debug.Log($"Broadcasted IDs to new client: {string.Join(", ", globalIDs)}");
        }
        
    }

    private void OnApplicationQuit()
    {
        isRunning = false;
        lock (clients)
        {
            foreach (Socket client in clients)
            {
                client.Close();
            }
        }
        listener.Close();
    }
}
