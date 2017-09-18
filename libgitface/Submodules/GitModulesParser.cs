using System;
using System.Collections.Generic;
using System.Linq;

namespace libgitface
{
	public class GitModulesParser
	{
		public IEnumerable<SubmoduleEntry> Parse (string gitModulesFileContent)
		{
			var entries = new List<SubmoduleEntry> ();
			var lines = gitModulesFileContent
				.Split (new[] { '\n', '\r', }, StringSplitOptions.RemoveEmptyEntries)
				.Select (l => l.Trim ());

			SubmoduleEntry entry = null;
			foreach (var line in lines) {
				if (line.StartsWith ("[submodule")) {
					entry = new SubmoduleEntry {
						Branch = "master",
						Name = line.TakeBetween ('\"', '\"')
					};
					entries.Add (entry);
				} else if (line.StartsWith ("path")) {
					entry.Path = line.TakeAfter ('=');
				} else if (line.StartsWith ("url")) {
					entry.Url = line.TakeAfter ('=');
					if (entry.Url.EndsWith (".git"))
						entry.Url = entry.Url.Substring (0, entry.Url.LastIndexOf (".git"));
				} else if (line.StartsWith ("branch")) {
					entry.Branch = line.TakeAfter ('=');
				}
			}

			return entries;
		}
	}
}
