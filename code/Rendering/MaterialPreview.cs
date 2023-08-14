using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Rendering;

public class MaterialPreview
{
	public Asset Asset { get; private set; }
	public SceneCamera? Camera { get; private set; }
	public SceneWorld? World { get; private set; }

	public Vector2 Resolution = new Vector2( 1920, 1080 );

	public Vector2 ScaleFactor { get; set; } = 1;

	public Transform CameraTransform { get; set; } = new Transform(
		new Vector3( 51.5225143433f, -38.3574523926f, 20.7468070984f ),
		new Rotation( -0.311045289f, 0.1483659297f, 0.8472904563f, 0.4041499197f ) );

	public MaterialPreview( Asset asset )
	{
		Asset = asset;
	}

	public virtual void InitializeScene()
	{
		World = new SceneWorld();
		Camera = new SceneCamera();
		Camera.World = World;

		Camera.FieldOfView = 30.0f;
		Camera.ZFar = 15000.0f;
		Camera.AmbientLightColor = Color.Black;

		Camera.Position = CameraTransform.Position;
		Camera.Rotation = CameraTransform.Rotation;
		Camera.FieldOfView = 60.0f;


		// lighting

		var right = Camera.Rotation.Right;

		var sun = new SceneSunLight( World, Rotation.From(38.43f, 323.78f, -.44f), (Color.White * 2.5f + Color.Cyan * 0.05f) * .1f );
		sun.ShadowsEnabled = true;
		//sun.SkyColor = Color.White * 0.5f + Color.Cyan * 0.025f;
		sun.ShadowTextureResolution = 1024;

		new SceneLight( World, Camera.Position + Vector3.Up * 500.0f + right * 100.0f, 1000.0f, new Color( 1.0f, 0.9f, 0.9f ) * 50.0f );
		var cubemap = new SceneCubemap( World, Texture.Load( "textures/cubemaps/sky_riverbank.vtex" ), BBox.FromPositionAndSize( Vector3.Zero, 1000 ) );
	}

	public virtual void InitializeAsset()
	{
		SceneModel modelObj;
		using ( Utility.DisableTextureStreaming() )
		{
			var model = Model.Load( "models/material_demo.vmdl" );
			modelObj = new SceneModel( World, model, Transform.Zero );
			modelObj.Update( 1 );

			var material = Material.Load( Asset.Path ).CreateCopy();
			material.Set( "g_vTexCoordScale", ScaleFactor );
			modelObj.SetMaterialOverride( material );
		}
	}

	public bool RenderPreview( string path )
	{
		if (Camera == null || World == null)
		{
			throw new InvalidOperationException( "Material preview hasn't finished setup." );
		}
		Camera.Size = Resolution;
		Pixmap pixmap = new Pixmap( Camera.Size );

		Camera.RenderToPixmap( pixmap );
		return pixmap.SaveJpg( path, 90 );
	}
}
