public class ItemSlot
{
    public ItemStack itemStack;
    private UiItemSlot uiItemSlot;

    public int maxAmount = 64;

    public bool isCreative;

    public bool HasItem
    {
        get
        {
            if (itemStack == null)
                return false;
            else
                return true;
        }
    }

    public ItemSlot(UiItemSlot uiItem)
    {
        itemStack = null;
        uiItemSlot = uiItem;
        uiItemSlot.Link(this);
    }

    public ItemSlot(UiItemSlot uiItem, ItemStack itemStackValue)
    {
        itemStack = itemStackValue;
        uiItemSlot = uiItem;
        uiItemSlot.Link(this);
    }

    public void LinkUISlot(UiItemSlot uiItem)
    {
        uiItemSlot = uiItem;
    }
    public void UnlinkUISlot()
    {
        uiItemSlot = null;
    }

    public void EmptySlot()
    {
        itemStack = null;
        if (uiItemSlot != null)
            uiItemSlot.UpdateSlot();
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
            uiItemSlot.UpdateSlot();
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
        int calcAmt = amt + itemStack.amount;
        if (calcAmt >= maxAmount)
        {
            itemStack.amount = maxAmount;
            uiItemSlot.UpdateSlot();
            return calcAmt - maxAmount;
        }

        itemStack.amount += amt;
        uiItemSlot.UpdateSlot();
        return 0;
    }

    public void InsertItemStack(ItemStack stack)
    {
        itemStack = stack;
        uiItemSlot.UpdateSlot();
    }
}
