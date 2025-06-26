using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using KNTCommon.Blazor.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using OxyPlot.Blazor.Services;
using Radzen;
using Blazored.LocalStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using KNTCommon.Business.Scripts;
using KNTCommon.Data.Models;
using KNTCommon.Business.Repositories;
using KNTCommon.Blazor;
using KNTCommon.Business.AutoMapper;

namespace KNTCommon.Business
{
    public static class WebApplicationHelper
    {
        public static void AddCommonAndRepositoryServices(this WebApplicationBuilder builder)
        {
            AddCommon(builder);
            AddAllRepositorys(builder);
        }

        static void AddCommon(WebApplicationBuilder builder)
        {
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
            builder.Services.AddScoped<HelperService>();
            builder.Services.AddScoped<WindowsServiceHelper>();
            builder.Services.AddScoped<PowerShellHelper>();
            builder.Services.AddScoped<PManager>();
            //builder.Services.AddScoped<KNTCommon.Business.Repositories.TablesRepository>();
            //builder.Services.AddScoped<KNTCommon.Business.Repositories.IParametersRepository, KNTCommon.Business.Repositories.ParametersRepository>();
            //builder.Services.AddScoped<KNTCommon.Business.Repositories.IAuthenticationRepository, KNTCommon.Business.Repositories.AuthenticationRepository>();
            //builder.Services.AddScoped<KNTCommon.Business.Repositories.IUsersAndGroupsRepository, KNTCommon.Business.Repositories.UsersAndGroupsRepository>();
            //builder.Services.AddScoped<KNTCommon.Business.Repositories.ITablesRepository, KNTCommon.Business.Repositories.TablesRepository>();

            builder.Services.AddScoped<IEncryption, Encryption>();
            builder.Services.AddSingleton<SharedContainerCommon>();
            builder.Services.AddTransient<IResizeObserverFactory, ResizeObserverFactory>();
            builder.Services.AddScoped<DialogService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<TooltipService>();
            builder.Services.AddScoped<ContextMenuService>();
            builder.Services.AddTransient<TimerService>();
            builder.Services.AddSingleton<Localization>();
            builder.Services.AddSingleton<ResultRepository>();

            var connString = builder.Configuration.GetConnectionString("connString");
            if (connString != null)
            {
                //builder.Services.AddDbContextFactory<EdnKntControllerMysqlContext>(opt => opt.UseMySQL(connString));
                builder.Services.AddDbContextFactory<EdnKntControllerMysqlContext>(opt => opt.UseMySQL(connString));
            }

            builder.Services.AddAutoMapper(typeof(MappingProfiles));

            builder.Services.AddBlazoredLocalStorage(config =>
            {
                config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                config.JsonSerializerOptions.WriteIndented = false;
            });

            builder.Services.AddBlazoredLocalStorageAsSingleton();
            builder.Services.AddScoped<LocalStorageService>();
        }

        static void AddAllRepositorys(WebApplicationBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("KNT"));

            var repositoryTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.Name.EndsWith("Repository") && t.IsInterface)
                .ToList();

            foreach (var repositoryType in repositoryTypes)
            {
                var implementationType = assemblies
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(t => repositoryType.IsAssignableFrom(t) && !t.IsInterface);

                if (implementationType != null)
                {
                    builder.Services.AddScoped(repositoryType, implementationType); // TODO log added scopes
                }
            }
        }
    }
}
