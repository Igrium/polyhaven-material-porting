using Editor;
using PolyHaven.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Rendering;

public static class MaterialRenderer
{
	public enum MaterialRenderType { Plane, Sphere }

	public static string RenderMaterialPreview( Asset material, MaterialRenderType type, float scale = 1 )
	{
		if ( material.AssetType != AssetType.Material )
		{
			throw new ArgumentException( "Asset must be a material.", nameof( material ) );
		}

		MaterialPreview preview = type == MaterialRenderType.Plane ? new PlaneMaterialPreview( material ) : new SphereMaterialPreview( material );

		preview.InitializeScene();
		preview.ScaleFactor = scale;
		preview.InitializeAsset();

		string? dirname = Path.GetDirectoryName( material.AbsolutePath ) + "/preview";
		if ( dirname == null )
		{
			throw new NullReferenceException( nameof( dirname ) );
		}

		Directory.CreateDirectory( dirname );
		Log.Info( "Created directory " + dirname );
		string path = Path.Combine( dirname, $"{material.Name}.jpg" );
		preview.Render( path );

		return path;
	}
}
