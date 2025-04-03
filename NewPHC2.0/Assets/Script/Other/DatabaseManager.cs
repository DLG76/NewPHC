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
    public static string LoginScene = "Lobby";

    private TokenManager tokenManager;
    private GameObject loadingCanvas;

    private bool loading = false;

    public override void Awake()
    {
        base.Awake();

        var tokenManagerObject = new GameObject("TokenManager");
        tokenManager = tokenManagerObject.AddComponent<TokenManager>();
        tokenManagerObject.transform.SetParent(transform);

        loadingCanvas = Instantiate(Resources.Load<GameObject>("UI/LoadingCanvas"), transform);
        loadingCanvas.SetActive(false);
    }

    private void ShowLoading()
    {
        loading = true;
        StartCoroutine(ShowLoadingIE());
    }

    private IEnumerator ShowLoadingIE()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        
        if (loading)
            loadingCanvas.SetActive(true);
    }

    private void HideLoading()
    {
        loadingCanvas.SetActive(false);
        loading = false;
    }

    public IEnumerator Login(string username, string password, System.Action<bool, string> callback)
    {
        //ShowLoading();

        //var textAsset = Resources.Load<TextAsset>("TesterUser");

        //string responseText = textAsset.text;
        //JObject responseJson = JObject.Parse(responseText);

        //if (responseJson["name"]?.ToString() == username)
        //    if (responseJson["name"]?.ToString() == password)
        //        callback?.Invoke(true, null);
        //    else
        //        callback?.Invoke(false, "Password ไม่ถูกต้อง");
        //else
        //    callback?.Invoke(false, "ไม่พบ Username");

        //HideLoading();

        //yield break;

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

        ShowLoading();
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

        HideLoading();
    }

    public IEnumerator GetProfile(System.Action<bool, JObject> callback)
    {
        //ShowLoading();

        //var textAsset = Resources.Load<TextAsset>("TesterUser");

        //string responseText = textAsset.text;
        //callback?.Invoke(true, JObject.Parse(responseText));

        //HideLoading();

        //yield break;

        UnityWebRequest request = new UnityWebRequest(Api.PROFILE_URL, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        ShowLoading();

        yield return tokenManager.HandleRequest(request, (request) =>
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

        //HideLoading();
    }

    public IEnumerator GetUser(string userId, System.Action<bool, JObject> callback)
    {
        UnityWebRequest request = new UnityWebRequest(Api.GetUsersUrl(userId), "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        ShowLoading();

        yield return tokenManager.HandleRequest(request, (request) =>
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

        HideLoading();
    }

    public IEnumerator GetStages(System.Action<bool, List<JObject>, List<JObject>> callback)
    {
        //ShowLoading();

        //var textAsset = Resources.Load<TextAsset>("PHC.stages");

        //string responseText = textAsset.text;
        //JObject responseJson = JObject.Parse(responseText);
        //List<JObject> clearedStages = responseJson["clearedStages"]?.ToObject<List<JObject>>();
        //List<JObject> stages = responseJson["stages"]?.ToObject<List<JObject>>();
        //callback?.Invoke(true, clearedStages, stages);

        //HideLoading();

        //yield break;

        UnityWebRequest request = new UnityWebRequest(Api.GET_STAGES_URL, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        ShowLoading();

        yield return tokenManager.HandleRequest(request, (request) =>
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

        HideLoading();
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

        ShowLoading();

        yield return tokenManager.HandleRequest(request, header, (request) =>
        {
            callback?.Invoke(request.result == UnityWebRequest.Result.Success);
        });

        HideLoading();
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

        ShowLoading();

        yield return tokenManager.HandleRequest(request, header, (request) =>
        {
            callback?.Invoke(request.result == UnityWebRequest.Result.Success);
        });

        HideLoading();
    }

    public IEnumerator RemoveItem(string itemId, System.Action<bool, List<JObject>> callback)
    {
        yield return RemoveItem(itemId, 1, callback);
    }

    public IEnumerator RemoveItem(string itemId, int count, System.Action<bool, List<JObject>> callback)
    {
        UnityWebRequest request = new UnityWebRequest(Api.RemoveItemUrl(itemId, count), "POST");
        request.downloadHandler = new DownloadHandlerBuffer();

        ShowLoading();

        yield return tokenManager.HandleRequest(request, (request) =>
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

        HideLoading();
    }

    public IEnumerator UpdateEquipment(Equipment equipment, System.Action<bool, JObject, List<JObject>> callback)
    {
        string jsonPayload = JsonConvert.SerializeObject(new JObject
        {
            new JProperty("equipmentData", new JObject
                {
                    new JProperty("weapon1", equipment.weapon1?.id),
                    new JProperty("weapon2", equipment.weapon2?.id),
                    new JProperty("weapon3", equipment.weapon3?.id),
                    new JProperty("core", equipment.core?.id)
                }
            )
        });
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(Api.UPDATE_EQUIPMENT, "PUT");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        var header = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };

        ShowLoading();

        yield return tokenManager.HandleRequest(request, header, (request) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                JObject responseJson = JObject.Parse(responseText);
                JObject newEquipment = responseJson["equipment"]?.ToObject<JObject>();
                List<JObject> inventory = responseJson["inventory"]?.ToObject<List<JObject>>();
                callback?.Invoke(true, newEquipment, inventory);
            }
            else
            {
                callback?.Invoke(false, null, null);
            }
        });

        HideLoading();
    }

    public IEnumerator FuseVoid(Item item1, Item item2, System.Action<bool, Item, List<JObject>> callback)
    {
        string jsonPayload = JsonConvert.SerializeObject(new JObject
        {
            new JProperty("item1", item1.id),
            new JProperty("item2", item2.id),
        });
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(Api.FUSE_VOID_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        var header = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };

        ShowLoading();

        yield return tokenManager.HandleRequest(request, header, (request) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                JObject responseJson = JObject.Parse(responseText);
                JObject itemJson = responseJson["resultItem"]?.ToObject<JObject>();
                Item item;
                if (itemJson["type"]?.ToString() == "CoreItem")
                    item = new CoreItem(itemJson);
                else if (itemJson["type"]?.ToString() == "VoidItem")
                    item = new VoidItem(itemJson);
                else
                    item = new Item(itemJson);
                List<JObject> inventory = responseJson["inventory"]?.ToObject<List<JObject>>();
                callback?.Invoke(true, item, inventory);
            }
            else
            {
                callback?.Invoke(false, null, null);
            }
        });

        HideLoading();
    }

    public void GoToLoginScene()
    {
        SceneManager.LoadScene(LoginScene);
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