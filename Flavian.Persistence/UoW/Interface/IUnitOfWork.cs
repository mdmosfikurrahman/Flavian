using Flavian.Persistence.Repositories.Interfaces.Demos;

namespace Flavian.Persistence.UoW.Interface;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();

    IDemoRepository Demos { get; }
    IDemoAuditRepository DemoAudits { get; }
}
