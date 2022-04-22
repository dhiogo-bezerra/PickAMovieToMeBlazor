using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace SuggestionAppUI
{
    public static class RegisterServices
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor().AddMicrosoftIdentityConsentHandler();
            builder.Services.AddMemoryCache();
            builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C")) ;

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.RequireClaim("jobTitle", allowedValues: "Admin");
                });
            });

            builder.Services.AddSingleton<IDbConnection, DbConnection>();

            builder.Services.AddSingleton<ICategoryData, MongoCategoryData>();
            builder.Services.AddSingleton<IStatusData, MongoStatusData>();
            builder.Services.AddSingleton<IUserData, MongoUserData>();
            builder.Services.AddSingleton<ISuggestionData, MongoSuggestionData>();
            builder.Services.AddSingleton<IMovieListData, MongoMovieListData>();
            builder.Services.AddSingleton<IMovieDbData, MongoMovieDb>();
            builder.Services.AddSingleton<ApiMovie>();
            builder.Services.AddSweetAlert2();
        }
    }
}
