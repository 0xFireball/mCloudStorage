/*
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MetroFramework.Forms;
using CG.Web.MegaApiClient;
using System.Linq;
using System.Threading.Tasks;

namespace mCloudStorage
{
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class CloudDrive : Form
	{
		public static MegaApiClient mclient = new MegaApiClient();
		INode mcNode;
		Dictionary<string,INode> nodeDict = new Dictionary<string, INode>();
		public static bool loggedIn = false;
		
		public CloudDrive()
		{
			Form.CheckForIllegalCrossThreadCalls = false;
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			//this.Opacity = 0;
			WaitForLogin();
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		async void WaitForLogin()
		{
			while (!loggedIn)
			{
				System.Threading.Thread.Sleep(100);
			}
			//await Task.Run(()=>new LoginForm().ShowDialog());
		}
		async void CloudDriveLoad(object sender, EventArgs e)
		{						
			//this.Opacity = 1.0;
			this.Show();
			this.WindowState = FormWindowState.Minimized;
			this.WindowState = FormWindowState.Normal;
			label1.Text = "Loading Nodes...";
			await Task.Run(()=>LoadData());
			label1.Text = "Ready.";
		}
		void LoadData()
		{
			try
			{
				label1.Text = "Fetching Nodes...";
				var nodes = mclient.GetNodes().ToList();
				INode root = nodes.Single(n => n.Type == NodeType.Root);
				bool hasmCloudFolder = false;
				label1.Text = "Scanning Nodes...";
				foreach (var node in nodes)
				{
					if (node.Name=="mCloud")
						hasmCloudFolder = true;
				}
				if (!hasmCloudFolder)
				{
					label1.Text = "Adding Nodes...";				
					mclient.CreateFolder("mCloud",root);
					nodes = mclient.GetNodes().ToList();
				}
				label1.Text = "Processing Nodes...";
				mcNode = nodes.Single(n => n.Name=="mCloud");
				List<INode> fileNodes = new List<INode>();
				foreach (var node in nodes)
				{
					if (node.ParentId==mcNode.Id)
					{
						fileNodes.Add(node);
					}
				}
				foreach (var node in fileNodes)
				{
					string fileName = node.Name;
                    if (!nodeDict.ContainsKey(fileName))
					    nodeDict.Add(fileName,node);
				}
				RefreshView();
			}
			catch (Exception ex)
			{
				MessageBox.Show("A fatal error occurred loading the Cloud Drive. mCloudStorage will now exit.");
				this.Close();
			}
		}
		void RefreshView()
		{
            listView1.Items.Clear();
			foreach (string filename in nodeDict.Keys)
			{
				listView1.Items.Add(filename);
			}
		}
		void RefreshNodes()
		{
			try
			{
				nodeDict = new Dictionary<string, INode>();
				var nodes = mclient.GetNodes().ToList();
				label1.Text = "Refreshing Nodes...";
				mcNode = nodes.Single(n => n.Name=="mCloud");
				
				foreach (var node in nodes)
				{
					if (node.ParentId==mcNode.Id)
					{
                        if (!nodeDict.ContainsKey(node.Name))
                            nodeDict.Add(node.Name,node);
					}
				}
				RefreshView();
                label1.Text = "Ready.";
            }
			catch (Exception ex)
			{
				MessageBox.Show("An error occurred refreshing nodes.");
                label1.Text = "Node Refresh Error.";
            }
		}
        bool forceClose = false;
		void Button1Click(object sender, EventArgs e)
		{
			try
			{
				mclient.Logout();
				Properties.Settings.Default.email="";
				Properties.Settings.Default.password="";
				Properties.Settings.Default.Save();
				this.Close();
                forceClose = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("An error occurred while logging out.");
			}
		}
		async void Button2Click(object sender, EventArgs e)
		{
			try
			{
				label1.Text = "Uploading...";
				OpenFileDialog ofd = new OpenFileDialog();
				ofd.Multiselect = true;
				DialogResult dr = ofd.ShowDialog();
				if (dr == DialogResult.OK)
				{
					foreach (string fileName in ofd.FileNames)
					{
						await Task.Run(()=>mclient.Upload(fileName, mcNode));
					}
				}
				label1.Text = "Uploaded Successfully.";
                RefreshNodes();
			}
			catch (Exception ex)
			{
				label1.Text = "Upload Error.";
				MessageBox.Show("An error occurred uploading the file.");
			}
		}
		async void Button3Click(object sender, EventArgs e)
		{
			ListViewItem lvitem = null;
			try
			{
				lvitem = listView1.Items[listView1.SelectedIndices[0]];
			}
			catch
			{
				return;
			}
			if (lvitem==null)
				return;
			try
			{
				label1.Text = "Downloading...";
				SaveFileDialog sfd = new SaveFileDialog();
				DialogResult dr = sfd.ShowDialog();
				if (dr == DialogResult.OK)
				{
					string fp = sfd.FileName;
					await Task.Run(()=> mclient.DownloadFile(nodeDict[lvitem.Text],fp));
				}
				label1.Text = "Downloaded Successfully.";
			}
			catch (Exception ex)
			{
				label1.Text = "Download Error.";
				MessageBox.Show("An error occurred downloading the file.");
			}
		}

        private async void button4_Click(object sender, EventArgs e)
        {
            await Task.Run(()=> RefreshNodes());
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            ListViewItem lvitem = null;
            try
            {
                lvitem = listView1.Items[listView1.SelectedIndices[0]];
            }
            catch
            {
                return;
            }
            if (lvitem == null)
                return;
            try
            {
                label1.Text = "Deleting...";
                DialogResult dr = MessageBox.Show("Are you sure you want to delete this item?","Warning",MessageBoxButtons.OKCancel);
                DialogResult tr = MessageBox.Show("Do you want to move this item to the trash?","Alert",MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.OK && tr != DialogResult.Cancel)
                {
                    await Task.Run(() => { mclient.Delete(nodeDict[lvitem.Text], tr == DialogResult.Yes); RefreshNodes(); });
                }
                label1.Text = "Deleted Successfully.";
            }
            catch
            {
                label1.Text = "Delete Error.";
                MessageBox.Show("An error occurred deleting the file.");
            }
        }

        private void CloudDrive_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !forceClose;
            this.Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Minimized;
            this.WindowState = FormWindowState.Normal;
        }
		void CloudDriveShown(object sender, EventArgs e)
		{
			//this.Hide();
		}
    }
}
