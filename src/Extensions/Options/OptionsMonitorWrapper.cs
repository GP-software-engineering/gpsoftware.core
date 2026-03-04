using System;
using Microsoft.Extensions.Options;

namespace GPSoftware.Core.Extensions.Options;

#nullable disable

/// <summary>
///     <see cref="IOptionsMonitor{TOptions}"/> wrapper for accessing options, allowing retrieval of the current value without change notifications.
/// </summary>
/// <remarks>This implementation does not support change notifications; the OnChange method does not perform any
/// actions.</remarks>
/// <typeparam name="TOptions">The type of options being monitored. Must be a reference type with a parameterless constructor.</typeparam>
/// 
public class OptionsMonitorWrapper<TOptions> : IOptionsMonitor<TOptions> where TOptions : class, new() {
    public OptionsMonitorWrapper(TOptions options) {
        CurrentValue = options;
    }

    public TOptions CurrentValue { get; }
    public TOptions Get(string name) => CurrentValue;
    public IDisposable OnChange(Action<TOptions, string> listener) => null;    // do nothing, we don't support change notifications
}

#nullable restore
