using PolyHaven.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Assets;

public interface IPolyAsset
{
	public string PolyHavenID { get; }
	public AssetEntry Asset { get; }
	public AssetType AssetType => Asset.Type;

	public Editor.Asset? SBoxAsset { get; }

	public Task<IEnumerable<string>> DownloadFiles();

	public void SetupMetadata();
}
