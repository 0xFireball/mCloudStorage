/*
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using CG.Web.MegaApiClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mCloudStorage
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class CloudDrive : MetroForm
	{
		public static MegaApiClient mclient = new MegaApiClient();
		INode mcNode;
		Dictionary<string,INode> nodeDict = new Dictionary<string, INode>();
		
		public CloudDrive()
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
		async void CloudDriveLoad(object sender, EventArgs e)
		{
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
					nodeDict.Add(fileName,node);
				}
				RefreshView();
			}
			catch (Exception e)
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
                        nodeDict.Add(node.Name,node);
					}
				}
				RefreshView();
                label1.Text = "Ready.";
            }
			catch (Exception ex)
			{
				MessageBox.Show("An error occurred refreshing nodes.");
			}
		}
		void Button1Click(object sender, EventArgs e)
		{
			try
			{
				mclient.Logout();
				Properties.Settings.Default.email="";
				Properties.Settings.Default.password="";
				Properties.Settings.Default.Save();
				this.Close();
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
	}
}
