using AMWin_RichPresence;
using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Data;
using System.Net.Http.Json;
using Windows.Web.Http;


internal class AgnosticHttpClient
{
	//readonly private Uri webServer = new Uri("http://localhost:3000/api/v1/apple-music");
	readonly private Uri webServer = new Uri("https://aaanh.com/api/v1/apple-music");


	/**
	 * Fields to send to HTTP server
	 */
	private string title;
	private string albumTitle;
	private string artist;
	private string albumCoverUrl;

	int maxStringLength = 127;

	private string TrimString(string str)
	{
		return str.Length > maxStringLength ? str.Substring(0, maxStringLength - 1) : str;
	}

	private string GetTrimmedArtistList(AppleMusicInfo amInfo)
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

	public AgnosticHttpClient()
	{
		this.title = "";
		this.albumTitle = "";
		this.artist = "";
		this.albumCoverUrl = "";
	}

	public AgnosticHttpClient(AppleMusicInfo amInfo)
	{
		this.title = TrimString(amInfo.SongName);
		this.artist = amInfo.SongArtist.Length > maxStringLength ? GetTrimmedArtistList(amInfo) : amInfo.SongArtist;
		this.albumTitle = TrimString(amInfo.SongAlbum);
		this.albumCoverUrl = amInfo.CoverArtUrl ?? Constants.DiscordAppleMusicImageKey;
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
				  this.albumCoverUrl
			  }),
			  Windows.Storage.Streams.UnicodeEncoding.Utf8,
			  "application/json");

		using Windows.Web.Http.HttpResponseMessage response = await httpClient.PostAsync(this.webServer, jsonContent);
		response.EnsureSuccessStatusCode();
	}
}