using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreItem : Item
{
    public float health { get; set; }
    public float armor { get; set; }

    public CoreItem(JObject itemJson, int count) : base(itemJson, count)
    {
        // ยังไม่เสร็จ
    }
}
