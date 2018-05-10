using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TreeTea
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            treeTea.Nodes[1].Nodes[0].Tag = new ListViewItem();
            treeTea.Nodes[1].Nodes[1].Tag = new ImageList();
            treeTea.Nodes[1].Nodes[2].Tag = new ListViewItem();
            treeTea.Nodes[2].Nodes[0].Tag = new ListViewItem();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            treeTea.MixedNodesMode = TreeTeaView.MixedStateMode.Automatic;

            //treeTea.SelectedNodes = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            treeTea.MixedNodesMode = (treeTea.MixedNodesMode == TreeTeaView.MixedStateMode.Checked) ? TreeTeaView.MixedStateMode.Unchecked : TreeTeaView.MixedStateMode.Checked;

            //List<TreeNode> list = new List<TreeNode>();
            //list.Add(treeTea.Nodes[0]);
            //list.Add(treeTea.Nodes[2]);
            //list.Add(treeTea.Nodes[3]);

            //treeTea.SelectedNodes = list;
        }
    }
}