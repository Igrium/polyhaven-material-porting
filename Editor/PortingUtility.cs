using System;
namespace PolyHaven;

public static class PortingUtility
{
	public static Project? EditorProject => Sandbox.Project.Current;

	/// <summary>
	/// The project we're porting into, or throw if the editor hasn't got one open.
	/// </summary>
	public static Project RequireProject()
	{
		return EditorProject ?? throw new InvalidOperationException( "No active project." );
	}

	/// <summary>
	/// Strip whitespace out of PolyHaven's tags/categories so they survive as single tokens.
	/// </summary>
	public static IEnumerable<string> ReplaceSpaces( IEnumerable<string> src )
	{
		foreach ( string s in src )
		{
			yield return s.Replace( " ", "" );
		}
	}
}
