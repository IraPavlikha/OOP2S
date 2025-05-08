using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Student
{
    public string Id { get; private set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Course { get; set; }
    public List<int> Grades { get; private set; }

    protected Student(string name, string surname, int course)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Surname = surname;
        Course = course;
        Grades = new List<int>();
    }

    public abstract string GetStudentType();
    public abstract decimal CalculateAverageGrade();
}

public class BachelorStudent : Student
{
    public BachelorStudent(string name, string surname, int course)
        : base(name, surname, course)
    {
    }

    public override string GetStudentType() => "Бакалавр";
    public override decimal CalculateAverageGrade()
    {
        return Grades.Any() ? (decimal)Grades.Average() : 0;
    }
}

public class MasterStudent : Student
{
    public MasterStudent(string name, string surname, int course)
        : base(name, surname, course)
    {
    }

    public override string GetStudentType() => "Магістр";
    public override decimal CalculateAverageGrade()
    {
        return Grades.Any() ? (decimal)Grades.Average() : 0;
    }
}

public interface IGradeEvaluationStrategy
{
    string EvaluatePerformance(decimal averageGrade);
}

public class TraditionalGradeStrategy : IGradeEvaluationStrategy
{
    public string EvaluatePerformance(decimal averageGrade)
    {
        if (averageGrade >= 90)
            return "Відмінно";
        else if (averageGrade >= 75)
            return "Добре";
        else if (averageGrade >= 60)
            return "Задовільно";
        else
            return "Незадовільно";
    }
}

public class ECTSGradeStrategy : IGradeEvaluationStrategy
{
    public string EvaluatePerformance(decimal averageGrade)
    {
        if (averageGrade >= 90)
            return "A";
        else if (averageGrade >= 75)
            return "B";
        else if (averageGrade >= 60)
            return "C";
        else if (averageGrade >= 50)
            return "D";
        else if (averageGrade >= 35)
            return "E";
        else
            return "FX";
    }
}

public class StudentManager
{
    private List<Student> _students;
    public StudentManager()
    {
        _students = new List<Student>();
    }

    public void AddStudent(Student student)
    {
        _students.Add(student);
    }

    public void AddGrade(Student student, int grade)
    {
        student.Grades.Add(grade);
    }

    public List<Student> GetAllStudents()
    {
        return new List<Student>(_students);
    }

    public decimal CalculateTotalAverageGrade()
    {
        return _students.Any()
            ? _students.Average(s => s.CalculateAverageGrade())
            : 0;
    }
}

public class StudentFactory
{
    public enum StudentType
    {
        Bachelor,
        Master
    }

    public Student CreateStudent(StudentType type, string name, string surname, int course)
    {
        switch (type)
        {
            case StudentType.Bachelor:
                return new BachelorStudent(name, surname, course);
            case StudentType.Master:
                return new MasterStudent(name, surname, course);
            default:
                throw new ArgumentException("Невідомий тип студента");
        }
    }
}

public interface IStudentCommand
{
    void Execute();
    void Undo();
}

public class AddGradeCommand : IStudentCommand
{
    private Student _student;
    private int _grade;
    private StudentManager _studentManager;

    public AddGradeCommand(Student student, int grade, StudentManager studentManager)
    {
        _student = student;
        _grade = grade;
        _studentManager = studentManager;
    }

    public void Execute()
    {
        _studentManager.AddGrade(_student, _grade);
    }

    public void Undo()
    {
        _student.Grades.Remove(_grade);
    }
}

public class CommandManager
{
    private Stack<IStudentCommand> _commands = new Stack<IStudentCommand>();

    public void ExecuteCommand(IStudentCommand command)
    {
        command.Execute();
        _commands.Push(command);
    }

    public void UndoLastCommand()
    {
        if (_commands.Any())
        {
            var command = _commands.Pop();
            command.Undo();
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        var studentManager = new StudentManager();
        var studentFactory = new StudentFactory();
        var commandManager = new CommandManager();

        bool running = true;
        while (running)
        {
            Console.WriteLine("Меню:");
            Console.WriteLine("1. Створити студента");
            Console.WriteLine("2. Додати оцінку");
            Console.WriteLine("3. Переглянути список студентів");
            Console.WriteLine("4. Підрахувати середній бал");
            Console.WriteLine("5. Змінити систему оцінювання");
            Console.WriteLine("6. Вийти");
            Console.Write("Виберіть дію: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("Створення студента:");
                    Console.WriteLine("1. Бакалавр");
                    Console.WriteLine("2. Магістр");
                    var typeChoice = Console.ReadLine();

                    Console.Write("Введіть ім'я студента: ");
                    string name = Console.ReadLine();

                    Console.Write("Введіть прізвище студента: ");
                    string surname = Console.ReadLine();

                    Console.Write("Введіть курс студента: ");
                    int course = int.Parse(Console.ReadLine());

                    Student newStudent = null;
                    if (typeChoice == "1")
                    {
                        newStudent = studentFactory.CreateStudent(StudentFactory.StudentType.Bachelor, name, surname, course);
                    }
                    else if (typeChoice == "2")
                    {
                        newStudent = studentFactory.CreateStudent(StudentFactory.StudentType.Master, name, surname, course);
                    }

                    if (newStudent != null)
                    {
                        studentManager.AddStudent(newStudent);
                        Console.WriteLine($"Студент {newStudent.Name} {newStudent.Surname} ({newStudent.GetStudentType()}) створений!");
                    }
                    break;

                case "2":
                    Console.WriteLine("Вибір студента для оцінки:");
                    var students = studentManager.GetAllStudents();
                    for (int i = 0; i < students.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {students[i].Name} {students[i].Surname} - {students[i].GetStudentType()}");
                    }
                    var studentChoice = int.Parse(Console.ReadLine()) - 1;
                    Console.Write("Введіть оцінку: ");
                    var grade = int.Parse(Console.ReadLine());
                    var addGradeCommand = new AddGradeCommand(students[studentChoice], grade, studentManager);
                    commandManager.ExecuteCommand(addGradeCommand);
                    break;

                case "3":
                    var allStudents = studentManager.GetAllStudents();
                    Console.WriteLine("Список студентів:");
                    foreach (var student in allStudents)
                    {
                        Console.WriteLine($"{student.Name} {student.Surname} - {student.GetStudentType()}, Курс: {student.Course}, Середній бал: {student.CalculateAverageGrade()}");
                    }
                    break;

                case "4":
                    Console.WriteLine($"Середній бал усіх студентів: {studentManager.CalculateTotalAverageGrade()}");
                    break;

                case "5":
                    Console.WriteLine("Вибір системи оцінювання:");
                    Console.WriteLine("1. Традиційна система");
                    Console.WriteLine("2. ECTS");
                    var gradingChoice = Console.ReadLine();
                    IGradeEvaluationStrategy strategy = gradingChoice == "1" ?
                        new TraditionalGradeStrategy() : new ECTSGradeStrategy();

                    Console.WriteLine("Система оцінювання змінена!");
                    break;

                case "6":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Невірний вибір!");
                    break;
            }
        }
    }
}
