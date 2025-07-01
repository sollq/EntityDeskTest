using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using EntityDesk.Core.Interfaces;
using EntityDesk.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDesk.UI.ViewModels;

public class CounterpartyDetailViewModel : BaseViewModel
{
    private readonly IServiceScopeFactory _scopeFactory;
    private Counterparty _counterparty;

    public CounterpartyDetailViewModel(IServiceScopeFactory scopeFactory, Counterparty? counterparty = null)
    {
        _scopeFactory = scopeFactory;
        Counterparty = counterparty ?? new Counterparty
        {
            Name = string.Empty, INN = string.Empty,
            Curator = new Employee { FullName = string.Empty, Position = Position.Worker, BirthDate = DateTime.Now }
        };

        SaveCommand = new RelayCommand(async _ => await Save());
        CancelCommand = new RelayCommand(_ => Cancel());

        Task.Run(LoadLookupData);
    }

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

    public ObservableCollection<Employee> AllCurators { get; set; } = [];

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

            AllCurators.Clear();
            foreach (var emp in employees)
                AllCurators.Add(emp);
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Не удалось загрузить список кураторов для контрагента.\n\n{ex.Message}",
                    "Ошибка загрузки данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
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
                    await unitOfWork.Counterparties.AddAsync(Counterparty);
                else
                    await unitOfWork.Counterparties.MergeAsync(Counterparty);
                await unitOfWork.CommitAsync();
            }

            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Не удалось сохранить контрагента.\n\n{ex.Message}",
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