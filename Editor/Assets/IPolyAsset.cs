using PolyHaven.API;
using System.Threading.Tasks;
namespace PolyHaven.Assets;

public interface IPolyAsset
{
	public string PolyHavenId { get; }
	public PolyHavenAssetInfo Info { get; }
	public PhAssetType AssetType => Info.Type;

	public Asset? SBoxAsset { get; }

	public Task<IEnumerable<string>> DownloadFiles();

	public void SetupMetadata();
}
