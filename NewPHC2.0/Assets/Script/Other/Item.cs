using Newtonsoft.Json.Linq;
using UnityEngine;

public class Item
{
    public string id;
    public string Name;
    public Sprite Icon;
    public string Description;
    public bool CanStack;

    public Item(JObject itemJson)
    {
        id = itemJson["_id"].ToString();
        Name = itemJson["name"].ToString();
        Icon = Resources.Load<Sprite>("ItemIcons/" + itemJson["name"].ToString());
        Description = itemJson["description"].ToString();
        CanStack = itemJson["canStack"].ToObject<bool>();
    }
}