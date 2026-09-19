using PolyHaven.Assets;
using System;
using System.Threading.Tasks;
namespace PolyHaven.Pipeline;

public static class AssetPublishing
{
	/// <summary>
	/// Set false to actually upload. Left on while the rewrite is in progress so a test run can't
	/// push a half-broken package to asset.party - once a version is live it's live.
	/// </summary>
	public static bool DryRun { get; set; } = true;

	public static async Task Publish( IPolyAsset asset )
	{
		if ( asset.SBoxAsset == null )
			throw new InvalidOperationException( "Material has not been generated." );

		if ( DryRun )
		{
			Log.Warning( $"[dry run] Skipping publish of {asset.PolyHavenId}. Set AssetPublishing.DryRun = false to upload for real." );
			return;
		}

		Log.Info( "Publishing " + asset.PolyHavenId );
		var project = asset.SBoxAsset.Publishing.CreateTemporaryProject();
		var publisher = await ProjectPublisher.FromAsset( asset.SBoxAsset );
		publisher.SetMeta( "polyhaven_id", asset.PolyHavenId );
		await publisher.PrePublish();

		Log.Info( "Uploading files" );

		// ProjectPublisher does its own batched uploads now, so we don't hand-roll the 8-at-a-time loop.
		await publisher.UploadFiles();

		publisher.SetChangeDetails( "Auto-generated upload", "" );
		await publisher.Publish();

		Log.Info( "Published to " + project.Config.FullIdent );
	}
}
