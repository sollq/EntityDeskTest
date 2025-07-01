using EntityDesk.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace EntityDesk.UI.ViewModels
{
    public class EmployeeDetailViewModel : BaseViewModel
    {
        private Employee _employee;
        public Employee Employee
        {
            get => _employee;
            set
            {
                if (_employee != value)
                {
                    _employee = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<Position> AllPositions { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? RequestClose;

        public EmployeeDetailViewModel(Employee? employee = null)
        {
            Employee = employee ?? new Employee { FullName = string.Empty, Position = Core.Models.Position.Worker, BirthDate = DateTime.Now }; // Инициализируем новую, если null
            AllPositions = Enum.GetValues(typeof(Position)).Cast<Position>().ToList();

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void Save()
        {
            // Здесь будет логика сохранения.
            // После сохранения нужно будет как-то сообщить родительской ViewModel (EmployeeViewModel),
            // что данные изменены и обновить список.
            RequestClose?.Invoke();
        }

        private void Cancel()
        {
            RequestClose?.Invoke();
        }
    }
} 