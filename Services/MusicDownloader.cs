using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MusicDownloader
{
    private readonly IYouTubeService _youtubeService;
    private readonly ISpotifyService _spotifyService;

    // Construtor recebe interfaces em vez de classes concretas
    public MusicDownloader(IYouTubeService youtubeService, ISpotifyService spotifyService)
    {
        _youtubeService = youtubeService;
        _spotifyService = spotifyService;
    }

    public async Task DownloadMusicFromSpotifyAsync(string spotifyUrl, string outputDirectory, bool isPlaylist)
    {
        if (isPlaylist)
        {
            var playlistTracks = await _spotifyService.GetPlaylistTrackNamesAsync(spotifyUrl);

            Console.WriteLine("Iniciando download da playlist...");

            foreach (var track in playlistTracks)
            {
                Console.WriteLine($"Buscando vídeo para a consulta: {track}");
                var videoUrl = await _youtubeService.SearchYouTubeVideoAsync(track);

                if (videoUrl != null)
                {
                    Console.WriteLine($"Baixando {track}...");
                    await _youtubeService.DownloadFromYouTubeAsync(videoUrl, outputDirectory);
                    Console.WriteLine($"Download de {track} concluído!");
                }
                else
                {
                    Console.WriteLine($"Nenhum vídeo encontrado para a consulta: {track}");
                }
            }
        }
        else
        {
            var trackName = await _spotifyService.GetTrackNameAsync(spotifyUrl);
            Console.WriteLine($"Buscando vídeo para a consulta: {trackName}");

            var videoUrl = await _youtubeService.SearchYouTubeVideoAsync(trackName);
            if (videoUrl != null)
            {
                Console.WriteLine($"Baixando {trackName}...");
                await _youtubeService.DownloadFromYouTubeAsync(videoUrl, outputDirectory);
                Console.WriteLine($"Download de {trackName} concluído!");
            }
            else
            {
                Console.WriteLine($"Nenhum vídeo encontrado para a consulta: {trackName}");
            }
        }
    }

    public async Task DownloadMusicFromYouTubeAsync(string youtubeUrl, string outputDirectory)
    {
        // Verifique se a URL é uma playlist
        if (youtubeUrl.Contains("playlist?list="))
        {
            // Obtenha os links dos vídeos na playlist usando o YoutubeExplode
            var playlistVideos = await _youtubeService.GetPlaylistVideoUrlsAsync(youtubeUrl);

            Console.WriteLine("Iniciando download da playlist...");

            foreach (var videoUrl in playlistVideos)
            {
                Console.WriteLine($"Baixando vídeo da playlist: {videoUrl}");

                try
                {
                    // Baixe o vídeo
                    await _youtubeService.DownloadFromYouTubeAsync(videoUrl, outputDirectory);
                    Console.WriteLine($"Download do vídeo {videoUrl} concluído!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao baixar o vídeo: {videoUrl}. Detalhes: {ex.Message}");
                }
            }
        }
        else
        {
            try
            {
                await _youtubeService.DownloadFromYouTubeAsync(youtubeUrl, outputDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao baixar o vídeo: {ex.Message}");
            }
        }
    }

    public async Task SearchAndDownloadFromYouTubeAsync(string searchTerm, string outputDirectory)
    {
        try
        {
            var results = await _youtubeService.SearchYouTubeVideosAsync(searchTerm);
            if (results.Count == 0)
            {
                Console.WriteLine("Nenhum vídeo encontrado.");
                return;
            }

            Console.WriteLine("Selecione o número do vídeo que deseja baixar:");
            for (int i = 0; i < results.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {results[i].Title}");
            }

            if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= results.Count)
            {
                await _youtubeService.DownloadFromYouTubeAsync(results[selection - 1].Url, outputDirectory);
            }
            else
            {
                Console.WriteLine("Seleção inválida.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar ou baixar o vídeo: {ex.Message}");
        }
    }
}
