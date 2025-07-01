using EntityDesk.Core.Models;
using EntityDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDesk.UI.ViewModels
{
    public class EmployeeDetailViewModel : BaseViewModel
    {
        private Employee _employee;
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

        private readonly IServiceScopeFactory _scopeFactory;

        public EmployeeDetailViewModel(IServiceScopeFactory scopeFactory, Employee? employee = null)
        {
            _scopeFactory = scopeFactory;
            Employee = employee ?? new Employee { FullName = string.Empty, Position = Core.Models.Position.Worker, BirthDate = DateTime.Now };
            AllPositions = Enum.GetValues(typeof(Position)).Cast<Position>().ToList();

            SaveCommand = new RelayCommand(async _ => await Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private async Task Save()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (Employee.Id == 0)
                    {
                        await unitOfWork.Employees.AddAsync(Employee);
                    }
                    else
                    {
                        await unitOfWork.Employees.MergeAsync(Employee);
                    }
                    await unitOfWork.CommitAsync();
                }
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения сотрудника: {ex.Message}");
            }
        }

        private void Cancel()
        {
            RequestClose?.Invoke();
        }
    }
} 