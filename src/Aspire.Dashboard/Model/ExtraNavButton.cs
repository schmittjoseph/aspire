// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.FluentUI.AspNetCore.Components;

namespace Aspire.Dashboard.Model;

public sealed record ExtraNavButton
{
    public required Icon Icon { get; init; }
    public required Icon SecondaryIcon { get; init; }

    public required string Name { get; init; }
    public required string HRef { get; init;  }
}
