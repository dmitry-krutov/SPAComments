using FileService;
using FileService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRepositories(builder.Configuration)
    .AddStorage(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapEndpoints();

app.Run();