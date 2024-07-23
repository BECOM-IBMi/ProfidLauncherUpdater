namespace ProfidLauncherUpdater.Infrastructure;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder;

    public TypeRegistrar(IServiceCollection builder)
    {
        _builder = builder;
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(_builder.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> func)
    {
        if (func is null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        _builder.AddSingleton(service, (provider) => func());
    }

    //private readonly HostApplicationBuilder _builder;

    //public TypeRegistrar(HostApplicationBuilder builder)
    //{
    //    _builder = builder;
    //}

    //public ITypeResolver Build()
    //{
    //    return new TypeResolver(_builder.Build());
    //}

    //public void Register(Type service, Type implementation)
    //{
    //    _builder.Services.AddSingleton(service, implementation);
    //}

    //public void RegisterInstance(Type service, object implementation)
    //{
    //    _builder.Services.AddSingleton(service, implementation);
    //}

    //public void RegisterLazy(Type service, Func<object> func)
    //{
    //    if (func is null) throw new ArgumentNullException(nameof(func));

    //    _builder.Services.AddSingleton(service, _ => func());
    //}
}
