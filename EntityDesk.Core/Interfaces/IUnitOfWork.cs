using EntityDesk.Core.Models;

namespace EntityDesk.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Employee> Employees { get; }
        IRepository<Counterparty> Counterparties { get; }
        IRepository<Order> Orders { get; }
        Task CommitAsync();
        new void Dispose();
    }
} 