using Api.Endpoints;
using Core.Abstractions;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var raw = builder.Configuration.GetConnectionString("AppDb") ?? "Data Source=data/app.db";
var csb = new SqliteConnectionStringBuilder(raw);
if (!Path.IsPathRooted(csb.DataSource))
{
    var full = Path.Combine(builder.Environment.ContentRootPath, csb.DataSource);
    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
    csb.DataSource = full;
}

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(csb.ConnectionString));
builder.Services.AddScoped<ICatalogStore, EfCatalogStore>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment())
    {
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapProducts();

app.Run();