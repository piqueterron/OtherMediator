namespace OtherMediator.Contracts;

using System.Reflection;

public interface IMediatorConfiguration
{
    Lifetime Lifetime { get; set; }

    DispatchStrategy DispatchStrategy { get; set; }

    bool UseExceptionHandler { get; set; }

    void AddOpenPipelineBehavior(Type type);

    void AddPipelineBehavior<TRequest, TResponse>(Type type) where TRequest : IRequest<TResponse>;

    void RegisterServicesFromAssemblies(params Assembly[] assemblies);

    void RegisterServicesFromAssembly<TAssembly>() where TAssembly : class;
}
