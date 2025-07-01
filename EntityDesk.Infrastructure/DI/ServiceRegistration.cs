using System.Diagnostics;
using EntityDesk.Core.Interfaces;
using EntityDesk.Infrastructure.NHibernate;
using EntityDesk.Infrastructure.NHibernate.Mappings;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace EntityDesk.Infrastructure.DI;

public static class ServiceRegistration
{
    public static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        try
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
            var databaseName = connectionStringBuilder.Database;
            connectionStringBuilder.Database = "";

            using (var connection = new MySqlConnection(connectionStringBuilder.ToString()))
            {
                connection.Open();
                using (var command = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{databaseName}`", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Debug.WriteLine($"База данных '{databaseName}' проверена/создана.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при проверке/создании базы данных: {ex.Message}");
        }

        services.AddSingleton<ISessionFactory>(sp =>
        {
            var sessionFactory = NHibernateHelper.CreateSessionFactory(connectionString);

            var cfg = Fluently.Configure()
                .Database(MySQLConfiguration.Standard.ConnectionString(connectionString))
                .Mappings(m => { m.FluentMappings.AddFromAssemblyOf<EmployeeMap>(); })
                .BuildConfiguration();

            new SchemaUpdate(cfg).Execute(true, true);

            return sessionFactory;
        });
        services.AddScoped<ISession>(sp => sp.GetRequiredService<ISessionFactory>().OpenSession());
        services.AddScoped<IUnitOfWork, NHUnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(NHRepository<>));
    }
}