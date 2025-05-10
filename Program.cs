using db_query_v1._0._0._1.Data;
using db_query_v1._0._0._1.Models;
using db_query_v1._0._0._1.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

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
                Env.Load();  // Load environment variables from .env
                builder.Configuration.AddEnvironmentVariables();  // Add environment variables to the configuration
            }

            // --- Configure Services ---

            builder.Services.AddSingleton<ComputerVisionClient>(provider =>
            {
                var apiKey = Env.GetString("AZURE_COMPUTER_VISION_API_KEY");
                var endpoint = Env.GetString("AZURE_COMPUTER_VISION_ENDPOINT");
                var credentials = new ApiKeyServiceClientCredentials(apiKey);
                return new ComputerVisionClient(credentials) { Endpoint = endpoint };
            });

            // EF Core SQL Server
            var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found. " +
                    "Please configure it under Azure → App Service → Configuration → Connection strings."
                );

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(defaultConn));

            // Identity
            builder.Services
                .AddDefaultIdentity<ApplicationUser>(opts => opts.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Your scoped services
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<Q>();

            // MVC + Razor
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // Sessions & cookies
            builder.Services.AddSession();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Developer exception page for EF
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // --- Configure OpenAI HTTP client via API key ---
            var openAiKey = builder.Configuration["OPENAI_API_KEY"];

            // In Production, ensure the key is present
            if (builder.Environment.IsProduction() && string.IsNullOrEmpty(openAiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API Key not found. " +
                    "Please configure 'OPENAI_API_KEY' under Azure → App Service → Configuration → Application settings."
                );
            }

            // Only register OpenAI client if a key is available
            if (!string.IsNullOrEmpty(openAiKey))
            {
                builder.Services.AddHttpClient("OpenAI", client =>
                {
                    client.BaseAddress = new Uri("https://api.openai.com/");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", openAiKey);
                });
            }

            // You can still add other HttpClients if needed
            builder.Services.AddHttpClient();

            // Build pipeline
            var app = builder.Build();

            // --- Middleware Pipeline ---
            app.UseSession();

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
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

            // Routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            await app.RunAsync();
        }
    }
}
