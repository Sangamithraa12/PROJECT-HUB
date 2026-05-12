using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectHubAPI.Data;
using ProjectHubAPI.Services;
using System.Text;
using Mapster;
using MapsterMapper;
using ProjectHubAPI.Mapping;
using ProjectHubAPI.Hubs;
using System;
using FluentValidation.AspNetCore;
using FluentValidation;
using ProjectHubAPI.Interfaces;
using ProjectHubAPI.Repositories;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddSignalR();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524288000;
    options.ValueCountLimit = 50000;
    options.ValueLengthLimit = 524288000;
});

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Mapster Configuration
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(Program).Assembly);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();
builder.Services.AddDirectoryBrowser();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure());
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectHub API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter your token below."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        // Patch: Fix missing 'TargetRole' column which was added to model but missed in migrations
        context.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Courses]') AND name = 'TargetRole') ALTER TABLE [Courses] ADD [TargetRole] nvarchar(max) NULL;");
        
        // Patch: Fix missing 'Enrollment' columns
        context.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'IsMandatory') ALTER TABLE [Enrollments] ADD [IsMandatory] bit NOT NULL DEFAULT 0;");
        context.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'AssignedById') ALTER TABLE [Enrollments] ADD [AssignedById] int NULL;");
        context.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'DueDate') ALTER TABLE [Enrollments] ADD [DueDate] datetime2 NULL;");
        
        // Patch: Create Messages table manually with File support
        context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Messages')
            CREATE TABLE [Messages] (
                [Id] int NOT NULL IDENTITY,
                [SenderId] int NOT NULL,
                [ReceiverId] int NOT NULL,
                [Content] nvarchar(max) NULL,
                [FileUrl] nvarchar(max) NULL,
                [FileType] nvarchar(max) NULL,
                [SentAt] datetime2 NOT NULL,
                [IsRead] bit NOT NULL,
                CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_Messages_Users_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [Users] ([Id]),
                CONSTRAINT [FK_Messages_Users_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Users] ([Id])
            );
            ELSE
            BEGIN
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Messages]') AND name = 'FileUrl')
                    ALTER TABLE [Messages] ADD [FileUrl] nvarchar(max) NULL;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Messages]') AND name = 'FileType')
                    ALTER TABLE [Messages] ADD [FileType] nvarchar(max) NULL;
            END
        ");

        DbInitializer.Seed(context);
    }
    catch (Exception)
    {
    }
}

app.UseCors("AllowAll");

app.UseCors("AllowAll");

app.UseMiddleware<ProjectHubAPI.Middleware.ExceptionMiddleware>();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectHub API V1");
});

app.UseRouting();

app.UseCors("AllowAll");

app.UseStaticFiles();

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();

