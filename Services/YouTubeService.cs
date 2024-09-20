using YoutubeExplode;
using YoutubeExplode.Converter;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;
using System.Text.RegularExpressions;
using System;

public class YouTubeService : IYouTubeService
{
    private readonly YoutubeClient _youtubeClient;
    private static int _totalTracksProcessed = 0; // Contador de músicas processadas

    public YouTubeService()
    {
        _youtubeClient = new YoutubeClient();
    }

    // Método para baixar vídeos do YouTube
    public async Task DownloadFromYouTubeAsync(string videoUrl, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var videoId = ExtractVideoId(videoUrl);
        if (string.IsNullOrEmpty(videoId))
        {
            throw new ArgumentException("A URL do vídeo não é válida.");
        }

        var video = await _youtubeClient.Videos.GetAsync(videoId);
        var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);
        var streamInfo = streamManifest.Streams
            .OrderBy(s => s.Bitrate)
            .FirstOrDefault();

        if (streamInfo != null)
        {
            var tempFilePath = Path.Combine(outputDirectory, "temp_video4.mp4");

            await _youtubeClient.Videos.Streams.DownloadAsync(streamInfo, tempFilePath);

            Console.WriteLine("\nDownload concluído!");

            // Sanitizar o título do vídeo para criar um nome de arquivo válido
            var sanitizedFileName = SanitizeFileName($"{video.Title}.mp3");
            var mp3FilePath = Path.Combine(outputDirectory, sanitizedFileName);

            await ConvertToMp3Async(tempFilePath, mp3FilePath);

            File.Delete(tempFilePath); // Remove o arquivo temporário

            // Atualiza o contador de músicas processadas
            _totalTracksProcessed++;
            Console.WriteLine($"Música baixada: {mp3FilePath}");
            Console.WriteLine($"Total de músicas processadas: {_totalTracksProcessed}");
        }
        else
        {
            Console.WriteLine("Não foi possível encontrar um fluxo de vídeo apropriado.");
        }
    }

    // Extrair o ID do vídeo da URL
    private string ExtractVideoId(string videoUrl)
    {
        var uri = new Uri(videoUrl);
        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        return queryParams["v"];
    }

    // Converter vídeo para MP3
    private async Task ConvertToMp3Async(string inputFile, string outputFile)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i \"{inputFile}\" \"{outputFile}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                var errorReader = process.StandardError;
                string errorOutput = await errorReader.ReadToEndAsync();
                var outputReader = process.StandardOutput;
                string standardOutput = await outputReader.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (!string.IsNullOrEmpty(errorOutput) &&
                    (errorOutput.ToLower().Contains("error") || errorOutput.ToLower().Contains("failed") || errorOutput.ToLower().Contains("invalid")))
                {
                    Console.WriteLine($"Erro durante a conversão de {inputFile} para {outputFile}, possível causa é que o vídeo encontrado no youtube não possui audio");
                }

                if (!string.IsNullOrEmpty(standardOutput))
                {
                    Console.WriteLine("Saída padrão do FFmpeg:");
                    Console.WriteLine(standardOutput);
                }

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"O processo FFmpeg falhou com código de saída: {process.ExitCode}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocorreu um erro ao converter o arquivo {inputFile}: {ex.Message}");
        }
    }

    // Sanitizar o nome do arquivo
    private string SanitizeFileName(string fileName)
    {
        return Regex.Replace(fileName, "[^a-zA-Z0-9_\\-\\.]", "_");
    }

    // Buscar vídeo no YouTube
    public async Task<string> SearchYouTubeVideoAsync(string query)
    {
        Console.WriteLine($"Buscando vídeo para a consulta: {query}");
        var searchResults = await _youtubeClient.Search.GetVideosAsync(query).CollectAsync(1);
        var firstResult = searchResults.FirstOrDefault();
        if (firstResult != null)
        {
            Console.WriteLine($"Primeiro resultado: {firstResult.Title} ({firstResult.Url})");
            return firstResult.Url;
        }
        else
        {
            Console.WriteLine("Nenhum vídeo encontrado.");
        }
        return null;
    }

    // Buscar múltiplos vídeos no YouTube
    public async Task<List<(string Title, string Url)>> SearchYouTubeVideosAsync(string query, int limit = 10)
    {
        var searchResults = await _youtubeClient.Search.GetVideosAsync(query).CollectAsync(limit);
        return searchResults.Select(result => (result.Title, result.Url)).ToList();
    }

    // Obter URLs de vídeos de uma playlist
    public async Task<List<string>> GetPlaylistVideoUrlsAsync(string playlistUrl)
    {
        var playlist = await _youtubeClient.Playlists.GetVideosAsync(playlistUrl);
        var videoUrls = playlist.Select(video => video.Url).ToList();
        return videoUrls;
    }
}
