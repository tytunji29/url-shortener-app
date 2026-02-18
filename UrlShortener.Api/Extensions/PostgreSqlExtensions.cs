namespace UrlShortener.Api.Extensions;

public static class PostgreSqlExtensions
{
    public static IServiceCollection AddPostgreSqlConfig(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
