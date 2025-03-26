using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidHotbarUI : Singleton<VoidHotbarUI>
{
    [SerializeField] private InventorySlot weapon1UI;
    [SerializeField] private InventorySlot weapon2UI;
    [SerializeField] private InventorySlot weapon3UI;

    private bool settedUp = false;
    private int selectedIndex = 0;
    private bool selected = false;

    private PlayerCombat player;
    private Equipment equipment;

    public void Setup(PlayerCombat player)
    {
        this.player = player;
        settedUp = true;

        equipment = new Equipment
        {
            weapon1 = User.me?.equipment?.weapon1,
            weapon2 = User.me?.equipment?.weapon2,
            weapon3 = User.me?.equipment?.weapon3,
            core = User.me?.equipment?.core
        };

        weapon1UI.SetItem(new InventoryItem
        {
            item = equipment.weapon1,
            count = 1
        });
        weapon2UI.SetItem(new InventoryItem
        {
            item = equipment.weapon2,
            count = 1
        });
        weapon3UI.SetItem(new InventoryItem
        {
            item = equipment.weapon3,
            count = 1
        });
    }

    private void Update()
    {
        if (!settedUp)
            return;

        if (User.me?.equipment == null)
        {
            DatabaseManager.Instance.GoToLoginScene();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            selectedIndex = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            selectedIndex = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            selectedIndex = 2;

        if (!selected)
        {
            switch (selectedIndex)
            {
                case 0:
                    if (equipment.weapon1 != null)
                        player.SelectVoidItem(equipment.weapon1);
                    break;
                case 1:
                    if (equipment.weapon2 != null)
                        player.SelectVoidItem(equipment.weapon2);
                    break;
                case 2:
                    if (equipment.weapon3 != null)
                        player.SelectVoidItem(equipment.weapon3);
                    break;
            }
            selected = true;
        }
        else
        {
            switch (selectedIndex)
            {
                case 0:
                    if (equipment.weapon1 == null)
                    {
                        selectedIndex = 1;
                        selected = false;
                    }
                    break;
                case 1:
                    if (equipment.weapon2 != null)
                    {
                        selectedIndex = 2;
                        selected = false;
                    }
                    break;
                case 2:
                    if (equipment.weapon3 != null)
                    {
                        selectedIndex = 0;
                        selected = false;
                    }
                    break;
            }
        }
    }

    public void AddItem(VoidItem item)
    {
        if (equipment.weapon1 == null)
        {
            equipment.weapon1 = item;
            weapon1UI.SetItem(new InventoryItem
            {
                item = equipment.weapon1,
                count = 1
            });
        }
        else if (equipment.weapon2 == null)
        {
            equipment.weapon2 = item;
            weapon2UI.SetItem(new InventoryItem
            {
                item = equipment.weapon2,
                count = 1
            });
        }
        else if (equipment.weapon3 == null)
        {
            equipment.weapon3 = item;
            weapon3UI.SetItem(new InventoryItem
            {
                item = equipment.weapon3,
                count = 1
            });
        }
    }

    public void RemoveItem(VoidItem item)
    {
        if (item == null) return;

        if (equipment.weapon1 == item)
        {
            equipment.weapon1 = null;
            weapon1UI.SetItem(null);
        }
        else if (equipment.weapon2 == item)
        {
            equipment.weapon2 = null;
            weapon2UI.SetItem(null);
        }
        else if (equipment.weapon3 == item)
        {
            equipment.weapon3 = null;
            weapon3UI.SetItem(null);
        }
    }
}
