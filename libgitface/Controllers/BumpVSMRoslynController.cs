using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace libgitface
{
	public class BumpVSMRoslynController
	{
		string AutoBumpBranchName => $"{Designer.BranchName}-update-vsm-roslyn-nuget";

		static readonly Uri VisualStudioMacUri = new Uri ("https://github.com/mono/monodevelop");

		public GitClient Designer {
			get;
		}

		public GitClient VisualStudioMac {
			get;
		}

		public string DesignerRoslynVersionFile => "build/SharedNuGets.props";
		public string VSMRoslynVersionFile => "main/Directory.Build.props";

		public BumpVSMRoslynController (GitClient client)
		{
			Designer = client;
			VisualStudioMac = client.WithRepository (new Repository (VisualStudioMacUri));
		}

		XElement GetDesignerRoslynElement (XElement element)
		{
			return element.Descendants ()
				          .Where (t => t.Name.LocalName == "PackageReference")
				          .Where (t => t.Attribute ("Include")?.Value == "Microsoft.CodeAnalysis")
				          .Where (e => e.Attribute ("Condition")?.Value?.Contains ("XamarinStudio") ?? false)
				          .Single ();
		}

		async Task<string> GetDesignerRoslynVersion ()
		{
			var content = await Designer.GetFileContent (DesignerRoslynVersionFile);
			var root = XDocument.Parse (content).Root;
			return GetDesignerRoslynElement (root)
				.Attribute ("Version")
				.Value;
		}

		async Task<string> GetVSMRoslynVersion ()
		{
			var content = await VisualStudioMac.GetFileContent (VSMRoslynVersionFile);
			var doc = XDocument.Parse (content).Root;
			return doc.Descendants ()
				      .Where (t => t.Name == "NuGetVersionRoslyn")
				      .Single ()
				      .Value;
		}

		public async Task<bool> NeedsUpdate ()
		{
			return await GetVSMRoslynVersion () != await GetDesignerRoslynVersion ();
		}

		public async Task UpdateDesignerRoslynVersion ()
		{
			var currentVersion = await GetDesignerRoslynVersion ();
			var desiredVersion = await GetVSMRoslynVersion ();
			if (currentVersion == desiredVersion)
				return;

			var content = await Designer.GetFileContent (DesignerRoslynVersionFile);
			var doc = XDocument.Parse (content);
			GetDesignerRoslynElement (doc.Root)
				.SetAttributeValue ("Version", desiredVersion);

			var title = $"[{Designer.BranchName}] Bump Roslyn nuget for VSM";
			var body = $"Updating from {currentVersion} to {desiredVersion}";

			var newFile = doc.ToString (SaveOptions.None);
			if (doc.Declaration != null)
				newFile = doc.Declaration + "\n" + newFile;

			// Create a branch from tip of our tracked branch
			if (await Designer.BranchExists (AutoBumpBranchName))
				await  Designer.DeleteBranch (AutoBumpBranchName);
			var head = await Designer.GetHeadSha ();
			var client = await Designer.CreateBranch (AutoBumpBranchName, head);

			// Update the content on the new branch.
			await client.UpdateFileContent (title, body, DesignerRoslynVersionFile, newFile);

			// Issue the PullRequest against the original branch
			await Designer.CreatePullRequest (AutoBumpBranchName, title, body);
		}
	}
}