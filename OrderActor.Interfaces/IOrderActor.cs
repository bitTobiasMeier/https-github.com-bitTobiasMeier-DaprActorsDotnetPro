using Dapr.Actors;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderActor.Interfaces
{
    public interface IOrderActor : IActor
    {
        Task PlaceOrderAsync(PizzaData pizza);

        Task<PizzaCookingInfo> GetOrderDataAsync();
    }
}
