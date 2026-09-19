using System;
using System.IO;
using System.Threading.Tasks;
namespace PolyHaven.Pipeline;

/// <summary>
/// A mass compile shouldn't stop because one asset out of a thousand is broken, so failures get
/// appended to a file to look at afterwards.
/// </summary>
public static class ErrorHandling
{
	public const string ErrorFile = "polyhaven_errors.txt";

	public static readonly string ErrorTemplate = @"Error converting '{0}':
{1}
---------------------

";

	/// <summary>
	/// The log lands in the project root rather than FileSystem.Root, which on this setup is the
	/// engine checkout - no reason to litter in there.
	/// </summary>
	public static string ErrorFilePath => Path.Combine( PortingUtility.RequireProject().GetRootPath(), ErrorFile );

	public static async Task WriteError( string id, string error )
	{
		var path = ErrorFilePath;

		string contents = File.Exists( path ) ? await File.ReadAllTextAsync( path ) : "";
		contents += string.Format( ErrorTemplate, id, error );

		await File.WriteAllTextAsync( path, contents );
	}

	public static Task WriteError( string id, Exception exception ) => WriteError( id, exception.ToString() );
}
