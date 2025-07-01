using Microsoft.Extensions.DependencyInjection;
using EntityDesk.Core.Interfaces;
using EntityDesk.Infrastructure.NHibernate;
using NHibernate;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using EntityDesk.Infrastructure.NHibernate.Mappings;

namespace EntityDesk.Infrastructure.DI
{
    public static class ServiceRegistration
    {
        public static void ConfigureServices(IServiceCollection services, string connectionString)
        {
            // Register NHibernate SessionFactory
            services.AddSingleton<ISessionFactory>(sp =>
            {
                var sessionFactory = NHibernateHelper.CreateSessionFactory(connectionString);
                
                // Update the database schema
                var cfg = Fluently.Configure()
                    .Database(MySQLConfiguration.Standard.ConnectionString(connectionString))
                    .Mappings(m =>
                    {
                        m.FluentMappings.AddFromAssemblyOf<EmployeeMap>(); // Assuming EmployeeMap is in the same assembly as other mappings
                    })
                    .BuildConfiguration();

                new SchemaUpdate(cfg).Execute(true, true); // Update schema, show SQL, and execute
                
                return sessionFactory;
            });

            // Register NHibernate Session as Scoped
            services.AddScoped<ISession>(sp => sp.GetRequiredService<ISessionFactory>().OpenSession());

            // Register Unit of Work and Repositories
            services.AddScoped<IUnitOfWork, NHUnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(NHRepository<>));
        }
    }
} 