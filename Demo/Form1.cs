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
            //note, that this will throw "expected" exceptions, like if you try to set mixed checked state while tristate is disabled
            //fatal exceptions, which cause severe damage to the treeview will still cause a throw.
            //This Handler is meant to catch common exceptions in critical areas
            treeTea.ExceptionHandler = new Action<Exception>((ex) =>
            {
                Console.WriteLine();
                Console.WriteLine(String.Format("Exception thrown: {0}\nMessage: {1}\nStackTrace:\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace));
                Console.WriteLine();
            });

            //save the current expansion state and save the current scroll position
            var expansionState = treeTea.GetExpansionState(out Point scrollPosition);
            //restore the expansion state
            treeTea.SetExpansionState(expansionState);
            //aand since that operation might screw the current scroll position (="which nodes are visible")
            treeTea.SetScrollPosition(scrollPosition); //you can actually pass the scrollposition with the setexpansionstate method...

            //I've added some convenient extensions
            TreeNode someNode = treeTea.Nodes[0];           //ignore this for now
            dynamic data; bool success;                     //some declarations
            TreeNode node; IEnumerable<dynamic> dataList;

            //there are various convenient methods to directly access the tag of a node like:
            data = someNode.GetTag<int>();                          //Get casted Tag directly
            success = someNode.GetTag(out data);                    //Get casted Tag and a boolean value whether cast was successful
            data = someNode.IsTagOfType<int>();                     //yeah guess what this is
            success = someNode.GetAncestorTag(out data);            //Searches all ancestors of the node for a tag of the type of data
            success = ((TreeView)treeTea)                           //get the casted tag of the currently selected node
                .GetSelectedTag(out dataList);
            success = treeTea.GetSelectedTag(out dataList);         //get the casted tags of the currently selected nodes (only treeteaview)
            success = treeTea.GetSelectedTags(out dataList);        //get the casted tags of the currently selected nodes (only treeteaview)
            success = treeTea.GetSelectedTagOfAncestor(out data);   //searches all ancestors for a tag of the given type

            //maybe you have some custom treenodes, that you can easily retrieve with the following:
            success = someNode.GetAncestorOfType(out node);             //search an ancestor of the given type (derived from TreeNode)
            success = someNode.GetAncestorWhere(                        //get an ancestor, where a custom function is true
                new Func<TreeNode, bool>((x) => x.Text == "Node21"),
                out node);
            success = treeTea.GetSelectedNode(out node);                //cast the currently selected node to a specific type (derive TreeNode)
            dataList = treeTea.GetSelectedNodesOfType<TreeNode>();       //retrieve all currently selected nodes of a specific type

            //you can also retrieve all descendants
            var descendants = treeTea.Nodes.Descendants<TreeNode>();
            descendants = treeTea.SelectedNode.Descendants<TreeNode>(new Func<TreeNode, bool>((whateverNode) => node.Text.StartsWith("Node2")));


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

            //let the selection clear, if the user expands or collapses any node, but before the BeforeExpand event
            treeTea.ClearSelectionOnExpand = TreeTeaView.ClearSelectionOnExpandMode.BeforeExpandAndCollapse;

            //enable the selection of hidden nodes, when selecting with the shift-key
            treeTea.SelectHiddenNodesAlsoOnShift = true;

            //subscribe the new NodeSelected event (fired after AfterSelect, which focuses on the selected nodes
            treeTea.AfterSelect += TreeTea_NodeSelected; //see into method for more info


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
            treeTea.FuncIsCheckboxEnabledForNode = new Func<TreeNode, bool>((whateverNode) =>
            {
                //return node.Index % 2 == 1;
                return whateverNode.Text.StartsWith("Node2") || whateverNode.Text == "Node11" || whateverNode.Text == "Node36" || whateverNode.Text == "Node37";
            });

            //set the initial checked state for a node
            treeTea.FuncSetInitialCheckedState = new Func<TreeNode, CheckedState>((whateverNode) =>
            {
                if (whateverNode.Text.StartsWith("Node2")) return CheckedState.Checked;
                else return CheckedState.Unchecked;
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
            //iDontCareAboutYourEmails@holzers.de
            //http://holzers.de/
        }

        private void TreeTea_NodeSelected(object sender, NodeSelectedEventArgs e)
        {
            //get the most recently selected node
            var lastSelectedNode = e.RecentlySelectedNode;

            //get a list of all currently selected nodes
            var allCurrentlySelectedNodes = e.AllSelectedNodes;
        }

        private void TreeTea_AfterCheck(object sender, CheckedStateChangedEventArgs e)
        {
            Console.WriteLine(String.Format("The node {0} is now {2} has changed its checkedState: {1}", e.Node.Text, /*e.CheckedState.ToString()*/ e.Node.GetCheckedState(), e.Node.Checked ? "checked" : "unchecked"));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            treeTea.CheckBoxes = true;
            treeTea.FuncIsCheckboxEnabledForNode = new Func<TreeNode, bool>((node) =>
            {
                //return node.Index % 2 == 1;
                //return node.Text.StartsWith("Node2") || node.Text == "Node11" || node.Text == "Node36" || node.Text == "Node37";
                return node.Text == "Node2" || node.Text == "Node26";
            });

            //treeTea.CheckBoxes = !treeTea.CheckBoxes;
            //Console.WriteLine(String.Format("Checkboxes are {0}", treeTea.CheckBoxes ? "enabled" : "disabled"));
        }

        //private int mode = 0;
        private void button2_Click(object sender, EventArgs e)
        {
            treeTea.Nodes[0].GetAncestorOfType<TreeNode>();
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
    }
}