using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeInventory : MonoBehaviour
{
    public GameObject slotPrefad;
    World world;

    List<ItemSlot> slots = new List<ItemSlot>();

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>(); // junk
        for (int i = 1; i < world.blockTypes.Length; i++)
        {
            GameObject newSlot = Instantiate(slotPrefad, transform);
            ItemStack stack = new ItemStack((byte)i, 64);
            ItemSlot slot = new ItemSlot(newSlot.GetComponent<UiItemSlot>(), stack);
            slot.isCreative = true;
        }
    }
}
