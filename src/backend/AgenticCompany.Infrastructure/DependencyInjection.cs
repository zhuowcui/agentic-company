using AgenticCompany.Core.Interfaces;
using AgenticCompany.Infrastructure.Agents;
using AgenticCompany.Infrastructure.Data;
using AgenticCompany.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticCompany.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            ));

        // Repositories
        services.AddScoped<INodeRepository, NodeRepository>();
        services.AddScoped<IPrincipleRepository, PrincipleRepository>();
        services.AddScoped<ISpecRepository, SpecRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ITaskItemRepository, TaskItemRepository>();

        // Agent providers
        services.AddSingleton<IAgentProvider, EchoAgentProvider>();
        services.AddSingleton<IAgentProvider, OpenAiAgentProvider>();
        services.AddSingleton<IAgentProvider, ClaudeAgentProvider>();
        services.AddSingleton<IAgentService, AgentService>();

        return services;
    }
}
