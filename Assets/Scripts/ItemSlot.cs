public class ItemSlot
{
    public ItemStack itemStack;
    private UiItemSlot _uiItemSlot;

    private const int MaxAmount = 64;

    public bool isCreative;

    public bool HasItem => itemStack != null;

    public ItemSlot(UiItemSlot uiItem)
    {
        itemStack = null;
        _uiItemSlot = uiItem;
        _uiItemSlot.Link(this);
    }

    public ItemSlot(UiItemSlot uiItem, ItemStack itemStackValue)
    {
        itemStack = itemStackValue;
        _uiItemSlot = uiItem;
        _uiItemSlot.Link(this);
    }

    public void LinkUISlot(UiItemSlot uiItem)
    {
        _uiItemSlot = uiItem;
    }
    public void UnlinkUISlot()
    {
        _uiItemSlot = null;
    }

    public void EmptySlot()
    {
        itemStack = null;
        if (_uiItemSlot)
            _uiItemSlot.UpdateSlot();
    }

    public int Take(int amt)
    {
        if (amt > itemStack.amount)
        {
            int amtGet = itemStack.amount;
            EmptySlot();
            return amtGet;
        }
        else if (amt < itemStack.amount)
        {
            itemStack.amount -= amt;
            _uiItemSlot.UpdateSlot();
            return amt;
        }
        else
        {
            EmptySlot();
            return amt;
        }
    }

    public ItemStack TakeAll()
    {
        ItemStack handOver = new ItemStack(itemStack.id, itemStack.amount);
        EmptySlot();
        return handOver;
    }

    public int InsertAmount(int amt)
    {
        var calcAmt = amt + itemStack.amount;
        if (calcAmt >= MaxAmount)
        {
            itemStack.amount = MaxAmount;
            _uiItemSlot.UpdateSlot();
            return calcAmt - MaxAmount;
        }

        itemStack.amount += amt;
        _uiItemSlot.UpdateSlot();
        return 0;
    }

    public void InsertItemStack(ItemStack stack)
    {
        itemStack = stack;
        _uiItemSlot.UpdateSlot();
    }
}
