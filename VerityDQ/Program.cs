using Blazored.LocalStorage;
using MudBlazor.Services;
using VerityDQ.Application;
using VerityDQ.Application.Abstractions;
using VerityDQ.Application.Agents;
using VerityDQ.Application.Data;
using VerityDQ.Components;

namespace VerityDQ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddScoped<IDataQualityEngine, DataQualityEngine>();

            builder.Services.AddScoped<IDataQualityRuleProvider>(sp =>
            {
                var environment = sp.GetRequiredService<IWebHostEnvironment>();

                return new JsonDataQualityRuleProvider(
                    Path.Combine(
                        environment.ContentRootPath,
                        "Data",
                        "wellbore-rules.json"));
            });

            builder.Services.AddMudServices();

            

            builder.Services.AddScoped<IDataQualityAssistant, DataQualityAssistant>();
            builder.Services.AddScoped<IWellBoreDataProvider, SqlServerWellBoreDataProvider>();
            builder.Services.AddBlazoredLocalStorage();

            var app = builder.Build();   // moved here, after all registrations

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}