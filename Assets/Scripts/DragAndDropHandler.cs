using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField]
    private UiItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;

    [SerializeField]
    private GraphicRaycaster m_Raycaster = null;
    private PointerEventData m_PointerEventData;

    [SerializeField]
    private EventSystem m_EventSystem = null;

    World world;

    private void Start()
    {

        world = GameObject.Find("World").GetComponent<World>(); // junk

        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if (!world.inUI) return;

        cursorSlot.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            HandleSlotClick(CheckForSlot());
        }
    }

    private void HandleSlotClick(UiItemSlot clickedSlot)
    {
        if (clickedSlot == null) return;
        if (!cursorItemSlot.HasItem && !clickedSlot.HasItem) return;

        if (clickedSlot.itemSlot.isCreative)
        {
                cursorItemSlot.EmptySlot();
                cursorItemSlot.InsertItemStack(clickedSlot.itemSlot.itemStack);
        }

        if (!cursorItemSlot.HasItem && clickedSlot.HasItem)
        {
            cursorItemSlot.InsertItemStack(clickedSlot.itemSlot.TakeAll());
            return;
        }
        if (cursorItemSlot.HasItem && !clickedSlot.HasItem)
        {
            clickedSlot.itemSlot.InsertItemStack(cursorItemSlot.TakeAll());
            return;
        }

        if (cursorItemSlot.HasItem && clickedSlot.HasItem)
        {
            if (cursorItemSlot.itemStack.id != clickedSlot.itemSlot.itemStack.id)
            {
                
                ItemStack oldCursorSlot = cursorItemSlot.TakeAll();
                ItemStack oldItemSlot = clickedSlot.itemSlot.TakeAll();

                clickedSlot.itemSlot.InsertItemStack(oldCursorSlot);
                cursorItemSlot.InsertItemStack(oldItemSlot);
            }else if (cursorItemSlot.itemStack.amount != clickedSlot.itemSlot.itemStack.amount)
            {
                int over = clickedSlot.itemSlot.InsertAmount(cursorItemSlot.itemStack.amount);
                cursorItemSlot.itemStack.amount = 0;
                cursorItemSlot.InsertAmount(over);
            }
        }
    }

    private UiItemSlot CheckForSlot()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.tag == "UIItemSlot")
                return result.gameObject.GetComponent<UiItemSlot>();
        }
        return null;
    }
}
