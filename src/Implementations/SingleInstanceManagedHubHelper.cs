using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;

public class SingleInstanceManagedHubHelper : IManagedHubHelper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger<SingleInstanceManagedHubHelper> _logger;
    private readonly ManagedSignalRConfiguration _configuration;

    public SingleInstanceManagedHubHelper
    (
        IServiceProvider serviceProvider,
        ICacheProvider cacheProvider,
        ILogger<SingleInstanceManagedHubHelper> logger,
        ManagedSignalRConfiguration configuration
    )
    {
        _serviceProvider = serviceProvider;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _configuration = configuration;
    }



    public Task SendToUser<THub>(string userId) where THub : ManagedHub
    {
        var keys = _cacheProvider.Scan()
    }


}
