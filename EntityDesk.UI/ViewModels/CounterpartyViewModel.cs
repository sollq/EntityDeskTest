using EntityDesk.Core.Models;
using EntityDesk.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System;

namespace EntityDesk.UI.ViewModels
{
    public class CounterpartyViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _unitOfWork;
        public ObservableCollection<Counterparty> Counterparties { get; set; } = new();
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

        public CounterpartyViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

        private async Task AddCounterparty()
        {
            // TODO: Реализовать добавление через форму
        }
        private async Task EditCounterparty(Counterparty counterparty)
        {
            // TODO: Реализовать редактирование через форму
        }
        private async Task DeleteCounterparty(Counterparty counterparty)
        {
            if (counterparty == null) return;
            await _unitOfWork.Counterparties.DeleteAsync(counterparty);
            await _unitOfWork.CommitAsync();
            Counterparties.Remove(counterparty);
        }
    }
} 