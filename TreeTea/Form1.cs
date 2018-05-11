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

            treeTea.AfterCheck += TreeTea_AfterCheck;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            treeTea.CheckBoxes = !treeTea.CheckBoxes;

            //treeTea.AfterCheck += TreeTea_AfterCheck;

            //treeTea.MixedNodesMode = TreeTeaView.MixedStateMode.Automatic;

            //treeTea.SelectedNodes = null;
        }

        private void TreeTea_AfterCheck(object sender, CheckedStateChangedEventArgs e)
        {
            Console.WriteLine(String.Format("The node {0} is now {2} has changed its checkedState: {1}", e.Node.Text, /*e.CheckedState.ToString()*/ e.Node.GetCheckedState(), e.Node.Checked ? "checked" : "unchecked"));
        }

        private int mode = 0;
        private void button2_Click(object sender, EventArgs e)
        {
            if (mode == 0)
                treeTea.MixedNodesMode = TreeTeaView.MixedStateMode.Checked;
            if (mode == 1)
                treeTea.MixedNodesMode = TreeTeaView.MixedStateMode.Unchecked;
            if (mode == 2)
                treeTea.MixedNodesMode = TreeTeaView.MixedStateMode.Automatic;

            Console.WriteLine();
            Console.WriteLine(String.Format("Mode switched to: {0}", treeTea.MixedNodesMode.ToString()));

            if (++mode >= 3) mode = 0;

            //List<TreeNode> list = new List<TreeNode>();
            //list.Add(treeTea.Nodes[0]);
            //list.Add(treeTea.Nodes[2]);
            //list.Add(treeTea.Nodes[3]);

            //treeTea.SelectedNodes = list;
        }
    }
}