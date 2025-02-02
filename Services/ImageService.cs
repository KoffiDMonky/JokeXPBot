using SkiaSharp;

namespace JokeXPBot.Services
{
    public class ImageService
    {
        public void CreateJokeImage(string joke, string filePath)
        {
            int width = 1080, height = 1080;
            int padding = 60;

            // Crée une image vide avec un fond personnalisé
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.DrawColor(SKColor.Parse("#015AEF")); // Couleur de fond

            // Initialiser les paramètres de police
            var font = new SKFont();
            var paint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            var strokePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 15
            };

            // Ajuster dynamiquement la taille du texte
            AdjustFontSize(joke, font, paint, width - (2 * padding), height - (2 * padding));

            // Découpe et centre le texte
            var lines = WrapText(joke, font, paint, width - (2 * padding));
            var y = (height / 2) - (lines.Count * font.Size / 2);

            foreach (var line in lines)
            {
                // Dessine d'abord le contour noir
                canvas.DrawText(line, width / 2, y, SKTextAlign.Center, font, strokePaint);

                // Dessine ensuite le texte blanc
                canvas.DrawText(line, width / 2, y, SKTextAlign.Center, font, paint);

                y += font.Size + 20; // Ajoute de l'espace entre les lignes
            }

            // Ajouter le logo en bas à droite
            DrawLogo(canvas, "assets/logo.png", width, height);

            // Sauvegarde l'image
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(filePath);
            data.SaveTo(stream);
        }

        public string GenerateImageForInstagram(string joke)
        {
            Console.WriteLine("Génération de l'image...");

            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            string fileName = $"{Guid.NewGuid()}.png";
            string filePath = Path.Combine(outputPath, fileName);

            int width = 1080, height = 1080;
            int padding = 60;

            // Crée une image vide avec un fond personnalisé
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.DrawColor(SKColor.Parse("#015AEF")); // Couleur de fond

            // Initialiser les paramètres de police
            var font = new SKFont();
            var paint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            var strokePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 15
            };

            // Ajuster dynamiquement la taille du texte
            AdjustFontSize(joke, font, paint, width - (2 * padding), height - (2 * padding));

            // Découpe et centre le texte
            var lines = WrapText(joke, font, paint, width - (2 * padding));
            var y = (height / 2) - (lines.Count * font.Size / 2);

            foreach (var line in lines)
            {
                // Dessine d'abord le contour noir
                canvas.DrawText(line, width / 2, y, SKTextAlign.Center, font, strokePaint);

                // Dessine ensuite le texte blanc
                canvas.DrawText(line, width / 2, y, SKTextAlign.Center, font, paint);

                y += font.Size + 20; // Ajoute de l'espace entre les lignes
            }

            // Ajouter le logo en bas à droite
            DrawLogo(canvas, "assets/logo.png", width, height);


            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(filePath);
            data.SaveTo(stream);

            Console.WriteLine($"Image générée : {filePath}");
            return filePath;
        }
        private void AdjustFontSize(string text, SKFont font, SKPaint paint, float maxWidth, float maxHeight)
        {
            float fontSize = 100; // Taille initiale du texte
            font.Size = fontSize;

            while (true)
            {
                var lines = WrapText(text, font, paint, maxWidth);
                var totalHeight = lines.Count * font.Size + (lines.Count - 1) * 20; // Inclut l'espacement

                if (totalHeight <= maxHeight)
                    break; // La taille actuelle convient

                fontSize -= 2; // Réduit la taille du texte
                font.Size = fontSize;

                if (fontSize <= 10) // Taille minimale
                    break;
            }
        }

        // private void DrawLogo(SKCanvas canvas, string logoPath, int canvasWidth, int canvasHeight)
        // {
        //     // Charger le logo
        //     using var logoBitmap = SKBitmap.Decode(logoPath);

        //     // Dimensions du logo
        //     int logoWidth = logoBitmap.Width / 5; // Réduction à 20% de la taille originale
        //     int logoHeight = logoBitmap.Height / 5;

        //     // Position du logo (bas à droite avec padding)
        //     float x = canvasWidth - logoWidth - 20; // 20px de marge
        //     float y = canvasHeight - logoHeight - 20;

        //     // Définir la taille et dessiner le logo
        //     var destRect = new SKRect(x, y, x + logoWidth, y + logoHeight);
        //     canvas.DrawBitmap(logoBitmap, destRect);
        // }

        private void DrawLogo(SKCanvas canvas, string logoPath, int canvasWidth, int canvasHeight)
        {
            // Vérification du chemin
            Console.WriteLine($"Tentative de chargement du logo depuis : {Path.GetFullPath(logoPath)}");

            if (!File.Exists(logoPath))
            {
                Console.WriteLine($"ERREUR : Le fichier logo n'existe pas à l'emplacement : {logoPath}");
                return; // ou throw new FileNotFoundException($"Logo non trouvé : {logoPath}");
            }

            try
            {
                // Charger le logo
                using var logoBitmap = SKBitmap.Decode(logoPath);

                if (logoBitmap == null)
                {
                    Console.WriteLine("ERREUR : Échec du décodage du logo");
                    return;
                }

                // Dimensions du logo
                int logoWidth = logoBitmap.Width / 5;
                int logoHeight = logoBitmap.Height / 5;

                // Position du logo (bas à droite avec padding)
                float x = canvasWidth - logoWidth - 20;
                float y = canvasHeight - logoHeight - 20;

                // Définir la taille et dessiner le logo
                var destRect = new SKRect(x, y, x + logoWidth, y + logoHeight);
                canvas.DrawBitmap(logoBitmap, destRect);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR lors du chargement/dessin du logo : {ex.Message}");
            }
        }
        private List<string> WrapText(string text, SKFont font, SKPaint paint, float maxWidth)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
                var textWidth = font.MeasureText(testLine, paint);

                if (textWidth > maxWidth)
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return lines;
        }
    }
}