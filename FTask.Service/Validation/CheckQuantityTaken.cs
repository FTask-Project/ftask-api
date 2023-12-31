﻿namespace FTask.Service.Validation;

public interface ICheckQuantityTaken
{
    int check(int quantity);
    public int PageQuantity { get; }
}

internal class CheckQuantityTaken : ICheckQuantityTaken
{
    private static readonly int MAX_QUANTITY = 100;
    private static readonly int PAGE_QUANTITY = 10;
    public int check(int quantity)
    {
        /*if (quantity > MAX_QUANTITY)
        {
            return MAX_QUANTITY;
        }
        else if (quantity == 0)
        {
            return 100;
        }
        else
        {
            return quantity;
        }*/
        return 100;
    }

    public int PageQuantity
    {
        get => PAGE_QUANTITY;
    }
}
