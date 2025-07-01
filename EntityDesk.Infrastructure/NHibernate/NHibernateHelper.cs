using EntityDesk.Infrastructure.NHibernate.Mappings;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;

namespace EntityDesk.Infrastructure.NHibernate;

public static class NHibernateHelper
{
    public static ISessionFactory CreateSessionFactory(string connectionString)
    {
        return Fluently.Configure()
            .Database(MySQLConfiguration.Standard.ConnectionString(connectionString))
            .Mappings(m =>
            {
                m.FluentMappings.Add<EmployeeMap>();
                m.FluentMappings.Add<CounterpartyMap>();
                m.FluentMappings.Add<OrderMap>();
            })
            .BuildSessionFactory();
    }
}