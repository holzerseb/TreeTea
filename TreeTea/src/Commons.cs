using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TreeTea
{
    public static class Extension
    {
        public static CheckedState GetCheckedState(this TreeNode node)
        {
            return (CheckedState)node.StateImageIndex;
        }
    }

    /// <summary>
    /// Defines the current checked state of a node
    /// </summary>
    public enum CheckedState { Uninitialised = -1, Unchecked = 0, Checked = 1, Mixed = 2 }

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
