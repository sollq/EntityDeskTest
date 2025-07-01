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
    public class CounterpartyDetailViewModel : BaseViewModel
    {
        private Counterparty _counterparty;
        public Counterparty Counterparty
        {
            get => _counterparty;
            set
            {
                if (_counterparty != value)
                {
                    _counterparty = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Employee> AllCurators { get; set; } = new();

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? RequestClose;

        private readonly IServiceScopeFactory _scopeFactory;

        public CounterpartyDetailViewModel(IServiceScopeFactory scopeFactory, Counterparty? counterparty = null)
        {
            _scopeFactory = scopeFactory;
            Counterparty = counterparty ?? new Counterparty { Name = string.Empty, INN = string.Empty, Curator = new Employee { FullName = string.Empty, Position = Core.Models.Position.Worker, BirthDate = DateTime.Now } }; // Инициализируем новую, если null

            SaveCommand = new RelayCommand(async _ => await Save());
            CancelCommand = new RelayCommand(_ => Cancel());

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

                    AllCurators.Clear();
                    foreach (var emp in employees)
                        AllCurators.Add(emp);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show(
                        $"Не удалось загрузить список кураторов для контрагента.\n\n{ex.Message}",
                        "Ошибка загрузки данных",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                });
            }
        }

        private async Task Save()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (Counterparty.Id == 0)
                    {
                        await unitOfWork.Counterparties.AddAsync(Counterparty);
                    }
                    else
                    {
                        await unitOfWork.Counterparties.MergeAsync(Counterparty);
                    }
                    await unitOfWork.CommitAsync();
                }
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show(
                        $"Не удалось сохранить контрагента.\n\n{ex.Message}",
                        "Ошибка сохранения",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                });
            }
        }

        private void Cancel()
        {
            RequestClose?.Invoke();
        }
    }
} 