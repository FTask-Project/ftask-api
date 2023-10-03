using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.Validation;

public interface ICheckQuantityTaken
{
    int check(int quantity);
}

internal class CheckQuantityTaken : ICheckQuantityTaken
{
    private static readonly int MAX_QUANTITY = 50;
    public int check(int quantity)
    {
        if(quantity > MAX_QUANTITY)
        {
            return MAX_QUANTITY;
        }
        else if(quantity == 0)
        {
            return 10;
        }
        else
        {
            return quantity;
        }
    }
}
