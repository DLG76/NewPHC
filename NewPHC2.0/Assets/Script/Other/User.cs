﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public VoidItem weapon1 { get; set; } = null;
    public VoidItem weapon2 { get; set; } = null;
    public VoidItem weapon3 { get; set; } = null;
    public CoreItem core { get; set; } = null;

    // ✅ เรียก constructor อื่นที่ไม่ส่งพารามิเตอร์
    public Equipment() : this(null) { }

    public Equipment(JObject equipmentJson)
    {
        if (equipmentJson == null)
        {
            Debug.LogWarning("equipmentJson is null. Skipping initialization.");
            return;
        }

        if (equipmentJson["weapon1"] != null && equipmentJson["weapon1"]?.Type != JTokenType.Null)
            weapon1 = new VoidItem(equipmentJson["weapon1"]?.ToObject<JObject>());
        if (equipmentJson["weapon2"] != null && equipmentJson["weapon2"]?.Type != JTokenType.Null)
            weapon2 = new VoidItem(equipmentJson["weapon2"]?.ToObject<JObject>());
        if (equipmentJson["weapon3"] != null && equipmentJson["weapon3"]?.Type != JTokenType.Null)
            weapon3 = new VoidItem(equipmentJson["weapon3"]?.ToObject<JObject>());
        if (equipmentJson["core"] != null && equipmentJson["core"]?.Type != JTokenType.Null)
            core = new CoreItem(equipmentJson["core"]?.ToObject<JObject>());
    }
}

public class InventoryItem
{
    public Item item { get; set; }
    public int count { get; set; }
}

public class ClearedStage
{
    public string type { get; set; }
    public string stageId { get; set; }
    public string rewardId { get; set; }
    public double time { get; set; }
}

public class Answer
{
    public string stageId { get; set; }
    public string code { get; set; }
}

public class User
{
    public static User me = new User();

    public bool haveData = false;

    public string id;
    public string name;
    public List<string> friends;

    public float maxHealth;
    public float health;
    public int level;
    public double maxExp;
    public double exp;
    public List<ClearedStage> clearedStages = new List<ClearedStage>();

    public Equipment equipment = new Equipment();
    public List<InventoryItem> inventory = new List<InventoryItem>();
    public List<Answer> answers = new List<Answer>();

    public void UpdateProfile(JObject user)
    {
        if (user == null) return;

        JObject stats = user["stats"]?.ToObject<JObject>();
        if (stats == null) return;

        id = user["_id"]?.ToString();
        name = user["name"]?.ToString();
        friends = user["friends"]?.ToObject<List<string>>();

        maxHealth = stats["maxHealth"].ToObject<float>();
        health = stats["health"].ToObject<float>();
        level = stats["level"].ToObject<int>();
        maxExp = stats["maxExp"].ToObject<double>();
        exp = stats["exp"].ToObject<double>();
        clearedStages = ConvertTextToClass<List<ClearedStage>>(stats["clearedStages"]?.ToString() ?? "[]");

        UpdateEquipment(stats["equipment"]?.ToObject<JObject>());

        if (stats["inventory"] != null && stats["inventory"]?.Type != JTokenType.Null)
            UpdateInventory(stats["inventory"]?.ToObject<List<JObject>>());

        answers = ConvertTextToClass<List<Answer>>(stats["answers"]?.ToString() ?? "[]");

        haveData = true;
    }

    public void UpdateEquipment(JObject equipmentJson)
    {
        equipment = new Equipment(equipmentJson);
    }

    public void UpdateInventory(List<JObject> inventoryJson)
    {
        inventory.Clear();

        if (inventoryJson == null)
            return;

        foreach (JObject inventorySlotJson in inventoryJson)
        {
            int count = inventorySlotJson["count"].ToObject<int>();
            JObject itemJson = inventorySlotJson["itemId"].ToObject<JObject>();

            if (itemJson != null)
            {
                if (itemJson["type"].ToString() == "VoidItem")
                    inventory.Add(new InventoryItem
                    {
                        item = new VoidItem(itemJson),
                        count = count
                    });
                else if (itemJson["type"].ToString() == "CoreItem")
                    inventory.Add(new InventoryItem
                    {
                        item = new CoreItem(itemJson),
                        count = count
                    });
                else if (itemJson["type"].ToString() == "NormalItem")
                    inventory.Add(new InventoryItem
                    {
                        item = new Item(itemJson),
                        count = count
                    });
            }
        }
    }

    private static T ConvertTextToClass<T>(string text)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        return JsonConvert.DeserializeObject<T>(text, settings);
    }
}
