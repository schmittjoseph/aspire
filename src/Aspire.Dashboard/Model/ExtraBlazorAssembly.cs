// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Aspire.Dashboard.Model;

public sealed record ExtraBlazorAssembly
{
    public required Assembly Assembly { get; init; }
}
