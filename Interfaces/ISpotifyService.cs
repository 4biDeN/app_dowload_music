using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISpotifyService
{
	Task InitializeSpotifyClient();
	string ExtractSpotifyId(string spotifyUrl);
	Task<string> GetTrackNameAsync(string trackUrlOrId);
	Task<List<string>> GetPlaylistTrackNamesAsync(string playlistUrlOrId);
}
