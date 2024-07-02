using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using UrlShortener.ApiService.Domain;
using UrlShortener.ApiService.Infrastructure.Database;
using UrlShortener.ApiService.Shared;

namespace UrlShortener.ApiService.Features
{
    public class ShortenUrl
    {
        public class Command : IRequest<Result<string>>
        {
            public string LongUrl { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.LongUrl).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<string>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;
            public const int NumberOfCharsInShortLink = 7;
            private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            private readonly Random _random = new();

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<Result<string>> Handle(Command request,  CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return Result.Failure<string>(new Error(
                        "ShortenUrl.Validation",
                        validationResult.ToString()));
                }

                var code = await GenerateUniqueCode();

                var shortenedUrl = new ShortenedUrl
                {
                    Id = Guid.NewGuid(),
                    LongUrl = request.LongUrl,
                    Code = code,
                    //ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/{code}",
                    CreatedOnUtc = DateTime.UtcNow
                };

                _dbContext.ShortenedUrls.Add(shortenedUrl);

                await _dbContext.SaveChangesAsync();

                return shortenedUrl.ShortUrl;
            }

            private async Task<string> GenerateUniqueCode()
            {
                var codeChars = new char[NumberOfCharsInShortLink];

                while (true)
                {
                    for (var i = 0; i < NumberOfCharsInShortLink; i++)
                    {
                        var randomIndex = _random.Next(Alphabet.Length - 1);

                        codeChars[i] = Alphabet[randomIndex];
                    }

                    var code = new string(codeChars);

                    if (!await _dbContext.ShortenedUrls.AnyAsync(s => s.Code == code))
                    {
                        return code;
                    }
                }
            }
        }
    }

    public class ShortenUrlEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/articles", async (ShortenUrl.Command request, ISender sender) =>
            {
                var result = await sender.Send(request);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Ok(result.Value);
            });
        }
    }

}
