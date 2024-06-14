using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using UrlShortener.ApiService.Domain;
using UrlShortener.ApiService.Infrastructure.Database;

namespace UrlShortener.ApiService.Features
{
    public class ShortenUrlQuery : IRequest<string>
    {
        public string LongUrl { get; set; }
    }


    internal sealed class ShortenUrlQueryHandler :
        IRequestHandler<ShortenUrlQuery, string>
    {
        private readonly ApplicationDbContext _context;
        public const int NumberOfCharsInShortLink = 7;
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private readonly Random _random = new();

        public ShortenUrlQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(ShortenUrlQuery request, CancellationToken cancellationToken)
        {
            if (!Uri.TryCreate(request.LongUrl, UriKind.Absolute, out _))
            {
                throw new Exception("The specified URL is invalid.");
                //return Results.BadRequest("The specified URL is invalid.");
            }

            var code = await GenerateUniqueCode();

            var shortenedUrl = new ShortenedUrl
            {
                Id = Guid.NewGuid(),
                LongUrl = request.LongUrl,
                Code = code,
                //ShortUrl = $"{_context.Request.Scheme}://{_context.Request.Host}/api/{code}",
                CreatedOnUtc = DateTime.UtcNow
            };

            _context.ShortenedUrls.Add(shortenedUrl);

            await _context.SaveChangesAsync();

            return shortenedUrl.ShortUrl;
        }

        public async Task<string> GenerateUniqueCode()
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

                if (!await _context.ShortenedUrls.AnyAsync(s => s.Code == code))
                {
                    return code;
                }
            }
        }
    }

    public class ShortenUrlEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/shorten/{longUrl}", async (String longUrl, ISender sender) =>
            {
                var query = new ShortenUrlQuery { LongUrl = longUrl };
                var result = await sender.Send(query);

                return Results.Ok(result);
            });
        }
    }
}
