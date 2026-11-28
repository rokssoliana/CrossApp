using System.Text.Json;
using Api.Endpoints;
using Core;
using Core.Abstractions;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

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

var webOrigin = builder.Configuration["Cors:WebOrigin"] ?? "http://localhost:5090";
builder.Services.AddCors(o => o.AddPolicy("web", p =>
    p.WithOrigins(webOrigin)
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is DomainException domainEx)
        {
            await Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Domain validation error",
                detail: domainEx.Message).ExecuteAsync(context);
            return;
        }

        if (exception is DbUpdateException dbEx &&
            dbEx.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
        {
            await Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Duplicate item",
                detail: "Товар із таким артикулом вже існує.").ExecuteAsync(context);
            return;
        }

        await Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal server error").ExecuteAsync(context);
    });
});

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("web");

app.MapHealthChecks("/health");
app.MapProducts();

app.Run();
