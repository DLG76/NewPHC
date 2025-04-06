using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class HotbarCombatUI : Singleton<HotbarCombatUI>
{
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern int IsMobileDevice();
#endif

    [SerializeField] private InventorySlot weapon1UI;
    [SerializeField] private InventorySlot weapon2UI;
    [SerializeField] private InventorySlot weapon3UI;

    private bool settedUp = false;
    private int selectedIndex = 0;

    private PlayerCombat player;
    private Equipment equipment;

    public void ResetPlayerData()
    {
        Setup(player);
    }

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

        weapon1UI.OpenButton.onClick.RemoveAllListeners();
        weapon1UI.OpenButton.onClick.AddListener(() => {
            selectedIndex = 0;
            Update();
        });
        weapon1UI.SetItem(new InventoryItem
        {
            item = equipment.weapon1,
            count = 1
        });
        weapon2UI.OpenButton.onClick.RemoveAllListeners();
        weapon2UI.OpenButton.onClick.AddListener(() =>
        {
            selectedIndex = 1;
            Update();
        });
        weapon2UI.SetItem(new InventoryItem
        {
            item = equipment.weapon2,
            count = 1
        });
        weapon3UI.OpenButton.onClick.RemoveAllListeners();
        weapon3UI.OpenButton.onClick.AddListener(() =>
        {
            selectedIndex = 2;
            Update();
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

        InventorySlot weaponUI = null;

        switch (selectedIndex)
        {
            case 0:
                player.SelectVoidItem(equipment.weapon1);
                weaponUI = weapon1UI;
                break;
            case 1:
                player.SelectVoidItem(equipment.weapon2);
                weaponUI = weapon2UI;
                break;
            case 2:
                player.SelectVoidItem(equipment.weapon3);
                weaponUI = weapon3UI;
                break;
        }

        if (weaponUI != null)
        {
            SelectItem(weaponUI);
            if (weaponUI != weapon1UI)
                DeselectItem(weapon1UI);
            if (weaponUI != weapon2UI)
                DeselectItem(weapon2UI);
            if (weaponUI != weapon3UI)
                DeselectItem(weapon3UI);
        }
    }

    private void SelectItem(InventorySlot inventorySlot)
    {
        inventorySlot.OpenButton.transform.DOScale(Vector3.one * 1.15f, 0.3f).SetEase(Ease.OutQuad);
        inventorySlot.SetAlpha(1);
    }

    private void DeselectItem(InventorySlot inventorySlot)
    {
        inventorySlot.OpenButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad);
        inventorySlot.SetAlpha(0.75f);
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
