using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// 1. Модель книги
public class Book
{
    public string ISBN { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int PublicationYear { get; set; }
}

// 2. Валідація
public class BookValidator
{
    public bool ValidateBook(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.ISBN) || string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author))
            return false;
        if (book.PublicationYear < 1000 || book.PublicationYear > DateTime.Now.Year)
            return false;
        return true;
    }
}

// 3. Сховище з підтримкою JSON
public class BookRepository
{
    private List<Book> books = new List<Book>();
    private readonly string filePath = "books.json";

    public BookRepository()
    {
        LoadFromFile();
    }

    public void AddBook(Book book)
    {
        books.Add(book);
        SaveToFile();
    }

    public List<Book> GetAllBooks()
    {
        return books.ToList();
    }

    public Book GetBookByISBN(string isbn)
    {
        return books.FirstOrDefault(b => b.ISBN == isbn);
    }

    private void SaveToFile()
    {
        string json = JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    private void LoadFromFile()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            books = JsonSerializer.Deserialize<List<Book>>(json) ?? new List<Book>();
        }
    }
}

// 4. Стратегії пошуку
public interface IBookSearchStrategy
{
    List<Book> Search(List<Book> books, string searchTerm);
}

public class TitleSearchStrategy : IBookSearchStrategy
{
    public List<Book> Search(List<Book> books, string searchTerm)
    {
        return books.Where(b => b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}

public class AuthorSearchStrategy : IBookSearchStrategy
{
    public List<Book> Search(List<Book> books, string searchTerm)
    {
        return books.Where(b => b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}

public class ISBNStrategy : IBookSearchStrategy
{
    public List<Book> Search(List<Book> books, string searchTerm)
    {
        return books.Where(b => b.ISBN.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}

public class BookSearcher
{
    private readonly IBookSearchStrategy _searchStrategy;
    public BookSearcher(IBookSearchStrategy strategy)
    {
        _searchStrategy = strategy;
    }

    public List<Book> Search(List<Book> books, string searchTerm)
    {
        return _searchStrategy.Search(books, searchTerm);
    }
}

// 5. Сповіщення
public interface INotificationService
{
    void SendNotification(string message);
}

public class ConsoleNotificationService : INotificationService
{
    public void SendNotification(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[СПОВІЩЕННЯ]: {message}");
        Console.ResetColor();
    }
}

// 6. Головне меню
public class LibraryApp
{
    private readonly BookRepository repository = new();
    private readonly BookValidator validator = new();
    private readonly INotificationService notificationService = new ConsoleNotificationService();

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("\n--- МЕНЮ БІБЛІОТЕКИ ---");
            Console.WriteLine("1. Додати книгу");
            Console.WriteLine("2. Переглянути всі книги");
            Console.WriteLine("3. Пошук за назвою");
            Console.WriteLine("4. Пошук за автором");
            Console.WriteLine("5. Пошук за ISBN");
            Console.WriteLine("6. Вийти");
            Console.Write("Оберіть опцію: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddBook();
                    break;
                case "2":
                    DisplayBooks(repository.GetAllBooks());
                    break;
                case "3":
                    SearchBooks(new TitleSearchStrategy());
                    break;
                case "4":
                    SearchBooks(new AuthorSearchStrategy());
                    break;
                case "5":
                    SearchBooks(new ISBNStrategy());
                    break;
                case "6":
                    return;
                default:
                    Console.WriteLine("Невірна опція!");
                    break;
            }
        }
    }

    private void AddBook()
    {
        var book = new Book();
        Console.Write("ISBN: ");
        book.ISBN = Console.ReadLine();
        Console.Write("Назва: ");
        book.Title = Console.ReadLine();
        Console.Write("Автор: ");
        book.Author = Console.ReadLine();
        Console.Write("Рік публікації: ");
        if (int.TryParse(Console.ReadLine(), out int year))
            book.PublicationYear = year;
        else
        {
            Console.WriteLine("Некоректний рік.");
            return;
        }

        if (validator.ValidateBook(book))
        {
            repository.AddBook(book);
            notificationService.SendNotification("Книгу успішно додано!");
        }
        else
        {
            Console.WriteLine("Дані книги некоректні!");
        }
    }

    private void DisplayBooks(List<Book> books)
    {
        if (books.Count == 0)
        {
            Console.WriteLine("Бібліотека порожня.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        foreach (var b in books)
        {
            Console.WriteLine($"[{b.ISBN}] \"{b.Title}\" — {b.Author} ({b.PublicationYear})");
        }
        Console.ResetColor();
    }

    private void SearchBooks(IBookSearchStrategy strategy)
    {
        Console.Write("Введіть пошуковий запит: ");
        string query = Console.ReadLine();
        var searcher = new BookSearcher(strategy);
        var results = searcher.Search(repository.GetAllBooks(), query);
        DisplayBooks(results);
    }
}

// 7. Точка входу
class Program
{
    static void Main(string[] args)
    {
        var app = new LibraryApp();
        app.Run();
    }
}
