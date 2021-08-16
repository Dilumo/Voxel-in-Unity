using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbar : MonoBehaviour
{
    public RectTransform highlight;
    public Player player;

    public UiItemSlot[] slots;

    public int slotIndex = 0;

    private void Start()
    {
        for (byte i = 0; i < slots.Length; i++)
        {
            ItemStack stack = new ItemStack(System.Convert.ToByte(i + 1), Random.Range(2, 99));
            ItemSlot slot = new ItemSlot(slots[i], stack);
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0)
                slotIndex--;
            else
                slotIndex++;

            if (slotIndex > slots.Length - 1)
                slotIndex = 0;
            if (slotIndex < 0)
                slotIndex = slots.Length - 1;

            highlight.position = slots[slotIndex].slotIcon.transform.position;
        }
    }
}
