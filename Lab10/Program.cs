using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// --- Патерн "Unified Interface" ---
public interface IFinancialOperation
{
    string Id { get; }
    decimal Amount { get; }
    string Category { get; }
    DateTime Date { get; }
    string GetOperationType();
}

// --- Базові транзакції ---
public class Income : IFinancialOperation
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public DateTime Date { get; } = DateTime.Now;
    public string GetOperationType() => "Дохід";
}

public class Expense : IFinancialOperation
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public DateTime Date { get; } = DateTime.Now;
    public string GetOperationType() => "Витрата";
}

// --- Нові типи операцій (Достатній рівень) ---
public class Investment
{
    public string OperationId = Guid.NewGuid().ToString();
    public decimal InvestedAmount;
    public string Sector;
    public DateTime InvestmentDate = DateTime.Now;
}

// --- Патерн Adapter ---
public class InvestmentAdapter : IFinancialOperation
{
    private Investment _investment;
    public InvestmentAdapter(Investment inv) => _investment = inv;

    public string Id => _investment.OperationId;
    public decimal Amount => _investment.InvestedAmount;
    public string Category => _investment.Sector;
    public DateTime Date => _investment.InvestmentDate;
    public string GetOperationType() => "Інвестиція";
}

// --- Декоратор для валют ---
public class CurrencyDecorator : IFinancialOperation
{
    private IFinancialOperation _inner;
    private decimal _rate;
    private string _currency;

    public CurrencyDecorator(IFinancialOperation inner, decimal rate, string currency)
    {
        _inner = inner;
        _rate = rate;
        _currency = currency;
    }

    public string Id => _inner.Id;
    public string Category => _inner.Category;
    public DateTime Date => _inner.Date;
    public decimal Amount => Math.Round(_inner.Amount * _rate, 2);
    public string GetOperationType() => $"{_inner.GetOperationType()} ({_currency})";
}

// --- Proxy (контроль доступу) ---
public enum Role { Reader, Editor, Admin }

public class FinancialDataProxy
{
    private Role _userRole;
    private FinancialManager _realManager;

    public FinancialDataProxy(Role role, FinancialManager manager)
    {
        _userRole = role;
        _realManager = manager;
    }

    public void Add(IFinancialOperation op)
    {
        if (_userRole == Role.Reader)
        {
            Console.WriteLine("❌ Доступ заборонено: тільки для редакторів.");
            return;
        }
        _realManager.AddTransaction(op);
    }

    public void Show()
    {
        foreach (var op in _realManager.GetTransactions())
        {
            Console.WriteLine($"{op.Date:dd.MM.yyyy} | {op.GetOperationType(),-15} | {op.Category,-10} | {op.Amount,8} грн");
        }
    }

    public void ShowStatisticsInCurrency(decimal rate, string currency)
    {
        var total = _realManager.GetTransactions()
            .Select(t => new CurrencyDecorator(t, rate, currency))
            .ToList();

        decimal sum = total.Sum(t => t.Amount);
        Console.WriteLine($"\n📊 Сума всіх операцій у {_realManager.GetTransactions().Count} транзакціях: {sum} {currency}\n");
    }
}

// --- Управління транзакціями ---
public class FinancialManager
{
    private List<IFinancialOperation> _transactions = new();

    public void AddTransaction(IFinancialOperation t) => _transactions.Add(t);

    public List<IFinancialOperation> GetTransactions() => _transactions;
}

// --- Симуляція користувача ---
class Program
{
    static void Main()
    {
        var manager = new FinancialManager();

        Console.WriteLine("=== Фінансовий Менеджер ===");
        Console.Write("Введіть роль користувача (Reader / Editor / Admin): ");
        string inputRole = Console.ReadLine();
        Role role = Enum.TryParse(inputRole, true, out Role parsedRole) ? parsedRole : Role.Reader;

        var proxy = new FinancialDataProxy(role, manager);

        while (true)
        {
            Console.WriteLine("\n--- Меню ---");
            Console.WriteLine("1. Додати дохід");
            Console.WriteLine("2. Додати витрату");
            Console.WriteLine("3. Додати інвестицію");
            Console.WriteLine("4. Переглянути всі транзакції");
            Console.WriteLine("5. Переглянути статистику у валюті");
            Console.WriteLine("0. Вихід");
            Console.Write("Ваш вибір: ");

            string choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Сума доходу: ");
                    decimal income = decimal.Parse(Console.ReadLine());
                    Console.Write("Категорія: ");
                    string incCat = Console.ReadLine();
                    proxy.Add(new Income { Amount = income, Category = incCat });
                    break;

                case "2":
                    Console.Write("Сума витрати: ");
                    decimal expense = decimal.Parse(Console.ReadLine());
                    Console.Write("Категорія: ");
                    string expCat = Console.ReadLine();
                    proxy.Add(new Expense { Amount = expense, Category = expCat });
                    break;

                case "3":
                    Console.Write("Сума інвестиції: ");
                    decimal invest = decimal.Parse(Console.ReadLine());
                    Console.Write("Сектор: ");
                    string sector = Console.ReadLine();
                    proxy.Add(new InvestmentAdapter(new Investment { InvestedAmount = invest, Sector = sector }));
                    break;

                case "4":
                    proxy.Show();
                    break;

                case "5":
                    Console.Write("Курс валюти (наприклад, USD = 0.027): ");
                    decimal rate = decimal.Parse(Console.ReadLine());
                    Console.Write("Назва валюти (наприклад, USD): ");
                    string curr = Console.ReadLine();
                    proxy.ShowStatisticsInCurrency(rate, curr);
                    break;

                case "0":
                    Console.WriteLine("До побачення!");
                    return;

                default:
                    Console.WriteLine("Невірна команда!");
                    break;
            }
        }
    }
}
