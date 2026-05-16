using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using ProjectHubAPI.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectHubAPI.Data;
using ProjectHubAPI.Hubs;
using ProjectHubAPI.Interfaces;
using ProjectHubAPI.Repositories;
using ProjectHubAPI.Services;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.ConfigureCors();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureApplicationServices();
builder.Services.ConfigureLibraries();
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.ConfigureSwagger();

builder.Services.AddSignalR();
builder.Services.AddDirectoryBrowser();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
    options.ValueLengthLimit = 100 * 1024 * 1024;
    options.ValueCountLimit = 5000;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // context.Database.Migrate(); // Uncomment if you want automatic migrations
        DbInitializer.Seed(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database Seeding Error: {ex.Message}");
    }
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectHub API V1");
    });
}

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseMiddleware<ProjectHubAPI.Common.Exceptions.ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

var uploadsPath = Path.Combine(builder.Environment.WebRootPath, "uploads");
if (Directory.Exists(uploadsPath))
{
    app.UseDirectoryBrowser(new DirectoryBrowserOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });
}

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();

 
