using PolyHaven.Assets;
using System;
using System.IO;
namespace PolyHaven;

public static class PortingUtility
{
	public static Project? EditorProject => Sandbox.Project.Current;

	/// <summary>
	/// The project we're porting into, or throw if the editor hasn't got one open.
	/// </summary>
	public static Project RequireProject()
	{
		return EditorProject ?? throw new InvalidOperationException( "No active project." );
	}

	/// <summary>
	/// Strip whitespace out of PolyHaven's tags/categories so they survive as single tokens.
	/// </summary>
	public static IEnumerable<string> ReplaceSpaces( IEnumerable<string> src )
	{
		foreach ( string s in src )
		{
			yield return s.Replace( " ", "" );
		}
	}

	/// <summary>
	/// Write generated material contents to disk and register it with the asset system, or throw if
	/// the asset system doesn't pick it up.
	/// </summary>
	/// <param name="vmatPath">Path relative to the project's assets folder.</param>
	public static Asset WriteAndRegisterMaterial( string vmatPath, string vmatContents )
	{
		var globalPath = Path.Combine( RequireProject().GetAssetsPath(), vmatPath );
		Directory.CreateDirectory( Path.GetDirectoryName( globalPath )! );
		File.WriteAllText( globalPath, vmatContents );

		// RegisterFile asserts it's on the main thread, and hands the asset straight back now -
		// no need for a separate FindByPath.
		var asset = AssetSystem.RegisterFile( globalPath ) ?? AssetSystem.FindByPath( vmatPath );
		if ( asset == null )
			throw new InvalidOperationException( $"Wrote {vmatPath} but the asset system didn't pick it up." );

		Log.Info( $"Wrote material to {asset}" );
		return asset;
	}

	/// <summary>
	/// The metadata setup shared by every asset kind - polyhaven id, scraped tags, and the publish
	/// config. <paramref name="license"/> is only set when the asset actually has one to declare.
	/// </summary>
	public static void ApplyStandardMetadata( IPolyAsset asset, string? license = null )
	{
		var sboxAsset = asset.SBoxAsset ?? throw new InvalidOperationException( "Material has not been generated." );

		sboxAsset.MetaData.Set( "polyhaven_id", asset.PolyHavenId );
		sboxAsset.Publishing.CreateTemporaryProject();

		var tags = new HashSet<string>();
		foreach ( var tag in asset.Info.Tags )
			tags.Add( tag );
		foreach ( var tag in asset.Info.Categories )
			tags.Add( tag );

		// ProjectConfig no longer carries Tags - modern s&box sets those on the package page - so we
		// stash the scrape on the asset instead of losing it.
		sboxAsset.MetaData.Set( "polyhaven_tags", ReplaceSpaces( tags ).ToArray() );

		// The engine validates and truncates idents itself now, so we can just hand it the PolyHaven id.
		sboxAsset.Publishing.ProjectConfig.Ident = asset.PolyHavenId;
		sboxAsset.Publishing.ProjectConfig.Title = asset.Info.Name;
		sboxAsset.Publishing.ProjectConfig.Org = "polyhaven";
		if ( license != null )
			sboxAsset.Publishing.ProjectConfig.SetMeta( "AssetLicense", license );
		sboxAsset.Publishing.Save();
	}
}
