using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace SeleniumPeer.MainApplication
{
    /// <summary>
    ///     This is the primary Windows Form for SeleniumPeer. Its main features are to record
    ///     and export page objects, to visually edit them, and create blocks separately.
    /// </summary>
    internal sealed class PageObjectCreatorGui : SeleniumPeerReceiverForm
    {
        public static string TestName = "MyDescriptiveTestName";
        private readonly FolderNode _head = Exporter.LoadPageObjectTree();
        private UserAction _recentAction;
        private bool _recording;

        private readonly DataGridView _grid = new DataGridView();
        private readonly Button _record = new Button();

        private readonly MenuItem _fileMenu = new MenuItem("File");
        private readonly MainMenu _menu = new MainMenu();
        private readonly MenuItem _newTest = new MenuItem("New Test...");
        private readonly MenuItem _addBlock = new MenuItem("Add a Block...");
        private readonly MenuItem _enterTestName = new MenuItem("Enter Test Name...");

        public PageObjectCreatorGui()
        {
            SuspendLayout();

            //Title bar text.
            Text = "SeleniumPeer / Page Object and Test Generator - Test Name: " + TestName;
            ClientSize = new Size(800, 600);
            MinimumSize = new Size(640, 480);

            //Menu bar setup.
            _newTest.Click += NewTest;
            _enterTestName.Click += EnterTestName;
            _addBlock.Click += AddBlock;
            _fileMenu.MenuItems.Add(_newTest);
            _fileMenu.MenuItems.Add(_enterTestName);
            _fileMenu.MenuItems.Add(_addBlock);
            _menu.MenuItems.Add(_fileMenu);
            Menu = _menu;

            _grid.Columns.Add("Label (Editable)", "Label (Editable)");
            _grid.Columns.Add("Detected Page", "Detected Page");
            _grid.Columns.Add("Selector", "Selector");
            _grid.Columns.Add("Text to Enter (Editable)", "Text to Enter (Editable)");
            _grid.Columns.Add("Make Enumerable", "Make Enumerable");

            //Grid settings.
            _grid.RowHeadersVisible = false;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToOrderColumns = false;
            _grid.EditMode = DataGridViewEditMode.EditOnKeystroke;

            _grid.CellEndEdit += GridEdit;
            _grid.KeyUp += KeyUpHandler;

            _record.Text = "Start Recording / New Test";
            _record.Click += Record;

            UpdateControlSize(null, null);

            Controls.Add(_grid);
            Controls.Add(_record);

            FormClosed += EndApplication;
            Resize += UpdateControlSize;

            ResumeLayout();

            //Notice how the BlockCreator uses ShowDialog (which restricts access to the parent form).
            Show();

            Activate();

            UpdateGridView();
        }

        /// <summary>
        ///     Called when the BlockCreator is closed. Simply reclaims the receiving form.
        /// </summary>
        public void FinishAddingBlock()
        {
            Receiver.SetReceivingForm(this);
        }

        /// <summary>
        ///     Creates a block creator, which removes access to this form.
        /// </summary>
        private void AddBlock(object o, EventArgs e)
        {
            new BlockCreatorGui(this);
        }

        /// <summary>
        ///     Resets the positioning of the grid and record button.
        /// </summary>
        private void UpdateControlSize(object o, EventArgs e)
        {
            _grid.Size = new Size(ClientSize.Width, ClientSize.Height - 40);
            foreach (DataGridViewColumn col in _grid.Columns)
            {
                col.Width = _grid.Width/_grid.ColumnCount - 20/_grid.ColumnCount;
            }

            _record.Location = new Point(0, ClientSize.Height - 40);
            _record.Size = new Size(ClientSize.Width, 40);
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
                case 3:
                    elements[e.RowIndex].Text = _grid.Rows[e.RowIndex].Cells[3].Value.ToString();
                    break;
                case 4:
                    elements[e.RowIndex].IsEnumerable = bool.Parse(_grid.Rows[e.RowIndex].Cells[4].Value.ToString());
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
        ///     When a user closes the window, the process ends.
        /// </summary>
        private void EndApplication(object o, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        ///     When a user starts a new recording or clicks the menu item to enter a test name
        ///     or make a new test, a new test name is asked for.
        /// </summary>
        private void EnterTestName(object o, EventArgs e)
        {
            TestName = Interaction.InputBox("Please provide a descriptive name for a new test.", "New Test",
                                            TestName);
            if (TestName == "")
            {
                TestName = "MyEmptyTestName";
            }

            Text = "SeleniumPeer / Page Object and Test Generator - Test Name: " + TestName;
        }

        /// <summary>
        ///     Clears the GUI and asks for a new test name.
        /// </summary>
        private void NewTest(object sender, EventArgs e)
        {
            elements.Clear();
            UpdateGridView();
            EnterTestName(null, null);
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
//                userAction.ResolveMultipleClassNames();

                //This action will receive characters for it's Text field (Text Entered).
                _recentAction = userAction;
                //It is also added to the list of actions.
                elements.Add(userAction);

                //Its label is set to the best deduced label.
                userAction.Label = userAction.GetBestLabel();
                //If the tree already contains this action, then the actions label has already been set and this method will set the label again.
                _head.UpdateAction(ref userAction);

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
            if (_recentAction != null)
            {
                _recentAction.Text += c;
            }
        }

        /// <summary>
        ///     Handles the logic behind the record button. Different things happen if users are
        ///     already recording or not.
        /// </summary>
        private void Record(object sender, EventArgs eventArgs)
        {
            if (_recording)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (i > 0)
                    {
                        elements[i - 1].ToPage = elements[i].Page;
                        elements[i].ToPage = elements[i].Page;
                    }

                    //Add every page to PagesToOpen so the files are opened in notepad.
                    Exporter.PagesToOpen.Clear();
                    if (!Exporter.PagesToOpen.Contains(elements[i].Page))
                    {
                        Exporter.PagesToOpen.Add(elements[i].Page);
                    }
                }

                Exporter.ExportToOutputFolder(elements, _head, TestName);

                _record.Text = "Start Recording / New Test";
                _recording = false;
            }
            else
            {
                if (elements.Count > 0)
                {
                    if (MessageBox.Show("Would you like to clear the list of recorded elements?",
                                        "New Test",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        elements.Clear();
                    }
                }
                EnterTestName(null, null);

                _record.Text = "Stop Recording / Export";
                _recording = true;
            }
            UpdateGridView();
        }

        /// <summary>
        ///     Updates the grid by clearing it and re-adding all the rows which represent the list of
        ///     recorded actions.
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

                var pageCell = new DataGridViewTextBoxCell();
                pageCell.Value = act.Page;
                row.Cells.Add(pageCell);

                var selectorCell = new DataGridViewTextBoxCell();
                selectorCell.Value = act.ToString();
                row.Cells.Add(selectorCell);

                var textCell = new DataGridViewTextBoxCell();
                textCell.Value = act.Text;
                row.Cells.Add(textCell);

                //This IsEnumerable cell has a checkbox in it!
                var enumCell = new DataGridViewCheckBoxCell();
                enumCell.Value = act.IsEnumerable;
                row.Cells.Add(enumCell);

                row.Height = 20;
                _grid.Rows.Add(row);
            }
        }
    }
}