using System;
using System.IO;
namespace PolyHaven.Util;

/// <summary>
/// Dead simple <c>&lt;#= Key #&gt;</c> substitution over a text file.
/// </summary>
/// <remarks>
/// Templates live in <c>templates/</c> at the project root rather than under <c>Assets/</c>,
/// because the assets folder is where we dump the stuff we're porting - it isn't checked in.
/// </remarks>
public class Template
{
	private readonly string contents;

	/// <param name="templateName">File name inside the project's <c>templates/</c> folder.</param>
	public Template( string templateName )
	{
		var path = Path.Combine( PortingUtility.RequireProject().GetRootPath(), "templates", templateName );
		if ( !File.Exists( path ) )
			throw new ArgumentException( "Unknown template: " + path, nameof( templateName ) );

		contents = File.ReadAllText( path );
	}

	/// <summary>
	/// Replace every <c>&lt;#= Key #&gt;</c> with its value. Missing or null values become empty strings.
	/// </summary>
	public string Parse( IReadOnlyDictionary<string, string?> values )
	{
		var str = contents;
		foreach ( var (key, value) in values )
		{
			str = str.Replace( $"<#= {key} #>", value ?? "" );
		}
		return str;
	}
}
