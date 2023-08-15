using PolyHaven.API;
using PolyHaven.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Util;

public static class TextUtils
{
	public static string WriteTags(IEnumerable<string> tags)
	{
		var newTags = tags.Select( t => t.Replace( " ", "" ) );

		string str = "";
		foreach( var tag in newTags )
		{
			str += tag + " ";
		}
		// Remove the trailing space
		str = str.Remove( str.Length - 1, 1 );
		return str;
	}

	public static string WriteAuthors(IEnumerable<string> authors)
	{
		List<string> list = new();
		list.AddRange( authors );

		if ( list.Count == 0 ) return "";
		if ( list.Count == 1 ) return list[0];
		if ( list.Count == 2 ) return $"{list[0]} and {list[1]}";

		string str = "";
		for (int i = 0; i < list.Count - 1; i++ )
		{
			str += list[i] + ", ";
		}

		str += "and " + list[list.Count - 1];
		return str;
	}

	public static string WriteDescription(AssetMeta asset)
	{
		return $"By {WriteAuthors( asset.AssetEntry.Authors.Keys )} on PolyHaven \n \n {WriteURL( asset.PolyID )}";
	}

	public static string WriteURL(string polyID)
	{
		return "https://polyhaven.com/a/" + polyID;
	}
}
