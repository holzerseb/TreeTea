using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TreeTea
{
    /// <summary>
    /// A TreeView enabling MultiSelection, Tristate
    /// and provides various Methods for easier usage
    /// </summary>
    /// <remarks>
    /// <para>'Mixed' nodes will always be checked=true</para>
    /// <para>Tree can be navigated by keyboard (cursor keys & space)</para>
    /// <para>Checkboxes can be enabled/disabled for specific nodes</para>
    /// <para>No need to do anything special in calling code</para>
    /// </remarks>
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public partial class TreeTeaView : TreeView
    {
        /* TODOS / IDEAS
         * 
         * maybe let the user disable "check childs if parent is checked" - i dont know this idea sounds stupid
         * 
         * test the funcsetinitialstate
         * 
         * test the Descendants and all getnode / gettags
         * 
         * method to create "relations" aka groups
         * 
         * */


        #region Members, Constructors and other stuff

        /* Member and Properties regarding the node selection and MultiSelection can be found in Region "MultiSelection | Node Selection Handling" */
        /* Member and Properties regarding the tristate can be found in Region "TriState | Checkbox Handling" */


        /* Constructor */
        /// <summary>
        /// Initializes the TreeTea
        /// </summary>
        public TreeTeaView()
        {
            /*Property Default Values*/
            SelectNodeOnImageClick = true;
            EnableDoubleBuffering = true;
            SupressCheckboxDoubleClick = true;
            IsMultiSelectionEnabled = true;
            IsTriStateEnabled = true;
            CheckBoxes = true;
            ClearSelectionOnExpand = ClearSelectionOnExpandMode.BeforeExpand;

            /*Members*/
            selectedNode = null;
            selectedNodes = new List<TreeNode>();
            ExceptionHandler = null;
            base.SelectedNode = null;
            this.StateImageList = GenerateCheckboxImageList();
            this.isCheckBoxesEnabled = false; //gets overwritten when the user sets the property, but we need this though...
            this.funcIsCheckboxEnabledForNode = null;

            /* Debug Stuff */
            InitializeDebugInstance(); //is called only in debug mode - TODO: Remove this if the whole thing goes public, because we don't know if others want to adapt our debug configuration
        }

        /// <summary>
        /// Initializes the TreeTea
        /// </summary>
        /// <param name="exceptionHandler">Function to handle the exceptions, otherwise the exception will be thrown</param>
        public TreeTeaView(Action<Exception> exceptionHandler) : this()
        {
            this.ExceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Initializes the TreeTea
        /// </summary>
        /// <param name="funcIsCheckboxEnabledForNode">Function which is used to determine, whether a node has a checkbox visible.
        /// If the Function returns false for the passed node, no checkbox will be visible for the node</param>
        public TreeTeaView(Func<TreeNode, bool> funcIsCheckboxEnabledForNode) : this()
        {
            this.funcIsCheckboxEnabledForNode = funcIsCheckboxEnabledForNode;
        }

        /// <summary>
        /// Initializes the TreeTea
        /// </summary>
        /// <param name="funcIsCheckboxEnabledForNode">Function which is used to determine, whether a node has a checkbox visible.
        /// If the Function returns false for the passed node, no checkbox will be visible for the node</param>
        /// <param name="exceptionHandler">Function to handle the exceptions, otherwise the exception will be thrown</param>
        public TreeTeaView(Func<TreeNode, bool> funcIsCheckboxEnabledForNode, Action<Exception> exceptionHandler) : this()
        {
            this.ExceptionHandler = exceptionHandler;
            this.funcIsCheckboxEnabledForNode = funcIsCheckboxEnabledForNode;
        }

        #endregion


        #region General Properties

        /* Member and Properties regarding the node selection can be found in Region "MultiSelection | Node Selection Handling" */
        /* Member and Properties regarding the tristate can be found in Region "TriState | Checkbox Handling" */


        /// <summary>
        /// Defines, whether a click on the Image (by StateImageIndex) will also select the node
        /// </summary>
        [Category("TreeTea General Configuration")]
        [Description("Defines, whether a click on the Image(by StateImageIndex) will also select the node")]
        [DefaultValue(true)]
        public bool SelectNodeOnImageClick { get; set; }

        /// <summary>
        /// <para>Enable this, if the TreeView is flickering.</para>
        /// </summary>
        /// <remarks><para>Flickering can sometimes occur, because the control gets repainted - I haven't found the exact reasons WHEN this does occur,
        /// but in case it does, DoubleBuffering will cause the repaint to be done in a seperate frame, while the old one remains to be shown</para>
        /// <para>This will not speed up the painting, nor will it delay it - it simply elimates the visible painting artifacts</para></remarks>
        [Category("TreeTea General Configuration")]
        [Description("Enable this, if the TreeView is flickering. This will not speed up the painting, nor will it delay it - it simply elimates the visible painting artifacts")]
        [DefaultValue(true)]
        public bool EnableDoubleBuffering { get; set; }

        /// <summary>
        /// <para>Function to handle exceptions. If this is null, the exception will be thrown</para>
        /// <para>This is used to catch "common" (rather "expected") exceptions, like if you try to set mixed state while tristate is disabled -
        /// fatal exceptions (aka exceptions that will cause severe damage to the treeView) still get thrown</para>
        /// <para>I've introduced this, because I didn't want to try/catch every method where exceptions can occur</para>
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)] //ExceptionHandler should not be exposed to the Property Window
        public Action<Exception> ExceptionHandler { get; set; }

        #endregion


        #region MultiSelection | Node Selection Handling

        /* Due to some handling issues with the base.SelectedNode,
         * we completly override the default behaviour and handle the selected nodes ourself */

        private TreeNode selectedNode;
        /// <summary>
        /// Property for the current (node set most recently) node
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)] //i really have no clue why designer wanted to show this
        public new TreeNode SelectedNode
        {
            get { return selectedNode; }
            set
            {
                ClearSelection();

                if (value == null)
                    return;

                SelectNode(value);
            }
        }

        private List<TreeNode> selectedNodes;
        /// <summary>
        /// Property for all selected nodes
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)] //i really have no clue why designer wanted to show this
        public IEnumerable<TreeNode> SelectedNodes
        {
            get { return selectedNodes; }
            set
            {
                ClearSelection();

                if (value == null)
                    return;

                if (IsMultiSelectionEnabled)
                {
                    foreach (var node in value)
                        ToggleNode(node, true);
                }
                else
                {
                    try
                    {
                        if (value.Count() > 1)
                            throw new Exception("There was an attempt to select multiple nodes, but MultiSelection is disabled. The current selection-state has not been modified.");

                        if (value.Count() == 1)
                            ToggleNode(value.First(), true);
                        //else  //this else is just symbolic -> if value.Count() == 0, we have already cleared the selection
                        //    ClearSelection();
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }
                }
            }
        }

        #region Properties / Configuration

        /* These properties are exposed to the Property Window and can be used by the developer to customize the TreeTea */


        /// <summary>
        /// <para>Used to configure the multiselection</para>
        /// <para>In example, if you only want the user to multiselect nodes with the same parent</para>
        /// </summary>
        [Flags]
        public enum MultiSelectionRestriction
        {
            /// <summary>
            /// You can multiselect each node of the treeview with each other node at the same time
            /// </summary>
            NoRestriction = 0,
            /// <summary>
            /// You can only multiselect nodes which have objects of the same type in their tag
            /// </summary>
            SameTagType = 1,
            /// <summary>
            /// You can only multiselct nodes which are on the same level
            /// </summary>
            SameLevel = 2,
            /// <summary>
            /// You can only multiselct nodes which have the same ancestor
            /// </summary>
            SameParent = 4,
            /// <summary>
            /// This will restrict multiselection to nodes on the same level sharing the same parent.
            /// </summary>
            SameLevelAndParent = 6,
            /// <summary>
            /// This will restrict multiselection to nodes on the same level and having objects of the same type as tag
            /// </summary>
            SameLevelAndTagType = 3,
            /// <summary>
            /// This will restrict multiselection to nodes sharing the same parent and having objects of the same type as tag
            /// </summary>
            SameParentAndTagType = 5,
            /// <summary>
            /// This will restrict multiselection to nodes on the same level, sharing the same parent and having objects of the same type as tag
            /// </summary>
            FullRestriction = 7,
        }

        /// <summary>
        /// Mode to determine, if the current selection should be cleared when expanding/collapsing and if, whether it should
        /// happen before of after the expand/collapse
        /// </summary>
        [Flags]
        public enum ClearSelectionOnExpandMode
        {
            DoNotClearSelection = 0,
            BeforeExpand = 1,
            BeforeCollapse = 2,
            BeforeExpandAndCollapse = 3,
            AfterExpand = 4,
            AfterCollapse = 8,
            AfterExpandAndCollapse = 12,
        }

        /// <summary>
        /// Select a mode, in which the MultiSelection will work. You can restrict Multiselection to Same Level, Parent or Type of Tag Object and/or combine them.
        /// </summary>
        [Category("TreeTea Multi-Selection")]
        [Description("Select a mode, in which the MultiSelection will work. You can restrict Multiselection to Same Level, Parent or Type of Tag Object and/or combine them.")]
        [DefaultValue(MultiSelectionRestriction.NoRestriction)]
        public MultiSelectionRestriction MultiSelectionMode { get; set; }

        /// <summary>
        /// If this is true, MultiSelection is enabled. Who would have guessed?
        /// </summary>
        [Category("TreeTea Multi-Selection")]
        [Description("If this is true, MultiSelection is enabled. Who would have guessed?")]
        [DefaultValue(true)]
        public bool IsMultiSelectionEnabled { get; set; }

        /// <summary>
        /// If this is true and the user selects a range of nodes with the SHIFT-Key, then also nodes which are not visible (=Parent not expanded) will be selected. Default is false
        /// </summary>
        /// <remarks>The default behaviour for most applications is that only visible nodes will be selected</remarks>
        [Category("TreeTea Multi-Selection")]
        [Description("If this is true and the user selects a range of nodes with the SHIFT-Key, then also nodes which are not visible (=Parent not expanded) will be selected. Default is false")]
        [DefaultValue(false)]
        public bool SelectHiddenNodesAlsoOnShift { get; set; }

        /// <summary>
        /// Use this to define, wheter expanding/collapsing a node will clear the current selection
        /// </summary>
        /// <remarks>Default behaviour for most application is that this is true</remarks>
        [Category("TreeTea Multi-Selection")]
        [Description("If this is true, the current selection will be cleared if any node gets expanded/collapsed")]
        [DefaultValue(ClearSelectionOnExpandMode.BeforeExpand)]
        public ClearSelectionOnExpandMode ClearSelectionOnExpand { get; set; }

        #endregion

        #region Overrides

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            try
            {
                //we have to completly disable the whole unholy native selection handling
                e.Cancel = true;
                base.SelectedNode = null;

                base.OnBeforeSelect(e);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            try
            {
                base.OnAfterSelect(e);
                base.SelectedNode = null; //no power to the old selectednode handling!
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
        {
            //OnBeforeExpand can be found in the TriState region -> overrides, because for the most parts, it belongs there
            if (ClearSelectionOnExpand.HasFlag(ClearSelectionOnExpandMode.BeforeExpand))
                ClearSelection();
            base.OnBeforeCollapse(e);
        }

        protected override void OnAfterCollapse(TreeViewEventArgs e)
        {
            //OnAfterExpand can be found in the TriState region -> overrides, because for the most parts, it belongs there
            if (ClearSelectionOnExpand.HasFlag(ClearSelectionOnExpandMode.AfterExpand))
                ClearSelection();
            base.OnAfterCollapse(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            NodeClicked(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            //NodeClicked(e); //no clue why I keep this here, maybe I'm clingy
        }

        /// <summary>
        /// Selects a node, if it wasn't before
        /// </summary>
        /// <param name="e"></param>
        private void NodeClicked(MouseEventArgs e)
        {
            try
            {
                base.SelectedNode = null; //old system is a disease

                var node = this.GetNodeAt(e.Location);
                if (node == null)
                {
                    base.OnMouseDown(e);
                    return;
                }

                //For more info about treeviewhittestinfo, see https://msdn.microsoft.com/en-us/library/system.windows.forms.treeviewhittestinfo(v=vs.110).aspx; shits convinient
                TreeViewHitTestInfo hitInfo = base.HitTest(e.Location);
                if (hitInfo.Location == TreeViewHitTestLocations.Label || (SelectNodeOnImageClick && hitInfo.Location == TreeViewHitTestLocations.Image))
                {
                    SelectNode(node);
                }

                base.OnMouseDown(e);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #endregion

        #region Node-Selection Methods

        /// <summary>
        /// Handler for NodeSelected; Always contains the most recently selected node as well as all currently selected nodes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public delegate void NodeSelectedEventHandler(object sender, NodeSelectedEventArgs e);
        /// <summary>
        /// Occurs once a new node has been selected (after AfterSelect) and returns also a list of all currently selected nodes
        /// </summary>
        public event NodeSelectedEventHandler NodeSelected;

        /// <summary>
        /// This will either select or unselect the passed node
        /// </summary>
        /// <param name="node">Node to un/select</param>
        /// <param name="select">true to select node</param>
        protected void ToggleNode(TreeNode node, bool select)
        {
            if (node == null || node.TreeView != this)
                return;

            if (select)
            {
                //First, we check if the selection is allowed
                if (selectedNode != null)
                {
                    if (MultiSelectionMode.HasFlag(MultiSelectionRestriction.SameLevel) && selectedNode.Level != node.Level)
                        return;
                    if (MultiSelectionMode.HasFlag(MultiSelectionRestriction.SameParent) && selectedNode.Parent != node.Parent)
                        return;
                    if (MultiSelectionMode.HasFlag(MultiSelectionRestriction.SameTagType) && selectedNode.Tag?.GetType() != node.Tag?.GetType())
                        return;
                }

                //Then perform the selection
                if (!selectedNodes.Contains(node))
                    selectedNodes.Add(node);

                try
                {
                    selectedNode = node;
                    NodeSelected?.Invoke(this, new NodeSelectedEventArgs(selectedNode, selectedNodes));

                    node.BackColor = System.Drawing.SystemColors.Highlight;
                    node.ForeColor = System.Drawing.SystemColors.HighlightText;
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
            else
            {
                if (selectedNodes.Contains(node))
                    selectedNodes.Remove(node);
                try
                {
                    if (selectedNode == node)
                        if (selectedNodes.Count > 0)
                            selectedNode = selectedNodes.Last();
                        else
                            selectedNode = null;

                    node.BackColor = this.BackColor;
                    node.ForeColor = this.ForeColor;
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }

        /// <summary>
        /// This will either select or unselect the passed node, depending on the current state of the node
        /// This method will determine the current state and then call ToggleNode(node, !currentState)
        /// </summary>
        /// <param name="node">Node to un/select</param>
        protected void ToggleNode(TreeNode node)
        {
            ToggleNode(node, !selectedNodes.Contains(node));
        }

        /// <summary>
        /// Selects a node
        /// </summary>
        /// <param name="node"></param>
        protected void SelectNode(TreeNode node)
        {
            this.BeginUpdate();

            //if there is no node selected, select the passed one;
            //do this also, if user wants to add a node to the selection
            if (IsMultiSelectionEnabled && (selectedNode == null || ModifierKeys == Keys.Control))
                ToggleNode(node);
            else if (IsMultiSelectionEnabled && ModifierKeys == Keys.Shift)
                SelectNode(selectedNodes.Last(), node);
            else
                ClearSelectionAndSelect(node);
            this.EndUpdate();
        }

        protected void SelectNode(TreeNode from, TreeNode to)
        {
            if (from == null) { HandleException(new NullReferenceException("The Source-Node cannot be null.")); return; }
            if (to == null) { HandleException(new NullReferenceException("The Target-Node cannot be null.")); return; }

            #region Local Functions
            //I declare a local function to toggle nodes, because we have to ensure, that the two passed nodes
            //definitly are toggled to true
            TreeNode initialFrom = from, initialTo = to;
            var ToggleNode = new Action<TreeNode>((x) =>
            {
                if (x == initialFrom || x == initialTo) this.ToggleNode(x, true);
                else this.ToggleNode(x);
            });
            //and an additional function for selecting all nodes between two nodes
            var SelectWalkingDown = new Action<TreeNode, TreeNode>((start, end) =>
            {
                while (start != end)
                {
                    if (SelectHiddenNodesAlsoOnShift)
                        start = start?.NextVisibleNode;
                    else
                    {
                        if (start?.Nodes.Count > 0)
                            start = start?.Nodes[0]; //If there are childs, select the first
                        else if (start?.NextNode != null)
                            start = start?.NextNode; //If there is a sibling, select the sibling
                        else
                        {
                            //In this case, the next child is a sibling of one of the nodes ancestors
                            if (start?.Parent?.NextNode != null)
                                start = start?.Parent?.NextNode; //the parents sibling is our next node
                            else
                            {
                                //we go the paths along the parents up, until we can go to the next sibling, as long as there exist some parents
                                while (start.Level != 0)
                                {
                                    start = start?.Parent;
                                    if (start?.NextNode != null)
                                    {
                                        start = start.NextNode;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (start == null) break;
                    ToggleNode(start);
                }
            });
            #endregion

            if (from.Parent == to.Parent)
            {
                //if the nodes share the same parent, this is going to be easy
                if (from.Index == to.Index)
                    return; //obviously
                else
                {
                    //if from is further down than to, we have to swap them
                    if (from.Index > to.Index) { var tmp = from; from = to; to = tmp; }
                    //then select all visibles nodes going down
                    ToggleNode(from);
                    SelectWalkingDown(from, to);
                }
            }
            else
            {
                //this is going to be complicated...
                //we search for a shared parent, because then we can determine whether from or to is "above"
                TreeNode fromAncestor = from, toAncestor = to;
                int sharedParentLevel = Math.Min(fromAncestor.Level, toAncestor.Level); //lower level to match up the parent

                //to find the shared parent, we first have to determine the ancestors on the same level
                while (fromAncestor.Level > sharedParentLevel) fromAncestor = fromAncestor?.Parent;
                while (toAncestor.Level > sharedParentLevel) toAncestor = toAncestor?.Parent;
                //then we can continue to search the shared parent
                try
                {
                    while (fromAncestor.Parent != toAncestor.Parent)
                    {
                        fromAncestor = fromAncestor.Parent;
                        toAncestor = toAncestor.Parent;
                    }
                }
                catch (Exception ex)
                {
                    HandleException(new Exception(String.Format("An error occured while searching the shared parent of the nodes '{0}' and '{1}'", from.Text, to.Text), ex));
                    return;
                }

                //okay, now we have found the ancestor of both nodes, which share the same parent
                //we can continue selecting the node, kinda similiar to above
                if (fromAncestor == toAncestor)
                {
                    //here we know, that one of the clicked node its ancestor as well, meaning that the node on the lower
                    //level must be below - so we can select going down to the node on the lower level
                    if (from.Level > to.Level) { var tmp = from; from = to; to = tmp; }
                    //and select walking down
                    ToggleNode(from);
                    SelectWalkingDown(from, to);
                }
                else
                {
                    //in that case, we have to check whether we have to "go down" or "go up" and swap iff we have to "go up"
                    if (fromAncestor.Index > toAncestor.Index) { var tmp = from; from = to; to = tmp; }
                    //now we simply have to go down
                    ToggleNode(from);
                    SelectWalkingDown(from, to);
                }
            }
        }

        /// <summary>
        /// This will set the selected-state of all the childs of the given parent to the same as the parent recursivly. The parent will not be toggled.
        /// </summary>
        /// <param name="parent">Node, whose childs will be selected</param>
        protected void SelectChildNodes(TreeNode parent)
        {
            foreach (TreeNode child in parent.Nodes)
            {
                ToggleNode(child, parent.IsSelected());
                SelectChildNodes(child);
            }
        }

        /// <summary>
        /// Clears the current selection and selects the passed node
        /// </summary>
        /// <param name="node">Node to select</param>
        protected void ClearSelectionAndSelect(TreeNode node)
        {
            if (node == null)
                return;

            ClearSelection();
            ToggleNode(node, true);
            node.EnsureVisible();
        }

        /// <summary>
        /// Clears out the current selection
        /// </summary>
        protected void ClearSelection()
        {
            try
            {
                try
                {
                    //Reset graphics for selectedNodes
                    foreach (TreeNode node in selectedNodes.Where(x => x.BackColor == System.Drawing.SystemColors.Highlight))
                    {
                        node.BackColor = this.BackColor;
                        node.ForeColor = this.ForeColor;
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }

                //clear everything
                selectedNodes.Clear();
                selectedNode = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #endregion

        #endregion


        #region TriState | Checkbox Handling

        /// <summary>
        /// <para>Semaphore to prevent the "Checked changed" event to fire for each individual node, while "bulk-updating".
        /// Like, if the user checks a node, each child will be checked too -> that would result in all nodes raising
        /// the checked events, but we do not want that</para>
        /// <para>Please use this Semaphor if you tend to override one of the following methods: OnCreateControl, OnAfterExpand, OnAfterCheck</para>
        /// </summary>
        protected int checkedChangedSemaphore = 0;
        /// <summary>
        /// Member for the Property "Checked"
        /// If this is true, Checkboxes will be visible
        /// </summary>
        private bool isCheckBoxesEnabled;
        /// <summary>
        /// A function which is used to determine, whether a node has a checkbox visible
        /// If the Function returns false for the passed node, no checkbox will be visible for the node
        /// </summary>
        protected Func<TreeNode, bool> funcIsCheckboxEnabledForNode;

        /// <summary>
        /// Generates a List of Bitmaps containing all three possible checkboxes
        /// </summary>
        /// <returns></returns>
        private ImageList GenerateCheckboxImageList()
        {
            //See http://msdn.microsoft.com/en-us/library/system.windows.forms.checkboxrenderer.aspx

            var list = new ImageList();
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    var bmp = new System.Drawing.Bitmap(16, 16);
                    var graphic = System.Drawing.Graphics.FromImage(bmp);

                    if (i == 0)
                        CheckBoxRenderer.DrawCheckBox(graphic, new System.Drawing.Point(0, 0), System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
                    if (i == 1)
                        CheckBoxRenderer.DrawCheckBox(graphic, new System.Drawing.Point(0, 0), System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
                    if (i == 2)
                        CheckBoxRenderer.DrawCheckBox(graphic, new System.Drawing.Point(0, 0), System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);

                    list.Images.Add(bmp);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return list;
        }

        /// <summary>
        /// This method will inform all subscribers of the TreeTea.AfterCheck event
        /// by firing the delegate
        /// </summary>
        /// <param name="argsToPass">The event args you want to pass</param>
        /// <remarks>This was put into a dedicated method, because it used various times
        /// and involves some error handling</remarks>
        protected void InvokeAfterCheckEvent(CheckedStateChangedEventArgs argsToPass)
        {
            try
            {
                AfterCheck?.Invoke(this, argsToPass);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #region Properties and Configuration

        /* These properties are exposed to the Property Window and can be used by the developer to customize the TreeTea */


        /// <summary>
        /// Defines whether mixed nodes are Checked=true or Checked=false
        /// </summary>
        public enum MixedStateMode
        {
            /// <summary>
            /// All mixed nodes are Checked=true
            /// </summary>
            Checked,
            /// <summary>
            /// All mixed nodes are Checked=false
            /// </summary>
            Unchecked,
            /// <summary>
            /// This mode will infer whether the mixed state is "checked" or not, by keeping checked nodes "checked" and unchecked "unchecked" after they become mixed
            /// </summary>
            Automatic
        }

        /// <summary>
        /// If this is true, Checkboxes can be set to 'mixed', meaning some childs are checked, some unchecked
        /// </summary>
        [Category("TreeTea TriState")]
        [Description("If this is true, Checkboxes can be set to 'mixed', meaning some childs are checked, some unchecked")]
        [DefaultValue(true)]
        public bool IsTriStateEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether check boxes are displayed next to the
        /// tree nodes in the tree view control.
        /// </summary>
        /// <remarks>This was overwritten, because we have to completely alter the default Checkboxes -> We never use the default ones,
        /// but instead we use "self-drawn" images in the stateimage of the nodes to simulate checkboxes</remarks>
        [Category("TreeTea TriState")]
        [Description("Gets or sets a value indicating whether check boxes are displayed next to the tree nodes in the tree view control")]
        [DefaultValue(true)]
        new public bool CheckBoxes
        {
            get => isCheckBoxesEnabled;
            set
            {
                isCheckBoxesEnabled = value;
                if (value) ShowAllCheckboxes();
                else HideAllCheckboxes();
            }
        }

        /// <summary>
        /// <para>Defines whether mixed nodes are Checked=true or Checked=false</para>
        /// <para>Automatic will keep checked nodes "checked" and unchecked "unchecked" after they become mixed</para>
        /// </summary>
        [Category("TreeTea TriState")]
        [Description("Defines whether mixed nodes are Checked=true or Checked=false. Automatic will keep checked nodes \"checked\" and unchecked \"unchecked\" after they become mixed")]
        [DefaultValue(MixedStateMode.Checked)]
        public MixedStateMode MixedNodesMode { get; set; }

        /// <summary>
        /// This function is used to determine, whether a node has a checkbox
        /// If the Function returns false for the passed node, no checkbox will be visible for the node
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Func<TreeNode, bool> FuncIsCheckboxEnabledForNode
        {
            get => funcIsCheckboxEnabledForNode;
            set
            {
                funcIsCheckboxEnabledForNode = value;
                if (CheckBoxes) UpdateAllCheckboxes(); //UpdateAllCheckboxes automatically takes advantage of the isCheckboxEnabledForNode Function
            }
        }

        /// <summary>
        /// <para>This function is used to determine the initial checkbox-state, after the checkbox has been shown for a node.</para>
        /// <para>Use this function to set custom default values for each single node. If this function is null, the initial CheckedState is Unchecked</para>
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Func<TreeNode, CheckedState> FuncSetInitialCheckedState { get; set; }

        /// <summary>
        /// <para>Enable this, to prevent the Nodes to expand/collapse by a double-click onto the checkboxes.</para>
        /// <para>This will not block the double-click to expand/collapse functionality when double-clicked onto the label</para>
        /// </summary>
        /// <remarks><para>This may be required/wanted, because our checkboxes are self-drawn and used in the stateimage
        /// resulting in double clicks on the checkbox to be used as if you have double clicked the label.</para>
        /// <para>This will not block the double-click to expand/collapse functionality when double-clicked onto the label</para></remarks>
        [Category("TreeTea TriState")]
        [Description("Enable this, to prevent the Nodes to expand/collapse by a double-click onto the checkboxes. This will affect double-clicks onto the node-label.")]
        [DefaultValue(true)]
        public bool SupressCheckboxDoubleClick { get; set; }

        #endregion

        #region Overrides

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            base.CheckBoxes = false; //default checkboxes have to be disabled by any chance; see this.Checkbox remarks
            //and depending on user-configuration, either show or hide checkboxes
            if (this.CheckBoxes) ShowAllCheckboxes();
            else HideAllCheckboxes();

            //We initialize all node
            checkedChangedSemaphore++;
            foreach (TreeNode node in this.Nodes)
                InheritCheckedStateFromChilds(node);
            checkedChangedSemaphore--;
        }

        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            //Some code for the MultiSelection...
            if (ClearSelectionOnExpand.HasFlag(ClearSelectionOnExpandMode.BeforeExpand))
                ClearSelection();
            base.OnBeforeExpand(e);

            if (checkedChangedSemaphore > 0)
                return;
            checkedChangedSemaphore++;

            //Some Developers tend to use LoD (Load on Demand) Nodes, so we ensure that the checkboxes are visible correctly
            if (this.CheckBoxes) ShowCheckbox(e.Node, CheckBoxes);
            else HideCheckbox(e.Node, CheckBoxes);
            checkedChangedSemaphore--;
        }

        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            //Some code for the MultiSelection...
            if (ClearSelectionOnExpand.HasFlag(ClearSelectionOnExpandMode.AfterExpand))
                ClearSelection();
            base.OnAfterExpand(e);

            if (checkedChangedSemaphore > 0)
                return;
            checkedChangedSemaphore++;

            InheritCheckedStateFromChilds(e.Node);
            checkedChangedSemaphore--;
        }

        protected override void OnAfterCheck(TreeViewEventArgs e)
        {
            base.OnAfterCheck(e);

            //if a checked changed process is already ongoing, prevent further changes
            //also prevent any changes, if the user didn't enable checkboxes in first place
            if (checkedChangedSemaphore > 0 || !this.CheckBoxes)
                return;
            checkedChangedSemaphore++;

            //we will update our checkbox based on the checkedstate that was set just now
            //e.Node.StateImageIndex = (int)(e.Node.Checked ? CheckedState.Checked : CheckedState.Unchecked);
            SetCheckedState(e.Node, (e.Node.Checked ? CheckedState.Checked : CheckedState.Unchecked));

            //Inform everyone that this nodes checkedstate has changed
            InvokeAfterCheckEvent(new CheckedStateChangedEventArgs(e.Node, (CheckedState)e.Node.StateImageIndex, e.Action));

            //In case TriState is enabled, we also have to update the childs and parents
            if (IsTriStateEnabled)
            {
                //and require all children of the node to update themself
                InheritCheckedStateFromChilds(e.Node.Nodes, (CheckedState)e.Node.StateImageIndex);

                //also, the ancestors might need to change its state
                InheritCheckedStateFromChilds(e.Node.Parent, true); //we can check first gen childs only here, because only one child of the parent has changed: e.Node
            }

            checkedChangedSemaphore--;
        }

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            base.OnNodeMouseClick(e);

            if (e.Node == null) return;

            //For more info about treeviewhittestinfo, see https://msdn.microsoft.com/en-us/library/system.windows.forms.treeviewhittestinfo(v=vs.110).aspx; shits convinient
            TreeViewHitTestInfo hitInfo = base.HitTest(e.Location);
            if (hitInfo?.Location != TreeViewHitTestLocations.StateImage)
                return;

            if (e.Node.StateImageIndex == (int)CheckedState.Checked)
                e.Node.Checked = false;
            else if (e.Node.StateImageIndex == (int)CheckedState.Unchecked)
                e.Node.Checked = true;
            else if (e.Node.StateImageIndex == (int)CheckedState.Mixed)
            {
                switch (MixedNodesMode)
                {
                    default:
                    case MixedStateMode.Checked:
                        e.Node.Checked = true;
                        break;
                    case MixedStateMode.Unchecked:
                        e.Node.Checked = false;
                        break;
                    case MixedStateMode.Automatic:
                        e.Node.Checked = !e.Node.Checked;
                        break;
                }
            }
        }

        #endregion

        #region Hide Checkboxes

        /// <summary>
        /// <para>Hides the Checkbox of the passed nodes</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="node"></param>
        public void HideCheckbox(TreeNode node)
        {
            HideCheckbox(new TreeNode[1] { node }, false);
        }

        /// <summary>
        /// <para>Hides the Checkbox of the passed nodes</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="nodes"></param>
        public void HideCheckbox(IEnumerable<TreeNode> nodes)
        {
            HideCheckbox(nodes.ToArray(), false);
        }

        /// <summary>
        /// <para>Hides the Checkbox of the passed nodes</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="nodes"></param>
        public void HideCheckbox(TreeNodeCollection nodes)
        {
            if (nodes.Count == 0) return;
            HideCheckbox(nodes.Cast<TreeNode>(), false);
        }

        /// <summary>
        /// <para>Hides the Checkbox of the passed nodes</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="nodes"></param>
        public void HideCheckbox(params TreeNode[] nodes)
        {
            HideCheckbox(nodes, false);
        }

        /// <summary>
        /// <para>Hides the Checkbox of the passed nodes and defines, whether the childnodes should have a checkbox too</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="hideCheckboxesOfChildren">If true, checkboxes of all nodes below will be hidden</param>
        public void HideCheckbox(TreeNode node, bool hideCheckboxesOfChildren)
        {
            HideCheckbox(new TreeNode[1] { node }, hideCheckboxesOfChildren);
        }

        /// <summary>
        /// <para>Hides the Checkbox of the passed nodes and defines, whether the childnodes should have a checkbox too</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="hideCheckboxesOfChildren">If true, checkboxes of all nodes below will be hidden</param>
        public void HideCheckbox(TreeNodeCollection nodes, bool hideCheckboxesOfChildren)
        {
            if (nodes.Count == 0) return;
            HideCheckbox(nodes.Cast<TreeNode>(), hideCheckboxesOfChildren);
        }

        /// <summary>
        /// <para>Hides the Checkbox of the passed nodes and defines, whether the childnodes should have a checkbox too</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="hideCheckboxesOfChildren">If true, checkboxes of all nodes below will be hidden</param>
        public void HideCheckbox(IEnumerable<TreeNode> nodes, bool hideCheckboxesOfChildren)
        {
            if (nodes == null || nodes.Count() == 0)
                return;

            foreach (var node in nodes)
            {
                //node.StateImageIndex = (int)CheckedState.Uninitialised;
                SetCheckedState(node, CheckedState.Uninitialised);

                if (hideCheckboxesOfChildren)
                    HideCheckbox(node.Nodes.Cast<TreeNode>(), hideCheckboxesOfChildren);
            }
        }

        /// <summary>
        /// <para>Hides the Checkbox for all possible nodes in the treeview</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        public void HideAllCheckboxes()
        {
            if (this.Nodes.Count == 0) return;
            HideCheckbox(this.Nodes, true);
        }

        #endregion

        #region Show Checkboxes

        /// <summary>
        /// <para>Shows the Checkbox of the passed node</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="node"></param>
        public void ShowCheckbox(TreeNode node)
        {
            ShowCheckbox(new TreeNode[1] { node }, false);
        }

        /// <summary>
        /// <para>Shows the Checkbox of the passed node</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="node"></param>
        public void ShowCheckbox(IEnumerable<TreeNode> nodes)
        {
            ShowCheckbox(nodes.ToArray(), false);
        }

        /// <summary>
        /// <para>Shows the Checkbox of the passed node</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="node"></param>
        public void ShowCheckbox(TreeNodeCollection nodes)
        {
            if (nodes.Count == 0) return;
            ShowCheckbox(nodes.Cast<TreeNode>(), false);
        }

        /// <summary>
        /// <para>Shows the Checkbox of the passed node</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="node"></param>
        public void ShowCheckbox(params TreeNode[] nodes)
        {
            ShowCheckbox(nodes, false);
        }

        /// <summary>
        /// <para>Shows the Checkbox of the passed node and defines, whether the childnodes should have a checkbox too</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="showCheckboxesOfChildren">If true, checkboxes of all nodes below will be shown</param>
        public void ShowCheckbox(TreeNode node, bool showCheckboxesOfChildren)
        {
            ShowCheckbox(new TreeNode[1] { node }, showCheckboxesOfChildren);
        }

        /// <summary>
        /// <para>Shows the Checkbox of the passed node and defines, whether the childnodes should have a checkbox too</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="showCheckboxesOfChildren">If true, checkboxes of all nodes below will be shown</param>
        public void ShowCheckbox(TreeNodeCollection nodes, bool showCheckboxesOfChildren)
        {
            if (nodes.Count == 0) return;
            ShowCheckbox(nodes.Cast<TreeNode>(), showCheckboxesOfChildren);
        }

        /// <summary>
        /// <para>Shows the Checkbox of the passed node and defines, whether the childnodes should have a checkbox too</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="showCheckboxesOfChildren">If true, checkboxes of all nodes below will be shown</param>
        public void ShowCheckbox(IEnumerable<TreeNode> nodes, bool showCheckboxesOfChildren)
        {
            if (nodes == null || nodes.Count() == 0)
                return;

            foreach (var node in nodes)
            {
                if ((CheckedState)node.StateImageIndex == CheckedState.Uninitialised)
                {
                    //this Try/Catch is solely for the FuncSetinitial blabla thing, because we dont know what the user puts in here
                    //the setcheckedstate handles its exceptions on its own
                    try
                    {
                        SetCheckedState(node, FuncSetInitialCheckedState?.Invoke(node) ?? CheckedState.Unchecked);
                    }
                    catch (Exception ex)
                    {
                        HandleException(new Exception(String.Format("An error occured while determining the initial CheckedState of the node {0}", node.Text), ex));
                        SetCheckedState(node, CheckedState.Unchecked);
                    }
                }

                if (showCheckboxesOfChildren)
                    ShowCheckbox(node.Nodes.Cast<TreeNode>(), showCheckboxesOfChildren);
            }
        }

        /// <summary>
        /// <para>Shows the Checkbox for all possible nodes in the treeview</para>
        /// <para>If "IsCheckboxesEnabledForNode" Property is set, this method will only affect nodes, where the property returns true (thus Checkbox is enabled for the node)</para>
        /// </summary>
        public void ShowAllCheckboxes()
        {
            if (this.Nodes.Count == 0) return;
            ShowCheckbox(this.Nodes, true);
        }

        #endregion

        #region Update Checkbox Visibility

        /// <summary>
        /// <para>Updates the visibility of the checkbox for the passed node</para>
        /// <para>This function does only make sense to use if you have FuncIsCheckboxEnabledForNode set</para>
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="updateChildrensCheckboxes"></param>
        public void UpdateCheckbox(TreeNodeCollection nodes, bool updateChildrensCheckboxes)
        {
            if (nodes.Count == 0) return;
            UpdateCheckbox(nodes.Cast<TreeNode>(), updateChildrensCheckboxes);
        }

        /// <summary>
        /// <para>Updates the visibility of the checkbox for the passed node</para>
        /// <para>This function does only make sense to use if you have FuncIsCheckboxEnabledForNode set</para>
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="updateChildrensCheckboxes"></param>
        public void UpdateCheckbox(IEnumerable<TreeNode> nodes, bool updateChildrensCheckboxes)
        {
            if (nodes == null || nodes.Count() == 0)
                return;

            foreach (var node in nodes)
            {
                //this Try/Catch is solely for the FuncSetinitial blabla thing, because we dont know what the user puts in here
                //the setcheckedstate handles its exceptions on its own
                try
                {
                    SetCheckedState(node, FuncSetInitialCheckedState?.Invoke(node) ?? CheckedState.Unchecked);
                }
                catch (Exception ex)
                {
                    HandleException(new Exception(String.Format("An error occured while determining the initial CheckedState of the node {0}", node.Text), ex));
                    SetCheckedState(node, CheckedState.Unchecked);
                }

                if (updateChildrensCheckboxes)
                    UpdateCheckbox(node.Nodes, updateChildrensCheckboxes);
            }
        }

        /// <summary>
        /// <para>Updates the visibility of the checkbox for the passed node</para>
        /// <para>This function does only make sense to use if you have FuncIsCheckboxEnabledForNode set</para>
        /// </summary>
        public void UpdateAllCheckboxes()
        {
            UpdateCheckbox(this.Nodes, true);
        }

        #endregion

        #region Update Checked State by checking the Childs

        public delegate void CheckedStateChangedEventHandler(object sender, CheckedStateChangedEventArgs e);
        public new event CheckedStateChangedEventHandler AfterCheck;

        /// <summary>
        /// This will update the CheckedState of the passed node by checking each childs CheckedState
        /// </summary>
        /// <param name="node"></param>
        /// <param name="checkDirectChildsOnly"></param>
        /// <returns></returns>
        protected CheckedState InheritCheckedStateFromChilds(TreeNode node)
        {
            return InheritCheckedStateFromChilds(node, false);
        }

        /// <summary>
        /// This will update the CheckedState of the passed node by checking each childs CheckedState
        /// </summary>
        /// <param name="node">The node to update, or the parent of the updated Node if isNodeParentNeedingUpdate</param>
        /// <param name="isNodeParentNeedingUpdate">
        /// <para>If this is true, this method only checks the first generation childs (for a better performance).</para>
        /// <para>Set this to true, if the childs of the passed node have just recently been updated, otherwise you might get wrong results</para></param>
        /// <returns>The new/current CheckedState of the passed node</returns>
        protected CheckedState InheritCheckedStateFromChilds(TreeNode node, bool isNodeParentNeedingUpdate)
        {
            //there aint no node, then there aint nothin to do
            if (node == null)
                return CheckedState.Uninitialised;

            //also, if tristate is disabled, we will not check the nodes children to determine this nodes state
            if (!IsTriStateEnabled)
                return (CheckedState)node.StateImageIndex;

            int uncheckedNodes = 0, checkedNodes = 0, mixedNodes = 0;

            //we determine the state of the childs to find out the state of the current node
            foreach (TreeNode childNode in node.Nodes)
            {
                switch (isNodeParentNeedingUpdate ? InheritCheckedStateFromChilds(childNode) : (CheckedState)childNode.StateImageIndex)
                {
                    case CheckedState.Unchecked:
                        uncheckedNodes++;
                        break;
                    case CheckedState.Checked:
                        checkedNodes++;
                        break;
                    case CheckedState.Mixed:
                        mixedNodes++;
                        break;
                    default:
                    case CheckedState.Uninitialised:
                        break;
                }
            }

            //then we can infer the current nodes state
            int oldStateImageIndex = node.StateImageIndex;
            if (mixedNodes > 0)
                //if atleast one child is mixed, the current node has to be mixed as well
                SetCheckedState(node, CheckedState.Mixed);
            else if (uncheckedNodes == 0 && checkedNodes > 0)
                //if no unchecked and at least one checked node (and since no mixed too), this node is checked
                SetCheckedState(node, CheckedState.Checked);
            else if (checkedNodes > 0)
                //since (uncheckedNodes != 0 || checkedNodes == 0), we can infer, that when checkedNodes > 0, uncheckedNodes > 0 too,
                //thus the children are mixed (some checked, some unchecked)
                SetCheckedState(node, CheckedState.Mixed);
            else if (uncheckedNodes > 0)
                //now, in case there are some unchecked nodes (and therefore any nodes at all) this node is unchecked
                SetCheckedState(node, CheckedState.Unchecked);
            //else
            //    //otherwise that node doesn't have any children and its state cannot be inferred by its children
            //    SetCheckedState(node, node.StateImageIndex == (int)CheckedState.Uninitialised ? CheckedState.Unchecked : (CheckedState)node.StateImageIndex);

            //Inform everyone that this nodes checkedstate has changed
            if (oldStateImageIndex != node.StateImageIndex)
                InvokeAfterCheckEvent(new CheckedStateChangedEventArgs(node, (CheckedState)node.StateImageIndex));

            if (isNodeParentNeedingUpdate && oldStateImageIndex != node.StateImageIndex)
                InheritCheckedStateFromChilds(node.Parent, true);

            return (CheckedState)node.StateImageIndex;
        }

        /// <summary>
        /// This will update the passed nodes to the passed state. You mostlikely have updated their parent and want them
        /// to adapt the recent change
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="setState"></param>
        protected void InheritCheckedStateFromChilds(TreeNodeCollection nodes, CheckedState setState)
        {
            foreach (TreeNode node in nodes)
            {
                CheckedState oldState = (CheckedState)node.StateImageIndex;
                SetCheckedState(node, setState);

                //Inform everyone that this nodes checkedstate has changed, at least if it did change
                //we also don't want to announce a checked change, if there is no checkbox
                if (oldState != setState && (CheckedState)node.StateImageIndex != CheckedState.Uninitialised)
                    InvokeAfterCheckEvent(new CheckedStateChangedEventArgs(node, (CheckedState)node.StateImageIndex));

                InheritCheckedStateFromChilds(node.Nodes, (CheckedState)node.StateImageIndex);
            }
        }

        /// <summary>
        /// This will update the Checked and CheckedState of each node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="setState"></param>
        /// <remarks>This was necessary, because I kept setting the node.Checked Property after setting the stateimageindex,
        /// resulting the node.Checked Property overwriting the stateimageindex</remarks>
        protected void SetCheckedState(TreeNode node, CheckedState setState)
        {
            try
            {
                //if the user has disabled the Checkboxes in first place, we ain't lettin the user show the checkbox here
                if (!this.CheckBoxes) setState = CheckedState.Uninitialised;

                //check if the node should have a checkbox at all
                if (!FuncIsCheckboxEnabledForNode?.Invoke(node) ?? false) //i dont know the precedences :(
                {
                    node.StateImageIndex = (int)CheckedState.Uninitialised;
                    return;
                }

                //then set the wanted state
                switch (setState)
                {
                    case CheckedState.Unchecked:
                        node.Checked = false;
                        node.StateImageIndex = (int)CheckedState.Unchecked;
                        break;
                    case CheckedState.Checked:
                        node.Checked = true;
                        node.StateImageIndex = (int)CheckedState.Checked;
                        break;
                    case CheckedState.Mixed:
                        //also here, we only allow the mixed state if the TriState is enabled
                        if (!IsTriStateEnabled)
                        {
                            HandleException(new InvalidOperationException(String.Format("There was an attempt to set \"Mixed\"-state for node {0}, but the TriState-mode is disabled.", node.Text)));
                            break;
                        }

                        switch (MixedNodesMode)
                        {
                            case MixedStateMode.Checked: node.Checked = true; break;
                            case MixedStateMode.Unchecked: node.Checked = false; break;
                        }
                        node.StateImageIndex = (int)CheckedState.Mixed;
                        break;
                    default:
                    case CheckedState.Uninitialised:
                        node.StateImageIndex = (int)CheckedState.Uninitialised;
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #endregion

        #endregion


        #region Altering and Modifying TreeViews Behaviour

        /// <summary>
        /// Gets the required creation parameters when the control handle is created.
        /// </summary>
        /// <remarks>This is currently used to enable Double Buffering</remarks>
        protected override CreateParams CreateParams
        {
            //See "Extended Window Styles": https://msdn.microsoft.com/en-us/library/windows/desktop/ff700543(v=vs.85).aspx
            //Note to my future self: Ensure that the window doesn't use CS_OWNDC or CS_CLASSDC
            get
            {
                CreateParams cp = base.CreateParams;
                if (EnableDoubleBuffering)
                    cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        /// <summary>
        /// Supresses the doubleclick on the stateimageindex (our self-drawn checkboxes) to prevent
        /// collapsing/expanding the node on doubleclick on the checkbox
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // Suppress WM_LBUTTONDBLCLK on Checkboxes
            if (SupressCheckboxDoubleClick && m.Msg == 0x203)
            {
                System.Windows.Forms.TreeViewHitTestInfo tvhti = HitTest(new System.Drawing.Point((int)m.LParam));
                if (tvhti != null && tvhti.Location == System.Windows.Forms.TreeViewHitTestLocations.StateImage)
                {
                    m.Result = System.IntPtr.Zero;
                    return;
                }
            }

            base.WndProc(ref m);
        }

        /* Imports for Expansion-State */
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
        private const int SB_HORZ = 0x0;
        private const int SB_VERT = 0x1;

        /// <summary>
        /// Get the current expansion state, meaning which nodes are currently expanded
        /// </summary>
        /// <returns>List of expanded nodes fullpaths</returns>
        public List<string> GetExpansionState()
        {
            return Nodes.Descendants<TreeNode>()
                        .Where(n => n.IsExpanded)
                        .Select(n => n.FullPath)
                        .ToList();
        }

        /// <summary>
        /// Get the current expansion state, meaning which nodes are currently expanded
        /// </summary>
        /// <param name="scrollPosition">Returns the position where the treeview is currently scrolled to BEFORE retrieving the expansion state</param>
        /// <returns>List of expanded nodes fullpaths</returns>
        public List<string> GetExpansionState(out Point scrollPosition)
        {
            scrollPosition = GetScrollPosition();
            return GetExpansionState();
        }

        /// <summary>
        /// Sets the current expansion state, meaning it will expand the nodes in the list
        /// </summary>
        /// <param name="savedExpansionState">List with FullPaths of all nodes to expand</param>
        public void SetExpansionState(List<string> savedExpansionState)
        {
            foreach (var node in Nodes.Descendants<TreeNode>().Where(n => n.TreeView != null && savedExpansionState.Contains(n.FullPath)))
            {
                node.Expand();
            }
        }

        /// <summary>
        /// Sets the current expansion state, meaning it will expand the nodes in the list and automatically scrolls to the passed position
        /// </summary>
        /// <param name="savedExpansionState">List with FullPaths of all nodes to expand</param>
        /// <param name="scrollPosition">Position where to scroll to after expanding</param>
        public void SetExpansionState(List<string> savedExpansionState, Point scrollPosition)
        {
            SetExpansionState(savedExpansionState);
            SetScrollPosition(scrollPosition);
        }

        /// <summary>
        /// Retrieves the position where the treeview is currently scrolled to
        /// </summary>
        /// <returns></returns>
        public Point GetScrollPosition()
        {
            return new Point(
               GetScrollPos(this.Handle, SB_HORZ),
               GetScrollPos(this.Handle, SB_VERT));
        }

        /// <summary>
        /// Scrolls to the passed point
        /// </summary>
        /// <param name="scrollPosition"></param>
        public void SetScrollPosition(Point scrollPosition)
        {
            SetScrollPos(this.Handle, SB_HORZ, scrollPosition.X, true);
            SetScrollPos(this.Handle, SB_VERT, scrollPosition.Y, true);
        }

        #endregion


        #region Helping Methods

        /// <summary>
        /// Handler for Exceptions - if the exceptionHandler is set, that one is used,
        /// otherwise it will simply throw the exception
        /// </summary>
        /// <param name="ex"></param>
        protected void HandleException(Exception ex)
        {
            if (ExceptionHandler != null)
                ExceptionHandler(ex);
            else
                throw ex;
        }

        /// <summary>
        /// To set some debug-stuff
        /// </summary>
        [Conditional("DEBUG")]
        private void InitializeDebugInstance()
        {
            ExceptionHandler = new Action<Exception>((ex) =>
            {
                string message = String.Format("\nAn exception was thrown: {0}\n{1}\nStacktrace:\n{2}\n", ex.GetType().ToString(), ex.Message, ex.StackTrace);
                Exception innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    message += String.Format("\n\tAn exception was thrown: {0}\n\t{1}\n\tStacktrace:\n\t{2}\n", innerEx.GetType().ToString(), innerEx.Message, innerEx.StackTrace);
                    innerEx = innerEx.InnerException;
                }
                Console.WriteLine(message);
                Debugger.Break();
            });
        }

        #endregion
    }
}
