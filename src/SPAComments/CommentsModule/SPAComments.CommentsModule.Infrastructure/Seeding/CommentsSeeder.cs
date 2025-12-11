using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SPAComments.CommentsModule.Application.Events.Integration;
using SPAComments.CommentsModule.Domain;
using SPAComments.CommentsModule.Domain.ValueObjects;
using SPAComments.CommentsModule.Infrastructure.DbContexts;
using SPAComments.CommentsModule.Infrastructure.Search;
using SPAComments.Core.Abstractions;
using SPAComments.SharedKernel.ValueObjects.Ids;

namespace SPAComments.CommentsModule.Infrastructure.Seeding;

public sealed class CommentsSeeder
{
    private readonly CommentsDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CommentsSeeder> _logger;
    private readonly IOptions<CommentsSeedingOptions> _options;
    private readonly ICommentSearchIndexer _searchIndexer;

    public CommentsSeeder(
        CommentsDbContext context,
        IDateTimeProvider dateTimeProvider,
        ILogger<CommentsSeeder> logger,
        IOptions<CommentsSeedingOptions> options,
        ICommentSearchIndexer searchIndexer)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _options = options;
        _searchIndexer = searchIndexer;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        if (!options.EnableSeeding)
        {
            _logger.LogInformation("Comments seeding is disabled in configuration");
            return;
        }

        if (options.ClearCommentsBeforeSeeding)
        {
            _logger.LogInformation("Clearing comments table before seeding");
            _context.Comments.RemoveRange(_context.Comments);
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (options.ClearElasticsearchBeforeSeeding)
        {
            _logger.LogInformation("Clearing Elasticsearch index before seeding");
            await _searchIndexer.ClearAsync(cancellationToken);
        }

        if (await _context.Comments.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Comments already exist. Seeding skipped");
            return;
        }

        var comments = BuildComments();

        await _context.Comments.AddRangeAsync(comments, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await IndexCommentsAsync(comments, cancellationToken);

        _logger.LogInformation("Seeded {Count} comments", comments.Count);
    }

    private List<Comment> BuildComments()
    {
        var now = _dateTimeProvider.UtcNow;
        var random = new Random(2024);

        var sentences = new[]
        {
            "Небольшой отклик, который я писал на бегу, но он сохранил главную мысль.",
            "Планирую переписать этот кусок и добавить пару диаграмм, но пока делюсь как есть.",
            "Коротко: решение работает, но требует аккуратного тюнинга под нагрузку.",
            "Вчера читал книгу о дизайне интерфейсов и поймал несколько инсайтов, делюсь ими тут.",
            "Экспериментировал с бэкендом и нашёл красивый паттерн, который упростил код раза в два.",
            "Если коротко, то да, кэш нужен, но только с чёткой стратегией инвалидации.",
            "Сделал набор заметок: как готовить тестовые данные, как проверять edge cases, как документировать решения.",
            "Тут очень длинная мысль о том, как команды договариваются о приоритетах, распределяют риски и не забывают про пользователей.",
            "Написал небольшой скрипт для миграции, теперь разворачиваем окружение за пару минут.",
            "Самое ценное в этом подходе — что он масштабируется без тяжёлой поддержки.",
            "Быстро сверстал макет, получил фидбек и за один вечер поправил все спорные места.",
            "Главное, не забывать прогонять линтеры и проверять правописание, чтобы не смущать пользователей опечатками.",
            "Добавил в пайплайн шаг с визуальными снапшотами, теперь дизайн не расползается.",
            "Мы выкатили релиз без даунтайма, хотя боялись, что миграции будут долгими.",
            "Люблю, когда код читается как хорошая статья: ясно, структурно и без сюрпризов.",
            "Сделал несколько профилировок и увидел, что узкое место было в неожиданном месте.",
            "Эксперимент с очередями показал, что можно обрабатывать больше задач без роста задержек.",
            "Этот текст специально длиннее, в нём есть и вступление, и примеры, и выводы, чтобы проверить, как интерфейс отображает объёмные комментарии."
        };

        var names = new[]
        {
            "Aurora7", "Borealis88", "Cascade19", "DeltaPilot", "Everest09", "Fjord27",
            "Glider33", "Helios54", "Icarus21", "Juniper11", "Krypton42", "Lumen73",
            "Meridian5", "Nimbus92", "Orchid16", "Pulse40", "Quartz58", "Riverine2",
            "Solstice7", "Tundra64", "Umbra81", "Vortex14", "Willow38", "Xenon67",
            "Yukon25", "Zephyr03"
        };

        var domains = new[] { "example.com", "mail.test", "postbox.dev" };
        var homePages = new[]
        {
            "https://labs.example.com", "https://portfolio.dev/works", "http://tech-journal.ru",
            "https://notes.space", "http://about.me/projects", "https://studio.codes" ,
            "https://quiet-sky.blog"
        };

        var comments = new List<SeedComment>();
        var parents = new List<(SeedComment Comment, int Depth)>();

        for (var i = 0; i < 20; i++)
        {
            var seed = CreateSeedComment(null, 1, i, now, random, sentences, names, domains, homePages);
            comments.Add(seed);
            parents.Add((seed, 1));
        }

        for (var i = 0; i < 60; i++)
        {
            var availableParents = parents.Where(p => p.Depth < 3).ToArray();
            var parent = availableParents[random.Next(availableParents.Length)];
            var seed = CreateSeedComment(parent.Comment.Id, parent.Depth + 1, i + 20, now, random, sentences, names, domains, homePages);
            comments.Add(seed);
            parents.Add((seed, parent.Depth + 1));
        }

        return comments
            .OrderBy(c => c.CreatedAt)
            .Select(ToDomain)
            .ToList();
    }

    private static SeedComment CreateSeedComment(
        Guid? parentId,
        int depth,
        int index,
        DateTime now,
        Random random,
        IReadOnlyList<string> sentences,
        IReadOnlyList<string> names,
        IReadOnlyList<string> domains,
        IReadOnlyList<string> homePages)
    {
        var name = names[(index + depth + random.Next(names.Count)) % names.Count];
        var email = $"{name.ToLower()}@{domains[random.Next(domains.Count)]}";

        var textPartsCount = random.Next(1, 6 + depth);
        var selected = new List<string>();
        for (var i = 0; i < textPartsCount; i++)
        {
            selected.Add(sentences[random.Next(sentences.Count)]);
        }

        var text = string.Join(" ", selected);
        if (text.Length < 80)
        {
            text += " Дополнительно уточнил детали, чтобы комментарий выглядел богаче.";
        }

        var createdAt = now.AddMinutes(-(index * 17 + random.Next(0, 40)));
        var homePage = random.NextDouble() > 0.55 ? homePages[random.Next(homePages.Count)] : null;

        return new SeedComment(
            Guid.NewGuid(),
            parentId,
            name,
            email,
            homePage,
            text,
            createdAt);
    }

    private static Comment ToDomain(SeedComment seed)
    {
        return new Comment(
            CommentId.Create(seed.Id),
            seed.ParentId != null ? CommentId.Create(seed.ParentId.Value) : null,
            UserName.Create(seed.UserName).Value,
            Email.Create(seed.Email).Value,
            seed.HomePage != null ? HomePage.Create(seed.HomePage).Value : null,
            Text.Create(seed.Text).Value,
            seed.CreatedAt,
            Array.Empty<CommentAttachment>());
    }

    private sealed record SeedComment(
        Guid Id,
        Guid? ParentId,
        string UserName,
        string Email,
        string? HomePage,
        string Text,
        DateTime CreatedAt);

    private async Task IndexCommentsAsync(
        IReadOnlyCollection<Comment> comments,
        CancellationToken cancellationToken)
    {
        foreach (var comment in comments)
        {
            var integrationEvent = new CommentCreatedIntegrationEvent(
                comment.Id.Value,
                comment.ParentCommentId?.Value,
                comment.UserName.Value,
                comment.Email.Value,
                comment.HomePage?.Value,
                comment.Text.Value,
                comment.CreatedAt,
                comment.Attachments.Select(a => a.FileId).ToArray());

            try
            {
                await _searchIndexer.IndexAsync(integrationEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to index seeded comment {CommentId}",
                    comment.Id.Value);
            }
        }

        _logger.LogInformation("Indexed {Count} comments into Elasticsearch", comments.Count);
    }
}
