using EntityDesk.Core.Models;
using EntityDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace EntityDesk.UI.ViewModels
{
    public class OrderDetailViewModel : BaseViewModel
    {
        private Order _order;
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

        public ObservableCollection<Employee> AllEmployees { get; set; } = new();
        public ObservableCollection<Counterparty> AllCounterparties { get; set; } = new();

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? RequestClose;

        private readonly IServiceScopeFactory _scopeFactory;

        public OrderDetailViewModel(IServiceScopeFactory scopeFactory, Order? order = null)
        {
            _scopeFactory = scopeFactory;
            Order = order ?? new Order { Date = DateTime.Now, Employee = new Employee { FullName = string.Empty, Position = Core.Models.Position.Worker, BirthDate = DateTime.Now }, Counterparty = new Counterparty { Name = string.Empty, INN = string.Empty, Curator = new Employee { FullName = string.Empty, Position = Core.Models.Position.Worker, BirthDate = DateTime.Now } } }; // Исправляем INN

            SaveCommand = new RelayCommand(async _ => await Save());
            CancelCommand = new RelayCommand(_ => Cancel());

            // Загружаем данные для ComboBox'ов при создании ViewModel
            Task.Run(async () => await LoadLookupData());
        }

        private async Task LoadLookupData()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки справочных данных для заказа: {ex.Message}");
            }
        }

        private async Task Save()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (Order.Id == 0)
                    {
                        await unitOfWork.Orders.AddAsync(Order);
                    }
                    else
                    {
                        await unitOfWork.Orders.MergeAsync(Order);
                    }
                    await unitOfWork.CommitAsync();
                }
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения заказа: {ex.Message}");
                // TODO: Добавить нормальное уведомление пользователя об ошибке
            }
        }

        private void Cancel()
        {
            RequestClose?.Invoke();
        }
    }
} 