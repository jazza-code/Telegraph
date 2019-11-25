using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using AR.Telegraph.Areas.Identity.Models;
using AR.Telegraph.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(AR.Telegraph.Areas.Identity.IdentityHostingStartup))]
namespace AR.Telegraph.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            if(builder != null)
            {
                builder.ConfigureServices((context, services) => {
                    services.AddDbContext<IdentityContext>(options =>
                        options.UseSqlServer(
                            context.Configuration.GetConnectionString("IdentityContextConnection")));

                    services.AddIdentity<UserData, IdentityRole>(options => { options.SignIn.RequireConfirmedAccount = false; })
                        .AddRoles<IdentityRole>()
                        .AddDefaultUI()
                        .AddEntityFrameworkStores<IdentityContext>()
                        .AddDefaultTokenProviders();
                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("RequireAdministratorRole",
                             policy => policy.RequireRole("Administrator"));
                    });
                    services.AddMvc()
                        .AddRazorPagesOptions(options =>
                        {
                            options.Conventions.AuthorizeAreaFolder("Identity", $"/Account/Manage");
                            options.Conventions.AuthorizeAreaFolder("Identity", $"/Admin", "RequireAdministratorRole");
                            options.Conventions.AuthorizeAreaPage("Identity", $"/Logout");
                        });

                    services.ConfigureApplicationCookie(options =>
                    {
                        options.LoginPath = $"/Login";
                        options.LogoutPath = $"/Logout";
                        options.AccessDeniedPath = $"/AccessDenied";
                    });
                    services.AddAuthentication()
                    .AddGoogle(options =>
                    {
                        IConfigurationSection googleAuthNSection = context.Configuration.GetSection("Authentication:Google");

                        options.ClientId = googleAuthNSection["ClientId"];
                        options.ClientSecret = googleAuthNSection["ClientSecret"];
                        options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
                        options.SaveTokens.Equals(true);
                        options.Events.OnCreatingTicket = ctx =>
                        {
                            List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();
                            tokens.Add(new AuthenticationToken()
                            {
                                Name = "TicketCreated",
                                Value = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture)
                            });
                            ctx.Properties.StoreTokens(tokens);
                            return Task.CompletedTask;
                        };
                    })
                    .AddFacebook(options =>
                    {
                        IConfigurationSection facebookAuthNSection = context.Configuration.GetSection("Authentication:Facebook");
                        options.AppId = facebookAuthNSection["AppId"];
                        options.AppSecret = facebookAuthNSection["AppSecret"];
                        options.ClaimActions.MapJsonKey("urn:facebook:picture", "user_photos", "url");
                        options.SaveTokens.Equals(true);
                        options.Events.OnCreatingTicket = ctx =>
                        {
                            List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();
                            tokens.Add(new AuthenticationToken()
                            {
                                Name = "TicketCreated",
                                Value = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture)
                            });
                            ctx.Properties.StoreTokens(tokens);
                            return Task.CompletedTask;
                        };
                    })
                    .AddTwitter(options =>
                    {
                        IConfigurationSection twitterAuthNSection = context.Configuration.GetSection("Authentication:Twitter");
                        options.ConsumerKey = twitterAuthNSection["ConsumerAPIKey"];
                        options.ConsumerSecret = twitterAuthNSection["ConsumerSecret"];
                        options.SaveTokens.Equals(true);
                        options.Events.OnCreatingTicket = ctx =>
                        {
                            List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();
                            tokens.Add(new AuthenticationToken()
                            {
                                Name = "TicketCreated",
                                Value = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture)
                            });
                            ctx.Properties.StoreTokens(tokens);
                            return Task.CompletedTask;
                        };
                    });
                    // using Microsoft.AspNetCore.Identity.UI.Services;
                    services.AddSingleton<IEmailSender, EmailSender>();
                });
            }
        }
    }
}