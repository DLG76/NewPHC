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
using System.Linq;
using System.Runtime.CompilerServices;

public class DatabaseManager : SingletonPersistent<DatabaseManager>
{
    public static string LoginScene = "Lobby";

    [SerializeField] private TextAsset stagesFile;

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
        ShowLoading();

        var textAsset = Resources.Load<TextAsset>("TesterUser");

        string responseText = textAsset.text;
        JObject responseJson = JObject.Parse(responseText);

        if (responseJson["name"]?.ToString() == username)
            if (responseJson["name"]?.ToString() == password)
                callback?.Invoke(true, null);
            else
                callback?.Invoke(false, "Password ไม่ถูกต้อง");
        else
            callback?.Invoke(false, "ไม่พบ Username");

        HideLoading();

        yield break;

        //string jsonPayload = JsonConvert.SerializeObject(new JObject
        //{
        //    new JProperty("username", username),
        //    new JProperty("password", password)
        //});
        //byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        //UnityWebRequest request = new UnityWebRequest(Api.LOGIN_URL, "POST");
        //request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        //request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("Content-Type", "application/json");

        //ShowLoading();
        //yield return request.SendWebRequest();

        //if (request.result == UnityWebRequest.Result.Success)
        //{
        //    string responseText = request.downloadHandler.text;
        //    JObject responseJson = JObject.Parse(responseText);

        //    tokenManager.SetToken(responseJson["accessToken"].ToString(), responseJson["refreshToken"].ToString());

        //    callback?.Invoke(true, null);
        //}
        //else
        //{
        //    tokenManager.ClearToken();
        //    Debug.LogError(request.responseCode + ": " + request.error);
        //}

        //HideLoading();
    }

    public IEnumerator GetProfile(System.Action<bool, JObject> callback)
    {
        ShowLoading();

        yield return GetStages((success, clearedStages, stages) =>
        {
            var textAsset = Resources.Load<TextAsset>("TesterUser");

            string responseText = textAsset.text;
            JObject responseJson = JObject.Parse(responseText);

            string answersString = PlayerPrefs.GetString("answers", "[]");
            JArray answers = JsonConvert.DeserializeObject<JArray>(answersString);
            responseJson["stats"]["answers"] = answers;

            JArray clearedStagesJson = new JArray();
            foreach (var clearedStage in clearedStages)
                clearedStagesJson.Add(new JObject
                {
                    new JProperty("type", clearedStage["type"]),
                    new JProperty("stageId", clearedStage["stageId"]),
                    new JProperty("rewardId", null),
                    new JProperty("time", 0)
                });
            responseJson["stats"]["clearedStages"] = clearedStagesJson;

            responseJson = LocalUserManager.LoadData(responseJson);

            callback?.Invoke(true, responseJson);
        });

        HideLoading();

        yield break;

        //UnityWebRequest request = new UnityWebRequest(Api.PROFILE_URL, "GET");
        //request.downloadHandler = new DownloadHandlerBuffer();

        //ShowLoading();

        //yield return tokenManager.HandleRequest(request, (request) =>
        //{
        //    if (request.result == UnityWebRequest.Result.Success)
        //    {
        //        string responseText = request.downloadHandler.text;
        //        callback?.Invoke(true, JObject.Parse(responseText));
        //    }
        //    else
        //    {
        //        Debug.LogError(request.responseCode + ": " + request.error);
        //        callback?.Invoke(false, null);
        //    }
        //});

        //HideLoading();
    }

    //public IEnumerator GetUser(string userId, System.Action<bool, JObject> callback)
    //{
    //    UnityWebRequest request = new UnityWebRequest(Api.GetUsersUrl(userId), "GET");
    //    request.downloadHandler = new DownloadHandlerBuffer();

    //    ShowLoading();

    //    yield return tokenManager.HandleRequest(request, (request) =>
    //    {
    //        if (request.result == UnityWebRequest.Result.Success)
    //        {
    //            string responseText = request.downloadHandler.text;
    //            callback?.Invoke(true, JObject.Parse(responseText));
    //        }
    //        else
    //        {
    //            Debug.LogError(request.responseCode + ": " + request.error);
    //            callback?.Invoke(false, null);
    //        }
    //    });

    //    HideLoading();
    //}











    private IEnumerator GetOnlyStages(System.Action<List<JObject>> callback)
    {
        List<JObject> stages = JsonConvert.DeserializeObject<List<JObject>>(stagesFile.text);
        callback?.Invoke(stages);
        yield break;
    }

    private IEnumerator SendClearedStage(string stageType, string stageId, System.Action<bool> callback)
    {
        string clearedStagesString = PlayerPrefs.GetString("clearedStage", null);
        List<JObject> clearedStages = new List<JObject>();

        var newClearedStage = new JObject
        {
            new JProperty("type", stageType),
            new JProperty("stageId", stageId),
        };

        if (!string.IsNullOrEmpty(clearedStagesString))
            clearedStages = JsonConvert.DeserializeObject<List<JObject>>(clearedStagesString);

        clearedStages.Add(newClearedStage);

        PlayerPrefs.SetString("clearedStage", JsonConvert.SerializeObject(clearedStages));
        PlayerPrefs.Save();

        LocalUserManager.CheckByteSize();

        callback?.Invoke(true);

        yield break;

        //string myUserId = PlayerPrefs.GetString("myUserId");

        //string jsonPayload = JsonConvert.SerializeObject(new JObject
        //{
        //    new JProperty("type", stageType),
        //    new JProperty("stageId", stageId),
        //    new JProperty("code", code),
        //});
        //byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        //UnityWebRequest request = new UnityWebRequest($"http://localhost:23000/stage/cleared/{myUserId}", "POST");
        //request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        //request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("Content-Type", "application/json");

        //yield return request.SendWebRequest();

        //if (request.result == UnityWebRequest.Result.Success)
        //{
        //    callback?.Invoke(true);
        //}
        //else
        //{
        //    Debug.LogError(request.responseCode + ": " + request.downloadHandler.text);
        //    callback?.Invoke(false);
        //}
    }











    public IEnumerator GetStages(System.Action<bool, List<JObject>, List<JObject>> callback)
    //public IEnumerator GetStages(System.Action<bool, List<JObject>, List<JObject>> callback)
    {
        ShowLoading();

        //string myUserId = PlayerPrefs.GetString("myUserId");

        List<JObject> stages = JsonConvert.DeserializeObject<List<JObject>>(stagesFile.text);
        List<JObject> clearedStages = new List<JObject>();

        string clearedStagesString = PlayerPrefs.GetString("clearedStage", null);

        if (!string.IsNullOrEmpty(clearedStagesString))
            clearedStages = JsonConvert.DeserializeObject<List<JObject>>(clearedStagesString);

        List<JObject> clearedStagesJson = new List<JObject>();
        foreach (var clearedStage in clearedStages)
            clearedStagesJson.Add(new JObject
                {
                    new JProperty("type", clearedStage["type"]),
                    new JProperty("stageId", clearedStage["stageId"]),
                    new JProperty("rewardId", null),
                    new JProperty("time", 0)
                });

        callback?.Invoke(true, clearedStagesJson, stages);

        //UnityWebRequest request = new UnityWebRequest($"http://localhost:23000/stage/cleared/{myUserId}", "GET");
        //request.downloadHandler = new DownloadHandlerBuffer();
        //yield return request.SendWebRequest();

        //if (request.result == UnityWebRequest.Result.Success)
        //{
        //    string responseText = request.downloadHandler.text;
        //    List<JObject> clearedStages = JsonConvert.DeserializeObject<List<JObject>>(responseText);
        //    callback?.Invoke(true, clearedStages, stages);
        //}
        //else
        //{
        //    Debug.LogError(request.responseCode + ": " + request.error);
        //    callback?.Invoke(false, null, null);
        //}

        HideLoading();

        yield break;

        //UnityWebRequest request = new UnityWebRequest(Api.GET_STAGES_URL, "GET");
        //request.downloadHandler = new DownloadHandlerBuffer();

        //ShowLoading();

        //yield return tokenManager.HandleRequest(request, (request) =>
        //{
        //    if (request.result == UnityWebRequest.Result.Success)
        //    {
        //        string responseText = request.downloadHandler.text;
        //        JObject responseJson = JObject.Parse(responseText);
        //        List<JObject> clearedStages = responseJson["clearedStages"]?.ToObject<List<JObject>>();
        //        List<JObject> stages = responseJson["stages"]?.ToObject<List<JObject>>();
        //        callback?.Invoke(true, clearedStages, stages);
        //    }
        //    else
        //    {
        //        Debug.LogError(request.responseCode + ": " + request.error);
        //        callback?.Invoke(false, null, null);
        //    }
        //});

        //HideLoading();
    }

    public IEnumerator FinishedDungeon(string stageId, bool isWinner, double time, System.Action<bool, JObject> callback)
    {
        ShowLoading();

        JObject stage = new JObject();

        yield return GetOnlyStages((stages) =>
        {
            var _stage = stages.FirstOrDefault(s => s["_id"]?.ToString() == stageId);

            if (_stage != null && _stage.Type != JTokenType.Null)
            {
                stage = _stage;
            }
        });

        yield return SendClearedStage("CombatStage", stageId, (success) =>
        {
            if (success)
            {
                JObject rewardClamed = LocalUserManager.GetReward(stage["rewardId"]?.ToString());

                callback?.Invoke(true, rewardClamed);
            }
            else
            {
                callback?.Invoke(false, null);
            }
        });

        HideLoading();

        yield break;

        //string jsonPayload = JsonConvert.SerializeObject(new JObject
        //{
        //    new JProperty("isWinner", isWinner),
        //    new JProperty("time", time),
        //});
        //byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        //UnityWebRequest request = new UnityWebRequest(Api.ClearDungeonUrl(stageId), "POST");
        //request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        //request.downloadHandler = new DownloadHandlerBuffer();
        //var header = new Dictionary<string, string>
        //{
        //    { "Content-Type", "application/json" }
        //};

        //ShowLoading();

        //yield return tokenManager.HandleRequest(request, header, (request) =>
        //{
        //    if (request.result == UnityWebRequest.Result.Success)
        //    {
        //        string responseText = request.downloadHandler.text;
        //        JObject responseJson = JObject.Parse(responseText);
        //        callback?.Invoke(true, responseJson);
        //    }
        //    else
        //    {
        //        callback?.Invoke(false, null);
        //    }
        //});

        //HideLoading();
    }

    public IEnumerator SendCode(string stageId, string code, System.Action<bool, JObject> callback)
    {
        ShowLoading();

        JObject stage = new JObject();

        yield return GetOnlyStages((stages) =>
        {
            var _stage = stages.FirstOrDefault(s => s["_id"]?.ToString() == stageId);

            if (_stage != null && _stage.Type != JTokenType.Null)
            {
                stage = _stage;
            }
        });

        yield return SendClearedStage("CodeStage", stageId, (success) =>
        {
            if (success)
            {
                string answersString = PlayerPrefs.GetString("answers", "[]");
                List<JObject> answers = JsonConvert.DeserializeObject<List<JObject>>(answersString);

                var newAnswer = new JObject
                {
                    new JProperty("stageId", stageId),
                    new JProperty("code", code),
                };

                answers.Add(newAnswer);

                PlayerPrefs.SetString("answers", JsonConvert.SerializeObject(answers));
                PlayerPrefs.Save();

                JObject rewardClamed = LocalUserManager.GetReward(stage["rewardId"]?.ToString());

                callback?.Invoke(true, rewardClamed);
            }
            else
            {
                callback?.Invoke(false, null);
            }
        });

        HideLoading();

        yield break;

        //string jsonPayload = JsonConvert.SerializeObject(new JObject
        //{
        //    new JProperty("code", code)
        //});
        //byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        //UnityWebRequest request = new UnityWebRequest(Api.SendCodeUrl(stageId), "POST");
        //request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        //request.downloadHandler = new DownloadHandlerBuffer();
        //var header = new Dictionary<string, string>
        //{
        //    { "Content-Type", "application/json" }
        //};

        //ShowLoading();

        //yield return tokenManager.HandleRequest(request, header, (request) =>
        //{
        //    if (request.result == UnityWebRequest.Result.Success)
        //    {
        //        string responseText = request.downloadHandler.text;
        //        JObject responseJson = JObject.Parse(responseText);
        //        callback?.Invoke(true, responseJson);
        //    }
        //    else
        //    {
        //        callback?.Invoke(false, null);
        //    }
        //});

        //HideLoading();
    }

    //public IEnumerator RemoveItem(string itemId, System.Action<bool, List<JObject>> callback)
    //{
    //    yield return RemoveItem(itemId, 1, callback);
    //}

    //public IEnumerator RemoveItem(string itemId, int count, System.Action<bool, List<JObject>> callback)
    //{
    //    UnityWebRequest request = new UnityWebRequest(Api.RemoveItemUrl(itemId, count), "POST");
    //    request.downloadHandler = new DownloadHandlerBuffer();

    //    ShowLoading();

    //    yield return tokenManager.HandleRequest(request, (request) =>
    //    {
    //        if (request.result == UnityWebRequest.Result.Success)
    //        {
    //            string responseText = request.downloadHandler.text;
    //            JObject responseJson = JObject.Parse(responseText);
    //            List<JObject> inventory = responseJson["inventory"]?.ToObject<List<JObject>>();
    //            callback?.Invoke(true, inventory);
    //        }
    //        else
    //        {
    //            callback?.Invoke(false, null);
    //        }
    //    });

    //    HideLoading();
    //}

    public IEnumerator UpdateEquipment(Equipment equipment, System.Action<bool, JObject, List<JObject>> callback)
    {
        ShowLoading();

        var oldEquipment = User.me.equipment;

        string GetItemIdByKey(Equipment eq, string key)
        {
            return key switch
            {
                "weapon1" => eq.weapon1?.id,
                "weapon2" => eq.weapon2?.id,
                "weapon3" => eq.weapon3?.id,
                "core" => eq.core?.id,
                _ => null
            };
        }

        var removedItems = new Dictionary<string, string>();
        var addedItems = new Dictionary<string, string>();

        var keys = new[] { "weapon1", "weapon2", "weapon3", "core" };

        foreach (var key in keys)
        {
            string oldId = GetItemIdByKey(oldEquipment, key);
            string newId = GetItemIdByKey(equipment, key);

            if (oldId != newId && newId != null)
            {
                if (oldId != null) removedItems[key] = oldId;
                if (newId != null) addedItems[key] = newId;
            }
        }

        foreach (var kvp in removedItems)
            LocalUserManager.AddItem(kvp.Value);

        foreach (var kvp in addedItems)
            LocalUserManager.RemoveItem(kvp.Value);

        LocalUserManager.SaveData();

        yield return GetProfile((_, userData) => callback?.Invoke(true, userData["stats"]["equipment"].ToObject<JObject>(), userData["stats"]["inventory"].ToObject<List<JObject>>()));

        HideLoading();

        yield break;

        //string jsonPayload = JsonConvert.SerializeObject(new JObject
        //{
        //    new JProperty("equipmentData", new JObject
        //        {
        //            new JProperty("weapon1", equipment.weapon1?.id),
        //            new JProperty("weapon2", equipment.weapon2?.id),
        //            new JProperty("weapon3", equipment.weapon3?.id),
        //            new JProperty("core", equipment.core?.id)
        //        }
        //    )
        //});
        //byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        //UnityWebRequest request = new UnityWebRequest(Api.UPDATE_EQUIPMENT, "PUT");
        //request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        //request.downloadHandler = new DownloadHandlerBuffer();
        //var header = new Dictionary<string, string>
        //{
        //    { "Content-Type", "application/json" }
        //};

        //ShowLoading();

        //yield return tokenManager.HandleRequest(request, header, (request) =>
        //{
        //    if (request.result == UnityWebRequest.Result.Success)
        //    {
        //        string responseText = request.downloadHandler.text;
        //        JObject responseJson = JObject.Parse(responseText);
        //        JObject newEquipment = responseJson["equipment"]?.ToObject<JObject>();
        //        List<JObject> inventory = responseJson["inventory"]?.ToObject<List<JObject>>();
        //        callback?.Invoke(true, newEquipment, inventory);
        //    }
        //    else
        //    {
        //        callback?.Invoke(false, null, null);
        //    }
        //});

        //HideLoading();
    }

    public IEnumerator FuseVoid(Item item1, Item item2, System.Action<bool, Item, List<JObject>> callback)
    {
        ShowLoading();

        var fuseFile = Resources.Load<TextAsset>("FuseData");
        var fuseDatas = JsonConvert.DeserializeObject<List<JObject>>(fuseFile.text);

        foreach (var fuseData in fuseDatas)
        {
            if ((fuseData["item1"]?.ToString() == item1.id && fuseData["item2"]?.ToString() == item2.id) ||
                (fuseData["item2"]?.ToString() == item1.id && fuseData["item1"]?.ToString() == item2.id))
            {
                var results = fuseData["results"]?.ToObject<List<JObject>>();

                int allRate = results.Sum(itemDrop => itemDrop["rate"].ToObject<int>());

                int rateRandom = Mathf.FloorToInt(Random.value * allRate) + 1;

                foreach (var itemDrop in results)
                {
                    rateRandom -= itemDrop["rate"].ToObject<int>();
                    if (rateRandom <= 0)
                    {
                        var item = LocalUserManager.AddItem(itemDrop["itemId"].ToString());
                        var inventory = JsonConvert.DeserializeObject<List<JObject>>(PlayerPrefs.GetString("inventory", "[]"));

                        foreach (var inventoryItem in inventory)
                            inventoryItem["itemId"] = LocalItemManager.GetItem(inventoryItem["itemId"]?.ToString());

                        callback?.Invoke(true, item, inventory);
                        break;
                    }
                }

                break;
            }
        }

        HideLoading();

        yield break;

        //string jsonPayload = JsonConvert.SerializeObject(new JObject
        //{
        //    new JProperty("item1", item1.id),
        //    new JProperty("item2", item2.id),
        //});
        //byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);

        //UnityWebRequest request = new UnityWebRequest(Api.FUSE_VOID_URL, "POST");
        //request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        //request.downloadHandler = new DownloadHandlerBuffer();
        //var header = new Dictionary<string, string>
        //{
        //    { "Content-Type", "application/json" }
        //};

        //ShowLoading();

        //yield return tokenManager.HandleRequest(request, header, (request) =>
        //{
        //    if (request.result == UnityWebRequest.Result.Success)
        //    {
        //        string responseText = request.downloadHandler.text;
        //        JObject responseJson = JObject.Parse(responseText);
        //        JObject itemJson = responseJson["resultItem"]?.ToObject<JObject>();
        //        Item item;
        //        if (itemJson["type"]?.ToString() == "CoreItem")
        //            item = new CoreItem(itemJson);
        //        else if (itemJson["type"]?.ToString() == "VoidItem")
        //            item = new VoidItem(itemJson);
        //        else
        //            item = new Item(itemJson);
        //        List<JObject> inventory = responseJson["inventory"]?.ToObject<List<JObject>>();
        //        callback?.Invoke(true, item, inventory);
        //    }
        //    else
        //    {
        //        callback?.Invoke(false, null, null);
        //    }
        //});

        //HideLoading();
    }

    public void GoToLoginScene()
    {
        SceneManager.LoadScene(LoginScene);
    }

    private class LocalUserManager
    {
        public static JObject LoadData(JObject userJson)
        {
            var inventory = PlayerPrefs.GetString("inventory", null);
            var equipment = PlayerPrefs.GetString("equipment", null);

            if (!string.IsNullOrEmpty(inventory) && !string.IsNullOrEmpty(equipment))
            {
                var inventoryIdJson = JArray.Parse(inventory);
                var inventoryJson = new JArray();
                foreach (var itemIdJson in inventoryIdJson)
                {
                    var itemJson = LocalItemManager.GetItem(itemIdJson["itemId"]?.ToString());

                    inventoryJson.Add(new JObject
                    {
                        new JProperty("itemId", itemJson),
                        new JProperty("count", itemIdJson["count"])
                    });
                }

                var equipmentIdJson = JObject.Parse(equipment);
                var equipmentJson = new JObject
                {
                    new JProperty("core", LocalItemManager.GetItem(equipmentIdJson["core"]?.ToString())),
                    new JProperty("weapon1", LocalItemManager.GetItem(equipmentIdJson["weapon1"]?.ToString())),
                    new JProperty("weapon2", LocalItemManager.GetItem(equipmentIdJson["weapon2"]?.ToString())),
                    new JProperty("weapon3", LocalItemManager.GetItem(equipmentIdJson["weapon3"]?.ToString()))
                };

                userJson["stats"]["inventory"] = inventoryJson;
                userJson["stats"]["equipment"] = equipmentJson;
            }

            return userJson;
        }

        public static void SaveData()
        {
            var inventoryJson = new JArray();
            var equipmentJson = new JObject();

            foreach (var inventoryItem in User.me.inventory)
            {
                var itemJson = LocalItemManager.GetItem(inventoryItem.item.id);
                if (itemJson != null)
                {
                    inventoryJson.Add(new JObject
                    {
                        new JProperty("itemId", inventoryItem.item.id),
                        new JProperty("count", inventoryItem.count)
                    });
                }
            }

            equipmentJson["core"] = User.me.equipment.core?.id;
            equipmentJson["weapon1"] = User.me.equipment.weapon1?.id;
            equipmentJson["weapon2"] = User.me.equipment.weapon2?.id;
            equipmentJson["weapon3"] = User.me.equipment.weapon3?.id;

            PlayerPrefs.SetString("inventory", JsonConvert.SerializeObject(inventoryJson));
            PlayerPrefs.SetString("equipment", JsonConvert.SerializeObject(equipmentJson));
            PlayerPrefs.Save();

            CheckByteSize();
        }

        public static void CheckByteSize()
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(
                PlayerPrefs.GetString("inventory", "") +
                PlayerPrefs.GetString("equipment", "") +
                PlayerPrefs.GetString("clearedStage", "") +
                PlayerPrefs.GetString("answers", "")
            );

            Debug.Log($"Byte size of string: {byteArray.Length} bytes {Mathf.Round((float)byteArray.Length / (5 * 1024 * 1024)) * 100}%");
        }

        public static JObject GetReward(string rewardId)
        {
            if (!User.me.haveData || rewardId == null) return null;

            var reward = LocalRewardManager.GetRewardData(rewardId);
            JObject rewardClamed = new JObject();

            if (reward == null) return null;

            int allRate = reward["itemDrops"].ToObject<List<JObject>>().Sum(itemDrop => itemDrop["rate"].ToObject<int>());

            int rateRandom = Mathf.FloorToInt(Random.value * allRate) + 1;

            foreach (var itemDrop in reward["itemDrops"].ToObject<List<JObject>>()) {
                rateRandom -= itemDrop["rate"].ToObject<int>();
                if (rateRandom <= 0)
                {
                    int itemCount = itemDrop["count"]?.ToObject<int>() ?? 1;
                    for (int i = 0; i < itemCount; i++)
                        AddItem(itemDrop["itemId"].ToString());
                    rewardClamed["item"] = LocalItemManager.GetItem(itemDrop["itemId"].ToString());
                    rewardClamed["itemCount"] = itemCount;
                    break;
                }
            }

            bool changedLevel = false;
            double nowExp = User.me.exp + (reward["exp"]?.ToObject<double>() ?? 0);
            rewardClamed["exp"] = reward["exp"];

            while (nowExp > (1.35 * (User.me.level - 1)) * 100)
            {
                nowExp -= (1.35 * (User.me.level - 1)) * 100;
                User.me.level += 1;
                changedLevel = true;
            }

            if (changedLevel)
            {
                User.me.maxHealth = ((User.me.level - 1) * 20) + 100;
                User.me.health = User.me.maxHealth;
                User.me.maxExp = (1.35 * (User.me.level - 1)) * 100;
            }

            return rewardClamed;
        }

        public static Item AddItem(string itemId)
        {
            if (!User.me.haveData || itemId == null) return null;

            var inventoryItem = User.me.inventory.FirstOrDefault(i => i.item.id == itemId);

            if (inventoryItem != null && inventoryItem.item.CanStack)
            {
                inventoryItem.count++;
                SaveData();
                return inventoryItem.item;
            }
            else
            {
                JObject itemJson = LocalItemManager.GetItem(itemId);

                if (itemJson != null && itemJson.Type != JTokenType.Null)
                {
                    Item item = null;

                    switch (itemJson["type"]?.ToString())
                    {
                        case "VoidItem":
                            item = new VoidItem(itemJson);
                            break;
                        case "CoreItem":
                            item = new CoreItem(itemJson);
                            break;
                        case "NormalItem":
                            item = new Item(itemJson);
                            break;
                    }

                    if (item != null)
                    {
                        User.me.inventory.Add(new InventoryItem
                        {
                            item = item,
                            count = 1
                        });
                        SaveData();

                        return item;
                    }
                }

                return null;
            }
        }

        public static void RemoveItem(string itemId)
        {
            if (!User.me.haveData || itemId == null) return;

            var inventoryItem = User.me.inventory.FirstOrDefault(i => i.item.id == itemId);

            if (inventoryItem != null)
            {
                inventoryItem.count--;

                if (inventoryItem.count <= 0)
                    User.me.inventory.Remove(inventoryItem);

                SaveData();
            }
        }
    }

    private class LocalItemManager
    {
        public static List<JObject> GetItems()
        {
            var itemFile = Resources.Load<TextAsset>("ItemData");
            List<JObject> itemJsons = JsonConvert.DeserializeObject<List<JObject>>(itemFile.text);
            return itemJsons;
        }

        public static JObject GetItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            var items = GetItems();

            var itemJson = items.FirstOrDefault(i => i["_id"]?.ToString() == itemId);

            if (itemJson != null)
            {
                return itemJson;
            }
            else
            {
                Debug.LogError($"Item with ID {itemId} not found.");
                return null;
            }
        }
    }

    private class LocalRewardManager
    {
        public static JObject GetRewardData(string rewardId)
        {
            if (rewardId == null) return null;

            var rewardFile = Resources.Load<TextAsset>("RewardData");
            List<JObject> rewards = JsonConvert.DeserializeObject<List<JObject>>(rewardFile.text);
            JObject reward = rewards.FirstOrDefault(r => r["_id"].ToString() == rewardId);

            if (reward != null)
            {
                return reward;
            }
            else
            {
                Debug.LogError($"Reward with ID {rewardId} not found.");
                return null;
            }
        }
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