using EntityDesk.Core.Interfaces;
using EntityDesk.Core.Models;
using NHibernate;
using System.Threading.Tasks;

namespace EntityDesk.Infrastructure.NHibernate
{
    public class NHUnitOfWork : IUnitOfWork
    {
        private readonly ISession _session;
        private readonly ITransaction _transaction;

        public IRepository<Employee> Employees { get; }
        public IRepository<Counterparty> Counterparties { get; }
        public IRepository<Order> Orders { get; }

        public NHUnitOfWork(ISession session)
        {
            _session = session;
            _transaction = _session.BeginTransaction();
            Employees = new NHRepository<Employee>(_session);
            Counterparties = new NHRepository<Counterparty>(_session);
            Orders = new NHRepository<Order>(_session);
        }

        public async Task CommitAsync() => await _transaction.CommitAsync();
        public void Dispose()
        {
            _session.Dispose();
            GC.SuppressFinalize(this);
        }
    }
} 