using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using EntityDesk.Core.Interfaces;
using EntityDesk.Core.Models;
using EntityDesk.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;

namespace EntityDesk.UI.ViewModels;

public class EmployeeViewModel : BaseViewModel
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceProvider _serviceProvider;
    private Employee _selectedEmployee;

    public EmployeeViewModel(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        AddCommand = new RelayCommand(async _ => await AddEmployee());
        EditCommand = new RelayCommand(async emp => await EditEmployee((Employee)emp));
        DeleteCommand = new RelayCommand(async emp => await DeleteEmployee((Employee)emp));
    }

    public ObservableCollection<Employee> Employees { get; set; } = [];
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

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

    public async Task InitAsync()
    {
        await LoadEmployees();
    }

    private async Task LoadEmployees()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var employees = await unitOfWork.Employees.GetAllAsync();
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
        var detailViewModel = _serviceProvider.GetRequiredService<EmployeeDetailViewModel>();
        detailViewModel.Employee = new Employee
            { FullName = string.Empty, Position = Position.Worker, BirthDate = DateTime.Now };
        var detailWindow = new Window
        {
            Content = new EmployeeDetailView(),
            DataContext = detailViewModel,
            Title = "Добавить сотрудника",
            Owner = Application.Current.MainWindow,
            Width = 600,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        detailViewModel.RequestClose += () => detailWindow.Close();

        detailWindow.ShowDialog();

        var employee = detailViewModel.Employee;
        if (employee == null ||
            string.IsNullOrWhiteSpace(employee.FullName) ||
            employee.BirthDate == default)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "Заполните все поля сотрудника (ФИО, дата рождения).",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            });
            return;
        }

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
            Content = new EmployeeDetailView(),
            DataContext = detailViewModel,
            Title = "Редактировать сотрудника",
            Owner = Application.Current.MainWindow,
            Width = 600,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        detailViewModel.RequestClose += () => detailWindow.Close();

        detailWindow.ShowDialog();
        await LoadEmployees();
    }

    private async Task DeleteEmployee(Employee employee)
    {
        if (employee == null) return;

        var result = MessageBox.Show($"Вы уверены, что хотите удалить сотрудника \"{employee.FullName}\" ?",
                                     "Подтверждение удаления",
                                     MessageBoxButton.YesNo,
                                     MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                var hasRelatedCounterparties = await session.QueryOver<Counterparty>()
                                                            .Where(c => c.Curator.Id == employee.Id)
                                                            .RowCountAsync() > 0;

                var hasRelatedOrders = await session.QueryOver<Order>()
                                                    .Where(o => o.Employee.Id == employee.Id)
                                                    .RowCountAsync() > 0;

                if (hasRelatedCounterparties || hasRelatedOrders)
                {
                    MessageBox.Show($"Невозможно удалить сотрудника \"{employee.FullName}\", так как с ним связаны контрагенты или заказы. Сначала удалите связанные записи.",
                                    "Ошибка удаления",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                await unitOfWork.Employees.DeleteAsync(employee);
                await unitOfWork.CommitAsync();
            }
            Employees.Remove(employee);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при удалении сотрудника: {ex.Message}",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
    }
}