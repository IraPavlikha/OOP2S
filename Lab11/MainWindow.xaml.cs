using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace TaskManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TaskManagerViewModel();
        }
    }

    public class Task : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        private DateTime _dueDate;
        private string _priority;
        private bool _isCompleted;

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(); }
        }

        public string Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(); }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class TaskManagerViewModel : INotifyPropertyChanged
    {
        private Task _selectedTask;
        private string _searchText;
        private string _statusMessage;

        public ObservableCollection<Task> Tasks { get; } = new ObservableCollection<Task>();
        public ObservableCollection<string> Priorities { get; } = new ObservableCollection<string> { "High", "Medium", "Low" };

        public Task SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterTasks();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string TaskCount => $"{Tasks.Count} tasks";

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand RefreshCommand { get; }

        public TaskManagerViewModel()
        {
            AddTaskCommand = new RelayCommand(AddTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask, () => SelectedTask != null);
            EditTaskCommand = new RelayCommand(EditTask, () => SelectedTask != null);
            SaveTaskCommand = new RelayCommand(SaveTask, () => SelectedTask != null);
            RefreshCommand = new RelayCommand(RefreshTasks);

            // Initialize with sample data
            Tasks.Add(new Task { Title = "Complete project", Description = "Finish the WPF Task Manager", DueDate = DateTime.Now.AddDays(3), Priority = "High" });
            Tasks.Add(new Task { Title = "Buy groceries", Description = "Milk, eggs, bread", DueDate = DateTime.Now.AddDays(1), Priority = "Medium" });
            Tasks.Add(new Task { Title = "Call mom", Description = "Discuss weekend plans", DueDate = DateTime.Now.AddDays(5), Priority = "Low" });

            SelectedTask = Tasks.FirstOrDefault();
            StatusMessage = "Ready";
        }

        private void AddTask()
        {
            var newTask = new Task
            {
                Title = "New Task",
                Description = "Description",
                DueDate = DateTime.Now.AddDays(7),
                Priority = "Medium"
            };

            Tasks.Add(newTask);
            SelectedTask = newTask;
            StatusMessage = "New task added";
        }

        private void DeleteTask()
        {
            if (SelectedTask != null)
            {
                Tasks.Remove(SelectedTask);
                SelectedTask = Tasks.FirstOrDefault();
                StatusMessage = "Task deleted";
            }
        }

        private void EditTask()
        {
            StatusMessage = "Editing task";
        }

        private void SaveTask()
        {
            StatusMessage = "Changes saved";
        }

        private void RefreshTasks()
        {
            OnPropertyChanged(nameof(Tasks));
            StatusMessage = "Tasks refreshed";
        }

        private void FilterTasks()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                OnPropertyChanged(nameof(Tasks));
                return;
            }

            var filtered = Tasks.Where(t => t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                     t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

            Tasks.Clear();
            foreach (var task in filtered)
            {
                Tasks.Add(task);
            }

            StatusMessage = $"Filtered {filtered.Count} tasks";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}