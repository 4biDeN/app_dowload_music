using System.Collections.Generic;
using System.Threading.Tasks;

    public interface IYouTubeService
    {
        Task DownloadFromYouTubeAsync(string videoUrl, string outputDirectory);
        Task<string> SearchYouTubeVideoAsync(string query);
        Task<List<(string Title, string Url)>> SearchYouTubeVideosAsync(string query, int limit = 10);
        Task<List<string>> GetPlaylistVideoUrlsAsync(string playlistUrl);
    }

