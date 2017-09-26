using System;
using System.Threading.Tasks;

namespace libgitface
{
	public class MPackInstaller
	{
		public string AppBundle {
			get;
		}

		public string[] MPackFiles {
			get;
		}

		public MPackInstaller (string appBundle, string[] mpackFiles)
		{
			AppBundle = appBundle;
			MPackFiles = mpackFiles;
		}

		public async Task Install ()
		{
			await Task.Delay (10);
		}
	}
}
