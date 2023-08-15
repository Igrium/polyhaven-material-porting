using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Rendering;

public class PlaneMaterialPreview : MaterialPreview
{
	public PlaneMaterialPreview( Asset asset ) : base( asset )
	{
	}

	public Transform CameraTransform { get; set; } = new Transform(
		new Vector3( 51.5225143433f, -38.3574523926f, 20.7468070984f ),
		new Rotation( -0.311045289f, 0.1483659297f, 0.8472904563f, 0.4041499197f ) );


	protected override void InitScene(SceneWorld world, SceneCamera camera)
	{

		camera.FieldOfView = 30.0f;
		camera.ZFar = 15000.0f;
		camera.AmbientLightColor = Color.Black;

		camera.Position = CameraTransform.Position;
		camera.Rotation = CameraTransform.Rotation;
		camera.FieldOfView = 60.0f;

		// lighting

		var right = camera.Rotation.Right;

		var sun = new SceneSunLight( World, Rotation.From( 38.43f, 323.78f, -.44f ), (Color.White * 2.5f) * .05f );
		sun.ShadowsEnabled = true;
		//sun.SkyColor = Color.White * 0.5f + Color.Cyan * 0.025f;
		sun.ShadowTextureResolution = 1024;
		sun.SkyColor = Color.Black;

		new SceneLight( World, camera.Position + Vector3.Up * 500.0f + right * 100.0f, 1000.0f, new Color( .85f, 0.9f, 1f ) * 50.0f );
		var cubemap = new SceneCubemap( World, Texture.Load( "textures/cubemaps/sky_gamrig.vtex" ), BBox.FromPositionAndSize( Vector3.Zero, 1024 ) );
	}

	protected override void InitAsset(Material material)
	{
		SceneModel modelObj;
		var model = Model.Load( "models/material_demo.vmdl" );
		modelObj = new SceneModel( World, model, Transform.Zero );
		modelObj.Update( 1 );
		modelObj.SetMaterialOverride( material );
	}
}
