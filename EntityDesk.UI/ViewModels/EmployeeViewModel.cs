using EntityDesk.Core.Models;
using EntityDesk.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDesk.UI.ViewModels
{
    public class EmployeeViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;

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

        public EmployeeViewModel(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
        {
            _serviceProvider = serviceProvider;
            _scopeFactory = scopeFactory;
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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var employees = await unitOfWork.Employees.GetAllAsync();
                    Employees.Clear();
                    foreach (var emp in employees)
                        Employees.Add(emp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }

        private async Task AddEmployee()
        {
            var detailViewModel = _serviceProvider.GetRequiredService<EmployeeDetailViewModel>();
            var detailWindow = new Window
            {
                Content = new Views.EmployeeDetailView(),
                DataContext = detailViewModel,
                Title = "Добавить сотрудника",
                Owner = Application.Current.MainWindow,
                Width = 300,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            detailViewModel.RequestClose += () => detailWindow.Close();

            detailWindow.ShowDialog();
            await LoadEmployees();
        }

        private async Task EditEmployee(Employee employee)
        {
            if (employee == null) return;

            var detailViewModel = _serviceProvider.GetRequiredService<EmployeeDetailViewModel>();
            var employeeToEdit = new Employee
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Position = employee.Position,
                BirthDate = employee.BirthDate
            };
            detailViewModel.Employee = employeeToEdit;

            var detailWindow = new Window
            {
                Content = new Views.EmployeeDetailView(),
                DataContext = detailViewModel,
                Title = "Редактировать сотрудника",
                Owner = Application.Current.MainWindow,
                Width = 400,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            detailViewModel.RequestClose += () => detailWindow.Close();

            detailWindow.ShowDialog();
            await LoadEmployees();
        }

        private async Task DeleteEmployee(Employee employee)
        {
            if (employee == null) return;

            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.Employees.DeleteAsync(employee);
                await unitOfWork.CommitAsync();
            }
            Employees.Remove(employee);
        }
    }
} 