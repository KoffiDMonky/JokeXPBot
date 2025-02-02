using System;
using System.Threading.Tasks;
using JokeXPBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using dotenv.net;

namespace JokeXPBot
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            // Lancement du serveur pour servir les fichiers statiques
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDirectoryBrowser();
            
            var app = builder.Build();
            app.UseStaticFiles();
            app.UseDirectoryBrowser();

            _ = app.RunAsync(); // Exécute le serveur en arrière-plan

            DotEnv.Load();

            // Exécution du bot
            var bot = new InstagramBot();
            await bot.RunAsync();
        }

    }
}