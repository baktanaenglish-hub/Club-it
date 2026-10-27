using ClubIt.Infrastructure.Persistence;
using Npgsql;

namespace ClubIt.Tests.Unit;

public class PostgresConnectionStringTests
{
    [Fact]
    public void Url_from_hosting_provider_is_converted()
    {
        var result = new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize(
            "postgresql://club%40it:p%3Ass@ep-cool-1.eu-central-1.aws.neon.tech/clubit?sslmode=require&channel_binding=require"));

        Assert.Equal("ep-cool-1.eu-central-1.aws.neon.tech", result.Host);
        Assert.Equal(5432, result.Port);
        Assert.Equal("clubit", result.Database);
        Assert.Equal("club@it", result.Username);
        Assert.Equal("p:ss", result.Password);
        Assert.Equal(SslMode.Require, result.SslMode);
    }

    [Fact]
    public void Explicit_port_is_kept()
    {
        var result = new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize("postgres://u:p@db:6543/x"));

        Assert.Equal(6543, result.Port);
    }

    [Fact]
    public void Npgsql_connection_string_is_returned_unchanged()
    {
        const string value = "Host=localhost;Database=clubit;Username=clubit;Password=clubit";
        Assert.Equal(value, PostgresConnectionString.Normalize(value));
    }
}
