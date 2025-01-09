
using System.Collections.Concurrent;
using System.Data;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using Palig.ICSS.data.interfaces;
using Palig.ICSS.support.Context;

namespace Palig.ICSS.data.implementation
{
    public partial class UnitOfWork : BaseUnitOfWork, IUnitOfWork
    {
        private readonly SQLDbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repositories = new ConcurrentDictionary<Type, object>();
        private bool _disposed;

        public UnitOfWork(SQLDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (_repositories.TryGetValue(typeof(TEntity), out var repository))
            {
                return (IGenericRepository<TEntity>)repository;
            }

            var newRepository = new GenericRepository<TEntity>(_context);
            _repositories[typeof(TEntity)] = newRepository;

            return newRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Rollback()
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public IEnumerable<object> TipoDocumento(object request)
        {
            try
            {
                var parm = new Parameter[] {
                    new Parameter("@Value" , request),
                };

                var result = this.ExecuteReader<object>("dbo.MPVC_SEL_TipoDocumento", CommandType.StoredProcedure, ref parm);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
