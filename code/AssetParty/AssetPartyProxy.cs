using Editor.Widgets.Packages;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.AssetParty;

public static class AssetPartyProxy
{
	public static readonly string PACKAGE_QUERY = "org:polyhaven type:material";

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

	public static async Task<bool> PackageExists(string id, IEnumerable<Package>? packages = null)
	{
		if (packages == null)
		{
			packages = await ExistingPackages();
		}
		return packages.Select( p => p.Ident ).Contains( id );
	}
}
