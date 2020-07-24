using Dapr.Actors;
using Dapr.Actors.Runtime;

using OrderActor.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderActor
{
    internal class OrderActor : Actor, IOrderActor, IRemindable
    {
        private static readonly string OrderStateName = "Order";
        private static readonly string CookingStateName = "CookingState";
        private static readonly string CookingReminder = "CookingReminder";

        public OrderActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task<PizzaCookingInfo> GetOrderDataAsync()
        {
            Console.WriteLine("Get order data called ...");
            var pizzaOrder =  await this.StateManager.GetStateAsync<PizzaData>(OrderStateName);
            var cookingState = await this.StateManager.GetStateAsync<CookingState>(CookingStateName);
            var info = new PizzaCookingInfo()
            {
                Order = pizzaOrder,
                State = cookingState
            };            
            return info;
        }

        public async Task PlaceOrderAsync(PizzaData pizza)
        {
            Console.WriteLine($"Placing Order: Pizza {pizza.Pizzatype} Price: {pizza.Price} Special: {String.Join(",", pizza.SpecialIngredients)} Customer: {pizza.Customername}");            
            await this.StateManager.SetStateAsync(OrderStateName, pizza);
            await this.StateManager.SetStateAsync(CookingStateName, CookingState.Ordered);
            await this.RegisterReminderAsync(CookingReminder, null, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(30));
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName == CookingReminder)
            {
                var nextState = CookingState.Ordered;
                var currentState = await this.StateManager.GetStateAsync<CookingState>(CookingStateName);
                if (currentState == CookingState.Ordered)
                {
                    nextState = CookingState.Cooking;
                }
                else
                {
                    nextState = CookingState.Cooked;
                }
                await this.StateManager.SetStateAsync(CookingStateName, nextState);
                if (nextState == CookingState.Cooked)
                {
                    await this.UnregisterReminderAsync(CookingReminder);
                }
            }            
        }
    }
}
