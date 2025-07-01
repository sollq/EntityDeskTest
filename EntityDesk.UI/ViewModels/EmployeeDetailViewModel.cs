using System.Windows;
using System.Windows.Input;
using EntityDesk.Core.Interfaces;
using EntityDesk.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDesk.UI.ViewModels;

public class EmployeeDetailViewModel : BaseViewModel
{
    private readonly IServiceScopeFactory _scopeFactory;
    private Employee _employee;

    public EmployeeDetailViewModel(IServiceScopeFactory scopeFactory, Employee? employee = null)
    {
        _scopeFactory = scopeFactory;
        Employee = employee ?? new Employee
            { FullName = string.Empty, Position = Position.Worker, BirthDate = DateTime.Now };
        AllPositions = Enum.GetValues(typeof(Position)).Cast<Position>().ToList();

        SaveCommand = new RelayCommand(async _ => await Save());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public Employee Employee
    {
        get => _employee;
        set
        {
            if (_employee != value)
            {
                _employee = value;
                OnPropertyChanged();
            }
        }
    }

    public List<Position> AllPositions { get; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? RequestClose;

    private async Task Save()
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                if (Employee.Id == 0)
                    await unitOfWork.Employees.AddAsync(Employee);
                else
                    await unitOfWork.Employees.MergeAsync(Employee);
                await unitOfWork.CommitAsync();
            }

            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Не удалось сохранить сотрудника.\n\n{ex.Message}",
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