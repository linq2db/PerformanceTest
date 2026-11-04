using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Data.Sqlite;

namespace ChartTool
{
	/// <summary>
	/// Turns a harness <c>Result.sqlite</c> into one SVG chart per test group.
	/// Usage: ChartTool &lt;Result.sqlite&gt; &lt;output directory&gt; [--platform "NET 10.0"].
	/// If a file <c>note.txt</c> lies beside the database, its text is printed under the chart title
	/// (used to mark provisional charts).
	/// The output depends only on the data and the arguments, so the same input gives identical files.
	/// </summary>
	static class Program
	{
		const double TicksPerSecond = 10_000_000.0;

		static int Main(string[] args)
		{
			string? db = null, outDir = null, platform = null;

			for (var i = 0; i < args.Length; i++)
			{
				var a = args[i];

				if (a == "--platform" && i + 1 < args.Length)
					platform = args[++i];
				else if (!a.StartsWith("--", StringComparison.Ordinal) && db == null)
					db = a;
				else if (!a.StartsWith("--", StringComparison.Ordinal) && outDir == null)
					outDir = a;
				else
				{
					Console.Error.WriteLine("Unknown or incomplete argument.");
					Console.Error.WriteLine("Usage: ChartTool <Result.sqlite> <output directory> [--platform \"NET 10.0\"]");
					return 2;
				}
			}

			if (db == null || outDir == null)
			{
				Console.Error.WriteLine("Usage: ChartTool <Result.sqlite> <output directory> [--platform \"NET 10.0\"]");
				return 2;
			}

			if (!File.Exists(db))
			{
				Console.Error.WriteLine($"File not found: {db}");
				return 2;
			}

			var runs = ReadRuns(db);

			var noteFile = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(db))!, "note.txt");
			var note     = File.Exists(noteFile) ? File.ReadAllText(noteFile).Trim() : null;

			// Charts show one environment: the platform given, or that of the newest run.
			var candidates = runs.Where(r => r.Platform != "All").ToList();

			if (candidates.Count == 0)
			{
				Console.Error.WriteLine("No test runs in the database.");
				return 1;
			}

			var newest = platform == null
				? candidates[^1]
				: candidates.LastOrDefault(r => r.Platform == platform);

			if (newest == null)
			{
				Console.Error.WriteLine($"No runs for platform '{platform}'.");
				return 1;
			}

			// Runs of the same platform with the same environment and library versions are samples of one measurement.
			var reference = Describe(newest.Info);
			var selected  = candidates.Where(r => r.Platform == newest.Platform && Describe(r.Info) == reference).ToList();
			foreach (var skipped in candidates.Where(r => !selected.Contains(r)))
				Console.Error.WriteLine($"Skipped run {skipped.Id} ({skipped.Platform}, {skipped.Name}, {skipped.CreatedOn.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}): its platform or environment differs from the newest run's.");

			var selectedIds = selected.Select(r => r.Id).ToHashSet();

			var samples = ReadSamples(db, selectedIds);

			Directory.CreateDirectory(outDir);

			var count = 0;

			foreach (var groupName in selected.Select(r => r.Name).Distinct().OrderBy(n => n, StringComparer.Ordinal))
			{
				var groupRuns = selected.Where(r => r.Name == groupName).ToList();
				var chart     = BuildGroup(groupName, groupRuns, samples, newest.Info, note);
				var path      = Path.Combine(outDir, Slug(groupName) + ".svg");

				File.WriteAllText(path, chart, new UTF8Encoding(false));
				Console.WriteLine($"{path}: {groupRuns.Count} run(s)");
				count++;
			}

			return count > 0 ? 0 : 1;
		}

		sealed class Run
		{
			public int Id;
			public string Platform = "";
			public string Name = "";
			public DateTime CreatedOn;
			public SortedDictionary<string, string> Info = new(StringComparer.Ordinal);
		}

		// One method of one group, identified like the harness does: name, repeat count and take count.
		sealed record MethodKey(string Name, int Repeat, int? Take);

		sealed class Sample
		{
			public int RunId;
			public MethodKey Method = null!;
			public string Provider = "";
			public double Seconds;
		}

		static List<Run> ReadRuns(string db)
		{
			var result = new List<Run>();

			using var con = new SqliteConnection($"Data Source={db};Mode=ReadOnly;Pooling=False");
			con.Open();

			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = "SELECT ID, Platform, Name, CreatedOn FROM TestRun ORDER BY ID";

				using var rd = cmd.ExecuteReader();

				while (rd.Read())
				{
					result.Add(new Run
					{
						Id        = rd.GetInt32(0),
						Platform  = rd.IsDBNull(1) ? "" : rd.GetString(1),
						Name      = rd.IsDBNull(2) ? "" : rd.GetString(2),
						CreatedOn = DateTime.Parse(rd.GetString(3), CultureInfo.InvariantCulture),
					});
				}
			}

			var byId = result.ToDictionary(r => r.Id);

			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = "SELECT TestRunID, Name, Value FROM TestRunInfo";

				using var rd = cmd.ExecuteReader();

				while (rd.Read())
				{
					if (byId.TryGetValue(rd.GetInt32(0), out var run))
						run.Info[rd.GetString(1)] = rd.IsDBNull(2) ? "" : rd.GetString(2);
				}
			}

			return result;
		}

		static List<Sample> ReadSamples(string db, HashSet<int> runIds)
		{
			var result = new List<Sample>();

			using var con = new SqliteConnection($"Data Source={db};Mode=ReadOnly;Pooling=False");
			con.Open();

			using var cmd = con.CreateCommand();

			// Both the times and the ticks column hold TimeSpan ticks (100 ns); runs from Windows and Linux alike.
			cmd.CommandText =
				"""
				SELECT m.TestRunID, m.Name, m.Repeat, m."Take", s.Provider, s.Ticks
				FROM TestStopwatch s JOIN TestMethod m ON m.ID = s.TestMethodID
				ORDER BY m.TestRunID, m.ID, s.ID
				""";

			using var rd = cmd.ExecuteReader();

			while (rd.Read())
			{
				var runId = rd.GetInt32(0);

				if (!runIds.Contains(runId))
					continue;

				result.Add(new Sample
				{
					RunId    = runId,
					Method   = new MethodKey(rd.GetString(1), rd.GetInt32(2), rd.IsDBNull(3) ? null : rd.GetInt32(3)),
					Provider = rd.GetString(4),
					Seconds  = rd.GetInt64(5) / TicksPerSecond,
				});
			}

			return result;
		}

		static string Describe(SortedDictionary<string, string> info)
		{
			return string.Join("\n", info.Select(p => p.Key + "=" + p.Value));
		}

		// -------------------------------------------------------------------------------------------------
		// Layout

		const int    PanelWidth  = 540;
		const int    Gap         = 20;
		const int    Margin      = 20;
		const int    LabelWidth  = 125;
		const int    BarArea     = 240;
		const int    RowHeight   = 16;
		const int    BarHeight   = 11;
		const int    PanelHead   = 26;
		const int    PanelGap    = 18;
		const string Font        = "font-family=\"Segoe UI, Helvetica, Arial, sans-serif\"";

		// Display order and colour of known providers: baselines grey, micro-ORMs teal, linq2db blue, EF Core orange.
		static readonly (string Name, string Color)[] Providers =
		{
			("AdoNet",               "#6b7280"),
			("AdoNet Param",         "#9ca3af"),
			("Dapper",               "#0f766e"),
			("PetaPoco",             "#14b8a6"),
			("L2DB Sql",             "#1e3a8a"),
			("L2DB Linq",            "#2563eb"),
			("L2DB Linq Inline",     "#60a5fa"),
			("L2DB Compiled",        "#4f46e5"),
			("L2DB Compiled Inline", "#818cf8"),
			("EF Core Sql",          "#9a3412"),
			("EF Core Linq",         "#ea580c"),
			("EF Core Compiled",     "#fb923c"),
		};

		const string OtherColor = "#a855f7";

		static string BuildGroup(string groupName, List<Run> runs, List<Sample> all, SortedDictionary<string, string> info, string? note)
		{
			var runIds  = runs.Select(r => r.Id).ToHashSet();
			var samples = all.Where(s => runIds.Contains(s.RunId)).ToList();

			var methods = new List<MethodKey>();

			foreach (var s in samples)
				if (!methods.Contains(s.Method))
					methods.Add(s.Method);

			var providerNames = samples.Select(s => s.Provider).Distinct().ToList();
			var order = providerNames
				.OrderBy(p => { var i = Array.FindIndex(Providers, x => x.Name == p); return i < 0 ? int.MaxValue : i; })
				.ThenBy(p => p, StringComparer.Ordinal)
				.ToList();

			var columns   = methods.Count == 1 ? 1 : 2;
			var rowsInGrid = (methods.Count + columns - 1) / columns;
			var panelHeight = PanelHead + order.Count * RowHeight + 4;
			var width     = Margin * 2 + columns * PanelWidth + (columns - 1) * Gap;

			var headerHeight = note != null ? 78 : 62;
			var captionLines = Caption(info, runs);
			var captionHeight = captionLines.Count * 15 + 14;
			var height = headerHeight + rowsInGrid * (panelHeight + PanelGap) + captionHeight;

			var sb = new StringBuilder();

			var withBase = methods.Count(m => samples.Any(x => x.Method == m && x.Provider == "AdoNet"));
			var subtitle = "Median time of one call (shorter is better)";

			if (withBase == methods.Count)
				subtitle += "; the number after it is the ratio to the \"AdoNet\" baseline.";
			else if (withBase > 0)
				subtitle += "; where the \"AdoNet\" baseline was measured, the number after it is the ratio to it.";
			else
				subtitle += ".";

			sb.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\">\n");
			sb.Append(CultureInfo.InvariantCulture, $"<title>{Esc(groupName)}</title>\n");
			sb.Append(CultureInfo.InvariantCulture, $"<rect width=\"{width}\" height=\"{height}\" fill=\"#ffffff\"/>\n");
			sb.Append(CultureInfo.InvariantCulture, $"<text x=\"{Margin}\" y=\"32\" {Font} font-size=\"22\" font-weight=\"600\" fill=\"#111827\">{Esc(groupName)}</text>\n");
			sb.Append(CultureInfo.InvariantCulture, $"<text x=\"{Margin}\" y=\"50\" {Font} font-size=\"12\" fill=\"#4b5563\">{Esc(subtitle)}</text>\n");

			if (note != null)
				sb.Append(CultureInfo.InvariantCulture, $"<text x=\"{Margin}\" y=\"66\" {Font} font-size=\"12\" font-weight=\"600\" fill=\"#b91c1c\">{Esc(note)}</text>\n");

			for (var m = 0; m < methods.Count; m++)
			{
				var col = m % columns;
				var row = m / columns;
				var x   = Margin + col * (PanelWidth + Gap);
				var y   = headerHeight + row * (panelHeight + PanelGap);

				AppendPanel(sb, x, y, methods[m], samples, order);
			}

			var cy = headerHeight + rowsInGrid * (panelHeight + PanelGap) + 6;

			foreach (var line in captionLines)
			{
				sb.Append(CultureInfo.InvariantCulture, $"<text x=\"{Margin}\" y=\"{cy + 9}\" {Font} font-size=\"11\" fill=\"#4b5563\">{Esc(line)}</text>\n");
				cy += 15;
			}

			sb.Append("</svg>\n");

			return sb.ToString();
		}

		static void AppendPanel(StringBuilder sb, int x, int y, MethodKey method, List<Sample> samples, List<string> order)
		{
			var medians = new Dictionary<string, double>();

			foreach (var p in order)
			{
				var values = samples.Where(s => s.Method == method && s.Provider == p).Select(s => s.Seconds).ToList();

				if (values.Count > 0)
					medians[p] = Median(values) / method.Repeat;
			}

			var max  = medians.Count == 0 ? 1.0 : medians.Values.Max();
			var @base = medians.TryGetValue("AdoNet", out var b) ? b : (double?)null;

			sb.Append(CultureInfo.InvariantCulture, $"<text x=\"{x}\" y=\"{y + 14}\" {Font} font-size=\"13\" font-weight=\"600\" fill=\"#111827\">{Esc(Title(method))}</text>\n");

			for (var i = 0; i < order.Count; i++)
			{
				var p  = order[i];
				var ry = y + PanelHead + i * RowHeight;

				sb.Append(CultureInfo.InvariantCulture, $"<text x=\"{x + LabelWidth}\" y=\"{ry + 10}\" {Font} font-size=\"11\" text-anchor=\"end\" fill=\"#111827\">{Esc(p)}</text>\n");

				if (!medians.TryGetValue(p, out var t))
					continue;

				var w = Math.Max(1.0, t / max * BarArea);
				var bx = x + LabelWidth + 6;
				var label = FormatTime(t) + (@base != null ? "  " + (t / @base.Value).ToString("0.00", CultureInfo.InvariantCulture) + "x" : "");

				sb.Append(CultureInfo.InvariantCulture, $"<rect x=\"{bx}\" y=\"{ry + 1}\" width=\"{F2(w)}\" height=\"{BarHeight}\" fill=\"{ColorOf(p)}\"/>\n");
				sb.Append(CultureInfo.InvariantCulture, $"<text x=\"{F2(bx + w + 5)}\" y=\"{ry + 10}\" {Font} font-size=\"10\" fill=\"#374151\">{Esc(label)}</text>\n");
			}
		}

		static string Title(MethodKey m)
		{
			var calls = m.Repeat.ToString("N0", CultureInfo.InvariantCulture);
			var take  = m.Take is { } t ? $", take {t.ToString("N0", CultureInfo.InvariantCulture)}" : "";

			return $"{m.Name} ({calls} {(m.Repeat == 1 ? "call" : "calls")}{take})";
		}

		static List<string> Caption(SortedDictionary<string, string> info, List<Run> runs)
		{
			string Get(string key)
			{
				return info.TryGetValue(key, out var v) ? v : "n/a";
			}

			var date = runs.Min(r => r.CreatedOn).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

			return new List<string>
			{
				$"Runtime {Get("Runtime")}, {Get("OS")}, {Get("CPU")}, GC {Get("GC")}.",
				$"SQL Server {Get("Database server")}.",
				$"linq2db {Get("linq2db")}. Measured {date}; median of {runs.Count} process run(s).",
			};
		}

		static string ColorOf(string provider)
		{
			var i = Array.FindIndex(Providers, x => x.Name == provider);

			return i < 0 ? OtherColor : Providers[i].Color;
		}

		static double Median(List<double> values)
		{
			var s = values.OrderBy(v => v).ToList();
			var n = s.Count;

			return n % 2 == 1 ? s[n / 2] : (s[n / 2 - 1] + s[n / 2]) / 2;
		}

		static string FormatTime(double seconds)
		{
			// Round to three significant digits first so that the unit is chosen from the printed value
			// (999.6 us is "1.00 ms", not "1000 us").
			var r = Round3(seconds);

			if (r < 1e-3)
				return Sig(Round3(r * 1e6)) + " µs";

			if (r < 1)
				return Sig(Round3(r * 1e3)) + " ms";

			return Sig(r) + " s";
		}

		static double Round3(double v)
		{
			if (v <= 0)
				return 0;

			return Math.Round(v, 2 - (int)Math.Floor(Math.Log10(v)), MidpointRounding.AwayFromZero);
		}

		// Three significant digits; the value is already rounded by Round3.
		static string Sig(double v)
		{
			if (v >= 100)
				return v.ToString("0", CultureInfo.InvariantCulture);

			if (v >= 10)
				return v.ToString("0.0", CultureInfo.InvariantCulture);

			return v.ToString("0.00", CultureInfo.InvariantCulture);
		}

		static string F2(double v)
		{
			return v.ToString("0.##", CultureInfo.InvariantCulture);
		}

		static string Slug(string name)
		{
			var sb = new StringBuilder();

			foreach (var c in name.ToLowerInvariant())
				sb.Append(char.IsAsciiLetterOrDigit(c) ? c : '-');

			var slug = sb.ToString().Trim('-');

			return slug.Length > 0 ? slug : "group";
		}

		static string Esc(string s)
		{
			return s.Replace("&", "&amp;", StringComparison.Ordinal).Replace("<", "&lt;", StringComparison.Ordinal).Replace(">", "&gt;", StringComparison.Ordinal).Replace("\"", "&quot;", StringComparison.Ordinal);
		}
	}
}
