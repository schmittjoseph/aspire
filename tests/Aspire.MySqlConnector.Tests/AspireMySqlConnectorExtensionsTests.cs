// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Components.Common.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Xunit;

namespace Aspire.MySqlConnector.Tests;

public class AspireMySqlConnectorExtensionsTests : IClassFixture<MySqlContainerFixture>
{
    private readonly MySqlContainerFixture _containerFixture;
    private string ConnectionString => RequiresDockerTheoryAttribute.IsSupported
                                        ? _containerFixture.GetConnectionString()
                                        : "Server=localhost;Database=test_aspire_mysql";
    private string NormalizedConnectionString => new MySqlConnectionStringBuilder(ConnectionString).ConnectionString;

    public AspireMySqlConnectorExtensionsTests(MySqlContainerFixture containerFixture)
        => _containerFixture = containerFixture;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReadsFromConnectionStringsCorrectly(bool useKeyed)
    {
        var builder = Host.CreateEmptyApplicationBuilder(null);
        builder.Configuration.AddInMemoryCollection([
            new KeyValuePair<string, string?>("ConnectionStrings:mysql", ConnectionString)
        ]);

        if (useKeyed)
        {
            builder.AddKeyedMySqlDataSource("mysql");
        }
        else
        {
            builder.AddMySqlDataSource("mysql");
        }

        var host = builder.Build();
        var dataSource = useKeyed ?
            host.Services.GetRequiredKeyedService<MySqlDataSource>("mysql") :
            host.Services.GetRequiredService<MySqlDataSource>();

        Assert.Equal(NormalizedConnectionString, dataSource.ConnectionString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConnectionStringCanBeSetInCode(bool useKeyed)
    {
        var builder = Host.CreateEmptyApplicationBuilder(null);
        builder.Configuration.AddInMemoryCollection([
            new KeyValuePair<string, string?>("ConnectionStrings:mysql", "unused")
        ]);

        void SetConnectionString(MySqlConnectorSettings settings) => settings.ConnectionString = ConnectionString;
        if (useKeyed)
        {
            builder.AddKeyedMySqlDataSource("mysql", SetConnectionString);
        }
        else
        {
            builder.AddMySqlDataSource("mysql", SetConnectionString);
        }

        var host = builder.Build();
        var dataSource = useKeyed ?
            host.Services.GetRequiredKeyedService<MySqlDataSource>("mysql") :
            host.Services.GetRequiredService<MySqlDataSource>();

        Assert.Equal(NormalizedConnectionString, dataSource.ConnectionString);
        // the connection string from config should not be used since code set it explicitly
        Assert.DoesNotContain("unused", dataSource.ConnectionString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConnectionNameWinsOverConfigSection(bool useKeyed)
    {
        var builder = Host.CreateEmptyApplicationBuilder(null);

        var key = useKeyed ? "mysql" : null;
        builder.Configuration.AddInMemoryCollection([
            new KeyValuePair<string, string?>(ConformanceTests.CreateConfigKey("Aspire:MySqlConnector", key, "ConnectionString"), "unused"),
            new KeyValuePair<string, string?>("ConnectionStrings:mysql", ConnectionString)
        ]);

        if (useKeyed)
        {
            builder.AddKeyedMySqlDataSource("mysql");
        }
        else
        {
            builder.AddMySqlDataSource("mysql");
        }

        var host = builder.Build();
        var dataSource = useKeyed ?
            host.Services.GetRequiredKeyedService<MySqlDataSource>("mysql") :
            host.Services.GetRequiredService<MySqlDataSource>();

        Assert.Equal(NormalizedConnectionString, dataSource.ConnectionString);
        // the connection string from config should not be used since it was found in ConnectionStrings
        Assert.DoesNotContain("unused", dataSource.ConnectionString);
    }
}
