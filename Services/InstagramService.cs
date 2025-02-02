// using System.Net.Http;
// using System.Text.Json;
// using System.Text.Json.Serialization;
// using System.Threading.Tasks;

// namespace JokeXPBot.Services
// {
//     public class InstagramService
//     {
//         private readonly HttpClient _httpClient;

//         public InstagramService()
//         {
//             _httpClient = new HttpClient();
//         }

//         public async Task<string> StartUploadSessionAsync(string fileName, long fileLength, string fileType, string accessToken, string appId)
//         {
//             var url = $"https://graph.facebook.com/v22.0/{appId}/uploads";
//             var requestContent = new MultipartFormDataContent
//             {
//                 { new StringContent(fileName), "file_name" },
//                 { new StringContent(fileLength.ToString()), "file_length" },
//                 { new StringContent(fileType), "file_type" },
//                 { new StringContent(accessToken), "access_token" }
//             };

//             Console.WriteLine($"StartUploadSessionAsync");
//             Console.WriteLine($"URL : {url}");


//             var response = await _httpClient.PostAsync(url, requestContent);
//             response.EnsureSuccessStatusCode();

//             var responseBody = await response.Content.ReadAsStringAsync();
//             var json = JsonSerializer.Deserialize<UploadSessionResponse>(responseBody);


//             if (json?.Id != null)
//             {
//                 var cleanedId = ExtractUploadId(json.Id); // Appel de la méthode ExtractUploadId
//                 Console.WriteLine($"ID de session nettoyé : {cleanedId}");
//                 return cleanedId;
//             }

//             return string.Empty;
//         }

//         private class UploadSessionResponse
//         {
//             [JsonPropertyName("id")]
//             public string? Id { get; set; }
//         }

//         public async Task<string> UploadImageChunkAsync(string imagePath, string uploadSessionId, string accessToken)
//         {
//             var url = $"https://graph.facebook.com/v22.0/{uploadSessionId}";
//             var fileBytes = await File.ReadAllBytesAsync(imagePath);
//             var requestContent = new MultipartFormDataContent
//             {
//                 { new ByteArrayContent(fileBytes), "source", Path.GetFileName(imagePath) },
//                 { new StringContent("0"), "file_offset" }, // Commence au début
//                 { new StringContent(accessToken), "access_token" }
//             };

//             Console.WriteLine($"URL : {url}");

//             var response = await _httpClient.PostAsync(url, requestContent);
//             response.EnsureSuccessStatusCode();

//             var responseBody = await response.Content.ReadAsStringAsync();

//             Console.WriteLine("Statut HTTP : " + response.StatusCode);
//             Console.WriteLine("Réponse complète : " + responseBody);

//             if (!response.IsSuccessStatusCode)
//             {
//                 throw new Exception($"Échec de l'upload : {responseBody}");
//             }
//             var json = JsonSerializer.Deserialize<FileUploadResponse>(responseBody);

//             return json?.Handle ?? string.Empty;
//         }

//         private class FileUploadResponse
//         {
//             public string? Handle { get; set; }
//         }


//         public async Task<string> PublishImageOnInstagramAsync(string imageHandle, string caption, string accessToken, string instagramBusinessAccountId)
//         {
//             var url = $"https://graph.facebook.com/v22.0/{instagramBusinessAccountId}/media";
//             var requestContent = new FormUrlEncodedContent(new[]
//             {
//                 new KeyValuePair<string, string>("image_handle", imageHandle),
//                 new KeyValuePair<string, string>("caption", caption),
//                 new KeyValuePair<string, string>("access_token", accessToken)
//             });

//             var response = await _httpClient.PostAsync(url, requestContent);
//             response.EnsureSuccessStatusCode();

//             var responseBody = await response.Content.ReadAsStringAsync();
//             Console.WriteLine("Réponse de l'API publish insta : " + responseBody);

//             var json = JsonSerializer.Deserialize<MediaCreationResponse>(responseBody);

//             return json?.Id ?? string.Empty;
//         }

//         private class MediaCreationResponse
//         {
//             public string? Id { get; set; }
//         }

//         public async Task<string> PublishMediaAsync(string creationId, string accessToken, string instagramBusinessAccountId)
//         {
//             var url = $"https://graph.facebook.com/v22.0/{instagramBusinessAccountId}/media_publish";
//             var requestContent = new FormUrlEncodedContent(new[]
//             {
//                 new KeyValuePair<string, string>("creation_id", creationId),
//                 new KeyValuePair<string, string>("access_token", accessToken)
//             });

//             var response = await _httpClient.PostAsync(url, requestContent);
//             response.EnsureSuccessStatusCode();

//             return "Publication réussie sur Instagram !";
//         }

//         private string ExtractUploadId(string rawId)
//         {
//             if (rawId.StartsWith("upload:"))
//             {
//                 // Supprime le préfixe "upload:"
//                 var cleanId = rawId.Substring("upload:".Length);
//                 // Supprime les éventuels paramètres (tout après le ?)
//                 var queryIndex = cleanId.IndexOf('?');
//                 if (queryIndex > 0)
//                 {
//                     cleanId = cleanId.Substring(0, queryIndex);
//                 }
//                 return cleanId;
//             }
//             return rawId;
//         }
//     }

//     public class InstagramUploadResponse
//     {
//         public required string Id { get; set; }
//     }

//     public class InstagramAuth
//     {
//         public required string UserName { get; set; }
//         public required string Password { get; set; }
//     }

// }

