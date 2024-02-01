using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ColumnQueue
{
    private List<Item> queue;
    private Vector3 defaultPosition;

    public ColumnQueue(Vector3 defaultPosition)
    {
        this.defaultPosition = defaultPosition;
        queue = new List<Item>();
    }

    public Vector3 GetNextPosition()
    {
        if (queue.Count == 0)
        {
            return defaultPosition;
        }
        return queue[Count() - 1].transform.position + (Vector3.up * Board.Instance.cellSize.y);
    }

    public void Enqueue(Item item)
    {
        queue.Add(item);
    }

    public void Dequeue(Item item)
    {
        queue.Remove(item);
    }

    public int Count()
    {
        return queue.Count;
    }
}
