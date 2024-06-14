using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortener.ApiService.Infrastructure.Database;

namespace UrlShortener.ApiService.Features
{
    public class RedirectUrlQuery : IRequest<string>
    {
        public string Code { get; set; }
    }

    internal sealed class RedirectUrlQueryHandler :
        IRequestHandler<RedirectUrlQuery, string>
    {
        private readonly ApplicationDbContext _context;
        public const int NumberOfCharsInShortLink = 7;
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private readonly Random _random = new();

        public RedirectUrlQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(RedirectUrlQuery request, CancellationToken cancellationToken)
        {
            var shortenedUrl = await _context
                    .ShortenedUrls
                    .FirstOrDefaultAsync(s => s.Code == request.Code);

            if (shortenedUrl is null)
            {
                //return Results.NotFound();
            }

            return shortenedUrl.ShortUrl;
        }
    }


    public class RedirectUrlEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/{code}", async (String code, ISender sender) =>
            {
                var query = new RedirectUrlQuery { Code = code };
                var result = await sender.Send(query);

                return Results.Ok(result);
            });
        }
    }
}
