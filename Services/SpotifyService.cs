using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class SpotifyService : ISpotifyService
{
    private readonly string _clientId = "40f071fe7f2944518f8aebab6d43eb8c"; // Coloque suas credenciais aqui
    private readonly string _clientSecret = "a750f6538c6949a1b8d0113fa1e05791";
    private SpotifyClient _spotifyClient;

    // Inicializa o cliente do Spotify com credenciais fixas
    public async Task InitializeSpotifyClient()
    {
        try
        {
            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(_clientId, _clientSecret);
            var response = await new OAuthClient(config).RequestToken(request);
            _spotifyClient = new SpotifyClient(config.WithToken(response.AccessToken));
            Console.WriteLine("Spotify Client inicializado com sucesso.");
        }
        catch (APIException ex)
        {
            Console.WriteLine($"Erro ao inicializar o Spotify Client: {ex.Message}");
            throw; // Re-throw para tratamento adicional se necessário
        }
    }

    // Extrai o ID da track ou playlist da URL do Spotify
    public string ExtractSpotifyId(string spotifyUrl)
    {
        // Ajustado o regex para lidar com '/intl-pt/' ou outros segmentos antes de 'track' ou 'playlist'
        var match = Regex.Match(spotifyUrl, @"(?:https:\/\/open\.spotify\.com\/(?:intl-[a-zA-Z]{2}\/)?(?:track|playlist)\/|spotify:(track|playlist):)([a-zA-Z0-9]+)");

        if (match.Success)
        {
            return match.Groups[2].Value; // Captura o ID da música ou playlist
        }
        throw new ArgumentException("URL do Spotify inválida.");
    }


    // Obtém o nome da música pelo ID
    public async Task<string> GetTrackNameAsync(string trackUrlOrId)
    {
        try
        {
            string trackId = ExtractSpotifyId(trackUrlOrId);
            var track = await _spotifyClient.Tracks.Get(trackId);
            return $"{track.Name} - {track.Artists[0].Name}";
        }
        catch (APIException ex)
        {
            Console.WriteLine($"Erro ao obter o nome da música: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro geral ao processar a música: {ex.Message}");
            throw;
        }
    }

    // Obtém os nomes das músicas de uma playlist
    public async Task<List<string>> GetPlaylistTrackNamesAsync(string playlistUrlOrId)
    {
        try
        {
            var playlistId = ExtractSpotifyId(playlistUrlOrId);
            var playlist = await _spotifyClient.Playlists.Get(playlistId);
            var trackNames = new List<string>();

            foreach (var item in playlist.Tracks.Items)
            {
                var track = item.Track as FullTrack;
                if (track != null)
                {
                    trackNames.Add($"{track.Name} - {track.Artists[0].Name}");
                }
            }

            return trackNames;
        }
        catch (APIException ex)
        {
            Console.WriteLine($"Erro ao obter as faixas da playlist: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro geral ao processar a playlist: {ex.Message}");
            throw;
        }
    }
}
