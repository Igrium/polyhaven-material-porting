using Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.API;

public struct FileReference
{
	public UInt64 Size { get; set; }
	public string MD5 { get; set; }
	public string URL { get; set; }

	public override string ToString()
	{
		return $"FileReference[Size={Size}, Hash={MD5}, URL={URL}]";
	}

	public async Task<byte[]> Download(HttpClient client)
	{
		Uri uri = new Uri(URL);
		byte[] fileBytes = await client.GetByteArrayAsync(uri);
		return fileBytes;
	}

	public async void Download(HttpClient client, string filepath)
	{
		var bytes = await Download(client);
		await File.WriteAllBytesAsync(filepath, bytes);
	}

	#nullable disable
	public Task<bool> DownloadAsync(string filepath)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(filepath));
		return Utility.DownloadAsync(URL, filepath);
	}
}
