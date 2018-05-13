#pragma warning disable 0162
//ignore the unreachable code warning... don't wanna provide code with a warning on opening the solution about something that I did by intention haha

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TreeTea;

namespace Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            /* Here are some code examples, they neither show the full potential nor are well thought
             * nor do they have a meaningful character or are compatible to each other - I made them just as an brief overview
             * Also, some of these are default anyway, but youknow its an overview */
            return;


            /* General Stuff */

            //enable double buffering
            treeTea.EnableDoubleBuffering = true;

            //enable selection by click on image
            treeTea.SelectNodeOnImageClick = true; //lets you select the node by clicking the node image

            //add an exception handler to handle all exceptions, useful if you have a dedicated error message system
            treeTea.ExceptionHandler = new Action<Exception>((ex) =>
            {
                Console.WriteLine();
                Console.WriteLine(String.Format("Exception thrown: {0}\nMessage: {1}\nStackTrace:\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace));
                Console.WriteLine();
            });


            /* MultiSelection */

            //enable multiselection
            treeTea.IsMultiSelectionEnabled = true;

            //set multiselection mode, ie. which nodes can be selected simultaneously
            treeTea.MultiSelectionMode = TreeTeaView.MultiSelectionRestriction.SameLevelAndParent;

            //Assign some tags for the multiselection test with sametag
            treeTea.Nodes[1].Nodes[0].Tag = new ListViewItem();
            treeTea.Nodes[1].Nodes[1].Tag = new ImageList();
            treeTea.Nodes[1].Nodes[2].Tag = new ListViewItem();
            treeTea.Nodes[2].Nodes[0].Tag = new ListViewItem();

            //set some nodes as selected
            treeTea.SelectedNodes = new List<TreeNode>() { treeTea.Nodes[0], treeTea.Nodes[0].Nodes[1], treeTea.Nodes[1].Nodes[1], treeTea.Nodes[2], treeTea.Nodes[2].Nodes[0] };
            //that assignment will only select Node0 and Node2, because of the MultiselectionMode!

            //set a single node
            treeTea.SelectedNode = treeTea.Nodes[1];


            /* TriState */

            //enable TriState
            treeTea.IsTriStateEnabled = true;

            //enable checkboxes
            treeTea.CheckBoxes = true;

            //set mixednodes mode, like, should mixed state be checked, unchecked or automatic (=mixed will not change the current)
            treeTea.MixedNodesMode = TreeTeaView.MixedStateMode.Automatic;

            //supress expanding/collapsing treenode if double clicked the checkbox
            treeTea.SupressCheckboxDoubleClick = true; //default anyway

            //function to only enable checkbox for some specific nodes
            treeTea.FuncIsCheckboxEnabledForNode = new Func<TreeNode, bool>((node) =>
            {
                //return node.Index % 2 == 1;
                return node.Text.StartsWith("Node2") || node.Text == "Node11" || node.Text == "Node36" || node.Text == "Node37";
            });

            //hide the checkbox of a single node
            treeTea.HideCheckbox(treeTea.Nodes[2]); //since the FuncIsCheckboxEnabledForNode property is set, this only works, since this node has the checkbox enabled

            //show all nodes checkboxes again
            treeTea.ShowAllCheckboxes();

            //custom aftercheck event
            treeTea.AfterCheck += TreeTea_AfterCheck;


            /* More to Come */

            //yeah, idk maybe
            //use the treetea and provide some suggestions, in case you need more or have something i could/should improve
            //sebi@holzers.de
            //http://holzers.de/
        }

        private void button1_Click(object sender, EventArgs e)
        {
            treeTea.CheckBoxes = !treeTea.CheckBoxes;
            Console.WriteLine(String.Format("Checkboxes are {0}", treeTea.CheckBoxes ? "enabled" : "disabled"));
        }

        //private int mode = 0;
        private void button2_Click(object sender, EventArgs e)
        {
            treeTea.IsTriStateEnabled = !treeTea.IsTriStateEnabled;
            Console.WriteLine(String.Format("TriState is {0}", treeTea.IsTriStateEnabled ? "enabled" : "disabled"));

            //treeTea.MultiSelectionMode = TreeTeaView.MultiSelectionRestriction.SameLevelAndParent;
            //treeTea.SelectedNodes = new List<TreeNode>() { treeTea.Nodes[0], treeTea.Nodes[0].Nodes[1], treeTea.Nodes[1].Nodes[1], treeTea.Nodes[2], treeTea.Nodes[2].Nodes[0] };

            //if (treeTea.FuncIsCheckboxEnabledForNode == null)
            //    treeTea.FuncIsCheckboxEnabledForNode = new Func<TreeNode, bool>((node) =>
            //    {
            //    //return node.Index % 2 == 1;
            //    return node.Text.StartsWith("Node2") || node.Text == "Node11" || node.Text == "Node36" || node.Text == "Node37";
            //    });
            //else treeTea.FuncIsCheckboxEnabledForNode = null;
        }

        private void TreeTea_AfterCheck(object sender, CheckedStateChangedEventArgs e)
        {
            Console.WriteLine(String.Format("The node {0} is now {2} has changed its checkedState: {1}", e.Node.Text, /*e.CheckedState.ToString()*/ e.Node.GetCheckedState(), e.Node.Checked ? "checked" : "unchecked"));
        }
    }
}