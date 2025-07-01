using EntityDesk.Core.Interfaces;
using EntityDesk.Core.Models;
using NHibernate;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace EntityDesk.Infrastructure.NHibernate
{
    public class NHUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ISession _session;
        private ITransaction? _transaction;

        public IRepository<Employee> Employees { get; }
        public IRepository<Counterparty> Counterparties { get; }
        public IRepository<Order> Orders { get; }

        public NHUnitOfWork(ISession session)
        {
            _session = session;
            _transaction = _session.BeginTransaction();
            Debug.WriteLine($"NHUnitOfWork: Transaction started. IsActive: {_transaction.IsActive}");
            Employees = new NHRepository<Employee>(_session);
            Counterparties = new NHRepository<Counterparty>(_session);
            Orders = new NHRepository<Order>(_session);
        }

        public async Task CommitAsync()
        {
            Debug.WriteLine("NHUnitOfWork: Attempting CommitAsync...");
            if (_transaction != null && _transaction.IsActive)
            {
                try
                {
                    await _transaction.CommitAsync();
                    Debug.WriteLine("NHUnitOfWork: CommitAsync successful.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"NHUnitOfWork: CommitAsync failed with exception: {ex.Message}");
                    await RollbackAsync();
                    throw;
                }
            }
            else
            {
                Debug.WriteLine("NHUnitOfWork: CommitAsync skipped, transaction is null or inactive.");
            }
        }

        public async Task RollbackAsync()
        {
            Debug.WriteLine("NHUnitOfWork: Attempting RollbackAsync...");
            if (_transaction != null && _transaction.IsActive)
            {
                await _transaction.RollbackAsync();
                Debug.WriteLine("NHUnitOfWork: RollbackAsync successful.");
            }
            else
            {
                Debug.WriteLine("NHUnitOfWork: RollbackAsync skipped, transaction is null or inactive.");
            }
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        if (_transaction.IsActive)
                        {
                            Debug.WriteLine("NHUnitOfWork: Transaction still active, rolling back in Dispose.");
                            _transaction.Rollback();
                        }
                        _transaction.Dispose();
                        _transaction = null;
                    }
                    if (_session != null)
                    {
                        _session.Dispose();
                        Debug.WriteLine("NHUnitOfWork: Session disposed.");
                    }
                }
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Debug.WriteLine("NHUnitOfWork: Dispose called.");
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
} 