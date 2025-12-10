using FileService.Communication;
using Microsoft.EntityFrameworkCore;
using SPAComments.CaptchaModule.Infrastructure;
using SPAComments.CaptchaModule.Presentation;
using SPAComments.CommentsModule.Application;
using SPAComments.CommentsModule.Infrastructure;
using SPAComments.CommentsModule.Infrastructure.DbContexts;
using SPAComments.CommentsModule.Infrastructure.Seeding;
using SPAComments.CommentsModule.Presentation;
using SPAComments.CommentsModule.Presentation.Hubs;
using SPAComments.Core.Mappings;
using SPAComments.Framework.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddCommentsModuleInfrastructure(builder.Configuration)
    .AddCommentsModuleApplication(builder.Configuration)
    .AddCommentsModulePresentation()
    .AddCaptchaModule(builder.Configuration);

builder.Services.AddFileServiceClient(builder.Configuration);

builder.Services.AddAutoMapper(cfg => { }, typeof(AssemblyMappingProfile).Assembly);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "spa-comments:";
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await ApplyMigrationsAsync(app);
await SeedCommentsAsync(app.Services);

app.UseExceptionMiddleware();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("DefaultCors");

app.MapControllers();
app.MapCaptchaEndpoints();
app.MapHub<CommentsHub>("/hubs/comments").RequireCors("SignalRPolicy");

app.Run();

async Task ApplyMigrationsAsync(WebApplication app)
{
    if (!app.Environment.IsEnvironment("Docker"))
    {
        return;
    }

    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CommentsDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying migrations.");
        throw;
    }
}

async Task SeedCommentsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<CommentsSeeder>();
    await seeder.SeedAsync();
}