/*
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using CG.Web.MegaApiClient;
using System.Threading;
using System.Threading.Tasks;

namespace mCloudStorage
{
	/// <summary>
	/// Description of LoginForm.
	/// </summary>
	public partial class LoginForm : Form
	{
		public LoginForm()
		{
			Form.CheckForIllegalCrossThreadCalls = false;
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		async void AttemptLogin(object sender, EventArgs e)
		{
			bool canExit = false;
            bool runCDrive = false;
			label3.Text = "Logging in...";
			button1.Enabled = false;
			await Task.Run(()=>{		               	
				string un = textBox1.Text;
				string pw = textBox2.Text;
				try
				{
					CloudDrive.mclient.Login(un,pw);
					Properties.Settings.Default.email = un;
					Properties.Settings.Default.password = pw;
					Properties.Settings.Default.Save();
					label3.Text = "Login success.";
					CloudDrive.loggedIn = true;
					this.Close();
				}
				catch (ApiException apie)
				{
					MessageBox.Show("Server rejected the credentials.");
					label3.Text = "Login failure.";
				}
				button1.Enabled = true;
			});
		}
		async void AttemptRegister(object sender, EventArgs e)
		{
			/*
			bool canExit = false;
			label3.Text = "Registering...";
			button1.Enabled = false;
			await Task.Run(()=>{		               	
				string un = textBox1.Text;
				string pw = textBox2.Text;
				try
				{
					
					msvc.Register(un, pw, ()=>{
					              	//Success
					              	Properties.Settings.Default.email = un;
					Properties.Settings.Default.password = pw;
					Properties.Settings.Default.Save();
					label3.Text = "Register success.";
					MessageBox.Show("Your account has been registered! Please log in now.");
					              }, e=>{
					              	
					              });
					this.Hide();
					new CloudDrive().ShowDialog();
					canExit = true;
				}
				catch (ApiException apie)
				{
					MessageBox.Show("Server rejected the credentials.");
					label3.Text = "Login failure.";
				}
				button1.Enabled = true;
			});
			if (canExit)
			{
				this.Close();
			}
			*/
		}
		void LoginFormLoad(object sender, EventArgs e)
		{
			bool emptyPass = string.IsNullOrWhiteSpace(Properties.Settings.Default.password);
			bool emptyUn = string.IsNullOrWhiteSpace(Properties.Settings.Default.email);
			if (!emptyPass&&!emptyUn)
			{
                this.Opacity = 0;
                textBox1.Text = Properties.Settings.Default.email;
				textBox2.Text = Properties.Settings.Default.password;
				AttemptLogin(null,null);
			}
		}

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://mega.nz/#register");
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            if (this.Opacity == 0)
                this.Hide();
        }
		void LoginFormFormClosing(object sender, FormClosingEventArgs e)
		{
            /*
            if (!CloudDrive.loggedIn)
			    Application.Exit();
            */
		}
    }
}
