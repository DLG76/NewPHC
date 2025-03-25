using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TokenManager : SingletonPersistent<TokenManager>
{
    private static string refreshToken;
    private static string accessToken;

    public static string AccessToken => accessToken;

    public override void Awake()
    {
        base.Awake();

        refreshToken = PlayerPrefs.GetString("refreshToken", null);
        accessToken = PlayerPrefs.GetString("accessToken", null);
    }

    public void SetToken(string accessToken, string refreshToken)
    {
        TokenManager.accessToken = accessToken;
        TokenManager.refreshToken = refreshToken;
        PlayerPrefs.SetString("accessToken", accessToken);
        PlayerPrefs.SetString("refreshToken", refreshToken);
        PlayerPrefs.Save();
    }

    public void ClearToken()
    {
        accessToken = null;
        refreshToken = null;
        PlayerPrefs.DeleteKey("accessToken");
        PlayerPrefs.DeleteKey("refreshToken");
        PlayerPrefs.Save();
    }

    public IEnumerator RefreshToken(System.Action<bool, long, string> callback)
    {
        UnityWebRequest request = new UnityWebRequest(Api.REFRESH_TOKEN_URL, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", refreshToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            JObject responseJson = JObject.Parse(responseText);
            accessToken = responseJson["accessToken"].ToString();
            PlayerPrefs.SetString("accessToken", accessToken);
            PlayerPrefs.Save();

            callback?.Invoke(true, 0, null);
        }
        else
        {
            PlayerPrefs.DeleteKey("accessToken");
            PlayerPrefs.DeleteKey("refreshToken");
            PlayerPrefs.Save();
            callback?.Invoke(false, request.responseCode, request.error);
        }
	}

	public IEnumerator HandleRequest(UnityWebRequest request, System.Action<UnityWebRequest> onRequestSuccess)
    {
        yield return HandleRequest(request, new Dictionary<string, string>(), onRequestSuccess);
	}


	public IEnumerator HandleRequest(UnityWebRequest request, Dictionary<string, string> otherHeaders, System.Action<UnityWebRequest> onRequestSuccess)
    {
        request.SetRequestHeader("Authorization", accessToken);
        foreach (var header in otherHeaders)
            request.SetRequestHeader(header.Key, header.Value);

        yield return request.SendWebRequest();

        if (request.responseCode == 401)
        {
            bool refreshSuccess = false;
            yield return RefreshToken((success, statusCode, error) =>
            {
                if (success)
                {
                    refreshSuccess = true;
                }
            });

            if (refreshSuccess)
			{
				UnityWebRequest newRequest = new UnityWebRequest(request.url, request.method);
				newRequest.downloadHandler = new DownloadHandlerBuffer();
				newRequest.uploadHandler = request.uploadHandler;

				yield return HandleRequest(newRequest, otherHeaders, onRequestSuccess);
            }
            else
            {
                DatabaseManager.Instance.GoToLoginScene();
            }
        }
        else
        {
            onRequestSuccess?.Invoke(request);
        }
    }
}
