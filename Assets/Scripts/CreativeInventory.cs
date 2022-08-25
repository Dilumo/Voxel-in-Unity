using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeInventory : MonoBehaviour
{
    public GameObject slotPrefad;
    private World _world;

    private List<ItemSlot> _slots = new List<ItemSlot>();

    private void Start()
    {
        _world = GameObject.Find("World").GetComponent<World>(); // junk
        for (var i = 1; i < _world.blockTypes.Length; i++)
        {
            var newSlot = Instantiate(slotPrefad, transform);
            var stack = new ItemStack((byte)i, 64);
            var slot = new ItemSlot(newSlot.GetComponent<UiItemSlot>(), stack)
            {
                isCreative = true
            };
        }
    }
}
