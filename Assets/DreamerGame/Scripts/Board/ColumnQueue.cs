using System.Collections.Generic;
using UnityEngine;

public class ColumnQueue
{
    private readonly List<Item> _queue;
    private readonly Vector3 _defaultPosition;

    public ColumnQueue(Vector3 defaultPosition)
    {
        _defaultPosition = defaultPosition;
        _queue = new List<Item>();
    }

    public Vector3 GetNextPosition()
    {
        if (_queue.Count == 0)
        {
            return _defaultPosition;
        }
        return _queue[Count() - 1].transform.position + (Vector3.up * Board.Instance.cellSize.y);
    }

    public void Enqueue(Item item)
    {
        _queue.Add(item);
    }

    public void Dequeue(Item item)
    {
        _queue.Remove(item);
    }

    public int Count()
    {
        return _queue.Count;
    }

    public Item GetFrontItem(Item item)
    {
        int index = _queue.IndexOf(item);
        if (index == 0)
        {
            return null;
        }

        return _queue[index - 1];
    }
}
