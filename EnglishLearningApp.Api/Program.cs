using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.User;
using EnglishLearningApp.Repository.Implementations;
using EnglishLearningApp.Repository.Interfaces;
using EnglishLearningApp.Service.Implementations;
using EnglishLearningApp.Service.Interfaces;
using ERSP.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using EnglishLearningApp.Api.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add DbContext with retry on failure for Azure SQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    ));

// Add Identity services
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();
builder.Services.AddScoped<IUserVocabularyRepository, UserVocabularyRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IClassMemberRepository, ClassMemberRepository>();
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

builder.Services.AddHttpClient<GeminiClient>();
builder.Services.AddScoped<ChatNlpService>();
builder.Services.AddScoped<ERSP.Api.Services.ChatBotService>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Email Service: Always use real EmailService with configured SMTP
builder.Services.AddScoped<IEmailService, EmailService>();
Console.WriteLine("📧 Using real EmailService with SMTP configuration.");

builder.Services.AddScoped<IVocabularyService, VocabularyService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
Console.WriteLine("📁 Using Local File Storage for documents.");

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "EnglishLearningApp",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "EnglishLearningApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? "your-secret-key-here-make-it-long-enough"))
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "https://fla.fptschoolsoctrang.edu.vn")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddSwaggerGen(options =>
{
    // Thông tin API
    options.SwaggerDoc("v1", new OpenApiInfo
    {
    Title = "FPT Learnify AI API",
    Version = "v1",
    Description = "API for FPT Learnify AI - AI-Powered English Learning Platform with JWT Authentication"
    });

    // Cấu hình bảo mật JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {token}"
    });

    // Áp dụng bảo mật cho tất cả các endpoint
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
      // Support for file uploads in Swagger
    options.OperationFilter<FileUploadOperationFilter>();
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable static files to serve uploaded documents
app.UseStaticFiles();

// Comment out HTTPS redirection for now
// app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
