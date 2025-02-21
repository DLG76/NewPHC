using UnityEngine;


[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/New Item")]
public class Item : ScriptableObject
{
    public string id;
    public string Name;
    public Sprite Icon;
    public string Description;
    public bool CanStack;
    public int MaxQuantity;

    public bool CanBuy;
    public int PriceBuy;
    public int PriceSell;
}