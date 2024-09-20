using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

class Program
{
    static async Task Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IYouTubeService, YouTubeService>()
            .AddSingleton<ISpotifyService, SpotifyService>()
            .AddSingleton<MusicDownloader>()
            .BuildServiceProvider();

        var spotifyService = serviceProvider.GetService<ISpotifyService>();
        var youtubeService = serviceProvider.GetService<IYouTubeService>();
        var downloader = serviceProvider.GetService<MusicDownloader>();

        await spotifyService.InitializeSpotifyClient(); // Inicializar Spotify

        // Diretório padrão
        string defaultOutputDirectory = Path.Combine(Environment.CurrentDirectory, "Downloads");

        Console.WriteLine("Bem-vindo ao downloader de músicas! Digite '-help' para ver os comandos disponíveis.");

        bool running = true;

        while (running)
        {
            Console.WriteLine("\nDigite um comando:");
            string input = Console.ReadLine();
            string[] inputArgs = input.Split(' ');

            string command = inputArgs[0];
            string url = inputArgs.Length > 1 ? inputArgs[1] : null;
            string outputDirectory = inputArgs.Length > 2 ? inputArgs[2] : defaultOutputDirectory;

            switch (command)
            {
                case "-spotify":
                    if (url != null && url.Contains("playlist"))
                    {
                        await downloader.DownloadMusicFromSpotifyAsync(url, outputDirectory, true);
                    }
                    else if (url != null)
                    {
                        await downloader.DownloadMusicFromSpotifyAsync(url, outputDirectory, false);
                    }
                    else
                    {
                        Console.WriteLine("URL do Spotify não fornecida.");
                    }
                    break;

                case "-youtube":
                    if (url != null)
                    {
                        await downloader.DownloadMusicFromYouTubeAsync(url, outputDirectory);
                    }
                    else
                    {
                        Console.WriteLine("URL do YouTube não fornecida.");
                    }
                    break;

                case "-search":
                    if (url != null)
                    {
                        var videoUrl = await youtubeService.SearchYouTubeVideoAsync(url);
                        await youtubeService.DownloadFromYouTubeAsync(videoUrl, outputDirectory);
                    }
                    else
                    {
                        Console.WriteLine("Termo de busca não fornecido.");
                    }
                    break;

                case "-help":
                    ShowHelp();
                    break;

                case "-exit":
                    running = false;
                    Console.WriteLine("Encerrando o programa...");
                    break;

                default:
                    Console.WriteLine("Comando não reconhecido.");
                    ShowHelp();
                    break;
            }

            // Exibe o diretório de saída padrão
            Console.WriteLine($"Diretório de saída: {outputDirectory}");
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Uso:");
        Console.WriteLine("-spotify [URL]   : Baixar música ou playlist do Spotify");
        Console.WriteLine("-youtube [URL]   : Baixar vídeo ou playlist do YouTube");
        Console.WriteLine("-search [termo]  : Buscar vídeo no YouTube por termo e baixar");
        Console.WriteLine("-exit            : Sair do programa");
    }
}
