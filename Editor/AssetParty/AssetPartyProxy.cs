using System.Threading.Tasks;
namespace PolyHaven.AssetParty;

public class AssetPartyProxy
{
	public const string PACKAGE_QUERY = "org:polyhaven type:material";
	
	public static async Task<List<Package>> ExistingPackages()
	{
		List<Package> packages = new();
		Package[] packageArray;
		int i = 0;
		do
		{
			packageArray = (await Package.FindAsync( PACKAGE_QUERY, take: 500, skip: i )).Packages;
			i += 500;
			packages.AddRange( packageArray );
		} while ( packageArray.Length > 0 );

		return packages;
	}
}
