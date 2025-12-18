namespace OtherMediator.Contracts;

public interface IMediatorConfiguration
{
    Lifetime Lifetime { get; set; }

    DispatchStrategy DispatchStrategy { get; set; }

    bool UseExceptionHandler { get; set; }
}
