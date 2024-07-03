using Azure.Core;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortener.ApiService.Infrastructure.Database;
using UrlShortener.ApiService.Shared;

namespace UrlShortener.ApiService.Features
{
    public class RedirectUrl
    {
        public class Query : IRequest<Result<string>>
        {
            public string Code { get; set; } = string.Empty;
        }

        internal sealed class Handler : IRequestHandler<Query, Result<string>>
        {
            private readonly ApplicationDbContext _dbContext;
            public const int NumberOfCharsInShortLink = 7;
            private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            private readonly Random _random = new();

            public Handler(ApplicationDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Result<string>> Handle(Query request, CancellationToken cancellationToken)
            {
                var shortenedUrl = await _dbContext.ShortenedUrls
                                .FirstOrDefaultAsync(s => s.Code == request.Code);

                if (shortenedUrl is null)
                {
                    return Result.Failure<string>(new Error(
                   "RedirectUrlResponse.Null",
                   "The longUrl with the specified shortUrl was not found"));
                }

                return shortenedUrl.LongUrl;
            }
        }

    }

    public class RedirectUrlEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/{code}", async (string code, ISender sender) =>
            {
                var query = new RedirectUrl.Query { Code = code };

                var result = await sender.Send(query);

                if (result.IsFailure)
                {
                    return Results.NotFound(result.Error);
                }

                return Results.Redirect(result.Value);
            });
        }
    }
}
