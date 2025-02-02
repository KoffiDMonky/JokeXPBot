using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using JokeXPBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SkiaSharp; // Pour la génération d'images
public class InstagramBot
{
    private readonly HttpClient _httpClient;
    private readonly string AccessToken;
    private readonly string InstagramAccountId;

    public InstagramBot()
    {
        // Charger les variables d'environnement
        AccessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN")
                      ?? throw new InvalidOperationException("ACCESS_TOKEN est manquant dans les variables d'environnement.");

        InstagramAccountId = Environment.GetEnvironmentVariable("INSTAGRAM_ACCOUNT_ID")
                             ?? throw new InvalidOperationException("INSTAGRAM_ACCOUNT_ID est manquant dans les variables d'environnement.");

        _httpClient = new HttpClient();
    }

    public async Task RunAsync()
    {
        var random = new Random();
        var jokeService = new JokeService();
        var imageService = new ImageService();

        while (true) // Boucle infinie
        {
            DateTime now = DateTime.Now;

            // ✅ Vérifier s'il est entre 1h et 6h du matin
            if (now.Hour >= 1 && now.Hour < 6)
            {
                Console.WriteLine("\n🌙 Pause nocturne... Aucune publication entre 1h et 6h.");
                TimeSpan waitTime = DateTime.Today.AddHours(6) - now;
                if (waitTime.TotalSeconds < 0)
                {
                    waitTime = TimeSpan.FromHours(6); // Sécurité : si on passe minuit, on attend 6h.
                }

                Console.WriteLine($"🕒 Attente jusqu'à 6h00... ({waitTime.TotalHours:F1} heures restantes)");
                await Task.Delay(waitTime);
                continue; // Recommencer la boucle après la pause
            }

            try
            {
                Console.WriteLine("\n--- 📢 Nouvelle publication en cours ---");

                // ✅ 1. Génération de la blague
                string joke = await jokeService.GetJokeAsync();

                // ✅ 2. Génération de l'image
                string imagePath = imageService.GenerateImageForInstagram(joke);

                // ✅ 3. Génération des hashtags aléatoires
                string[] allHashtags = { "#blague", "#humour", "#fun", "#rire", "#détente", "#joke", "#lol", "#comédie", "#drôle", "#hilarant", "#divertissement" };
                int numberOfHashtags = random.Next(5, 10);
                string[] selectedHashtags = allHashtags.OrderBy(x => random.Next()).Take(numberOfHashtags).ToArray();
                string randomHashtagString = string.Join(" ", selectedHashtags);
                string caption = $"Voici une blague pour te faire sourire : {joke} !\n\n #JokeXP #JokeDePapa {randomHashtagString}";

                // ✅ 4. Création de l'URL publique de l'image
                string publicUrl = $"http://jokexpbot.agenorhouessou.fr/images/{Path.GetFileName(imagePath)}";
                Console.WriteLine($"🌍 URL publique : {publicUrl}");

                // ✅ 5. Publication sur Instagram
                await PublishToInstagramAsync(publicUrl, caption);

                Console.WriteLine("✅ Publication réussie !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de la publication : {ex.Message}");
            }

            // ✅ 6. Calcul du délai aléatoire avant la prochaine publication (55 à 65 minutes)
            int delayMinutes = random.Next(150, 180);
            Console.WriteLine($"\n⏳ Prochaine publication dans {delayMinutes} minutes...");

            // ✅ 7. Décompte avant la prochaine publication
            for (int i = delayMinutes; i > 0; i--)
            {
                Console.Write($"\r🕒 Attente : {i} minute(s) restante(s)...  ");
                await Task.Delay(TimeSpan.FromMinutes(1)); // Attente de 1 minute
            }

            Console.WriteLine("\n⏰ C'est parti pour une nouvelle blague !");
        }
    }


    private async Task PublishToInstagramAsync(string imageUrl, string caption)
    {
        Console.WriteLine("Publication sur Instagram...");

        try
        {
            // Étape 1 : Télécharger l'image sur Instagram
            var uploadResponse = await _httpClient.PostAsync(
                $"https://graph.instagram.com/v21.0/{InstagramAccountId}/media",
                new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("image_url", imageUrl),
                new KeyValuePair<string, string>("caption", caption),
                new KeyValuePair<string, string>("access_token", AccessToken)
                }));

            // 🔍 Lire la réponse en cas d'échec
            string uploadResult = await uploadResponse.Content.ReadAsStringAsync();
            if (!uploadResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Erreur de l'API (upload) : {uploadResponse.StatusCode} - {uploadResult}");
                return;
            }

            Console.WriteLine($"✅ Réponse de l'upload : {uploadResult}");

            var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(uploadResult);
            if (!jsonResponse.TryGetValue("id", out string creationId))
            {
                throw new Exception("Impossible de récupérer 'creation_id' à partir de la réponse de l'upload.");
            }

            // 🏆 **Étape 2 : Publier l'image avec un système de retry**
            int maxRetries = 5; // 🔄 Nombre de tentatives avant d'abandonner
            int waitTimeSeconds = 10; // ⏳ Temps d'attente entre chaque essai

            for (int i = 0; i < maxRetries; i++)
            {
                Console.WriteLine($"⏳ Tentative {i + 1}/{maxRetries} de publication...");

                var publishResponse = await _httpClient.PostAsync(
                    $"https://graph.instagram.com/v21.0/{InstagramAccountId}/media_publish",
                    new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("creation_id", creationId),
                    new KeyValuePair<string, string>("access_token", AccessToken)
                    }));

                string publishResult = await publishResponse.Content.ReadAsStringAsync();

                if (publishResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Publication réussie sur Instagram !");
                    return;
                }
                else
                {
                    Console.WriteLine($"⚠️ Erreur de l'API (publish) : {publishResponse.StatusCode} - {publishResult}");

                    // 🔍 Vérifie si l'erreur demande d'attendre avant de republier
                    if (publishResult.Contains("2207027")) // Code "Le contenu n'est pas prêt"
                    {
                        Console.WriteLine($"🕰️ Attente de {waitTimeSeconds} secondes avant de réessayer...");
                        await Task.Delay(TimeSpan.FromSeconds(waitTimeSeconds));
                    }
                    else
                    {
                        Console.WriteLine("❌ Erreur critique, arrêt de la tentative de publication.");
                        break;
                    }
                }
            }

            Console.WriteLine("🚨 Échec final : Impossible de publier après plusieurs tentatives.");

            //     var publishResponse = await _httpClient.PostAsync(
            // $"https://graph.instagram.com/v21.0/{InstagramAccountId}/media_publish",
            // new FormUrlEncodedContent(new[]
            // {
            //         new KeyValuePair<string, string>("creation_id", creationId),
            //         new KeyValuePair<string, string>("access_token", AccessToken)
            // }));

            //     if (!publishResponse.IsSuccessStatusCode)
            //     {
            //         string errorContent = await publishResponse.Content.ReadAsStringAsync();
            //         Console.WriteLine($"Erreur de l'API (publish) : {publishResponse.StatusCode} - {errorContent}");
            //         return;
            //     }

            //     Console.WriteLine("Publication réussie sur Instagram !");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur inattendue : {ex.Message}");
        }
    }
}
