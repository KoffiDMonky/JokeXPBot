using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JokeXPBot.Services
{
    public class JokeService
    {
        private static readonly HttpClient Client = new HttpClient();

        public async Task<string> GetJokeAsync()
        {
            var url = "https://v2.jokeapi.dev/joke/Any?lang=fr&blacklistFlags=religious,political,racist,sexist&safe-mode";

            try
            {
                var response = await Client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var jokeResponse = JsonConvert.DeserializeObject<JokeResponse>(json);

                    return jokeResponse?.Setup != null
                        ? $"{jokeResponse.Setup} {jokeResponse.Delivery}"
                        : jokeResponse?.Joke ?? "Pas de blague trouvée.";
                }
                else
                {
                    return $"Erreur : {response.StatusCode}";
                }
            }
            catch (HttpRequestException e)
            {
                return $"Erreur lors de la récupération de la blague : {e.Message}";
            }
        }

        private class JokeResponse
        {
            public string? Setup { get; set; }
            public string? Delivery { get; set; }
            public string? Joke { get; set; }
        }
    }
}