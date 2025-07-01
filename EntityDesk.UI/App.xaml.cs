using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using EntityDesk.Infrastructure.DI;
using EntityDesk.UI.ViewModels;
using Microsoft.Extensions.Configuration;

namespace EntityDesk.UI
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        private IServiceScope? _mainScope;

        protected override void OnStartup(StartupEventArgs e)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("Default");

            var services = new ServiceCollection();
            if (connectionString != null) 
                services.AddInfrastructure(connectionString);
            services.AddScoped<EmployeeViewModel>();
            services.AddScoped<CounterpartyViewModel>();
            services.AddScoped<OrderViewModel>();
            services.AddScoped<EmployeeDetailViewModel>();
            services.AddScoped<OrderDetailViewModel>();
            services.AddScoped<CounterpartyDetailViewModel>();
            services.AddScoped<MainViewModel>();
            services.AddScoped<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            _mainScope = _serviceProvider.CreateScope();
            var mainVM = _mainScope.ServiceProvider.GetRequiredService<MainViewModel>();
            var mainWindow = _mainScope.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = mainVM;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mainScope?.Dispose();
            base.OnExit(e);
        }
    }
}
