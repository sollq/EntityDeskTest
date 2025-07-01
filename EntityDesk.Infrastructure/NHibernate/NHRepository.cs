using EntityDesk.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate;

namespace EntityDesk.Infrastructure.NHibernate
{
    public class NHRepository<T>(ISession session) : IRepository<T> where T : class
    {
        protected readonly ISession _session = session;

        public async Task<T> GetByIdAsync(int id) => await _session.GetAsync<T>(id);
        public async Task<IList<T>> GetAllAsync() => await _session.QueryOver<T>().ListAsync();
        public async Task AddAsync(T entity) => await _session.SaveAsync(entity);
        public async Task UpdateAsync(T entity) => await _session.UpdateAsync(entity);
        public async Task MergeAsync(T entity) => await _session.MergeAsync(entity);
        public async Task DeleteAsync(T entity) => await _session.DeleteAsync(entity);
    }
} 