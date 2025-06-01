using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Concurrent;

namespace TaskAPI.Helpers
{
    public static class Memoizer
    {
        public static Func<TInput, TResult> Memoize<TInput, TResult>(Func<TInput, TResult> func)
        {
            var cache = new ConcurrentDictionary<TInput, TResult>();
            return input => cache.GetOrAdd(input, func);
        }
    }
}
