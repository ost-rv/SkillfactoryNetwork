using SkillfactoryNetwork.DAL.Repository;
using System;

namespace SkillfactoryNetwork.DAL.Models.UoW
{
    public interface IUnitOfWork : IDisposable
    {
        int SaveChanges(bool ensureAutoHistory = false);

        IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = true) where TEntity : class;
    }
}
