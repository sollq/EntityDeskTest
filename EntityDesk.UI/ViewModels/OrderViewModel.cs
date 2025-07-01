using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using EntityDesk.Core.Interfaces;
using EntityDesk.Core.Models;
using EntityDesk.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;

namespace EntityDesk.UI.ViewModels;

public class OrderViewModel : BaseViewModel
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceProvider _serviceProvider;
    private Order _selectedOrder;

    public OrderViewModel(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        AddCommand = new RelayCommand(async _ => await AddOrder());
        EditCommand = new RelayCommand(async o => await EditOrder((Order)o));
        DeleteCommand = new RelayCommand(async o => await DeleteOrder((Order)o));
    }

    public ObservableCollection<Order> Orders { get; set; } = [];
    public ObservableCollection<Employee> Employees { get; set; } = [];
    public ObservableCollection<Counterparty> Counterparties { get; set; } = [];
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public Order SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            if (_selectedOrder != value)
            {
                _selectedOrder = value;
                OnPropertyChanged();
            }
        }
    }

    public async Task InitAsync()
    {
        await LoadAll();
    }

    private async Task LoadAll()
    {
        await LoadEmployees();
        await LoadCounterparties();
        await LoadOrders();
    }

    private async Task LoadOrders()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var session = scope.ServiceProvider.GetRequiredService<ISession>();

            var orders = await session.QueryOver<Order>()
                .Fetch(SelectMode.Fetch, x => x.Employee)
                .Fetch(SelectMode.Fetch, x => x.Counterparty)
                .ListAsync();

            Orders.Clear();
            foreach (var o in orders)
                Orders.Add(o);
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Не удалось загрузить заказы.\n\n{ex.Message}",
                    "Ошибка загрузки заказов",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            });
        }
    }

    private async Task LoadEmployees()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var employees = await unitOfWork.Employees.GetAllAsync();
            Employees.Clear();
            foreach (var e in employees)
                Employees.Add(e);
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Не удалось загрузить сотрудников.\n\n{ex.Message}",
                    "Ошибка загрузки сотрудников",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            });
        }
    }

    private async Task LoadCounterparties()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var counterparties = await unitOfWork.Counterparties.GetAllAsync();
            Counterparties.Clear();
            foreach (var c in counterparties)
                Counterparties.Add(c);
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Не удалось загрузить контрагентов.\n\n{ex.Message}",
                    "Ошибка загрузки контрагентов",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            });
        }
    }

    private async Task AddOrder()
    {
        var detailViewModel = _serviceProvider.GetRequiredService<OrderDetailViewModel>();
        detailViewModel.Order = new Order { Date = DateTime.Now, Amount = 0, Employee = null, Counterparty = null };
        var detailWindow = new Window
        {
            Content = new OrderDetailView(),
            DataContext = detailViewModel,
            Title = "Добавить заказ",
            Owner = Application.Current.MainWindow,
            Width = 600,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        detailViewModel.RequestClose += () => detailWindow.Close();

        detailWindow.ShowDialog();
        await LoadAll();
    }

    private async Task EditOrder(Order order)
    {
        if (order == null) return;

        var detailViewModel = _serviceProvider.GetRequiredService<OrderDetailViewModel>();
        var orderToEdit = new Order
        {
            Id = order.Id,
            Date = order.Date,
            Amount = order.Amount,
            Employee = order.Employee,
            Counterparty = order.Counterparty
        };
        detailViewModel.Order = orderToEdit;

        var detailWindow = new Window
        {
            Content = new OrderDetailView(),
            DataContext = detailViewModel,
            Title = "Редактировать заказ",
            Owner = Application.Current.MainWindow,
            Width = 600,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        detailViewModel.RequestClose += () => detailWindow.Close();

        detailWindow.ShowDialog();
        await LoadAll();
    }

    private async Task DeleteOrder(Order order)
    {
        if (order == null) return;

        var result = MessageBox.Show(
                "Вы уверены? Это действие нельзя будет отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        using (var scope = _scopeFactory.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.Orders.DeleteAsync(order);
            await unitOfWork.CommitAsync();
        }

        Orders.Remove(order);
    }
}