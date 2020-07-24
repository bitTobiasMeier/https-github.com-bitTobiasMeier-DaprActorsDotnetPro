using System;
using System.Collections;

namespace OrderActor.Interfaces
{
    [Serializable()]
    public class PizzaData
    {
        public string CustomerId { get; set; }
        public string Customername { get; set; }
        public string Pizzatype { get; set; }
        public string[] SpecialIngredients { get; set; }
        public decimal Price { get; set; }
    }

    public class PizzaCookingInfo
    {
        public PizzaData Order { get; set; }
        public CookingState State { get; set; }        
    }

    public enum CookingState
    {
        Ordered = 0,
        Cooking = 1,
        Cooked = 2
    }

    [Serializable]
    public class CustomerData
    {
        public string Name { get; set; }
    }
}
