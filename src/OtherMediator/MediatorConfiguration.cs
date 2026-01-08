namespace OtherMediator;

using OtherMediator.Contracts;

/// <inheritdoc cref="IMediatorConfiguration" />
public class MediatorConfiguration : IMediatorConfiguration
{
    private Lifetime _lifetime = Lifetime.Transient;

    /// <inheritdoc />
    public Lifetime Lifetime
    {
        get => _lifetime;
        set => _lifetime = value;
    }

    /// <inheritdoc />
    public bool UseExceptionHandler { get; set; } = true;

    /// <inheritdoc />
    public DispatchStrategy DispatchStrategy { get; set; } = DispatchStrategy.Parallel;
}
