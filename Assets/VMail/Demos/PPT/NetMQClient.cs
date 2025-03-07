using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

public class NetMQClient : MonoBehaviour
{
    private Thread clientThread;
    private bool running;
    private SubscriberSocket subSocket;
    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>(); // Thread-safe queue

    [System.Serializable]
    public class SlideChangeEvent : UnityEvent<int> { } // Unity Event for slide changes
    public SlideChangeEvent OnSlideChanged; // Exposed Unity Event

    void Start()
    {
        StartClient();
    }

    private void StartClient()
    {
        running = true;
        clientThread = new Thread(ClientLoop);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ClientLoop()
    {
        AsyncIO.ForceDotNet.Force(); // Required for NetMQ in Unity

        using (subSocket = new SubscriberSocket())
        {
            subSocket.Connect("tcp://localhost:5557"); // Change to match your server
            subSocket.Subscribe(""); // Subscribe to all messages

            Debug.Log("🎧 NetMQ Subscriber Connected...");

            while (running)
            {
                try
                {
                    // Use non-blocking receive to prevent Unity from freezing
                    if (subSocket.TryReceiveFrameString(out string message))
                    {
                        Debug.Log($"📩 Received: {message}");
                        ProcessMessage(message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"⚠️ NetMQ Error: {e.Message}");
                }
            }
        }
    }

    private void ProcessMessage(string jsonMessage)
    {
        try
        {
            JObject parsedMessage = JObject.Parse(jsonMessage);
            string eventType = parsedMessage["event"]?.ToString();

            if (eventType == "SlideChanged")
            {
                int slideNumber = parsedMessage["data"]?.Value<int>() ?? -1;
                Debug.Log($"📜 Slide Changed Event: {slideNumber}");

                // Queue action to run on Unity main thread using ConcurrentQueue
                mainThreadActions.Enqueue(() => OnSlideChanged?.Invoke(slideNumber));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"⚠️ JSON Parsing Error: {e.Message}");
        }
    }

    void Update()
    {
        // Execute queued actions on the main thread
        while (mainThreadActions.TryDequeue(out Action action))
        {
            action?.Invoke();
        }
    }

    void OnDestroy()
    {
        StopClient();
    }

    private void StopClient()
    {
        if (!running) return;

        running = false;
        clientThread?.Join(); // Wait for the thread to exit
        subSocket?.Dispose();
        NetMQConfig.Cleanup();
        Debug.Log("🔌 NetMQ Client Stopped.");
    }
}
