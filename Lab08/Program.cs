using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public class OrderItem
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public abstract class Order
{
    public string OrderId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public decimal TotalAmount { get; protected set; }
    public List<OrderItem> Items { get; private set; }

    protected Order()
    {
        OrderId = Guid.NewGuid().ToString();
        OrderDate = DateTime.Now;
        Items = new List<OrderItem>();
    }

    public abstract void CalculateTotal();
    public abstract string GetOrderType();
}

public class StandardOrder : Order
{
    public override void CalculateTotal()
    {
        TotalAmount = Items.Sum(item => item.Price * item.Quantity);
    }

    public override string GetOrderType()
    {
        return "Стандартне";
    }
}

public class ExpressOrder : Order
{
    private const decimal EXPRESS_FEE = 50.0m;

    public override void CalculateTotal()
    {
        decimal itemsTotal = Items.Sum(item => item.Price * item.Quantity);
        TotalAmount = itemsTotal + EXPRESS_FEE;
    }

    public override string GetOrderType()
    {
        return "Експресне";
    }
}

public class WholesaleOrder : Order
{
    private const decimal DISCOUNT_RATE = 0.1m; // 10% знижка для оптових замовлень

    public override void CalculateTotal()
    {
        decimal itemsTotal = Items.Sum(item => item.Price * item.Quantity);
        TotalAmount = itemsTotal - (itemsTotal * DISCOUNT_RATE);
    }

    public override string GetOrderType()
    {
        return "Оптове";
    }
}

public class OrderFactory
{
    public enum OrderType
    {
        Standard,
        Express,
        Wholesale
    }

    public Order CreateOrder(OrderType type)
    {
        switch (type)
        {
            case OrderType.Standard:
                return new StandardOrder();
            case OrderType.Express:
                return new ExpressOrder();
            case OrderType.Wholesale:
                return new WholesaleOrder();
            default:
                throw new ArgumentException("Невідомий тип замовлення");
        }
    }
}

public interface ILogger
{
    void Log(string message);
}

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"Лог: {message}");
    }
}

public class OrderManager
{
    private readonly ILogger _logger;
    private readonly string _filePath = "orders.json";
    private List<Order> _orders;

    public OrderManager(ILogger logger)
    {
        _logger = logger;
        _orders = LoadOrders();
    }

    public void AddOrder(Order order)
    {
        _orders.Add(order);
        _logger.Log($"Замовлення {order.OrderId} додано.");
        SaveOrders();
    }

    public void ViewOrders()
    {
        foreach (var order in _orders)
        {
            Console.WriteLine($"ID замовлення: {order.OrderId}, Тип: {order.GetOrderType()}, Загальна сума: {order.TotalAmount:C}");
        }
    }

    public void SaveOrders()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_orders, Formatting.Indented);
            File.WriteAllText(_filePath, json);
            _logger.Log("Замовлення збережено у файл.");
        }
        catch (Exception ex)
        {
            _logger.Log($"Помилка при збереженні замовлень: {ex.Message}");
        }
    }

    private List<Order> LoadOrders()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<List<Order>>(json) ?? new List<Order>();
            }
            catch (Exception ex)
            {
                _logger.Log($"Помилка при завантаженні замовлень: {ex.Message}");
                return new List<Order>();
            }
        }
        else
        {
            return new List<Order>();
        }
    }
}

public class Program
{
    public static void Main()
    {
        var logger = new ConsoleLogger();
        var orderFactory = new OrderFactory();
        var orderManager = new OrderManager(logger);

        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Створити замовлення");
            Console.WriteLine("2. Переглянути замовлення");
            Console.WriteLine("3. Вийти");
            Console.Write("Оберіть опцію: ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.WriteLine("Оберіть тип замовлення: ");
                Console.WriteLine("1. Стандартне");
                Console.WriteLine("2. Експресне");
                Console.WriteLine("3. Оптове");
                Console.Write("Оберіть тип замовлення: ");
                var orderTypeChoice = Console.ReadLine();

                Order order = null;

                switch (orderTypeChoice)
                {
                    case "1":
                        order = orderFactory.CreateOrder(OrderFactory.OrderType.Standard);
                        break;
                    case "2":
                        order = orderFactory.CreateOrder(OrderFactory.OrderType.Express);
                        break;
                    case "3":
                        order = orderFactory.CreateOrder(OrderFactory.OrderType.Wholesale);
                        break;
                    default:
                        Console.WriteLine("Невірний вибір!");
                        continue;
                }

                Console.Write("Введіть кількість товарів: ");
                int itemCount;
                if (int.TryParse(Console.ReadLine(), out itemCount))
                {
                    for (int i = 0; i < itemCount; i++)
                    {
                        Console.Write($"Введіть назву товару {i + 1}: ");
                        var itemName = Console.ReadLine();
                        Console.Write($"Введіть ціну товару {i + 1}: ");
                        decimal itemPrice = decimal.Parse(Console.ReadLine());
                        Console.Write($"Введіть кількість товару {i + 1}: ");
                        int itemQuantity = int.Parse(Console.ReadLine());

                        order.Items.Add(new OrderItem
                        {
                            Name = itemName,
                            Price = itemPrice,
                            Quantity = itemQuantity
                        });
                    }

                    order.CalculateTotal();
                    orderManager.AddOrder(order);
                    Console.WriteLine($"Замовлення створено! Загальна сума: {order.TotalAmount:C}");
                }
                else
                {
                    Console.WriteLine("Невірна кількість товарів.");
                }
            }
            else if (choice == "2")
            {
                orderManager.ViewOrders();
                Console.WriteLine("Натисніть будь-яку клавішу для продовження...");
                Console.ReadKey();
            }
            else if (choice == "3")
            {
                break;
            }
            else
            {
                Console.WriteLine("Невірний вибір.");
            }
        }
    }
}
