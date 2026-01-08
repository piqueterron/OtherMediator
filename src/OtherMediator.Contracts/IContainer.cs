namespace OtherMediator.Contracts;

/// <summary>
/// Defines a contract for a dependency injection container that provides service resolution capabilities.
/// This interface abstracts the underlying DI container implementation, allowing the mediator framework
/// to work with any compatible dependency injection provider.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IContainer"/> interface serves as an abstraction layer between the mediator framework
/// and specific DI containers (like Microsoft.Extensions.DependencyInjection, Autofac, etc.).
/// Implementations should wrap the native container's resolution mechanisms.
/// </para>
/// <para>
/// Key design principles:
/// <list type="bullet">
/// <item><description><strong>Abstraction</strong>: Decouples the mediator from specific DI implementations</description></item>
/// <item><description><strong>Flexibility</strong>: Allows swapping DI containers without changing mediator code</description></item>
/// <item><description><strong>Testability</strong>: Enables easy mocking for unit tests</description></item>
/// <item><description><strong>Consistency</strong>: Provides uniform API across different container implementations</description></item>
/// </list>
/// </para>
/// <para>
/// Service resolution follows these rules:
/// <list type="bullet">
/// <item><description>Services must be registered before they can be resolved</description></item>
/// <item><description>Resolution respects the configured <see cref="Lifetime"/> of services</description></item>
/// <item><description>Null is returned for unregistered services (not an exception)</description></item>
/// <item><description>Disposable services are tracked by the container based on their lifetime</description></item>
/// </list>
/// </para>
/// <example>
/// Example implementation for Microsoft DI:
/// <code>
/// public class MicrosoftContainer : IContainer
/// {
///     private readonly IServiceProvider _serviceProvider;
///     
///     public MicrosoftContainer(IServiceProvider serviceProvider)
///         => _serviceProvider = serviceProvider;
///     
///     public T? Resolve&lt;T&gt;() where T : class
///         => _serviceProvider.GetService&lt;T&gt;();
///     
///     public IEnumerable&lt;T&gt;? Resolve&lt;T&gt;(Type type)
///         => _serviceProvider.GetServices(type) as IEnumerable&lt;T&gt;;
/// }
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="Lifetime"/>
/// <seealso cref="IServiceProvider"/>
public interface IContainer
{
    /// <summary>
    /// Resolves a single instance of the specified service type from the container.
    /// If multiple implementations are registered for the same service type,
    /// this method returns the last registered implementation (container-specific).
    /// </summary>
    /// <typeparam name="T">
    /// The service type to resolve. Must be a reference type (class).
    /// This is typically an interface or abstract class that was registered.
    /// </typeparam>
    /// <returns>
    /// An instance of type <typeparamref name="T"/> if the service is registered;
    /// otherwise, <see langword="null"/>. The instance's lifetime and disposal
    /// are managed according to its <see cref="Lifetime"/> configuration.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Behavior details:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <strong>Singleton</strong>: Returns the same instance on every resolution.
    /// The instance is created on first request and persisted for the container's lifetime.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <strong>Scoped</strong>: Returns the same instance within the same scope.
    /// Different scopes get different instances. In web applications, scopes typically
    /// correspond to HTTP requests.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <strong>Transient</strong>: Returns a new instance on every resolution.
    /// Each call creates a new object; no instance sharing occurs.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Important considerations:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// For services with constructor dependencies, the container automatically
    /// resolves and injects all required dependencies recursively.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Circular dependencies are generally not supported and will cause
    /// resolution failures (container-specific behavior).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Services implementing <see cref="IDisposable"/> or <see cref="IAsyncDisposable"/>
    /// are automatically disposed by the container when their scope ends or
    /// the container is disposed, according to their lifetime.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// Basic usage examples:
    /// <code>
    /// // Resolving a singleton service
    /// var logger = container.Resolve&lt;ILogger&gt;();
    /// // 'logger' is the same instance every time within this container
    /// 
    /// // Resolving a scoped service
    /// var dbContext = container.Resolve&lt;ApplicationDbContext&gt;();
    /// // 'dbContext' is the same instance within this scope
    /// 
    /// // Resolving a transient service
    /// var validator = container.Resolve&lt;IValidator&lt;User&gt;&gt;();
    /// // Each call creates a new validator instance
    /// </code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    /// May be thrown by implementations if the service cannot be constructed
    /// (e.g., missing dependencies, circular dependencies). However, most
    /// implementations return null instead of throwing for unregistered services.
    /// </exception>
    T? Resolve<T>() where T : class;

    /// <summary>
    /// Resolves all registered implementations of a specified service type from the container.
    /// This method is used when multiple implementations of the same interface or base class
    /// are registered and need to be retrieved as a collection.
    /// </summary>
    /// <typeparam name="T">
    /// The element type of the returned collection. This is typically the interface or base class
    /// that multiple implementations share. Must be a reference type.
    /// </typeparam>
    /// <param name="type">
    /// The service type to resolve. This should match the registration type used when services
    /// were registered. For example, if services were registered as <c>IMyService</c>, pass
    /// <c>typeof(IMyService)</c>.
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> containing all registered implementations of the specified type,
    /// or <see langword="null"/> if no implementations are registered. The collection may be empty
    /// if the service type is registered but no implementations exist (edge case).
    /// The order of elements typically matches the registration order in the container.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Common use cases for multiple implementations:
    /// <list type="bullet">
    /// <item><description>Plugin architectures where multiple plugins implement the same interface</description></item>
    /// <item><description>Strategy pattern implementations</description></item>
    /// <item><description>Notification handlers (multiple handlers for the same notification)</description></item>
    /// <item><description>Middleware pipelines</description></item>
    /// <item><description>Validation rules collections</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Implementation details:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// This method internally constructs a generic type <c>IEnumerable&lt;TService&gt;</c> and
    /// requests it from the underlying container. Most DI containers have special handling
    /// for <see cref="IEnumerable{T}"/> registrations.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Each resolved instance follows its configured <see cref="Lifetime"/>. If a service is
    /// registered as Singleton, the same instance will appear in the collection on every
    /// resolution within the same container/scope.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The method uses <paramref name="type"/> rather than <typeparamref name="T"/> directly
    /// to allow scenarios where the element type differs from the registration type
    /// (though this is uncommon).
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// Working with multiple implementations:
    /// <code>
    /// // Registration
    /// container.Register&lt;INotificationHandler&lt;OrderShipped&gt;, EmailNotificationHandler&gt;();
    /// container.Register&lt;INotificationHandler&lt;OrderShipped&gt;, SmsNotificationHandler&gt;();
    /// container.Register&lt;INotificationHandler&lt;OrderShipped&gt;, AuditNotificationHandler&gt;();
    /// 
    /// // Resolution
    /// var handlers = container.Resolve&lt;INotificationHandler&lt;OrderShipped&gt;&gt;(
    ///     typeof(INotificationHandler&lt;OrderShipped&gt;));
    /// 
    /// // Execute all handlers
    /// if (handlers != null)
    /// {
    ///     foreach (var handler in handlers)
    ///     {
    ///         await handler.HandleAsync(notification, cancellationToken);
    ///     }
    /// }
    /// </code>
    /// 
    /// Using with strategy pattern:
    /// <code>
    /// // Multiple payment processors
    /// var processors = container.Resolve&lt;IPaymentProcessor&gt;(typeof(IPaymentProcessor));
    /// var processor = processors?.FirstOrDefault(p => p.CanProcess(paymentMethod));
    /// 
    /// if (processor != null)
    /// {
    ///     await processor.ProcessAsync(payment);
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="type"/> is not compatible with <typeparamref name="T"/>
    /// or is not a valid service type.
    /// </exception>
    IEnumerable<T>? Resolve<T>(Type type);
}

/// <summary>
/// Defines the lifetime management strategies for service instances in the dependency injection container.
/// The lifetime determines how long a service instance lives and when it is disposed or recreated.
/// </summary>
/// <remarks>
/// <para>
/// Choosing the correct lifetime is crucial for:
/// <list type="bullet">
/// <item><description><strong>Performance</strong>: Singleton reduces instance creation overhead</description></item>
/// <item><description><strong>Memory management</strong>: Proper disposal prevents memory leaks</description></item>
/// <item><description><strong>Thread safety</strong>: Singleton services must be thread-safe</description></item>
/// <item><description><strong>State management</strong>: Scoped services can maintain request-specific state</description></item>
/// </list>
/// </para>
/// <para>
/// Lifetime comparison matrix:
/// <list type="table">
/// <listheader>
/// <term>Aspect</term>
/// <description>Singleton</description>
/// <description>Scoped</description>
/// <description>Transient</description>
/// </listheader>
/// <item>
/// <term>Instance Sharing</term>
/// <description>Global single instance</description>
/// <description>Per scope instance</description>
/// <description>New instance every time</description>
/// </item>
/// <item>
/// <term>Disposal Time</term>
/// <description>Container disposal</description>
/// <description>Scope disposal</description>
/// <description>Garbage collection or explicit disposal</description>
/// </item>
/// <item>
/// <term>Thread Safety</term>
/// <description>Must be thread-safe</description>
/// <description>Scope-bound, typically thread-safe within scope</description>
/// <description>Usually not needed (instance per consumer)</description>
/// </item>
/// <item>
/// <term>Use Case</term>
/// <description>Configuration, logging, caching</description>
/// <description>DbContext, unit of work, request-specific services</description>
/// <description>Validators, mappers, stateless services</description>
/// </item>
/// </list>
/// </para>
/// <para>
/// Lifetime propagation:
/// When a service with a longer lifetime depends on a service with a shorter lifetime,
/// it's called a "captive dependency" and can cause issues. For example:
/// <code>
/// // PROBLEMATIC: Singleton depends on Transient
/// public class SingletonService
/// {
///     public SingletonService(TransientService transient) { ... }
///     // TransientService instance is captured and never recreated
/// }
/// </code>
/// Most DI containers detect and warn about captive dependencies.
/// </para>
/// </remarks>
/// <example>
/// Appropriate lifetime selection:
/// <code>
/// // Singleton: App-wide configuration
/// services.AddSingleton&lt;IConfiguration&gt;(Configuration);
/// 
/// // Scoped: Database context per request
/// services.AddScoped&lt;ApplicationDbContext&gt;();
/// 
/// // Transient: Stateless validators
/// services.AddTransient&lt;IValidator&lt;UserDto&gt;, UserValidator&gt;();
/// </code>
/// </example>
public enum Lifetime
{
    /// <summary>
    /// A single instance is created for the entire lifetime of the container.
    /// The same instance is returned on every resolution request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Characteristics:
    /// <list type="bullet">
    /// <item><description><strong>Creation</strong>: Instantiated on first request (lazy) or at startup (eager)</description></item>
    /// <item><description><strong>Sharing</strong>: Shared across all consumers and threads</description></item>
    /// <item><description><strong>Disposal</strong>: Disposed when the container is disposed</description></item>
    /// <item><description><strong>Thread safety</strong>: Must be designed for concurrent access</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Ideal for:
    /// <list type="bullet">
    /// <item><description>Configuration readers and app settings</description></item>
    /// <item><description>Logging services and telemetry clients</description></item>
    /// <item><description>Cache managers and in-memory stores</description></item>
    /// <item><description>HttpClient factories (with proper configuration)</description></item>
    /// <item><description>Background service schedulers</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Considerations:
    /// <list type="bullet">
    /// <item><description>Avoid state that should be request-specific</description></item>
    /// <item><description>Ensure thread safety for all public methods</description></item>
    /// <item><description>Be cautious with dependencies on shorter-lived services</description></item>
    /// <item><description>Memory is held until container disposal</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class AppConfigService : IAppConfigService
    /// {
    ///     private readonly IConfiguration _config;
    ///     private readonly Lazy&lt;AppSettings&gt; _settings;
    ///     
    ///     public AppConfigService(IConfiguration config)
    ///     {
    ///         _config = config;
    ///         _settings = new Lazy&lt;AppSettings&gt;(() => LoadSettings());
    ///     }
    ///     
    ///     public AppSettings GetSettings() => _settings.Value;
    ///     
    ///     // Thread-safe property access
    ///     public string ApiUrl => _config["Api:Url"];
    /// }
    /// 
    /// // Registration
    /// services.AddSingleton&lt;IAppConfigService, AppConfigService&gt;();
    /// </code>
    /// </example>
    Singleton,

    /// <summary>
    /// A new instance is created for each scope. Within the same scope,
    /// the same instance is returned on every resolution request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Characteristics:
    /// <list type="bullet">
    /// <item><description><strong>Creation</strong>: Instantiated on first request within a scope</description></item>
    /// <item><description><strong>Sharing</strong>: Shared within the same scope, different across scopes</description></item>
    /// <item><description><strong>Disposal</strong>: Disposed when the scope is disposed</description></item>
    /// <item><description><strong>Thread safety</strong>: Typically used within a single logical operation (e.g., HTTP request)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Scope definitions:
    /// <list type="bullet">
    /// <item><description><strong>Web Applications</strong>: Each HTTP request creates a new scope</description></item>
    /// <item><description><strong>Background Services</strong>: Manual scope creation for each operation</description></item>
    /// <item><description><strong>Console Apps</strong>: Typically one scope for the entire app or per operation</description></item>
    /// <item><description><strong>Unit Tests</strong>: New scope for each test or test class</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Ideal for:
    /// <list type="bullet">
    /// <item><description>Entity Framework DbContext and unit of work patterns</description></item>
    /// <item><description>Request-specific caches and state management</description></item>
    /// <item><description>User session information and authentication context</description></item>
    /// <item><description>Transaction management and business operation context</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Creating scopes:
    /// <code>
    /// using (var scope = serviceProvider.CreateScope())
    /// {
    ///     var scopedService = scope.ServiceProvider.GetService&lt;IMyScopedService&gt;();
    ///     // Use scopedService
    /// } // scopedService is disposed here
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class OrderService : IOrderService
    /// {
    ///     private readonly ApplicationDbContext _context;
    ///     private readonly IUserContext _userContext;
    ///     
    ///     public OrderService(ApplicationDbContext context, IUserContext userContext)
    ///     {
    ///         _context = context;
    ///         _userContext = userContext; // Both scoped to the same request
    ///     }
    ///     
    ///     public async Task&lt;Order&gt; CreateOrderAsync(OrderRequest request)
    ///     {
    ///         var order = new Order
    ///         {
    ///             UserId = _userContext.UserId, // Request-specific user
    ///             CreatedAt = DateTime.UtcNow
    ///         };
    ///         
    ///         _context.Orders.Add(order);
    ///         await _context.SaveChangesAsync(); // Same DbContext instance
    ///         
    ///         return order;
    ///     }
    /// }
    /// 
    /// // Registration
    /// services.AddScoped&lt;IOrderService, OrderService&gt;();
    /// services.AddScoped&lt;ApplicationDbContext&gt;();
    /// services.AddScoped&lt;IUserContext, HttpUserContext&gt;();
    /// </code>
    /// </example>
    Scoped,

    /// <summary>
    /// A new instance is created every time the service is requested from the container.
    /// No instance sharing occurs, even within the same scope.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Characteristics:
    /// <list type="bullet">
    /// <item><description><strong>Creation</strong>: New instance on every resolution request</description></item>
    /// <item><description><strong>Sharing</strong>: No sharing, each consumer gets a new instance</description></item>
    /// <item><description><strong>Disposal</strong>: Not managed by container (for IDisposable); may cause memory leaks if not disposed manually</description></item>
    /// <item><description><strong>Thread safety</strong>: Typically not required as each thread gets its own instance</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Important considerations for disposable transients:
    /// <list type="bullet">
    /// <item><description>Transient services that implement <see cref="IDisposable"/> are NOT automatically disposed by most containers</description></item>
    /// <item><description>You must explicitly call <c>Dispose()</c> or use <c>using</c> statements</description></item>
    /// <item><description>Leaving transient disposables undisposed can cause resource leaks (database connections, file handles, etc.)</description></item>
    /// <item><description>Consider using factory patterns or pooled instances for expensive resources</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Ideal for:
    /// <list type="bullet">
    /// <item><description>Stateless services with no dependencies on request context</description></item>
    /// <item><description>Lightweight validators, mappers, and converters</description></item>
    /// <item><description>Algorithm implementations and mathematical services</description></item>
    /// <item><description>Factory classes and builders</description></item>
    /// <item><description>View models and DTO assemblers</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Performance note:
    /// While transient services have more allocation overhead, they are often suitable for
    /// lightweight, stateless operations. For expensive object creation, consider Singleton
    /// or Scoped lifetimes with thread-safe design.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class EmailValidator : IValidator&lt;EmailDto&gt;
    /// {
    ///     // Stateless - no fields/properties
    ///     public ValidationResult Validate(EmailDto email)
    ///     {
    ///         var result = new ValidationResult();
    ///         
    ///         if (string.IsNullOrEmpty(email.Address))
    ///             result.AddError("Email address is required");
    ///         else if (!IsValidEmail(email.Address))
    ///             result.AddError("Invalid email format");
    ///             
    ///         return result;
    ///     }
    ///     
    ///     private bool IsValidEmail(string email) { ... }
    /// }
    /// 
    /// // Safe usage - validator is stateless and disposable
    /// services.AddTransient&lt;IValidator&lt;EmailDto&gt;, EmailValidator&gt;();
    /// 
    /// // PROBLEMATIC - disposable without proper cleanup
    /// services.AddTransient&lt;IFileProcessor, FileProcessor&gt;(); // FileProcessor is IDisposable
    /// // Solution: Use Scoped or implement proper disposal
    /// </code>
    /// 
    /// Proper disposal pattern:
    /// <code>
    /// public class ResourceService : IResourceService, IDisposable
    /// {
    ///     private readonly Stream _stream;
    ///     private bool _disposed;
    ///     
    ///     public ResourceService()
    ///     {
    ///         _stream = new FileStream("data.txt", FileMode.Open);
    ///     }
    ///     
    ///     public void ProcessData() { ... }
    ///     
    ///     public void Dispose()
    ///     {
    ///         if (!_disposed)
    ///         {
    ///             _stream.Dispose();
    ///             _disposed = true;
    ///         }
    ///     }
    /// }
    /// 
    /// // Usage with explicit disposal
    /// using (var service = serviceProvider.GetService&lt;IResourceService&gt;())
    /// {
    ///     service.ProcessData();
    /// }
    /// </code>
    /// </example>
    Transient
}

/// <summary>
/// Defines execution strategies for dispatching notifications to multiple handlers.
/// This strategy determines how the mediator coordinates the execution of multiple
/// handlers registered for the same notification type.
/// </summary>
/// <remarks>
/// <para>
/// The dispatch strategy is a critical performance and correctness consideration:
/// <list type="table">
/// <listheader>
/// <term>Strategy</term>
/// <description>Performance</description>
/// <description>Order Guarantee</description>
/// <description>Error Handling</description>
/// <description>Resource Usage</description>
/// </listheader>
/// <item>
/// <term><see cref="Parallel"/></term>
/// <description>High (concurrent execution)</description>
/// <description>No guarantee</description>
/// <description>Independent per handler</description>
/// <description>Higher (multiple threads)</description>
/// </item>
/// <item>
/// <term><see cref="Sequential"/></term>
/// <description>Lower (serial execution)</description>
/// <description>Registration order</description>
/// <description>Stops on first error (optional)</description>
/// <description>Lower (single thread)</description>
/// </item>
/// </list>
/// </para>
/// <para>
/// Handler design considerations based on strategy:
/// <list type="bullet">
/// <item><description><strong>For Parallel execution</strong>: Handlers must be thread-safe and independent</description></item>
/// <item><description><strong>For Sequential execution</strong>: Handlers can have dependencies on previous handlers</description></item>
/// <item><description><strong>Common to both</strong>: Should handle cancellation and be idempotent when possible</description></item>
/// </list>
/// </para>
/// <para>
/// Error propagation behavior:
/// <list type="bullet">
/// <item><description><strong>Parallel</strong>: Exceptions are aggregated (AggregateException) or handled per configuration</description></item>
/// <item><description><strong>Sequential</strong>: Execution stops at first exception (fail-fast) or continues based on configuration</description></item>
/// </list>
/// </para>
/// <para>
/// Configuration example:
/// <code>
/// // Configure mediator with parallel dispatch
/// services.AddMediator(config =>
/// {
///     config.DispatchStrategy = DispatchStrategy.Parallel;
///     // Handlers execute concurrently
/// });
/// 
/// // Configure mediator with sequential dispatch  
/// services.AddMediator(config =>
/// {
///     config.DispatchStrategy = DispatchStrategy.Sequential;
///     // Handlers execute one after another
/// });
/// </code>
/// </para>
/// </remarks>
/// <example>
/// Handler execution patterns:
/// <code>
/// // Parallel execution (typical scenario)
/// await mediator.Publish(new OrderShippedNotification(order));
/// // All three handlers execute concurrently:
/// // 1. SendEmailHandler  --\
/// // 2. UpdateInventoryHandler >-- All start at same time
/// // 3. NotifyAnalyticsHandler --/
/// 
/// // Sequential execution (when order matters)
/// await mediator.Publish(new UserRegisteredNotification(user));
/// // Handlers execute in registration order:
/// // 1. ValidateUserHandler (must complete)
/// // 2. CreateUserProfileHandler (depends on validation)
/// // 3. SendWelcomeEmailHandler (depends on profile creation)
/// </code>
/// </example>
public enum DispatchStrategy
{
    /// <summary>
    /// All notification handlers execute concurrently using asynchronous parallel execution.
    /// This strategy provides the best performance for independent handlers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementation details:
    /// <list type="bullet">
    /// <item><description>Uses <c>Task.WhenAll</c> or similar construct to execute handlers concurrently</description></item>
    /// <item><description>Handlers start execution in undefined order</description></item>
    /// <item><description>Completion order is also undefined</description></item>
    /// <item><description>The publishing task completes when ALL handlers complete</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When to use Parallel dispatch:
    /// <list type="bullet">
    /// <item><description>Handlers are completely independent (no shared state or order requirements)</description></item>
    /// <item><description>Performance is critical and handlers are I/O bound</description></item>
    /// <item><description>Handlers are stateless or properly synchronized</description></item>
    /// <item><description>You want to leverage multi-core processors for CPU-bound operations</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Error handling in Parallel mode:
    /// <code>
    /// try
    /// {
    ///     await mediator.Publish(notification);
    /// }
    /// catch (AggregateException aggrEx)
    /// {
    ///     // Multiple handlers may have failed
    ///     foreach (var ex in aggrEx.InnerExceptions)
    ///     {
    ///         logger.LogError(ex, "Handler failed");
    ///     }
    /// }
    /// </code>
    /// Some implementations may wrap exceptions differently or provide
    /// configuration options for error handling behavior.
    /// </para>
    /// <para>
    /// Resource considerations:
    /// <list type="bullet">
    /// <item><description>Each handler may run on a different thread pool thread</description></item>
    /// <item><description>Database connections may be multiplied across handlers</description></item>
    /// <item><description>Consider using <c>ConfigureAwait(false)</c> in handlers to avoid context capture</description></item>
    /// <item><description>Monitor thread pool usage for high-volume scenarios</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Parallel-appropriate handlers
    /// public class AnalyticsHandler : INotificationHandler&lt;OrderCompletedNotification&gt;
    /// {
    ///     public async Task Handle(OrderCompletedNotification notification, CancellationToken ct)
    ///     {
    ///         // Independent analytics logging
    ///         await _analytics.LogAsync(notification.OrderId, ct);
    ///     }
    /// }
    /// 
    /// public class EmailHandler : INotificationHandler&lt;OrderCompletedNotification&gt;
    /// {
    ///     public async Task Handle(OrderCompletedNotification notification, CancellationToken ct)
    ///     {
    ///         // Independent email sending
    ///         await _emailService.SendReceiptAsync(notification.CustomerEmail, ct);
    ///     }
    /// }
    /// 
    /// // These can execute in parallel safely
    /// </code>
    /// </example>
    Parallel,

    /// <summary>
    /// Notification handlers execute one after another in sequential order.
    /// Each handler must complete before the next handler begins execution.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementation details:
    /// <list type="bullet">
    /// <item><description>Handlers execute in the order they were registered in the container</description></item>
    /// <item><description>Uses sequential <c>await</c> calls or similar synchronous pattern</description></item>
    /// <item><description>The publishing task completes when the LAST handler completes</description></item>
    /// <item><description>If a handler throws, subsequent handlers may not execute (configurable)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When to use Sequential dispatch:
    /// <list type="bullet">
    /// <item><description>Handlers have dependencies on previous handlers' side effects</description></item>
    /// <item><description>Order of execution is semantically important</description></item>
    /// <item><description>Handlers share resources that require serialized access</description></item>
    /// <item><description>You need strict transaction boundaries across handlers</description></item>
    /// <item><description>Debugging or tracing requires deterministic execution order</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Controlling execution order:
    /// Registration order in the DI container determines execution order:
    /// <code>
    /// // First registered = first executed
    /// services.AddScoped&lt;INotificationHandler&lt;MyNotification&gt;, FirstHandler&gt;();
    /// services.AddScoped&lt;INotificationHandler&lt;MyNotification&gt;, SecondHandler&gt;();
    /// services.AddScoped&lt;INotificationHandler&lt;MyNotification&gt;, ThirdHandler&gt;();
    /// // Execution order: FirstHandler → SecondHandler → ThirdHandler
    /// </code>
    /// Some frameworks allow explicit ordering via attributes or interfaces.
    /// </para>
    /// <para>
    /// Error handling in Sequential mode:
    /// <list type="bullet">
    /// <item><description><strong>Fail-fast</strong>: Default behavior stops execution on first exception</description></item>
    /// <item><description><strong>Continue-on-error</strong>: Some implementations allow continuing despite errors</description></item>
    /// <item><description>Exceptions propagate immediately to the caller</description></item>
    /// <item><description>Partial completion may require compensation actions</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sequential handlers with dependencies
    /// public class ValidationHandler : INotificationHandler&lt;UserRegistrationNotification&gt;
    /// {
    ///     public async Task Handle(UserRegistrationNotification notification, CancellationToken ct)
    ///     {
    ///         // Must complete before profile creation
    ///         await _validator.ValidateAsync(notification.User, ct);
    ///     }
    /// }
    /// 
    /// public class ProfileHandler : INotificationHandler&lt;UserRegistrationNotification&gt;
    /// {
    ///     public async Task Handle(UserRegistrationNotification notification, CancellationToken ct)
    ///     {
    ///         // Depends on successful validation
    ///         await _profileService.CreateAsync(notification.User, ct);
    ///     }
    /// }
    /// 
    /// public class WelcomeHandler : INotificationHandler&lt;UserRegistrationNotification&gt;
    /// {
    ///     public async Task Handle(UserRegistrationNotification notification, CancellationToken ct)
    ///     {
    ///         // Depends on profile creation
    ///         await _emailService.SendWelcomeAsync(notification.User.Email, ct);
    ///     }
    /// }
    /// 
    /// // These MUST execute sequentially
    /// </code>
    /// 
    /// Registration with explicit ordering:
    /// <code>
    /// // Order matters - register in execution order
    /// var services = new ServiceCollection();
    /// 
    /// services.AddScoped&lt;INotificationHandler&lt;OrderNotification&gt;, ValidateOrderHandler&gt;();
    /// services.AddScoped&lt;INotificationHandler&lt;OrderNotification&gt;, ChargePaymentHandler&gt;();
    /// services.AddScoped&lt;INotificationHandler&lt;OrderNotification&gt;, UpdateInventoryHandler&gt;();
    /// services.AddScoped&lt;INotificationHandler&lt;OrderNotification&gt;, SendConfirmationHandler&gt;();
    /// 
    /// services.AddMediator(config =>
    /// {
    ///     config.DispatchStrategy = DispatchStrategy.Sequential;
    /// });
    /// 
    /// // Execution follows registration order
    /// </code>
    /// </example>
    Sequential
}
