﻿namespace ProfidLauncherUpdater.Infrastructure;

public sealed class TypeResolver : ITypeResolver, IDisposable
{
    //private readonly IHost _host = provider ?? throw new ArgumentNullException(nameof(provider));

    //public object? Resolve(Type? type)
    //{
    //    return type != null ? _host.Services.GetService(type) : null;
    //}

    //public void Dispose()
    //{
    //    _host.Dispose();
    //}

    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public object? Resolve(Type type)

    {
        if (type == null)
        {
            return null;
        }

        return _provider.GetService(type);
    }
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
