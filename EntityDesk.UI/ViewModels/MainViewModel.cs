namespace EntityDesk.UI.ViewModels
{
    public class MainViewModel(EmployeeViewModel employeeVM, CounterpartyViewModel counterpartyVM, OrderViewModel orderVM) : BaseViewModel
    {
        public EmployeeViewModel EmployeeVM { get; } = employeeVM;
        public CounterpartyViewModel CounterpartyVM { get; } = counterpartyVM;
        public OrderViewModel OrderVM { get; } = orderVM;

        public async Task InitAsync()
        {
            await EmployeeVM.InitAsync();
            await CounterpartyVM.InitAsync();
            await OrderVM.InitAsync();
        }
    }
} 