namespace OtherMediator.Extensions.Microsoft.DependencyInjection;

using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Contracts;

internal class MicrosoftContainer(IServiceProvider serviceProvider) : IContainer
{
    public T? Resolve<T>() where T : class
    {
        return serviceProvider.GetService<T>();
    }

    public IEnumerable<T>? Resolve<T>(Type type)
    {
        var serviceType = typeof(IEnumerable<>).MakeGenericType(type);
        return serviceProvider.GetService(serviceType) as IEnumerable<T>;
    }
}
