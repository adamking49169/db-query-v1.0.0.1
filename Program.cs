using db_query_v1._0._0._1.Data;
using db_query_v1._0._0._1.Models;
using db_query_v1._0._0._1.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Linq;

namespace db_query_v1._0._0._1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // In Development only, load a local .env file
            if (builder.Environment.IsDevelopment())
            {
                Env.Load();
                builder.Configuration.AddEnvironmentVariables();
            }

            // --- Configure Services ---

            // Azure Computer Vision
            builder.Services.AddSingleton<ComputerVisionClient>(provider =>
            {
                var apiKey = Env.GetString("AZURE_COMPUTER_VISION_API_KEY");
                var endpoint = Env.GetString("AZURE_COMPUTER_VISION_ENDPOINT");
                var credentials = new ApiKeyServiceClientCredentials(apiKey);
                return new ComputerVisionClient(credentials) { Endpoint = endpoint };
            });

            // EF Core SQL Server
            var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(defaultConn));

            // Identity
            builder.Services
                .AddDefaultIdentity<ApplicationUser>(opts => opts.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Scoped services
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<Q>();

            // OCR & ChatGPT
            builder.Services.AddSingleton<OcrService>();
            builder.Services.AddHttpClient<ChatGptService>();
            builder.Services.AddSingleton(sp => new ChatGptService(
                sp.GetRequiredService<HttpClient>(),
                builder.Configuration["OPENAI_API_KEY"]));

            // MVC & API
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1",
                    Description = "Describe your API here"
                });
            });

            // Sessions & Cookies
            builder.Services.AddSession();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // EF Developer Page
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // OpenAI HTTP client
            var openAiKey = builder.Configuration["OPENAI_API_KEY"];
            if (builder.Environment.IsProduction() && string.IsNullOrEmpty(openAiKey))
            {
                throw new InvalidOperationException("OpenAI API Key not found.");
            }
            if (!string.IsNullOrEmpty(openAiKey))
            {
                builder.Services.AddHttpClient("OpenAI", client =>
                {
                    client.BaseAddress = new Uri("https://api.openai.com/");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", openAiKey);
                });
            }
            builder.Services.AddHttpClient();

            var app = builder.Build();

            // Automatically open Swagger on IIS Express
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                string url = null;
                // Kestrel
                var addresses = app.Services.GetService<IServerAddressesFeature>()?.Addresses;
                var address = addresses?.FirstOrDefault();
                if (!string.IsNullOrEmpty(address))
                {
                    url = address.TrimEnd('/') + "/swagger";
                }
                else
                {
                    // IIS Express
                    var port = Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
                    if (!string.IsNullOrEmpty(port))
                    {
                        url = $"https://localhost:{port}/swagger";
                    }
                }
                if (!string.IsNullOrEmpty(url))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    }
                    catch { }
                }
            });

            // Middleware
            app.UseSession();
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            await app.RunAsync();
        }
    }
}
