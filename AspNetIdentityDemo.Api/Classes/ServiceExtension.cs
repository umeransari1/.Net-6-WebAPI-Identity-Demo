using AspNetIdentityDemo.Api.Model;
using AspNetIdentityDemo.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace AspNetIdentityDemo.Api.Classes
{
    public static class ServiceExtension
    {
        public static void ConfigureIdentity(this WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = Convert.ToBoolean(builder.Configuration["IdentitySettings:RequireDigit"]);
                options.Password.RequireLowercase = Convert.ToBoolean(builder.Configuration["IdentitySettings:RequireLowercase"]);
                options.Password.RequiredLength = Convert.ToInt32(builder.Configuration["IdentitySettings:RequiredLength"]);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = Convert.ToBoolean(builder.Configuration["AuthSettings:ValidateIssuer"]),
                    ValidateAudience = Convert.ToBoolean(builder.Configuration["AuthSettings:ValidateAudience"]),
                    RequireExpirationTime = Convert.ToBoolean(builder.Configuration["AuthSettings:RequireExpirationTime"]),
                    ValidAudience = builder.Configuration["AuthSettings:ValidAudience"],
                    ValidIssuer = builder.Configuration["AuthSettings:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthSettings:IssuerSigningKey"])),
                    ValidateIssuerSigningKey = Convert.ToBoolean(builder.Configuration["AuthSettings:ValidateIssuerSigningKey"])
                };
            });
        }

        public static void ConfigureExceptionHandler (this IApplicationBuilder app)
        {
            app.UseExceptionHandler(error =>
            {
                error.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if(contextFeature != null)
                    {
                        Log.Error($"Something went Wrong in the { contextFeature.Error }");

                        await context.Response.WriteAsync(new ErrorHandlerModel
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "Internal Server Error. Please Try Again Later"
                        }.ToString());
                    }
                });
            });
        }
    }
}
