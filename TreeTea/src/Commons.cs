using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TreeTea
{
    /// <summary>
    /// This class provides some useful extensions
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// <para>This will retrieve the CheckedState-Property of the node</para>
        /// <para>The Checked-Property reflects the CheckedState-Property, with respects to the MixedStateMode</para>
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Yeah, guess what it is</returns>
        /// <remarks>I've used an extension method here, because Extension Properties do not exist and I didn't
        /// want the further developers (me including) to have to use dedicated TreeTeaNodes, rather than the usual TreeNode</remarks>
        public static CheckedState GetCheckedState(this TreeNode node)
        {
            return (CheckedState)node.StateImageIndex;
        }
    }

    /// <summary>
    /// Defines the current checked state of a node
    /// </summary>
    public enum CheckedState { Uninitialised = -1, Unchecked = 0, Checked = 1, Mixed = 2 }

    /// <summary>
    /// EventArgs for the TreeTeaView.AfterCheck Event
    /// </summary>
    public class CheckedStateChangedEventArgs : TreeViewEventArgs
    {
        public CheckedState CheckedState { get; set; }

        public CheckedStateChangedEventArgs(TreeNode node, CheckedState checkedState) : base(node)
        {
            this.CheckedState = checkedState;
        }

        public CheckedStateChangedEventArgs(TreeNode node, CheckedState checkedState, TreeViewAction action) : base(node, action)
        {
            this.CheckedState = checkedState;
        }
    }
}
