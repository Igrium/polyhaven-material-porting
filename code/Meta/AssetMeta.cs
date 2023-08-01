﻿using PolyHaven.API;
using PolyHaven.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PolyHaven.Meta;

public struct AssetMeta
{
	public AssetEntry AssetEntry { get; set; }

	[JsonPropertyName("poly_id")]
	public string PolyID { get; set; }

	public AssetMeta(PolyAsset asset)
	{
		AssetEntry = asset.Asset;
		PolyID = asset.PolyHavenID;
	}
}