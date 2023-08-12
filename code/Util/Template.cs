using Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Util;

/// <summary>
/// Shitty simple template format parser
/// </summary>
public class Template
{
	private string TemplateContents { get; set; }

	public Template( string templatePath )
	{
		if ( !FileSystem.Content.FileExists( templatePath ) )
			throw new ArgumentException( "Unknown file: " + templatePath, nameof( templatePath ) );
		TemplateContents = FileSystem.Content.ReadAllText( templatePath );
	}


	public string Parse( Dictionary<string, string?> values )
	{
		//
		// example template:
		// "Blah<#= value #>blah"
		//
		// called with Parse( new[]{ ( "value", "Hello!" ) } ):
		// "BlahHello!blah"
		//

		var str = TemplateContents;
		foreach ( var pair in values )
		{
			str = str.Replace( $"<#= {pair.Key} #>", pair.Value != null ? pair.Value : "" );
		}

		return str;
	}
}
