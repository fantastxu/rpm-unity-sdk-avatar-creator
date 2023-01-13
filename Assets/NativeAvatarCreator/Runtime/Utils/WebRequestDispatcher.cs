using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NativeAvatarCreator
{
    public enum Method
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }

    public static class WebRequestDispatcher
    {
        private const int TIMEOUT = 60;

        public static async Task<Response> SendRequest(
            string url,
            Method method,
            Dictionary<string, string> headers = null,
            string payload = null,
            DownloadHandler downloadHandler = default,
            CancellationToken token = new CancellationToken())
        {
            using var request = new UnityWebRequest();
            request.timeout = TIMEOUT;
            request.url = url;
            request.method = method.ToString();

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            downloadHandler ??= new DownloadHandlerBuffer();

            request.downloadHandler = downloadHandler;

            if (!string.IsNullOrEmpty(payload))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
                request.uploadHandler = new UploadHandlerRaw(bytes);
            }

            var asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone && !token.IsCancellationRequested)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.downloadHandler.text);
                throw new Exception(request.error);
            }

            Texture texture = null;
            if (downloadHandler is DownloadHandlerTexture downloadHandlerTexture)
            {
                texture = downloadHandlerTexture.texture;
            }

            return new Response
            {
                Text = request.downloadHandler.text,
                Data = request.downloadHandler.data,
                Texture = texture
            };
        }
    }
}
