using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PolyHaven.Pipeline;

public static class AssetPublishing
{
	public static async Task Publish( PolyAsset asset )
	{
		if ( asset.Material == null )
			throw new InvalidOperationException( "Material has not been generated." );

		Log.Info( "Publishing " + asset.PolyHavenID );
		var project = asset.Material.Publishing.CreateTemporaryProject();
		var publisher = await ProjectPublisher.FromAsset( asset.Material );
		publisher.SetMeta( "polyhaven_id", asset.PolyHavenID );
		await publisher.PrePublish();

		Log.Info( $"Uploading files" );
		await DoUploads( project, publisher );

		publisher.SetChangeDetails( "Auto-generated upload", "" );
		await publisher.Publish();
		Log.Info( "Published to " + project.ViewUrl );
	}

	static async Task DoUploads( LocalProject project, ProjectPublisher publisher )
	{
		var token = new CancellationToken();
		var uploads = publisher.Files.Where( x => !x.Skip ).ToArray();

		var tasks = new List<Task>();

		foreach ( var uploaded in uploads )
		{
			tasks.Add( UploadFile( project, publisher, uploaded, token ) );

			//
			// max 8 uploads at the same time then wait for one to complete
			//
			while ( tasks.Count > 8 )
			{
				await Task.WhenAny( tasks.ToArray() );
				tasks.RemoveAll( x => x.IsCompleted );
			}
		}

		await Task.WhenAll( tasks.ToArray() );
	}

	static async Task UploadFile( LocalProject project, ProjectPublisher publisher, ProjectPublisher.ProjectFile file, CancellationToken token = default )
	{
		file.SizeUploaded = 1;

		Log.Info( "Uploading " + file );
		if ( file.Contents != null )
		{
			bool skip = await project.Package.UploadFile( file.Contents, file.Name, ( progress ) => { }, token );
			if ( skip ) file.Skip = true;
		}
		else if ( file.AbsolutePath != null )
		{
			bool skip = await project.Package.UploadFile( file.AbsolutePath, file.Name, ( progress ) => { }, token );
			if ( skip ) file.Skip = true;
		}
		else
		{
			Log.Warning( $"Unable to upload file {file} - has no content defined!" );
		}
	}
}
