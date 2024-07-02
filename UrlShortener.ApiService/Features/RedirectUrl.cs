//using Azure.Core;
//using Carter;
//using FluentValidation;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using UrlShortener.ApiService.Infrastructure.Database;
//using UrlShortener.ApiService.Shared;

//namespace UrlShortener.ApiService.Features
//{
//    public class RedirectUrl
//    {
//        public class Query : IRequest<Result<RedirectUrlResponse>>
//        {
//            public string Code { get; set; } = string.Empty;
//        }

//        internal sealed class Handler : IRequestHandler<Query, Result<RedirectUrlResponse>>
//        {
//            private readonly ApplicationDbContext _context;
//            public const int NumberOfCharsInShortLink = 7;
//            private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

//            private readonly Random _random = new();

//            public Handler(ApplicationDbContext context)
//            {
//                _context = context;
//            }

//            public async Task<Result<RedirectUrlResponse>> Handle(Query request, CancellationToken cancellationToken)
//            {
//                var item = await _context
//                        .ShortenedUrls
//                        .Where(s => s.Code == request.Code)
//                        .Select(x => new RedirectUrlResponse
//                        {
//                            LongUrl = x.LongUrl,
//                        })
//                        .FirstOrDefaultAsync(cancellationToken);

//                if (item is null)
//                {
//                    return Result.Failure<RedirectUrlResponse>(new Error(
//                   "RedirectUrlResponse.Null",
//                   "The longUrl with the specified shortUrl was not found"));
//                }

//                return item;
//            }
//        }

//    }

//    public class RedirectUrlEndpoint : ICarterModule
//    {
//        public void AddRoutes(IEndpointRouteBuilder app)
//        {
//            app.MapGet("api/redirect/{shortUrl}", async (string shortUrl, ISender sender) =>
//            {
//                var query = new RedirectUrl.Query { Code = shortUrl };

//                var result = await sender.Send(query);

//                if (result.IsFailure)
//                {
//                    return Results.NotFound(result.Error);
//                }

//                return Results.Ok(result.Value);
//            });
//        }
//    }

//    public class RedirectUrlResponse
//    {
//        public string LongUrl { get; set; } = string.Empty;
//    }
//}
