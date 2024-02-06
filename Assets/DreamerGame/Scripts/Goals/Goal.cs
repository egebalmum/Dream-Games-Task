using System;
using UnityEngine.Events;

[Serializable]
public class Goal
{
    public (ItemType type, ColorType color) goalIdentity;
    public int amount;
    public UnityEvent onAmountDecrement = new UnityEvent();
    public UnityEvent onAmountIncrement = new UnityEvent();
    public UnityEvent onFinished = new UnityEvent();

    public Goal((ItemType type, ColorType color) goalIdentity)
    {
        this.goalIdentity = goalIdentity;
        amount = 0;
    }

    public void IncrementAmount()
    {
        amount += 1;
        onAmountIncrement?.Invoke();
    }

    public void DecrementAmount()
    {
        if (amount == 0)
        {
            //FINISHED
            return;
        }
        amount -= 1;
        onAmountDecrement?.Invoke();
        if (amount == 0)
        {
            onFinished?.Invoke();
        }
    }
}
