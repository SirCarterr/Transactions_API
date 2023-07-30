using AspNetCore.Authentication.ApiKey;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Transactions_API.Helper;
using Transactions_API.Service;
using Transactions_API.Service.IService;
using Transactions_DataAccess.Data;

namespace Transactions_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddScoped<IDbService, DbService>();
            builder.Services.AddScoped<IFileManagerService, FIleManagerService>();

            builder.Services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
                .AddApiKeyInHeader<ApiKeyProvider>(options =>
                {
                    options.KeyName = "API-Key";
                    options.SuppressWWWAuthenticateHeader = true;
                });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Transactions API",
                    Description = "An ASP.Net Core Web API for importing exel, exporting csv files with trasnsactions data. As well as updating existing data about transactions in database."
                });

                options.AddSecurityDefinition("API-Key", new OpenApiSecurityScheme
                {
                    Description = "API-Key must appear in header",
                    Type = SecuritySchemeType.ApiKey,
                    Name = "API-Key",
                    In = ParameterLocation.Header,
                    Scheme = "ApiKeyScheme"
                });

                var key = new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "API-Key"
                    },
                    In = ParameterLocation.Header
                };

                var requirement = new OpenApiSecurityRequirement
                {
                    { key, new List<string>() }
                };
                options.AddSecurityRequirement(requirement);

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            builder.Services.AddRouting(opt => opt.LowercaseUrls = true);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}