using AMWin_RichPresence;
using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Data;
using System.Net.Http.Json;
using Windows.Web.Http;
using System.IO;


internal class AgnosticHttpClient
{
	
	/**
	 * Fields to send to HTTP server
	 */
	private string title;
	private string albumTitle;
	private string artist;
	private string albumCoverUrl;
    private string endpointKey = "";
	private Uri webServer;
	private Logger? logger;


    int maxStringLength = 127;

	private string TrimString(string str)
	{
		return str.Length > maxStringLength ? str.Substring(0, maxStringLength - 1) : str;
	}

    static private string GetTrimmedArtistList(AppleMusicInfo amInfo)
	{
		if (amInfo.ArtistList?.Count > 1)
		{
			return $"{amInfo.ArtistList.First()}, Various Artists";
		}
		else
		{
			return amInfo.SongArtist; // TODO fix this so it always prevents string length violations
		}
	}

	public AgnosticHttpClient(Logger? logger)
	{
        // load env
        var rootDir = Directory.GetCurrentDirectory();
        var dotenvPath = Path.Combine(rootDir, ".env.local");
        DotEnvLoader.Load(dotenvPath);

		this.endpointKey = Environment.GetEnvironmentVariable("HTTP_SERVER_ENDPOINT_KEY") ?? "";
		this.webServer = new Uri(Environment.GetEnvironmentVariable("HTTP_SERVER_ENDPOINT_URL") ?? "http://localhost:3000/api/v1/apple-music");

		// logger
		this.logger = logger;

        this.title = "";
		this.albumTitle = "";
		this.artist = "";
		this.albumCoverUrl = "";
	}

	public async void UpdateNowPlaying(AppleMusicInfo amInfo)
	{
        var httpClient = new Windows.Web.Http.HttpClient();

		this.title = TrimString(amInfo.SongName);
		this.artist = amInfo.SongArtist.Length > maxStringLength ? GetTrimmedArtistList(amInfo) : amInfo.SongArtist;
		this.albumTitle = TrimString(amInfo.SongAlbum);
		this.albumCoverUrl = amInfo.CoverArtUrl ?? Constants.DiscordAppleMusicImageKey;

		using Windows.Web.Http.HttpStringContent jsonContent = new(
			  JsonSerializer.Serialize(new
			  {
				  this.title,
				  this.albumTitle,
				  this.artist,
				  this.albumCoverUrl,
				  endpointKey
			  }),
			  Windows.Storage.Streams.UnicodeEncoding.Utf8,
			  "application/json");

		try
		{
			using Windows.Web.Http.HttpResponseMessage response = await httpClient.PostAsync(this.webServer, jsonContent);
			response.EnsureSuccessStatusCode();
		}
		catch (Exception ex)
		{
			this.logger?.Log("HTTP Server is set to (app): " + this.webServer.ToString() +"\n");
			this.logger?.Log("HTTP Server is set to (host): " + Environment.GetEnvironmentVariable("HTTP_SERVER_ENDPOINT_URL") + "\n");
            this.logger?.Log(ex.ToString());

        }
    }
}