using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;

namespace TestRunner.Tools
{
	/// <summary>
	/// Describes the machine, the runtime and the library versions of a test run.
	/// </summary>
	public static class RunEnvironment
	{
		/// <summary>
		/// Items added by the test host (library versions, database server, settings).
		/// </summary>
		public static IDictionary<string,string?> Items { get; } = new SortedDictionary<string,string?>();

		/// <summary>
		/// Records the version of the assembly that defines <paramref name="type"/>.
		/// </summary>
		public static void AddVersion(string name, Type type)
		{
			Items[name] = GetVersion(type.Assembly);
		}

		public static string GetVersion(Assembly assembly)
		{
			var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

			if (!string.IsNullOrEmpty(informational))
				return informational!;

			return assembly.GetName().Version?.ToString() ?? "";
		}

		public static IEnumerable<KeyValuePair<string,string?>> GetAll()
		{
			var all = new SortedDictionary<string,string?>
			{
				["Runtime"]              = RuntimeInformation.FrameworkDescription,
				["OS"]                   = $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})",
				["Process architecture"] = RuntimeInformation.ProcessArchitecture.ToString(),
				["CPU"]                  = GetCpuName(),
				["Processor count"]      = Environment.ProcessorCount.ToString(),
				["GC"]                   = (GCSettings.IsServerGC ? "Server" : "Workstation") + ", " + GCSettings.LatencyMode,
				["Stopwatch frequency"]  = Stopwatch.Frequency.ToString(),
				// TestStopwatch.Ticks of this run; runs recorded before this table existed stored
				// raw Stopwatch ticks, which equal TimeSpan ticks only at 10 MHz (Windows).
				["Tick unit"]            = "TimeSpan ticks (100 ns)",
#if DEBUG
				["Build configuration"]  = "Debug",
#else
				["Build configuration"]  = "Release",
#endif
			};

			foreach (var item in Items)
				all[item.Key] = item.Value;

			return all;
		}

		static string? GetCpuName()
		{
			try
			{
#if NET481
				using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
					if (key?.GetValue("ProcessorNameString") is string name)
						return name.Trim();
#else
				if (OperatingSystem.IsWindows())
				{
					using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");

					if (key?.GetValue("ProcessorNameString") is string name)
						return name.Trim();
				}
				else if (File.Exists("/proc/cpuinfo"))
				{
					var line = File.ReadLines("/proc/cpuinfo").FirstOrDefault(l => l.StartsWith("model name"));

					if (line != null)
						return line.Substring(line.IndexOf(':') + 1).Trim();
				}
#endif
			}
			catch
			{
				// The CPU name is informational only.
			}

			return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
		}
	}
}
