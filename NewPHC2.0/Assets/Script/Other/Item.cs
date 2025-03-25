using Newtonsoft.Json.Linq;
using UnityEngine;

public class Item
{
    public string id;
    public string Name;
    public Sprite Icon;
    public string Description;
    public bool CanStack;
    public int Count;

    public Item(JObject itemJson, int count)
    {
        id = itemJson["_id"].ToString();
        Name = itemJson["name"].ToString();
        //Icon = Resources.Load<Sprite>("ItemIcons/" + itemJson["icon"].ToString());
        Description = itemJson["description"].ToString();
        CanStack = itemJson["canStack"].ToObject<bool>();
        Count = count;
    }
}