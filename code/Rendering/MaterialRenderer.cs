using Editor;
using PolyHaven.Assets;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Rendering;

public static class MaterialRenderer
{
	public enum MaterialRenderType { Plane, Sphere, Both }

	public static string RenderMaterialPreview( Asset material, MaterialPreview preview, float scale = 1, string? suffix = null )
	{
		if ( material.AssetType != AssetType.Material )
		{
			throw new ArgumentException( "Asset must be a material.", nameof( material ) );
		}


		preview.InitializeScene();
		preview.ScaleFactor = scale;
		preview.InitializeAsset();

		string? dirname = Path.Join( Path.GetDirectoryName( material.AbsolutePath ), "preview" );
		if ( dirname == null )
		{
			throw new NullReferenceException( nameof( dirname ) );
		}

		Directory.CreateDirectory( dirname );
		string filename = $"{material.Name}{(suffix != null ? "_" + suffix : "")}.jpg";
		string path = Path.Combine( dirname, filename );

		preview.Render( path );
		AssetSystem.RegisterFile( filename );
		Log.Info( $"Wrote image to {AssetSystem.FindByPath( path )}" );
		return path;
	}

	public static IEnumerable<string> RenderMaterialPreviews( Asset material, MaterialRenderType type, float scale = 1 )
	{
		List<string> outputs = new List<string>( 2 );
		if ( type == MaterialRenderType.Plane || type == MaterialRenderType.Both )
		{
			outputs.Add( RenderMaterialPreview( material, new PlaneMaterialPreview( material ), scale, "plane" ) );
		}
		if ( type == MaterialRenderType.Sphere || type == MaterialRenderType.Both )
		{
			outputs.Add( RenderMaterialPreview( material, new SphereMaterialPreview( material ), scale, "sphere" ) );
		}
		return outputs;
	}

	[ConCmd.Engine( "render_material_previews" )]
	public static void RenderMaterialPreviews( string path, string renderType = "", float scale = 1 )
	{
		Asset asset = AssetSystem.FindByPath( path );
		Log.Info( "Generating preview(s) for " + asset.Path );

		if ( !Enum.TryParse<MaterialRenderType>( renderType, true, out var matRenderType ) )
		{
			matRenderType = MaterialRenderType.Both;
		};
		MaterialRenderer.RenderMaterialPreviews( asset, matRenderType, scale );
	}
}
