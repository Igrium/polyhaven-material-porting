using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Rendering;

public class SphereMaterialPreview : MaterialPreview
{
	public SphereMaterialPreview( Asset asset ) : base( asset )
	{
	}

	protected override void InitScene( SceneWorld world, SceneCamera camera )
	{
		camera.BackgroundColor = "#32415e";
		//camera.Angles = new Angles( 20, 180 + 45, 0 );
		camera.FieldOfView = 30.0f;
		camera.ZFar = 15000.0f;
		camera.AmbientLightColor = Color.White * 0.05f;
		camera.Position = camera.Rotation.Forward * -64;

		// lighting

		var right = camera.Rotation.Right;

		var sun = new SceneSunLight( world, Rotation.From(50, 120, 0), Color.White * 2.5f + Color.Cyan * 0.05f );
		sun.ShadowsEnabled = true;
		sun.SkyColor = Color.White * 0.5f + Color.Cyan * 0.025f;
		sun.ShadowTextureResolution = 1024;

		//new SceneLight( world, camera.Position + Vector3.Up * 500.0f + -right * 100.0f, 1000.0f, new Color( 1.0f, 0.9f, 0.9f ) * 50.0f );
		new SceneCubemap( world, Texture.Load( "textures/cubemaps/env_blocky_studio.vtex" ), BBox.FromPositionAndSize( Vector3.Zero, 1000 ) );
	}

	protected override void InitAsset( Material material )
	{
		var model = Model.Load( "models/material_demo_sphere.vmdl" );
		var modelObj = new SceneModel( World, model, Transform.Zero );
		modelObj.Update( 1 );
		modelObj.SetMaterialOverride( material );
	}
}
