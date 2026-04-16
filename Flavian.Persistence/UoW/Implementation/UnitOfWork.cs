using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Flavian.Persistence.Data;
using Flavian.Persistence.Repositories.Interfaces.Demos;
using Flavian.Persistence.UoW.Interface;

namespace Flavian.Persistence.UoW.Implementation;

public class UnitOfWork(FlavianDbContext dbContext, IServiceProvider serviceProvider)
    : IUnitOfWork
{
    private readonly FlavianDbContext _dbContext = dbContext;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public async Task<int> SaveChangesAsync() => await _dbContext.SaveChangesAsync();

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null) return;
        _transaction = await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null) throw new InvalidOperationException("No transaction started.");
        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null) throw new InvalidOperationException("No transaction started.");
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _transaction?.Dispose();
            _dbContext.Dispose();
        }

        _disposed = true;
    }

    private T GetRepository<T>() where T : class => _serviceProvider.GetRequiredService<T>();

    public IDemoRepository Demos => GetRepository<IDemoRepository>();
    public IDemoAuditRepository DemoAudits => GetRepository<IDemoAuditRepository>();
}
