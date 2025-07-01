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
    public class CounterpartyViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;

        public ObservableCollection<Counterparty> Counterparties { get; set; } = [];
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        private Counterparty _selectedCounterparty;
        public Counterparty SelectedCounterparty
        {
            get => _selectedCounterparty;
            set
            {
                if (_selectedCounterparty != value)
                {
                    _selectedCounterparty = value;
                    OnPropertyChanged();
                }
            }
        }

        public CounterpartyViewModel(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
        {
            _serviceProvider = serviceProvider;
            _scopeFactory = scopeFactory;
            AddCommand = new RelayCommand(async _ => await AddCounterparty());
            EditCommand = new RelayCommand(async c => await EditCounterparty((Counterparty)c));
            DeleteCommand = new RelayCommand(async c => await DeleteCounterparty((Counterparty)c));
        }

        public async Task InitAsync()
        {
            await LoadCounterparties();
        }

        private async Task LoadCounterparties()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var counterparties = await session.QueryOver<Counterparty>()
                                                  .Fetch(NHibernate.SelectMode.Fetch, x => x.Curator)
                                                  .ListAsync();
                Counterparties.Clear();
                foreach (var c in counterparties)
                    Counterparties.Add(c);
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show(
                        $"Не удалось загрузить контрагентов.\n\n{ex.Message}",
                        "Ошибка загрузки контрагентов",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                });
            }
        }

        private async Task AddCounterparty()
        {
            var detailViewModel = _serviceProvider.GetRequiredService<CounterpartyDetailViewModel>();
            var detailWindow = new Window
            {
                Content = new Views.CounterpartyDetailView(),
                DataContext = detailViewModel,
                Title = "Добавить контрагента",
                Owner = Application.Current.MainWindow,
                Width = 400,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            detailViewModel.RequestClose += () => detailWindow.Close();

            detailWindow.ShowDialog();
            await LoadCounterparties();
        }
        private async Task EditCounterparty(Counterparty counterparty)
        {
            if (counterparty == null) return;

            var detailViewModel = _serviceProvider.GetRequiredService<CounterpartyDetailViewModel>();
            var counterpartyToEdit = new Counterparty
            {
                Id = counterparty.Id,
                Name = counterparty.Name,
                INN = counterparty.INN,
                Curator = counterparty.Curator
            };
            detailViewModel.Counterparty = counterpartyToEdit;

            var detailWindow = new Window
            {
                Content = new Views.CounterpartyDetailView(),
                DataContext = detailViewModel,
                Title = "Редактировать контрагента",
                Owner = Application.Current.MainWindow,
                Width = 400,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            detailViewModel.RequestClose += () => detailWindow.Close();

            detailWindow.ShowDialog();
            await LoadCounterparties();
        }
        private async Task DeleteCounterparty(Counterparty counterparty)
        {
            if (counterparty == null) return;
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.Counterparties.DeleteAsync(counterparty);
                await unitOfWork.CommitAsync();
            }
            Counterparties.Remove(counterparty);
        }
    }
} 