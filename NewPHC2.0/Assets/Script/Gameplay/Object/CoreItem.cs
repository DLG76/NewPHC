using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreItem : Item
{
    public float health { get; set; }
    public float armor { get; set; }

    public CoreItem(JObject itemJson) : base(itemJson)
    {
        health = itemJson["health"]?.Value<float>() ?? 0;
        armor = itemJson["armor"]?.Value<float>() ?? 0;
    }
}
