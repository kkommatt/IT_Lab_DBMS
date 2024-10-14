namespace DBMS_MVC;

public class DbHost: IHostedService
{
    private readonly DbProcessor _dbProcessor;

    public DbHost(DbProcessor dbProcessor)
    {
        _dbProcessor = dbProcessor;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _dbProcessor.Dispose();
        return Task.CompletedTask;
    }
}