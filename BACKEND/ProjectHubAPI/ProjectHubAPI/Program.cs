using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssembly(
    Assembly.GetExecutingAssembly());

builder.Services.AddSignalR();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
    options.ValueLengthLimit = 100 * 1024 * 1024;
    options.ValueCountLimit = 5000;
});

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        Assembly.GetExecutingAssembly()));

var mapsterConfig = TypeAdapterConfig.GlobalSettings;

mapsterConfig.Scan(Assembly.GetExecutingAssembly());

builder.Services.AddSingleton(mapsterConfig);

builder.Services.AddScoped<IMapper, ServiceMapper>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });

    options.ConfigureWarnings(warnings =>
    {
        warnings.Ignore(
            Microsoft.EntityFrameworkCore.Diagnostics
            .RelationalEventId.PendingModelChangesWarning);
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSection["Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new Exception("JWT Key is missing in appsettings.json");
}

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;

    options.SaveToken = true;

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(key),

            ClockSkew = TimeSpan.Zero
        };
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "ProjectHub API",
            Version = "v1",
            Description = "ProjectHub Backend API"
        });

    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter JWT Token using Bearer scheme"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:4200",
                    "https://localhost:4200"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddDirectoryBrowser();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context =
            services.GetRequiredService<AppDbContext>();

        context.Database.Migrate();

        DbInitializer.Seed(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database Error: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "ProjectHub API V1");
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<ProjectHubAPI.Middleware.ExceptionMiddleware>();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseStaticFiles();

var uploadsPath = Path.Combine(
    builder.Environment.WebRootPath,
    "uploads");

if (Directory.Exists(uploadsPath))
{
    app.UseDirectoryBrowser(new DirectoryBrowserOptions
    {
        FileProvider =
            new PhysicalFileProvider(uploadsPath),

        RequestPath = "/uploads"
    });
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chathub");

app.Run();
