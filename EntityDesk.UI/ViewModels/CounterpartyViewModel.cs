using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using EntityDesk.Core.Interfaces;
using EntityDesk.Core.Models;
using EntityDesk.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;

namespace EntityDesk.UI.ViewModels;

public class CounterpartyViewModel : BaseViewModel
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceProvider _serviceProvider;
    private Counterparty _selectedCounterparty;

    public CounterpartyViewModel(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        AddCommand = new RelayCommand(async _ => await AddCounterparty());
        EditCommand = new RelayCommand(async c => await EditCounterparty((Counterparty)c));
        DeleteCommand = new RelayCommand(async c => await DeleteCounterparty((Counterparty)c));
    }

    public ObservableCollection<Counterparty> Counterparties { get; set; } = [];
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

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
                .Fetch(SelectMode.Fetch, x => x.Curator)
                .ListAsync();
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

    private async Task AddCounterparty()
    {
        var detailViewModel = _serviceProvider.GetRequiredService<CounterpartyDetailViewModel>();
        detailViewModel.Counterparty = new Counterparty { Name = string.Empty, INN = string.Empty, Curator = null };
        var detailWindow = new Window
        {
            Content = new CounterpartyDetailView(),
            DataContext = detailViewModel,
            Title = "Добавить контрагента",
            Owner = Application.Current.MainWindow,
            Width = 600,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        detailViewModel.RequestClose += () => detailWindow.Close();

        detailWindow.ShowDialog();

        var counterparty = detailViewModel.Counterparty;
        if (counterparty == null ||
            string.IsNullOrWhiteSpace(counterparty.Name) ||
            string.IsNullOrWhiteSpace(counterparty.INN) ||
            counterparty.Curator == null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "Заполните все поля контрагента (название, ИНН, куратор).",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            });
            return;
        }

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
            Content = new CounterpartyDetailView(),
            DataContext = detailViewModel,
            Title = "Редактировать контрагента",
            Owner = Application.Current.MainWindow,
            Width = 600,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        detailViewModel.RequestClose += () => detailWindow.Close();

        detailWindow.ShowDialog();
        await LoadCounterparties();
    }

    private async Task DeleteCounterparty(Counterparty counterparty)
    {
        if (counterparty == null) return;

        var result = MessageBox.Show($"Вы уверены, что хотите удалить контрагента \"{counterparty.Name}\" ?",
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

                var hasRelatedOrders = await session.QueryOver<Order>()
                                                    .Where(o => o.Counterparty.Id == counterparty.Id)
                                                    .RowCountAsync() > 0;

                if (hasRelatedOrders)
                {
                    MessageBox.Show($"Невозможно удалить контрагента \"{counterparty.Name}\", так как с ним связаны заказы. Сначала удалите связанные заказы.",
                                    "Ошибка удаления",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                await unitOfWork.Counterparties.DeleteAsync(counterparty);
                await unitOfWork.CommitAsync();
            }
            Counterparties.Remove(counterparty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при удалении контрагента: {ex.Message}",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
    }
}