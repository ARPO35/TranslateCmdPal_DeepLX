// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.CommandPalette.Extensions;

namespace TranslateDeepLXCmdPal;

[Guid("3c68b3bf-72b5-488d-8591-7aba53f82054")]
public sealed partial class TranslateDeepLXCmdPal : IExtension, IDisposable
{
    private readonly ManualResetEvent _extensionDisposedEvent;

    private readonly TranslateDeepLXCmdPalCommandsProvider _provider = new();

    public TranslateDeepLXCmdPal(ManualResetEvent extensionDisposedEvent)
    {
        this._extensionDisposedEvent = extensionDisposedEvent;
    }

    public object? GetProvider(ProviderType providerType)
    {
        return providerType switch
        {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }

    public void Dispose() => this._extensionDisposedEvent.Set();
}

