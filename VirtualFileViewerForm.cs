using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VirtualFileViewer
{
    public partial class VirtualFileViewerForm : Form
    {
        private OpenFileDialog fileDialog;
        private FolderBrowserDialog browserDialog;
        private SaveFileDialog saveFileDialog;
        private VirtualFolder rootFile = null;

        private TreeNode lastSelectedNode = null;

        public VirtualFileViewerForm()
        {
            InitializeComponent();

            fileDialog = new OpenFileDialog()
            {
                FileName = "Select a file",
                Filter = "Virtual text file (*.txt)|*.txt",
                Title = "Select virtual text file",
            };

            browserDialog = new FolderBrowserDialog()
            {
                Description = "Select extract directory",
                ShowNewFolderButton = false,
            };

            saveFileDialog = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = ".txt",
                Filter = "Virtual text file (*.txt)|*.txt",
                CheckPathExists = true,
            };
        }

        private void InitVirtualFiles(string filePath)
        {
            rootFile = VirtualFolder.CreateVirtualFile(filePath);
        }

        private void UpdateFileTreeView()
        {
            fileTreeView.Nodes.Clear();

            fileTreeView.Nodes.Add(rootFile.CreateTreeNode());
            fileTreeView.Nodes[0].ExpandAll();

            lastSelectedNode = fileTreeView.Nodes[0];
            fileTreeView.SelectedNode = lastSelectedNode;
            fileTreeView.Focus();
        }

        public void FindNext(string findStr)
        {
            findStr = findStr.ToLower();
            var startNode = lastSelectedNode;
            do
            {
                lastSelectedNode = GetNextNode(lastSelectedNode, false);
                if (lastSelectedNode.Text.ToLower().Contains(findStr) == true)
                {
                    fileTreeView.SelectedNode = lastSelectedNode;
                    break;
                }
            }
            while (startNode != lastSelectedNode);

            lastSelectedNode = fileTreeView.SelectedNode;
            fileTreeView.Focus();
        }

        private TreeNode GetNextNode(TreeNode targetNode, bool ignoreChild)
        {
            if (ignoreChild == false)
            {
                if (targetNode.Nodes.Count > 0)
                {
                    return targetNode.Nodes[0];
                }
            }

            var nextNode = targetNode.NextNode;
            if (nextNode == null)
            {
                if (targetNode.Parent == null)
                {
                    return targetNode;
                }
                else
                {
                    return GetNextNode(targetNode.Parent, true);
                }
            }
            return nextNode;
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (textBoxFilePath.Text.Equals(fileDialog.FileName) == false)
                {
                    textBoxFilePath.Text = fileDialog.FileName;
                    InitVirtualFiles(fileDialog.FileName);
                    UpdateFileTreeView();
                }
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    VirtualFolder.CreateTextFile(browserDialog.SelectedPath, saveFileDialog.FileName);
                    MessageBox.Show("Extract complete!");
                }
            }
        }

        private void BtnFind_Click(object sender, EventArgs e)
        {
            if (lastSelectedNode == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(textBoxFindStr.Text) == true)
            {
                return;
            }
            FindNext(textBoxFindStr.Text);
        }

        private void fileTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            lastSelectedNode = fileTreeView.SelectedNode;
        }

        private void textBoxFindStr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (string.IsNullOrEmpty(textBoxFindStr.Text) == true)
                {
                    return;
                }
                FindNext(textBoxFindStr.Text);
            }
        }
    }
}
