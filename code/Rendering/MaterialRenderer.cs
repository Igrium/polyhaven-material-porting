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
	public static string RenderMaterialPreview( Asset material )
	{
		if ( material.AssetType != AssetType.Material )
		{
			throw new ArgumentException( "Asset must be a material.", nameof( material ) );
		}

		var preview = new MaterialPreview( material );

		preview.InitializeScene();
		preview.InitializeAsset();

		string? dirname = Path.GetDirectoryName( material.AbsolutePath ) + "/preview";
		if ( dirname == null )
		{
			throw new NullReferenceException( nameof( dirname ) );
		}

		Directory.CreateDirectory( dirname );
		Log.Info( "Created directory " + dirname );
		string path = Path.Combine( dirname, $"{material.Name}.jpg" );
		preview.RenderPreview( path );

		return path;
	}
}
