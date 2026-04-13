using System;
using System.Collections.Generic;
using UnityEngine;

public class Memoized<TResult, TArgs>
{
    private readonly Func<TArgs, TResult> _factory;
    private readonly Dictionary<TArgs, TResult> _cache;

    public Memoized(Func<TArgs, TResult> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _cache = new Dictionary<TArgs, TResult>();
    }

    public TResult Get(TArgs args)
    {
        if (_cache.TryGetValue(args, out var cached))
            return cached;

        var result = _factory(args);
        _cache.Add(args, result);
        return result;
    }
}

public static class Memoized
{
    public static readonly Memoized<string, int> IntToString = new(i => i.ToString());
}
