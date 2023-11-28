// Server.cs
using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using TMPro;

public class Server : MonoBehaviour
{
    [SerializeField] private int port = 7777;
    [SerializeField] private TextMeshProUGUI statusText;
    
    private TcpListener tcpListener;
    private List<TcpClient> clients = new List<TcpClient>();
    private bool isRunning = false;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    
    void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            statusText.text += "\n" + message;
        }
    }
    
    public void StartServer()
    {
        if (isRunning) return;
        
        isRunning = true;
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        Thread acceptThread = new Thread(AcceptClients);
        acceptThread.Start();
        
        UpdateStatusText("Server started...");
    }

    private void AcceptClients()
    {
        try
        {
            while (isRunning)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                lock (clients)
                {
                    clients.Add(client);
                }
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e.ToString());
        }
        finally
        {
            tcpListener.Stop();
        }
    }

    private void HandleClient(TcpClient client)
    {
        var endPoint = client.Client.RemoteEndPoint.ToString();
        UpdateStatusText($"Client connected: {endPoint}");

        using (NetworkStream stream = client.GetStream())
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    while (client.Connected)
                    {
                        int count = reader.ReadInt32();
                        List<int> numbers = new List<int>();
                        for (int i = 0; i < count; i++)
                        {
                            numbers.Add(reader.ReadInt32());
                        }
                        UpdateStatusText($"Received from {endPoint}: {string.Join(", ", numbers)}");
                    }
                }
                catch (Exception e)
                {
                    UpdateStatusText($"Client {endPoint} error: {e.Message}");
                }
            }
        }

        lock (clients)
        {
            clients.Remove(client);
        }
        UpdateStatusText($"Client disconnected: {endPoint}");
    }

    private void UpdateStatusText(string message)
    {
        Debug.Log(message);
        // Since this method is called from another thread, we need to use Unity's main thread to update the UI
        messageQueue.Enqueue(message);
    }

    private void OnApplicationQuit()
    {
        isRunning = false;
        tcpListener?.Stop();
        lock (clients)
        {
            foreach (TcpClient client in clients)
            {
                client.Close();
            }
        }
    }
}
