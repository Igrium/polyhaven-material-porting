using Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Pipeline;

public static class ErrorHandling
{
	public static readonly string ErrorTemplate = @"Error converting '{0}':
{1}
---------------------

";

	public static async Task writeError(string id, string error)
	{
		string file_str = "";
		if (FileSystem.Root.FileExists("polyhaven_errors"))
		{
			file_str = await FileSystem.Root.ReadAllTextAsync( "polyhaven_errors" );
		}

		file_str += String.Format( ErrorTemplate, id, error );

		FileSystem.Root.WriteAllText( "polyhaven_errors", file_str );
	}

	public static Task writeError(string id, Exception exception)
	{
		return writeError(id, exception.ToString());
	}
}
