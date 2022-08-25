using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField]
    private UiItemSlot cursorSlot = null;
    private ItemSlot _cursorItemSlot;

    [FormerlySerializedAs("m_Raycaster")] [SerializeField]
    private GraphicRaycaster mRaycaster = null;
    private PointerEventData _mPointerEventData;

    [FormerlySerializedAs("m_EventSystem")] [SerializeField]
    private EventSystem mEventSystem = null;

    private World _world;

    private void Start()
    {

        _world = GameObject.Find("World").GetComponent<World>(); // junk

        _cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if (!_world.inUI) return;

        cursorSlot.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            HandleSlotClick(CheckForSlot());
        }
    }

    private void HandleSlotClick(UiItemSlot clickedSlot)
    {
        if (!clickedSlot) return;
        if (!_cursorItemSlot.HasItem && !clickedSlot.HasItem) return;

        if (clickedSlot.itemSlot.isCreative)
        {
                _cursorItemSlot.EmptySlot();
                _cursorItemSlot.InsertItemStack(clickedSlot.itemSlot.itemStack);
        }

        switch (_cursorItemSlot.HasItem)
        {
            case false when clickedSlot.HasItem:
                _cursorItemSlot.InsertItemStack(clickedSlot.itemSlot.TakeAll());
                return;
            case true when !clickedSlot.HasItem:
                clickedSlot.itemSlot.InsertItemStack(_cursorItemSlot.TakeAll());
                return;
            case true when clickedSlot.HasItem:
            {
                if (_cursorItemSlot.itemStack.id != clickedSlot.itemSlot.itemStack.id)
                {
                
                    var oldCursorSlot = _cursorItemSlot.TakeAll();
                    var oldItemSlot = clickedSlot.itemSlot.TakeAll();

                    clickedSlot.itemSlot.InsertItemStack(oldCursorSlot);
                    _cursorItemSlot.InsertItemStack(oldItemSlot);
                }else if (_cursorItemSlot.itemStack.amount != clickedSlot.itemSlot.itemStack.amount)
                {
                    var over = clickedSlot.itemSlot.InsertAmount(_cursorItemSlot.itemStack.amount);
                    _cursorItemSlot.itemStack.amount = 0;
                    _cursorItemSlot.InsertAmount(over);
                }

                break;
            }
        }
    }

    private UiItemSlot CheckForSlot()
    {
        _mPointerEventData = new PointerEventData(mEventSystem)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        mRaycaster.Raycast(_mPointerEventData, results);

        return (from result in results where result.gameObject.CompareTag("UIItemSlot") select result.gameObject.GetComponent<UiItemSlot>()).FirstOrDefault();
    }
}
