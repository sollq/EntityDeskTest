using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using EntityDesk.Core.Interfaces;
using EntityDesk.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDesk.UI.ViewModels;

public class OrderDetailViewModel : BaseViewModel
{
    private readonly IServiceScopeFactory _scopeFactory;
    private Order _order;

    public OrderDetailViewModel(IServiceScopeFactory scopeFactory, Order? order = null)
    {
        _scopeFactory = scopeFactory;
        Order = order ?? new Order
        {
            Date = DateTime.Now,
            Employee = new Employee { FullName = string.Empty, Position = Position.Worker, BirthDate = DateTime.Now },
            Counterparty = new Counterparty
            {
                Name = string.Empty, INN = string.Empty,
                Curator = new Employee { FullName = string.Empty, Position = Position.Worker, BirthDate = DateTime.Now }
            }
        };

        SaveCommand = new RelayCommand(async _ => await Save());
        CancelCommand = new RelayCommand(_ => Cancel());

        Task.Run(async () => await LoadLookupData());
    }

    public Order Order
    {
        get => _order;
        set
        {
            if (_order != value)
            {
                _order = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<Employee> AllEmployees { get; set; } = [];
    public ObservableCollection<Counterparty> AllCounterparties { get; set; } = [];

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? RequestClose;

    private async Task LoadLookupData()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var employees = await unitOfWork.Employees.GetAllAsync();
            var counterparties = await unitOfWork.Counterparties.GetAllAsync();

            AllEmployees.Clear();
            foreach (var emp in employees)
                AllEmployees.Add(emp);

            AllCounterparties.Clear();
            foreach (var cp in counterparties)
                AllCounterparties.Add(cp);
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Не удалось загрузить справочные данные для заказа.\n\n{ex.Message}",
                    "Ошибка загрузки данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            });
        }
    }

    private async Task Save()
    {
        if (Order == null ||
            Order.Employee == null ||
            Order.Counterparty == null ||
            Order.Amount <= 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "Заполните все поля заказа (сотрудник, контрагент, сумма > 0).",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            });
            return;
        }

        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                if (Order.Id == 0)
                    await unitOfWork.Orders.AddAsync(Order);
                else
                    await unitOfWork.Orders.MergeAsync(Order);
                await unitOfWork.CommitAsync();
            }

            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Не удалось сохранить заказ.\n\n{ex.Message}",
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            });
        }
    }

    private void Cancel()
    {
        RequestClose?.Invoke();
    }
}