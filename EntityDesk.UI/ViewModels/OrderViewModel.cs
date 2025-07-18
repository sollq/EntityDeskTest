using EntityDesk.Core.Models;
using EntityDesk.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;

namespace EntityDesk.UI.ViewModels
{
    public class OrderViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;

        public ObservableCollection<Order> Orders { get; set; } = [];
        public ObservableCollection<Employee> Employees { get; set; } = new();
        public ObservableCollection<Counterparty> Counterparties { get; set; } = new();
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        private Order _selectedOrder;
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

        public OrderViewModel(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
        {
            _serviceProvider = serviceProvider;
            _scopeFactory = scopeFactory;
            AddCommand = new RelayCommand(async _ => await AddOrder());
            EditCommand = new RelayCommand(async o => await EditOrder((Order)o));
            DeleteCommand = new RelayCommand(async o => await DeleteOrder((Order)o));
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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<ISession>();
                    
                    var orders = await session.QueryOver<Order>()
                                              .Fetch(NHibernate.SelectMode.Fetch, x => x.Employee)
                                              .Fetch(NHibernate.SelectMode.Fetch, x => x.Counterparty)
                                              .ListAsync();

                    Orders.Clear();
                    foreach (var o in orders)
                        Orders.Add(o);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show(
                        $"Не удалось загрузить заказы.\n\n{ex.Message}",
                        "Ошибка загрузки заказов",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                });
            }
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
                    foreach (var e in employees)
                        Employees.Add(e);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show(
                        $"Не удалось загрузить сотрудников.\n\n{ex.Message}",
                        "Ошибка загрузки сотрудников",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                });
            }
        }
        private async Task LoadCounterparties()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var counterparties = await unitOfWork.Counterparties.GetAllAsync();

                if (counterparties == null || !counterparties.Any())
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show(
                            "Контрагенты не найдены или список пустой. Проверь, что данные вообще есть.",
                            "Внимание",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning
                        );
                    });
                    return;
                }

                Counterparties.Clear();
                foreach (var c in counterparties)
                    Counterparties.Add(c);
            }
        }
        private async Task AddOrder()
        {
            var detailViewModel = _serviceProvider.GetRequiredService<OrderDetailViewModel>();
            detailViewModel.Order = new Order { Date = DateTime.Now, Amount = 0, Employee = null, Counterparty = null };
            var detailWindow = new Window
            {
                Content = new Views.OrderDetailView(),
                DataContext = detailViewModel,
                Title = "Добавить заказ",
                Owner = Application.Current.MainWindow,
                Width = 400,
                Height = 550,
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
                Content = new Views.OrderDetailView(),
                DataContext = detailViewModel,
                Title = "Редактировать заказ",
                Owner = Application.Current.MainWindow,
                Width = 400,
                Height = 550,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            detailViewModel.RequestClose += () => detailWindow.Close();

            detailWindow.ShowDialog();
            await LoadAll();
        }
        private async Task DeleteOrder(Order order)
        {
            if (order == null) return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ №{order.Id} от {order.Date:d}?",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    await unitOfWork.Orders.DeleteAsync(order);
                    await unitOfWork.CommitAsync();
                }
                Orders.Remove(order);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
} 