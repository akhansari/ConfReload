namespace ConfReload;

using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

public class DependencyScope : IDependencyScope
{
    private readonly IServiceScope _scope;

    public DependencyScope(IServiceProvider provider) =>
        _scope = provider.CreateScope();

    public void Dispose() =>
        _scope.Dispose();

    public object GetService(Type serviceType) =>
        _scope.ServiceProvider.GetService(serviceType);

    public IEnumerable<object?> GetServices(Type serviceType) =>
        _scope.ServiceProvider.GetServices(serviceType);
}

public class DefaultDependencyResolver : IDependencyResolver
{
    private readonly IServiceProvider _provider;

    public DefaultDependencyResolver(IServiceProvider provider) =>
        _provider = provider;

    public IDependencyScope BeginScope() =>
        new DependencyScope(_provider);

    public void Dispose() { }

    public object GetService(Type serviceType) =>
        _provider.GetService(serviceType);

    public IEnumerable<object?> GetServices(Type serviceType) =>
        _provider.GetServices(serviceType);
}
