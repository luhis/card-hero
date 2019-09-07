﻿using CardHero.Data.Abstractions;
using CardHero.Data.SqlServer;
using CardHero.Data.SqlServer.EntityFramework;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCardHeroDataSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCardHeroDataSqlServerDbContext(configuration);

            services.AddCardHeroDataSqlServerMappers();

            services.AddCardHeroDataSqlServerRepositories();

            return services;
        }

        private static IServiceCollection AddCardHeroDataSqlServerDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CardHeroSqlServerConnection");
            var options = new CardHeroDataDbOptions
            {
                ConnectionString = connectionString,
            };

            services.AddScoped(x => options);

            services
                .AddScoped<ICardHeroDataDbContextFactory, CardHeroDataDbContextFactory>()
            ;

            return services;
        }

        private static IServiceCollection AddCardHeroDataSqlServerMappers(this IServiceCollection services)
        {
            services
                .AddScoped<IMapper<Game, GameData>, GameMapper>()
            ;

            return services;
        }

        private static IServiceCollection AddCardHeroDataSqlServerRepositories(this IServiceCollection services)
        {
            services
                .AddScoped<IGameRepository, GameRepository>()
            ;

            return services;
        }
    }
}