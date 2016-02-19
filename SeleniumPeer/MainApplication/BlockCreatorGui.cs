using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace SeleniumPeer.MainApplication
{
    /// <summary>
    ///     This is a more-encapsulated version of the PageObjectCreator because it doesn't deal
    ///     with or affect the tree of page objects.
    /// </summary>
    internal sealed class BlockCreatorGui : SeleniumPeerReceiverForm
    {
        private string _blockName = "MyBlockName";
        private readonly PageObjectCreatorGui _main;

        private readonly DataGridView _grid = new DataGridView();
        private readonly Button _viewBlock = new Button();

        private readonly MainMenu _menu = new MainMenu();
        private readonly MenuItem _fileMenu = new MenuItem("File");
        private readonly MenuItem _enterBlockName = new MenuItem("Enter Block Name...");
        private readonly MenuItem _reset = new MenuItem("Reset!");

        public BlockCreatorGui(PageObjectCreatorGui main)
        {
            _main = main;
            Owner = main;

            Receiver.SetReceivingForm(this);

            EnterBlockName(null, null);

            SuspendLayout();

            ClientSize = new Size(400, 300);
            MinimumSize = new Size(320, 240);

            _enterBlockName.Click += EnterBlockName;
            _reset.Click += Reset;
            _fileMenu.MenuItems.Add(_enterBlockName);
            _fileMenu.MenuItems.Add(_reset);
            _menu.MenuItems.Add(_fileMenu);
            Menu = _menu;

            _grid.Columns.Add("Label (Editable)", "Label (Editable)");
            _grid.Columns.Add("Selector", "Selector");
            _grid.Columns.Add("Make Enumerable", "Make Enumerable");

            _grid.RowHeadersVisible = false;
            _grid.AllowUserToResizeRows = false;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToOrderColumns = false;

            _grid.EditMode = DataGridViewEditMode.EditOnKeystroke;

            _grid.CellEndEdit += GridEdit;
            _grid.KeyUp += KeyUpHandler;

            _viewBlock.Text = "Save and View my Block";
            _viewBlock.Click += ViewBlock;

            UpdateControlSize(null, null);

            Controls.Add(_grid);
            Controls.Add(_viewBlock);

            FormClosed += EndApplication;
            Resize += UpdateControlSize;

            ResumeLayout();

            //Show dialog restricts the parent to be interacted with.
            ShowDialog();

            Activate();

            UpdateGridView();
        }

        /// <summary>
        ///     Clears the GUI and asks for a new block name.
        /// </summary>
        private void Reset(object o, EventArgs e)
        {
            elements.Clear();
            UpdateGridView();
            EnterBlockName(null, null);
        }

        /// <summary>
        ///     Exports the block.
        /// </summary>
        private void ViewBlock(object o, EventArgs e)
        {
            Exporter.PagesToOpen.Clear();
            Exporter.PagesToOpen.Add(_blockName);
            Exporter.ExportBlock(elements, _blockName);
        }

        /// <summary>
        ///     When the GUI is closed, the block is exported and the parent is given control again.
        /// </summary>
        private void EndApplication(object o, EventArgs e)
        {
            ViewBlock(null, null);
            _main.FinishAddingBlock();
        }

        /// <summary>
        ///     When a user opens the block creator or resets the creator, a new block name is
        ///     requested.
        /// </summary>
        private void EnterBlockName(object o, EventArgs e)
        {
            _blockName = Interaction.InputBox("Please provide a descriptive name for your new block.", "New Block",
                                              _blockName);
            if (_blockName == "")
            {
                _blockName = "MyEmptyTestName";
            }

            Text = "SeleniumPeer / Block Creator - Block Name: " + _blockName;
        }

        /// <summary>
        ///     Adds an action from the receiver to the list of elements.
        /// </summary>
        /// <param name="userAction">
        ///     The recently serialized action from the receiver.
        /// </param>
        public override void AddAction(UserAction userAction)
        {
            //If this exact action already exists in the list then the click event is ignored.
            if (!ActionExists(userAction))
            {
                //It is also added to the list of actions.
                elements.Add(userAction);

                //Its label is set to the best deduced label.
                userAction.Label = userAction.GetBestLabel();

                //Updates the grid to include the new action.
                UpdateGridView();
                //The scrollbar is set to the bottom of the grid.
                _grid.FirstDisplayedScrollingRowIndex = _grid.RowCount - 1;
            }
        }

        /// <summary>
        ///     Adds a character to the Text field of the most recently added action.
        /// </summary>
        /// <param name="c">
        ///     A character recently parsed from the receiver.
        /// </param>
        public override void AddCharacter(char c)
        {
        }

        /// <summary>
        ///     Resets the positioning of the grid and view button.
        /// </summary>
        private void UpdateControlSize(object o, EventArgs e)
        {
            _grid.Size = new Size(ClientSize.Width, ClientSize.Height - 40);
            foreach (DataGridViewColumn col in _grid.Columns)
            {
                col.Width = _grid.Width/_grid.ColumnCount - 20/_grid.ColumnCount;
            }
            _viewBlock.Location = new Point(0, ClientSize.Height - 40);
            _viewBlock.Size = new Size(ClientSize.Width, 40);
        }

        /// <summary>
        ///     Updates the UserAction metadata when editable grid cells are edited.
        /// </summary>
        private void GridEdit(object o, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 0:
                    elements[e.RowIndex].Label = _grid.Rows[e.RowIndex].Cells[0].Value.ToString();
                    break;
                case 2:
                    elements[e.RowIndex].IsEnumerable = bool.Parse(_grid.Rows[e.RowIndex].Cells[2].Value.ToString());
                    break;
            }
        }

        /// <summary>
        ///     Removes elements from the list of actions based on the grid's selected cells.
        /// </summary>
        private void KeyUpHandler(object o, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (_grid.SelectedCells.Count > 0)
                {
                    elements.RemoveAt(_grid.SelectedCells[0].RowIndex);
                }
                UpdateGridView();
            }
        }

        /// <summary>
        ///     Updates the grid by clearing it and re-adding all the rows which represent the list of
        ///     recorded actions. Doesn't have a page or text column because no test is generated and
        ///     only one page will be generated (the name of the block).
        /// </summary>
        private void UpdateGridView()
        {
            _grid.Rows.Clear();

            foreach (UserAction act in elements)
            {
                var row = new DataGridViewRow();

                var labelCell = new DataGridViewTextBoxCell();
                labelCell.Value = act.Label;
                row.Cells.Add(labelCell);

                var stringCell = new DataGridViewTextBoxCell();
                stringCell.Value = act.ToString();
                row.Cells.Add(stringCell);

                var enumCell = new DataGridViewCheckBoxCell();
                enumCell.Value = act.IsEnumerable;
                row.Cells.Add(enumCell);

                row.Height = 20;
                _grid.Rows.Add(row);
            }
        }
    }
}