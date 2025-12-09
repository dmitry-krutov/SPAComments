using FileService.Communication;
using SPAComments.CaptchaModule.Infrastructure;
using SPAComments.CaptchaModule.Presentation;
using SPAComments.CommentsModule.Application;
using SPAComments.CommentsModule.Infrastructure;
using SPAComments.CommentsModule.Presentation;
using SPAComments.CommentsModule.Presentation.Hubs;
using SPAComments.Core.Mappings;
using SPAComments.Framework.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
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

app.UseCors("AllowAll");

app.MapControllers();
app.MapCaptchaEndpoints();
app.MapHub<CommentsHub>("/hubs/comments").RequireCors("SignalRPolicy");
;

app.UseCors(policy =>
    policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
);

app.Run();