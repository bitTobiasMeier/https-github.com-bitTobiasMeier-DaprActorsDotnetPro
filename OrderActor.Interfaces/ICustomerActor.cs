using Dapr.Actors;

using System.Threading.Tasks;

namespace OrderActor.Interfaces
{
    public interface ICustomerActor : IActor
    {
        Task<long> PlaceOrderAsync(PizzaData pizza);
        
        Task<PizzaCookingInfo> GetOrderDataAsync(long orderId);

        Task SetDataAsync(CustomerData data);

        Task<CustomerData> GetDataAsync();
    }
}
