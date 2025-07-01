using Microsoft.Extensions.DependencyInjection;
using EntityDesk.Core.Interfaces;
using EntityDesk.Infrastructure.NHibernate;
using NHibernate;

namespace EntityDesk.Infrastructure.DI
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton(provider => NHibernateHelper.CreateSessionFactory(connectionString));
            services.AddScoped(provider => provider.GetRequiredService<ISessionFactory>().OpenSession());
            services.AddScoped<IUnitOfWork, NHUnitOfWork>();
            // Здесь можно добавить регистрацию Serilog
        }
    }
} 