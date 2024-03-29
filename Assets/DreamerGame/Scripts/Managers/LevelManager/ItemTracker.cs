using System;
using System.Collections.Generic;
[Serializable]
public class ItemTracker
{
    public HashSet<Item> markedItems;
    public int coroutineCount;

    public ItemTracker()
    {
        markedItems = new HashSet<Item>();
        coroutineCount = 0;
    }

    public void ResetTracker()
    {
        markedItems.Clear();
        coroutineCount = 0;
    }
}
