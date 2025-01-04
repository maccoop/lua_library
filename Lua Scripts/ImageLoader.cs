using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ImageLoader : MonoBehaviour
{
    private static ImageLoader _instance;
    private static ImageLoader Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new GameObject("ImageLoader").AddComponent<ImageLoader>();
            }
            return _instance;
        }
    }

    public static void LoadImage(string url, SpriteRenderer renderer)
    {
        Instance.StartCoroutine(DownloadImageCoroutine(url, (texture) =>
        {
            renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }));
    }

    public static void LoadImageCallback(string url, System.Action<Texture2D> callback)
    {
        Instance.StartCoroutine(DownloadImageCoroutine(url, callback));
    }

    private static IEnumerator DownloadImageCoroutine(string url, System.Action<Texture2D> callback)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error downloading image: {request.error}");
                callback?.Invoke(null);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                callback?.Invoke(texture);
            }
        }
    }
}
