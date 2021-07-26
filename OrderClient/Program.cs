using Dapr.Actors;
using Dapr.Actors.Client;

using OrderActor.Interfaces;

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrderClient
{
    class Program
    {
        private static readonly string CustomerActorType = "CustomerActor";
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            WriteSuccess("Willkommen bei Dapr Pizza!");
            string customerId = GetInput("Bitte geben Sie ihre BenutzerId ein : ");

            var customerActorId = new ActorId(customerId);
            var customerProxy = ActorProxy.Create<ICustomerActor>(customerActorId, CustomerActorType);
            var customerdata = await customerProxy.GetDataAsync();
            if (customerdata == null || string.IsNullOrEmpty(customerdata.Name))
            {
                customerdata ??= new CustomerData();
                Console.Write("Customername: ");
                var customername = Console.ReadLine();
                customerdata.Name = customername;
                await customerProxy.SetDataAsync(customerdata);
            }

            WriteSuccess($"Hallo {customerdata.Name}!");

            MenuAction selected = MenuAction.None;
            while (selected != MenuAction.Exit)
            {
                try
                {
                    selected = DisplayMenu();
                    switch (selected)
                    {
                        case MenuAction.OrderInfo:
                            await GetOrderInfo(customerProxy, customerdata.Name, customerId);
                            break;
                        case MenuAction.NewOrder:
                            await PlaceNewOrderAsync(customerProxy, customerdata.Name, customerId);
                            break;
                        case MenuAction.Generate:
                            await GenerateOrders(customerProxy, customerdata.Name, customerId);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    WriteCaption("Fehler: " + ex.Message, ConsoleColor.Red, ConsoleColor.White);
                }
            }



        }

        private static string GetInput(string caption)
        {
            WriteCaption(caption, ConsoleColor.White, ConsoleColor.Black);
            string line;
            while (string.IsNullOrEmpty(line = Console.ReadLine()))
            {
            }

            return line;
        }

        private static void WriteSuccess(string message)
        {
            WriteCaption(message, ConsoleColor.Green, ConsoleColor.Black);
        }

        private static void WriteError (string error)
        {
            WriteCaption(error, ConsoleColor.Red, ConsoleColor.White);
        }

        private static void WriteCaption(string caption, ConsoleColor background, ConsoleColor foreground)
        {
            var oldBackground = Console.BackgroundColor;
            var oldForeground = Console.ForegroundColor;
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
            Console.WriteLine(caption);
            Console.BackgroundColor = oldBackground;
            Console.ForegroundColor = oldForeground;
        }

        private static async Task GetOrderInfo(ICustomerActor customerProxy, string customername, string customerId)
        {
            var ordernr = GetInput("Bitte geben Sie die Bestellnummer ein:");
            var orderId = int.Parse(ordernr);
            var ordered = await customerProxy.GetOrderDataAsync(orderId);            
            switch (ordered.State)
            {
                case CookingState.Cooking:
                    WriteSuccess($"Die Pizza {ordered.Order.Pizzatype} mit den Extras {string.Join(", ", ordered.Order.SpecialIngredients)} wird gerade gekocht");
                    break;
                case CookingState.Cooked:
                    WriteSuccess($"Guten Appetit: Pizza {ordered.Order.Pizzatype} mit den Extras {string.Join(", ", ordered.Order.SpecialIngredients)} wird ausgeliefert");
                    break;
                default:
                    WriteSuccess(JsonSerializer.Serialize(ordered));
                    break;

            }
        }

        private static async Task GenerateOrders(ICustomerActor customerProxy, string customername, string customerId)
        {
            var anzahlstr = GetInput("Wieviele Bestellungen sollen getätigt werden ?");
            var anzahl = Convert.ToInt32(anzahlstr);
            var pizza = GetInput("Pizza (Preis 7.55€): ");
            var ingredients = GetInput("Extra Zutaten(Zutat1, Zutat2).Preis pro Zutat: 1.31€: ");

            var price = 7.55m + (ingredients.Length) * 1.31m;
            var data = new PizzaData()
            {
                CustomerId = customerId,
                Customername = customername,
                Pizzatype = pizza,
                SpecialIngredients = ingredients.Split(","),
                Price = price
            };
            for (int i = 0; i < anzahl; i++)
            {
                var orderId = await customerProxy.PlaceOrderAsync(data);
                WriteSuccess($"Vielen Dank. Der Auftrag mit der Id {orderId} wurde aufgegeben.");
            }
        }

        private static async Task PlaceNewOrderAsync(ICustomerActor customerProxy, string customername, string customerId)
        {
            var pizza = GetInput("Pizza (Preis 7.55€): ");                        
            var ingredients = GetInput("Extra Zutaten(Zutat1, Zutat2).Preis pro Zutat: 1.31€: ");

            var price = 7.55m + (ingredients.Length) * 1.31m;                                    
            var data = new PizzaData()
            {
                CustomerId = customerId,
                Customername = customername,
                Pizzatype = pizza,
                SpecialIngredients = ingredients.Split(","),
                Price = price
            };
            Console.WriteLine("Auftrag wird aufgegeben ....");
            var orderId = await customerProxy.PlaceOrderAsync(data).ConfigureAwait(true);
            WriteSuccess($"Vielen Dank. Der Auftrag mit der Id {orderId} wurde aufgegeben.");
            Console.WriteLine("Aktueller Status:");
            var ordered = await customerProxy.GetOrderDataAsync(orderId);
            WriteSuccess(JsonSerializer.Serialize(ordered));
        }

        static MenuAction DisplayMenu ()
        {
            WriteCaption("Bitte wählen Sie:", ConsoleColor.White, ConsoleColor.Black);
            Console.WriteLine(" 1: Bestellung abfragen");
            Console.WriteLine(" 2: Bestellung aufgeben");
            Console.WriteLine(" 3: Beenden");
            Console.WriteLine(" 9: Generate new customers");
            var result = Console.ReadKey();
            switch (result.KeyChar)
            {
                case '1': return MenuAction.OrderInfo;
                case '2': return MenuAction.NewOrder;
                case '3': return MenuAction.Exit;
                case '9': return MenuAction.Generate;
                default: return MenuAction.None;
            }
        }
    }

    public enum MenuAction
    {
        None = 0,
        OrderInfo = 1,
        NewOrder = 2,
        Exit = 3,
        Generate = 9
    }
}
