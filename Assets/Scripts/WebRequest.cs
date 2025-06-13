using System;
using System.Collections;
using TreeEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class WebRequest
{
    //http://www.pd4-examwebservice.com:5216/api/maze/get/by-name/Maze8x8
    //general get request that will return a string
    public static string Get(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SendWebRequest();
            while (!request.isDone)
            {
                // Wait for the request to complete
                //log the progress if needed
                Debug.Log($"Web request progress: {request.downloadProgress * 100}%");
            }
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }
            return request.downloadHandler.text;
        }
    }

    //general post request that will return a string
    public static string Post(string url, string jsonData)
    {
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, jsonData))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SendWebRequest();
            while (!request.isDone)
            {
                // Wait for the request to complete
                //log the progress if needed
                Debug.Log($"Web request progress: {request.downloadProgress * 100}%");
            }
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }
            return request.downloadHandler.text;
        }
    }

    //general put request that will return a string
    public static string Put(string url, string jsonData)
    {
        using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SendWebRequest();
            while (!request.isDone)
            {
                // Wait for the request to complete
                //log the progress if needed
                Debug.Log($"Web request progress: {request.downloadProgress * 100}%");
            }
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }
            return request.downloadHandler.text;
        }
    }

    //general delete request that will return a string
    public static string Delete(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SendWebRequest();
            while (!request.isDone)
            {
                // Wait for the request to complete
                //log the progress if needed
                Debug.Log($"Web request progress: {request.downloadProgress * 100}%");
            }
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }
            return request.downloadHandler.text;
        }
    }


    public static Texture2D GetImage(string url)
    {
        string cacheBustedUrl = url + "?t=" + DateTime.UtcNow.Ticks; //prevent caching previous textures

        //UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(cacheBustedUrl);
        //yield return uwr.SendWebRequest();
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(cacheBustedUrl);
        uwr.SendWebRequest();
        while (!uwr.isDone)
        {
            // Wait for the request to complete
            //log the progress if needed
            Debug.Log($"Image download progress: {uwr.downloadProgress * 100}%");
        }

        // Check for errors
        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(uwr.error);
            Debug.LogError($"Error downloading image: {uwr.error}");
            return null; // Return null if the request failed
        }
        else
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
            Debug.Log("Image downloaded successfully.");

            //return the texture to the caller
            return tex;
        }
        return null; // Return null if the request failed
    }
}
