namespace UrlShortener.ApiService.Infrastructure
{
    public interface IEndpoint
    {
        void MapEndpoint(IEndpointRouteBuilder app);
    }
}
