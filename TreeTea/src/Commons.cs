using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
        #region Checked and Selected State

        /// <summary>
        /// <para>This will retrieve the CheckedState-Property of the node</para>
        /// <para>The Checked-Property reflects the CheckedState-Property, with respects to the MixedStateMode</para>
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Yeah, guess what it is</returns>
        /// <remarks>I've used an extension method here, because Extension Properties do not exist and I didn't
        /// want the further developers (me including) to have to use dedicated TreeTeaNodes, rather than the usual TreeNode</remarks>
        public static CheckedState GetCheckedState(this TreeNode node) => (CheckedState)node.StateImageIndex;

        /// <summary>
        /// Checks, whether the node is currently selected in its TreeView
        /// </summary>
        /// <param name="node"></param>
        /// <returns>True, if the node is selected</returns>
        public static bool IsSelected(this TreeNode node)
        {
            if (node.TreeView == null) throw new InvalidOperationException("The passed node is not assigned to any treeview and cannot have a selected state");

            if (node.TreeView is TreeTeaView)
                return ((TreeTeaView)node.TreeView).SelectedNodes.Contains(node);
            else
                return node.IsSelected;
        }

        /// <summary>
        /// Checks, whether the node is currently selected in this TreeView
        /// </summary>
        /// <param name="node"></param>
        /// <returns>True, if the node is selected</returns>
        public static bool IsSelected(this TreeView treeView, TreeNode node)
        {
            if (node == null) throw new NullReferenceException("Node is null");

            if (treeView is TreeTeaView)
                return ((TreeTeaView)treeView).SelectedNodes.Contains(node);
            else
                return node.IsSelected;
        }

        #endregion

        #region Get TreeNodes

        /// <summary>
        /// Returns the currently selected node to the type T
        /// </summary>
        /// <typeparam name="T">Must be derived class of TreeNode</typeparam>
        /// <param name="treeView"></param>
        /// <returns></returns>
        public static T GetSelectedNode<T>(this TreeView treeView) where T : TreeNode
        {
            return (T)treeView.SelectedNode;
        }

        /// <summary>
        /// Tries to cast the currently selected node to the type T and returns true if that cast was successful
        /// </summary>
        /// <typeparam name="T">Must be derived class of TreeNode</typeparam>
        /// <param name="treeView"></param>
        /// <param name="data"></param>
        /// <returns>True if the selected node is of type T</returns>
        public static bool GetSelectedNode<T>(this TreeView treeView, out T data) where T : TreeNode
        {
            try
            {
                data = (T)treeView.SelectedNode;
                if (data != null)
                    return true;
            }
            catch
            {
                data = default(T);
            }
            return false;
        }

        /// <summary>
        /// Checks whether the currently selected node is of type T
        /// </summary>
        /// <typeparam name="T">Must be derived class of TreeNode</typeparam>
        /// <param name="treeView"></param>
        /// <returns>True if the selected node is of type T</returns>
        public static bool IsSelectedNodeOfType<T>(this TreeView treeView) where T : TreeNode
        {
            return treeView.SelectedNode is T;
        }

        #endregion

        #region Retrieve TreeNode Tag conveniently

        //Get Tag of passed node

        /// <summary>
        /// Casts the Tag of the node to the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">If the cast fails</exception>
        public static T GetTag<T>(this TreeNode self)
        {
            return (T)(self?.Tag ?? default(T));
        }

        /// <summary>
        /// Tries to cast the Tag of the node to the type T and returns true on success
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="data"></param>
        /// <returns>True if the Tag of the node is of type T</returns>
        public static bool GetTag<T>(this TreeNode self, out T data)
        {
            data = self.GetTag<T>();
            return (self.Tag is T);
        }

        /// <summary>
        /// Returns true if the Tag of the node is of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsTagOfType<T>(this TreeNode self)
        {
            return (self.Tag is T);
        }

        //Get Tag of selected node in regular treeview

        /// <summary>
        /// Casts the Tag of the currently selected node to the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">If the cast fails</exception>
        public static T GetSelectedTag<T>(this TreeView self)
        {
            if (!(self.SelectedNode?.Tag is T))
                return default(T);
            return (T)self.SelectedNode?.Tag;
        }

        /// <summary>
        /// Tries to cast the Tag of the currently selected node to the type T and returns true on success
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="data"></param>
        /// <returns>True if the Tag of the selected node is of type T</returns>
        public static bool GetSelectedTag<T>(this TreeView self, out T data)
        {
            if (!(self.SelectedNode?.Tag is T))
            {
                data = default(T);
                return false;
            }

            data = (T)self.SelectedNode?.Tag;
            return true;
        }

        /// <summary>
        /// Returns true if the Tag of the currently selected node is of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsSelectedTagOfType<T>(this TreeView self)
        {
            return (self.SelectedNode?.Tag is T);
        }

        //Get Tag of selected nodes in treeteaview

        /// <summary>
        /// Casts the Tags of all currently selected nodes to the type T, but only returns the succesfully casted tags (=only where tag is of type T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns>Returns only the tags, where the tag actually is of type T</returns>
        public static IEnumerable<T> GetSelectedTag<T>(this TreeTeaView self)
        {
            List<T> data = new List<T>();
            foreach (TreeNode node in self.SelectedNodes)
                if (node.Tag is T)
                    data.Add((T)node.Tag);
            return data;
        }

        /// <summary>
        /// Tries to cast the Tag of the all currently selected nodes to the type T and returns true if all nodes have been succesfully casted. Returns only succesfully casted tags (=only whea tag is T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="data"></param>
        /// <returns>True if all the Tags of the selected nodes are of type T</returns>
        public static bool GetSelectedTag<T>(this TreeTeaView self, out IEnumerable<T> data)
        {
            List<T> tmp = new List<T>();
            bool allSuccessfullyCasted = true;
            foreach (TreeNode node in self.SelectedNodes)
            {
                if (node.Tag is T)
                    tmp.Add((T)node.Tag);
                else
                    allSuccessfullyCasted = false;
            }
            data = tmp;
            return allSuccessfullyCasted;
        }

        /// <summary>
        /// Returns true if all the Tags of the any currently selected nodes are of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsSelectedTagOfType<T>(this TreeTeaView self)
        {
            return self.SelectedNodes.All(x => x.Tag is T);
        }

        //Get Tag of an ancestor of the selected node

        /// <summary>
        /// Searches all ancestors for a tag of type T - if no parent along the path has a tag of type T, it will return default(T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static T GetSelectedTagOfAncestor<T>(this TreeView self)
        {
            if (!self.GetSelectedTagOfAncestor(out T data))
                return default(T);

            return (T)data;
        }

        /// <summary>
        /// Searches all ancestors for a tag of type T, returns true if it finds a tag of type T and stores the casted object into data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="data"></param>
        /// <param name="searchNodeItself"></param>
        /// <returns></returns>
        public static bool GetSelectedTagOfAncestor<T>(this TreeView self, out T data, bool searchNodeItself = true)
        {
            if (self.SelectedNode == null)
            {
                data = default(T);
                return false;
            }
            data = self.SelectedNode.GetAncestorTag<T>(searchNodeItself);
            return data != null;
        }

        #endregion

        #region Navigate through TreeView

        /// <summary>
        /// Searches the Ancestors of this node for a Node of the given Type.
        /// </summary>
        /// <typeparam name="T">Must be a derivate of TreeNode</typeparam>
        /// <param name="self"></param>
        /// <param name="searchNodeItself">If this is true, and the node this method is called upon is the type that is searched for, the node itself will be returned</param>
        /// <returns>Null, if no Parent of the given Type could be found</returns>
        public static T GetAncestorOfType<T>(this TreeNode self, bool searchNodeItself = false) where T : TreeNode
        {
            TreeNode n = self;
            while (n.Parent != null)
            {
                //We either traverse the collection in pre-order or in-order
                if (!searchNodeItself)
                    n = n.Parent;

                if (n is T)
                    return (T)self;

                if (searchNodeItself)
                    n = n.Parent;
            }

            return null;
        }

        /// <summary>
        /// Searches the Ancestors of this node for a Node of the given Type.
        /// </summary>
        /// <typeparam name="T">Must be a derivate of TreeNode</typeparam>
        /// <param name="self"></param>
        /// <param name="node"></param>
        /// <param name="searchNodeItself">If this is true, and the node this method is called upon is the type that is searched for, the node itself will be returned</param>
        /// <returns>True, if an ancestor was successfully found</returns>
        public static bool GetAncestorOfType<T>(this TreeNode self, out T node, bool searchNodeItself = false) where T : TreeNode
        {
            node = self.GetAncestorOfType<T>(searchNodeItself);
            return node != null;
        }

        /// <summary>
        /// Searches the Ancestors of this node for a Node with a Tag of the given Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="searchNodeItself">If this is true, and the node this method is called upon is the type that is searched for, the node itself will be returned</param>
        /// <returns>default(T), if no Parent with a Tag of the given Type could be found</returns>
        public static T GetAncestorTag<T>(this TreeNode self, bool searchNodeItself = false)
        {
            TreeNode n = self;
            while (n.Parent != null)
            {
                //We either traverse the collection in pre-order or in-order
                if (!searchNodeItself)
                    n = n.Parent;

                if (n is T)
                    return (T)self.Tag;

                if (searchNodeItself)
                    n = n.Parent;
            }

            return default(T);
        }

        /// <summary>
        /// Searches the Ancestors of this node for a Node with a Tag of the given Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="data">default(T), if no Parent with a Tag of the given Type could be found</param>
        /// <param name="searchNodeItself">If this is true, and the node this method is called upon is the type that is searched for, the node itself will be returned</param>
        /// <returns>True if an ancestor have been found with a tag of the given type</returns>
        public static bool GetAncestorTag<T>(this TreeNode self, out T data, bool searchNodeItself = false)
        {
            data = self.GetAncestorTag<T>(searchNodeItself);
            return data != null;
        }

        /// <summary>
        /// Searches the Ancestors of this node for a Node of the given Type.
        /// </summary>
        /// <typeparam name="T">Must be a derivate of TreeNode</typeparam>
        /// <param name="self"></param>
        /// <param name="query">Query to search the parents for</param>
        /// <param name="searchNodeItself">If this is true, and the node this method is called upon is the type that is searched for, the node itself will be returned</param>
        /// <returns>Null, if no Parent of the given Type could be found</returns>
        public static T GetAncestorWhere<T>(this TreeNode self, Func<TreeNode, bool> query, bool searchNodeItself = false) where T : TreeNode
        {
            TreeNode node = self;
            while (node.Parent != null)
            {
                //We either traverse the collection in pre-order or in-order
                if (!searchNodeItself)
                    node = node.Parent;

                if (query(node))
                    return (T)node;

                if (searchNodeItself)
                    node = node.Parent;
            }

            return null;
        }

        /// <summary>
        /// Searches the Ancestors of this node for a Node of the given Type.
        /// </summary>
        /// <typeparam name="T">Must be a derivate of TreeNode</typeparam>
        /// <param name="self"></param>
        /// <param name="query">Query to search the parents for</param>
        /// <param name="searchNodeItself">If this is true, and the node this method is called upon is the type that is searched for, the node itself will be returned</param>
        /// <returns>Null, if no Parent of the given Type could be found</returns>
        public static bool GetAncestorWhere<T>(this TreeNode self, Func<TreeNode, bool> query, out T node, bool searchNodeItself = false) where T : TreeNode
        {
            node = GetAncestorWhere<T>(self, query, searchNodeItself);
            return node != null;
        }

        /// <summary>
        /// Searches the Descendents of this nodecollection for all Nodes of the given Type.
        /// </summary>
        /// <typeparam name="T">Must be a derivate of TreeNode</typeparam>
        /// <param name="self"></param>
        /// <param name="includeSelf">If true, it will also include the node itself - as long as it qualifies to</param>
        /// <returns>Null, if no Parent of the given Type could be found</returns>
        public static IEnumerable<T> Descendants<T>(this TreeNodeCollection self, bool includeSelf = false) where T : TreeNode
        {
            return Descendants<T>(self, null, includeSelf);
        }

        /// <summary>
        /// Searches the Descendents of this nodecollection for all Nodes of the given Type and where the query returns true
        /// </summary>
        /// <typeparam name="T">Must be a derivate of TreeNode</typeparam>
        /// <param name="self"></param>
        /// <param name="query">Query to for which the Node has to return true to qualify</param>
        /// <param name="includeSelf">If true, it will also include the node itself - as long as it qualifies to</param>
        /// <returns>Null, if no Parent of the given Type could be found</returns>
        public static IEnumerable<T> Descendants<T>(this TreeNodeCollection self, Func<TreeNode, bool> query, bool includeSelf = false) where T : TreeNode
        {
            foreach (TreeNode node in self)
            {
                foreach (var descendent in node.Descendants<T>(query, includeSelf))
                    yield return descendent;
            }
        }

        /// <summary>
        /// Searches the Descendents of this node for a Node of the given Type.
        /// </summary>
        /// <typeparam name="T">Must be a derivate of TreeNode</typeparam>
        /// <param name="self"></param>
        /// <returns>Null, if no Parent of the given Type could be found</returns>
        private static IEnumerable<T> Descendants<T>(this TreeNode self, Func<TreeNode, bool> query, bool includeSelf = false) where T : TreeNode
        {
            var list = new List<T>();
            if (self is T && (query?.Invoke(self) ?? true))
                list.Add((T)self);

            list.AddRange(self.Nodes.Descendants<T>(query, true));
            return list;
        }

        #endregion
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
        /// <summary>
        /// The CheckedState of the node, which has invoked the event
        /// </summary>
        public CheckedState CheckedState { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="checkedState"></param>
        public CheckedStateChangedEventArgs(TreeNode node, CheckedState checkedState) : base(node)
        {
            this.CheckedState = checkedState;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="checkedState"></param>
        public CheckedStateChangedEventArgs(TreeNode node, CheckedState checkedState, TreeViewAction action) : base(node, action)
        {
            this.CheckedState = checkedState;
        }
    }

    /// <summary>
    /// EventArgs for the NodeSelected event
    /// </summary>
    public class NodeSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Returns the node which has invoked the event
        /// </summary>
        public TreeNode RecentlySelectedNode { get; set; }

        /// <summary>
        /// Returns a list of all currently selected nodes
        /// </summary>
        public List<TreeNode> AllSelectedNodes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lastSelectedNode"></param>
        /// <param name="selectedNodes"></param>
        public NodeSelectedEventArgs(TreeNode lastSelectedNode, List<TreeNode> selectedNodes)
        {
            this.RecentlySelectedNode = lastSelectedNode;
            this.AllSelectedNodes = selectedNodes;
        }
    }
}
