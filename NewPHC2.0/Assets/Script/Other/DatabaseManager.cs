using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class DatabaseManager : SingletonPersistent<DatabaseManager>
{
    [SerializeField] private string loginScene;

    private TokenManager tokenManager;

    public override void Awake()
    {
        base.Awake();

        var tokenManagerObject = new GameObject("TokenManager");
        tokenManager = tokenManagerObject.AddComponent<TokenManager>();
    }

    public IEnumerator Login(string username, string password, System.Action<bool, string> callback)
    {
        string jsonPayload = JsonConvert.SerializeObject(new JObject
        {
            new JProperty("username", username),
            new JProperty("password", password)
        });
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(Api.LOGIN_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            JObject responseJson = JObject.Parse(responseText);

            tokenManager.SetToken(responseJson["accessToken"].ToString(), responseJson["refreshToken"].ToString());

            callback?.Invoke(true, null);
        }
        else
        {
            tokenManager.ClearToken();
            Debug.LogError(request.responseCode + ": " + request.error);
        }
    }

    public IEnumerator GetProfile(System.Action<bool, JObject> callback)
    {
        UnityWebRequest request = new UnityWebRequest(Api.PROFILE_URL, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return tokenManager.HandleRequest(request, (responseRequest) =>
        {
            if (responseRequest.result == UnityWebRequest.Result.Success)
            {
                string responseText = responseRequest.downloadHandler.text;
                callback?.Invoke(true, JObject.Parse(responseText));
            }
            else
            {
                Debug.LogError(request.responseCode + ": " + request.error);
                callback?.Invoke(false, null);
            }
        });
    }

    public IEnumerator GetUser(string userId, System.Action<bool, JObject> callback)
    {
        UnityWebRequest request = new UnityWebRequest(Api.GetUsersUrl(userId), "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return tokenManager.HandleRequest(request, (responseRequest) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                callback?.Invoke(true, JObject.Parse(responseText));
            }
            else
            {
                Debug.LogError(request.responseCode + ": " + request.error);
                callback?.Invoke(false, null);
            }
        });
    }

    public IEnumerator GetStages(System.Action<bool, List<JObject>, List<JObject>> callback)
    {
        UnityWebRequest request = new UnityWebRequest(Api.GET_STAGES_URL, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return tokenManager.HandleRequest(request, (responseRequest) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                JObject responseJson = JObject.Parse(responseText);
                List<JObject> clearedStages = responseJson["clearedStages"]?.ToObject<List<JObject>>();
                List<JObject> stages = responseJson["stages"]?.ToObject<List<JObject>>();
                callback?.Invoke(true, clearedStages, stages);
            }
            else
            {
                Debug.LogError(request.responseCode + ": " + request.error);
                callback?.Invoke(false, null, null);
            }
        });
    }

    public IEnumerator FinishedDungeon(string stageId, bool isWinner, double time, System.Action<bool> callback)
    {
        string jsonPayload = JsonConvert.SerializeObject(new JObject
        {
            new JProperty("isWinner", isWinner),
            new JProperty("time", time),
        });
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(Api.ClearDungeonUrl(stageId), "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        var header = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };

        yield return tokenManager.HandleRequest(request, header, (responseRequest) =>
        {
            callback?.Invoke(request.result == UnityWebRequest.Result.Success);
        });
    }

    public IEnumerator SendCode(string stageId, string code, System.Action<bool> callback)
    {
        string jsonPayload = JsonConvert.SerializeObject(new JObject
        {
            new JProperty("code", code)
        });
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(Api.SendCodeUrl(stageId), "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        var header = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };

        yield return tokenManager.HandleRequest(request, header, (responseRequest) =>
        {
            callback?.Invoke(request.result == UnityWebRequest.Result.Success);
        });
    }

    public IEnumerator RemoveItem(string itemId, System.Action<bool, List<JObject>> callback)
    {
        yield return RemoveItem(itemId, 1, callback);
    }

    public IEnumerator RemoveItem(string itemId, int count, System.Action<bool, List<JObject>> callback)
    {
        UnityWebRequest request = new UnityWebRequest(Api.RemoveItemUrl(itemId, count), "POST");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return tokenManager.HandleRequest(request, (responseRequest) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                JObject responseJson = JObject.Parse(responseText);
                List<JObject> inventory = responseJson["inventory"]?.ToObject<List<JObject>>();
                callback?.Invoke(true, inventory);
            }
            else
            {
                callback?.Invoke(false, null);
            }
        });
    }

    public IEnumerator UpdateEquipment(Equipment equipment, System.Action<bool, Equipment, List<JObject>> callback)
    {
        string jsonPayload = JsonConvert.SerializeObject(new JObject
        {
            new JProperty("weapon1", equipment.weapon1),
            new JProperty("weapon2", equipment.weapon2),
            new JProperty("weapon3", equipment.weapon3),
            new JProperty("core", equipment.core)
        });
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(Api.UPDATE_EQUIPMENT, "PUT");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        var header = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };

        yield return tokenManager.HandleRequest(request, header, (responseRequest) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                JObject responseJson = JObject.Parse(responseText);
                Equipment newEquipment = new Equipment(responseJson["equipment"]?.ToObject<JObject>());
                List<JObject> inventory = responseJson["inventory"]?.ToObject<List<JObject>>();
                callback?.Invoke(true, newEquipment, inventory);
            }
            else
            {
                callback?.Invoke(false, null, null);
            }
        });
    }

    public void GoToLoginScene()
    {
        //SceneManager.LoadScene(loginScene);
    }
}

public class Api
{
    public static string BASE_URL = "http://localhost:23000";
    public static string LOGIN_URL = BASE_URL + "/login";
    public static string REFRESH_TOKEN_URL = BASE_URL + "/refreshToken";
    public static string GET_STAGES_URL = BASE_URL + "/stages";

    public static string PROFILE_URL = BASE_URL + "/profile";

    private static string USERS_URL = BASE_URL + "/users";
    public static string GetUsersUrl(string userId) => $"{USERS_URL}/{userId}";

    private static string REMOVE_ITEM_URL = BASE_URL + "/item/remove";
    public static string RemoveItemUrl(string itemId, int count) => $"{REMOVE_ITEM_URL}/{itemId}?count={count}";

    public static string UPDATE_EQUIPMENT = BASE_URL + "/item/equipment/update";
    public static string FUSE_VOID_URL = BASE_URL + "/item/void/fuse";

    public static string CLEAR_DUNGEON_URL = BASE_URL + "/dungeon/clear";
    public static string ClearDungeonUrl(string stageId) => $"{CLEAR_DUNGEON_URL}/{stageId}";

    public static string SEND_CODE_URL = BASE_URL + "/code/send";
    public static string SendCodeUrl(string stageId) => $"{SEND_CODE_URL}/{stageId}";
}