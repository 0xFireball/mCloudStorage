/*
 */
using System;
using System.Windows.Forms;

namespace mCloudStorage
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			if (Properties.Settings.Default.upgradeRequired)
			{
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.upgradeRequired = false;
			}
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new LoginForm());
		}
		
	}
}
