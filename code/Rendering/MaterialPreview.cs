using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Rendering;
public abstract class MaterialPreview
{
	public Asset Asset { get; init; }
	public SceneCamera? Camera { get; private set; }
	public SceneWorld? World { get; private set; }

	public Vector2 ScaleFactor { get; set; } = 1;
	public Vector2 Resolution { get; set; } = new Vector2( 1920, 1080 );

	public MaterialPreview(Asset asset)
	{
		if ( asset.AssetType != AssetType.Material )
			throw new ArgumentException( "Asset must be a material.", nameof( asset ) );
		this.Asset = asset;
	}

	public void InitializeScene()
	{
		World = new SceneWorld();
		Camera = new SceneCamera();
		Camera.World = World;
		InitScene( World, Camera );
	}

	protected abstract void InitScene( SceneWorld world, SceneCamera camera );

	public void InitializeAsset()
	{
		using (Utility.DisableTextureStreaming() )
		{
			var material = Material.Load( Asset.Path );
			if ( material == null )
				throw new InvalidOperationException( "Unable to load material." );
			material = material.CreateCopy();

			material.Set( "g_vTexCoordScale", ScaleFactor );
			InitAsset( material );
		}
	}

	protected abstract void InitAsset( Material material );

	public virtual void Render( string path )
	{
		if ( Camera == null || World == null )
		{
			throw new InvalidOperationException( "Material preview hasn't finished setup." );
		}
		Camera.Size = Resolution;
		Pixmap pixmap = new Pixmap( Camera.Size );

		Camera.RenderToPixmap( pixmap );
		pixmap.SaveJpg( path, 90 );
	}
}
