namespace UrlShortener.Api.Features.ShortenUrls.GetShortenUrl;

public interface IGetShortUrlHandler : IHandler
{
    Task<GetShortUrlResult> HandleAsync(GetShortUrlQuery query, CancellationToken cancellationToken);
    Task<GetShortUrlResult> HandleAsync(GetShortUrlByIdQuery query, CancellationToken cancellationToken);
}

public sealed record GetShortUrlQuery(string Code);
public sealed record GetShortUrlByIdQuery(string Id);
public sealed record GetShortUrlResult(string LongUrl);
public sealed class GetShortUrlHandler(
    ApplicationDbContext dbConetxt,
    HybridCache cache) : IGetShortUrlHandler
{
    public async Task<GetShortUrlResult> HandleAsync(GetShortUrlQuery query, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(query.Code));

        ShortenUrl? shortenUrl = await GetOrCreateAsync(query.Code, cancellationToken);

        if (shortenUrl is null)
        {
            throw new ArgumentException("Not Found!");
        }

        return new GetShortUrlResult(shortenUrl.LongUrl!);
    }

    public async Task<GetShortUrlResult> HandleAsync(GetShortUrlByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(query.Id));

        ShortenUrl? shortenUrl = await GetOrCreateByIdAsync(query.Id, cancellationToken);

        if (shortenUrl is null)
        {
            throw new ArgumentException("Not Found!");
        }

        return new GetShortUrlResult(shortenUrl.LongUrl!);
    }

    private async Task<ShortenUrl?> GetOrCreateByIdAsync(
        string id,
        CancellationToken cancellationToken) =>
             await cache.GetOrCreateAsync(
             $"id-{id}",
            async _ => await GetByValueFromDbAsync(id, cancellationToken),
            cancellationToken: cancellationToken);


    private async Task<ShortenUrl?> GetOrCreateAsync(
        string uniqueCode,
        CancellationToken cancellationToken) =>
             await cache.GetOrCreateAsync(
             $"code-{uniqueCode}",
            async _ => await GetByValueFromDbAsync(uniqueCode, cancellationToken),
            cancellationToken: cancellationToken);


    private async Task<ShortenUrl?> GetByValueFromDbAsync(
        string value,
        CancellationToken cancellationToken)
    {
        ShortenUrl? shortenUrl = null;
        if (Guid.TryParse(value, out Guid valueAsGuid))
        {
            shortenUrl = await dbConetxt.ShortenUrls.AsNoTracking()
               .SingleOrDefaultAsync(d => d.Id == valueAsGuid, cancellationToken);
        }
        else
        {
            shortenUrl = await dbConetxt.ShortenUrls.AsNoTracking()
                .SingleOrDefaultAsync(d => d.UniqueCode == value, cancellationToken);
        }
        return shortenUrl;
    }

}
