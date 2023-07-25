using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHaven.Util;

public static class ProcessLauncher
{
	/// <summary>
	/// Execute a process asynchronously
	/// </summary>
	/// <param name="process">The process</param>
	/// <returns>The exit code</returns>
	public static Task<int> RunProcessAsync(Process process)
	{
		var tsc = new TaskCompletionSource<int>();
		process.EnableRaisingEvents = true;

		process.Exited += ( sender, args ) =>
		{
			tsc.SetResult( process.ExitCode );
			process.Dispose();
		};

		process.Start();
		return tsc.Task;
	}
}
