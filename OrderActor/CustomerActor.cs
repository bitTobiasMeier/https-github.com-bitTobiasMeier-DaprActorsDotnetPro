using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;

using OrderActor.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderActor
{
    internal class CustomerActor : Actor, ICustomerActor
    {
        private static readonly string CustomerDataStateName = "Customerdata";
        private static readonly string CustomerOrderIdStateName = "CustomerOrderIdState";
        public CustomerActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            var lastOrderId = await this.GetLastOrderIdAsync();
            Console.WriteLine($"Last OrderId {lastOrderId}");
        }

        public async Task<PizzaCookingInfo> GetOrderDataAsync(long orderId)
        {
            try
            {
                var orderactorId = new ActorId("o" + this.Id + $"_{orderId}");
                var orderproxy = ActorProxy.Create<IOrderActor>(orderactorId, "OrderActor");
                return await orderproxy.GetOrderDataAsync();
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public async Task<long> PlaceOrderAsync(PizzaData pizza)
        {
            try
            {
                var lastOrderId = await this.GetLastOrderIdAsync();
                var orderId = lastOrderId + 1;
                var orderactorId = new ActorId("o" + this.Id + $"_{orderId}");
                var orderproxy = ActorProxy.Create<IOrderActor>(orderactorId, "OrderActor");
                await orderproxy.PlaceOrderAsync(pizza);
                await this.StateManager.SetStateAsync(CustomerOrderIdStateName, orderId);
                return orderId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during PlaceOrderAsync: {ex.Message}. Stacktrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task SetDataAsync(CustomerData data)
        {
            await this.StateManager.SetStateAsync(CustomerDataStateName, data);
        }

        public async Task<CustomerData> GetDataAsync()
        {
            try 
            { 
                return await this.StateManager.GetStateAsync<CustomerData>(CustomerDataStateName);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        private async Task<long> GetLastOrderIdAsync ()
        {
            try
            {
                return await this.StateManager.GetStateAsync<long>(CustomerOrderIdStateName);
            } catch (KeyNotFoundException notfoundex)
            {
                Console.WriteLine("Key not found" + notfoundex.Message);
                await this.StateManager.SetStateAsync(CustomerOrderIdStateName, 0);
                return 0;
            }            
        }        
    }
}
