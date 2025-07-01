using EntityDesk.Core.Models;
using EntityDesk.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Windows;

namespace EntityDesk.UI.ViewModels
{
    public class EmployeeViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _unitOfWork;
        public ObservableCollection<Employee> Employees { get; set; } = new();
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        private Employee _selectedEmployee;
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (_selectedEmployee != value)
                {
                    _selectedEmployee = value;
                    OnPropertyChanged();
                }
            }
        }

        public EmployeeViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            AddCommand = new RelayCommand(async _ => await AddEmployee());
            EditCommand = new RelayCommand(async emp => await EditEmployee((Employee)emp));
            DeleteCommand = new RelayCommand(async emp => await DeleteEmployee((Employee)emp));
        }

        public async Task InitAsync()
        {
            await LoadEmployees();
        }

        private async Task LoadEmployees()
        {
            try
            {
                var employees = await _unitOfWork.Employees.GetAllAsync();
                Employees.Clear();
                foreach (var emp in employees)
                    Employees.Add(emp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }

        private async Task AddEmployee()
        {
            var vm = new EmployeeEditViewModel();
            var window = new EmployeeEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                var newEmployee = vm.ToEmployee();
                await _unitOfWork.Employees.AddAsync(newEmployee);
                await _unitOfWork.CommitAsync();
                Employees.Add(newEmployee);
            }
        }

        private async Task EditEmployee(Employee employee)
        {
            if (employee == null) return;
            var vm = new EmployeeEditViewModel(employee);
            var window = new EmployeeEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                var updated = vm.ToEmployee(employee);
                await _unitOfWork.Employees.UpdateAsync(updated);
                await _unitOfWork.CommitAsync();
                await LoadEmployees();
            }
        }

        private async Task DeleteEmployee(Employee employee)
        {
            if (employee == null) return;
            await _unitOfWork.Employees.DeleteAsync(employee);
            await _unitOfWork.CommitAsync();
            Employees.Remove(employee);
        }
    }
} 