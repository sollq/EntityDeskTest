using EntityDesk.Core.Models;
using EntityDesk.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System;

namespace EntityDesk.UI.ViewModels
{
    public class OrderViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _unitOfWork;
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

        public OrderViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                var orders = await _unitOfWork.Orders.GetAllAsync();
                Orders.Clear();
                foreach (var o in orders)
                    Orders.Add(o);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки заказов: {ex.Message}");
            }
        }
        private async Task LoadEmployees()
        {
            try
            {
                var employees = await _unitOfWork.Employees.GetAllAsync();
                Employees.Clear();
                foreach (var e in employees)
                    Employees.Add(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }
        private async Task LoadCounterparties()
        {
            try
            {
                var counterparties = await _unitOfWork.Counterparties.GetAllAsync();
                Counterparties.Clear();
                foreach (var c in counterparties)
                    Counterparties.Add(c);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки контрагентов: {ex.Message}");
            }
        }
        private async Task AddOrder()
        {
            // TODO: Реализовать добавление через форму
        }
        private async Task EditOrder(Order order)
        {
            // TODO: Реализовать редактирование через форму
        }
        private async Task DeleteOrder(Order order)
        {
            if (order == null) return;
            await _unitOfWork.Orders.DeleteAsync(order);
            await _unitOfWork.CommitAsync();
            Orders.Remove(order);
        }
    }
} 