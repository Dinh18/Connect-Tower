using UnityEngine;
using UnityEditor;
using System.Net;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System;

public class AIUnityBridge : EditorWindow
{
    private static HttpListener listener;
    private static Thread listenerThread;
    private static bool isRunning = false;
    private const int PORT = 8080;

    // Hàng đợi để thực thi các lệnh trên Main Thread của Unity
    private static Queue<Action> mainThreadActions = new Queue<Action>();

    [MenuItem("Tools/AI Bridge/Control Panel")]
    public static void ShowWindow()
    {
        GetWindow<AIUnityBridge>("AI Bridge");
    }

    private void OnGUI()
    {
        GUILayout.Label("AI Unity Bridge (MCP-like)", EditorStyles.boldLabel);
        
        if (isRunning)
        {
            EditorGUILayout.HelpBox($"Server is RUNNING on http://localhost:{PORT}/", MessageType.Info);
            if (GUILayout.Button("Stop Server", GUILayout.Height(30)))
            {
                StopServer();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Server is STOPPED.", MessageType.Warning);
            if (GUILayout.Button("Start Server", GUILayout.Height(30)))
            {
                StartServer();
            }
        }
    }

    private static void StartServer()
    {
        if (isRunning) return;

        listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{PORT}/");
        try
        {
            listener.Start();
            isRunning = true;
            
            listenerThread = new Thread(ListenForRequests);
            listenerThread.IsBackground = true;
            listenerThread.Start();
            
            EditorApplication.update += UpdateMainThread;
            Debug.Log($"[AI Bridge] Started on port {PORT}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AI Bridge] Failed to start. Make sure port {PORT} is not in use: {e.Message}");
            isRunning = false;
        }
    }

    private static void StopServer()
    {
        if (!isRunning) return;
        
        EditorApplication.update -= UpdateMainThread;
        isRunning = false;
        
        if (listener != null)
        {
            listener.Stop();
            listener.Close();
            listener = null;
        }
        
        if (listenerThread != null && listenerThread.IsAlive)
        {
            listenerThread.Abort();
        }
        
        Debug.Log("[AI Bridge] Stopped.");
    }

    private static void ListenForRequests()
    {
        while (isRunning && listener != null && listener.IsListening)
        {
            try
            {
                var context = listener.GetContext(); // Chờ request (Blocking)
                ProcessRequest(context);
            }
            catch (HttpListenerException)
            {
                // Xảy ra khi đóng listener
            }
            catch (Exception e)
            {
                Debug.LogError($"[AI Bridge] Error: {e.Message}");
            }
        }
    }

    private static void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        
        string responseString = "{\"status\":\"error\", \"message\":\"Unknown endpoint\"}";
        int statusCode = 404;

        if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/ping")
        {
            responseString = "{\"status\":\"ok\", \"message\":\"AI Bridge is alive!\"}";
            statusCode = 200;
            SendResponse(response, responseString, statusCode);
        }
        else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/hierarchy")
        {
            // Mọi thao tác với Unity API (lấy Scene, GameObjects) đều phải chạy trên Main Thread
            bool isDone = false;
            
            lock(mainThreadActions)
            {
                mainThreadActions.Enqueue(() => {
                    try {
                        var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                        List<string> names = new List<string>();
                        foreach(var go in rootObjects) {
                            names.Add(go.name);
                        }
                        
                        // Tạo JSON đơn giản
                        StringBuilder sb = new StringBuilder();
                        sb.Append("{\"status\":\"ok\", \"scene\":\"" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "\", \"root_objects\":[");
                        for(int i=0; i<names.Count; i++) {
                            sb.Append("\"" + names[i] + "\"");
                            if(i < names.Count - 1) sb.Append(",");
                        }
                        sb.Append("]}");
                        
                        responseString = sb.ToString();
                        statusCode = 200;
                    } catch(Exception e) {
                        responseString = "{\"status\":\"error\", \"message\":\"" + e.Message + "\"}";
                        statusCode = 500;
                    } finally {
                        isDone = true;
                    }
                });
            }
            
            // Chờ cho đến khi Main Thread xử lý xong
            while(!isDone && isRunning) { Thread.Sleep(10); }
            SendResponse(response, responseString, statusCode);
        }
        else
        {
            SendResponse(response, responseString, statusCode);
        }
    }

    private static void SendResponse(HttpListenerResponse response, string responseString, int statusCode)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        
        try {
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        } catch {}
    }

    private static void UpdateMainThread()
    {
        if (mainThreadActions.Count > 0)
        {
            lock (mainThreadActions)
            {
                while (mainThreadActions.Count > 0)
                {
                    var action = mainThreadActions.Dequeue();
                    action?.Invoke();
                }
            }
        }
    }
}
