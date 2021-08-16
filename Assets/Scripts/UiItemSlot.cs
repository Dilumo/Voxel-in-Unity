using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiItemSlot : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public Text slotAmount;

    World world;

    public bool HasItem
    {
        get 
        {
            if (itemSlot == null)
                return false;
            else
                return itemSlot.HasItem;
        }
    }

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>(); // junk
    }

    public void Link(ItemSlot item)
    {
        itemSlot = item;
        isLinked = true;
        itemSlot.LinkUISlot(this);
        UpdateSlot();
    }

    public void Unlink()
    {
        itemSlot.UnlinkUISlot();
        itemSlot = null;
        UpdateSlot();
        
        isLinked = false;
    }

    public void UpdateSlot()
    {
        if (itemSlot != null && itemSlot.HasItem)
        {
            slotIcon.sprite = world.blockTypes[itemSlot.itemStack.id].icon;
            slotAmount.text = itemSlot.itemStack.amount.ToString();
            slotIcon.enabled = true;
            slotAmount.enabled = true;
        }
        else
            Clear();
    }

    public void Clear()
    {
        slotIcon.sprite = null;
        slotAmount.text = "";
        slotIcon.enabled = false;
        slotAmount.enabled = false;
    }

    private void OnDestroy()
    {
        if (isLinked && itemSlot != null)
            itemSlot.UnlinkUISlot();
    }
}