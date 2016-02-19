using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace SeleniumPeer.MainApplication
{
    /// <summary>
    ///     This class handles all exporting needs, including the translation between UserActions to
    ///     tree nodes, the creation of files, the updating of the tree XML, and the handling of
    ///     tree nodes.
    /// </summary>
    public class Exporter
    {
        //The path to SeleniumPeer's output folder. It is in MyDocuments!
        public static string OutputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SeleniumPeerOutput");

        //Three folders for three different types of .cs class files generated.
        public static string PageObjectsFolder = "\\PageObjects\\";
        public static string TestsFolder = "\\Tests\\";
        public static string BlocksFolder = "\\Blocks\\";

        public static string TreeName = "MBRegressionLibrary.xml";
        
        public static string PageObjectLibraryName = "MBRegressionLibrary";
        public static string PageObjectTestLibraryName = "MBRegressionLibrary.Tests";

        //An instance variable to keep track of the .cs class files to open after creating them.
        public static List<string> PagesToOpen = new List<string>();

        /// <summary>
        ///     This function updates the tree by passing each action to the tree's root node
        ///     (head) and either updating elements if they already exist or by creating new ones
        ///     which will be added to the XML/Tree.
        /// </summary>
        /// <param name="actions">
        ///     A list of UserActions to update the tree with.
        /// </param>
        /// <param name="head">
        ///     The head of the tree.
        /// </param>
        private static void UpdateTreeWithActions(List<UserAction> actions, FolderNode head)
        {
            foreach (UserAction action in actions)
            {
                if (head.Update(action) == false)
                {
                    Debug.WriteLine("The action was not able to be updated to the XML head!");
                }
            }
        }

        /// <summary>
        ///     This method uses an XmlTextWriter to write the tree to an XML file, starting with
        ///     the head node.
        /// </summary>
        /// <param name="head">
        ///     The root node of the tree.
        /// </param>
        private static void WriteTreeToXml(FolderNode head)
        {
            Directory.CreateDirectory(OutputPath);
            var writer = new XmlTextWriter(OutputPath + "\\" + TreeName, null);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();

            head.WriteToXml(writer);

            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        ///     This method simply makes sure each directory exists for smooth file writing.
        /// </summary>
        private static void CheckDirectories()
        {
            if (!Directory.Exists(OutputPath + PageObjectsFolder))
            {
                Directory.CreateDirectory(OutputPath + PageObjectsFolder);
            }

            if (!Directory.Exists(OutputPath + TestsFolder))
            {
                Directory.CreateDirectory(OutputPath + TestsFolder);
            }

            if (!Directory.Exists(OutputPath + BlocksFolder))
            {
                Directory.CreateDirectory(OutputPath + BlocksFolder);
            }
        }

        /// <summary>
        ///     This method, for the BlockCreatorGui, creates a miniature tree and fills it with the
        ///     given actions for the block. Then, it builds the tree.
        /// </summary>
        /// <param name="elements">
        ///     The elements of the block.
        /// </param>
        /// <param name="blockName">
        ///     The name of the block.
        /// </param>
        public static void ExportBlock(List<UserAction> elements, string blockName)
        {
            if (elements.Count > 0)
            {
                CheckDirectories();

                var block = new PageObjectNode(null, blockName);
                foreach (UserAction action in elements)
                {
                    block.Children.Add(new WebElementNode(block, action.Label, action.Name, action.Id, action.ClassName,
                                                          action.Node, action.Type, action.Path, blockName,
                                                          action.IsEnumerable));
                }

                block.BuildRaw();
            }
        }

        /// <summary>
        ///     For the PageObjectCreatorGui, this method will update the tree with the actions,
        ///     write the tree to an XML file, check directories, build the files, and create
        ///     a test file.
        /// </summary>
        /// <param name="actions">
        ///     The actions from the PageObjectCreatorGui.
        /// </param>
        /// <param name="head">
        ///     The head of the tree.
        /// </param>
        /// <param name="testName">
        ///     The desired name of the test.
        /// </param>
        public static void ExportToOutputFolder(List<UserAction> actions, FolderNode head, string testName)
        {
            if (actions.Count > 1)
            {
                UpdateTreeWithActions(actions, head);
                WriteTreeToXml(head);

                CheckDirectories();

                head.BuildRaw();
                CreateTestRaw(actions, testName);

                MessageBox.Show(
                    actions.Count + " new page objects " + (actions.Count > 1 ? " and a new test " : "") +
                    "were exported to \"" + OutputPath + "\"!",
                    "Export to Output Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        ///     Used when the application is first run, this method reads the entire XML file and
        ///     creates corresponding node objects (FolderNode, PageObjectNode, and WebElementNode)
        ///     to be used later for exporting purposes.
        /// </summary>
        /// <returns>
        ///     Returns the root node (head) of the loaded tree.
        /// </returns>
        public static FolderNode LoadPageObjectTree()
        {
            var head = new FolderNode(null, PageObjectLibraryName);
            PageObjectNode pageObject = null;

            try
            {
                var reader = new XmlTextReader(OutputPath + "\\" + TreeName);
                FolderNode curFolder;
                while (true)
                {
                    reader.Read();
                    if (reader.Name == PageObjectLibraryName)
                    {
                        head = new FolderNode(null, PageObjectLibraryName);
                        curFolder = head;
                        break;
                    }
                }
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == PageObjectNode.PageObjectString)
                        {
                            pageObject = new PageObjectNode(curFolder, reader.GetAttribute("Name"));
                            curFolder.Children.Add(pageObject);
                        }
                        else if (reader.Name == WebElementNode.WebElementString)
                        {
                            string label = reader.GetAttribute("Label");
                            string name = reader.GetAttribute("Name");
                            string id = reader.GetAttribute("Id");
                            string cclass = reader.GetAttribute("Class");
                            string node = reader.GetAttribute("Node");
                            string type = reader.GetAttribute("Type");
                            string path = reader.GetAttribute("Path");
                            string toname = reader.GetAttribute("ToPage");
                            string enumerable = reader.GetAttribute("IsEnumerable");

                            if (pageObject == null)
                            {
                                Debug.WriteLine("Bad tree XML! A web element was found before a page object! Exiting");
                                return head;
                            }

                            pageObject.Children.Add(new WebElementNode(pageObject, label, name, id, cclass, node, type,
                                                                       path,
                                                                       toname, bool.Parse(enumerable)));
                        }
                        else
                        {
                            var folder = new FolderNode(curFolder, reader.Name);
                            curFolder.Children.Add(folder);
                            curFolder = folder;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.Name != PageObjectNode.PageObjectString &&
                             reader.Name != WebElementNode.WebElementString)
                    {
                        curFolder = curFolder.Parent;
                    }
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("Creating a new page objext tree!");
            }
            return head;
        }

        /// <summary>
        ///     This is a convenience method to get a set of lines representing a valid (compilable)
        ///     .cs test class file. It is intended to be used for raw files and Roslyn compilation
        ///     units.
        /// </summary>
        /// <param name="actions">
        ///     A list of actions a user has made.
        /// </param>
        /// <param name="testName">
        ///     The desired name of the test.
        /// </param>
        /// <returns>
        ///     An IEnumerable of strings, where each string is a line of a valid .cs class file.
        /// </returns>
        private static IEnumerable<string> BuildTest(List<UserAction> actions, string testName)
        {
            var lines = new List<string>();

            lines.Add("using MBRegressionLibrary.Base;");
            lines.Add("using MBRegressionLibrary.Clients;");
            lines.Add("using MBRegressionLibrary.Tests.Attributes;");
            lines.Add("using MBRegressionLibrary.Tests.Tests.BusinessMode;");
            lines.Add("using MbUnit.Framework;");
            lines.Add("using Bumblebee.Extensions;");
            lines.Add("using " + PageObjectLibraryName + ";");
            lines.Add("");
            lines.Add("namespace " + PageObjectTestLibraryName);
            lines.Add("{");
            lines.Add("\t[Parallelizable]");
            lines.Add("\t[Site(\"AutobotMaster2\")]");
            lines.Add("\tinternal class " + testName + "Tests : AbstractBusinessModeTestSuite");
            lines.Add("\t{");
            lines.Add("\t\t[Test]");
            lines.Add("\t\tpublic void RunSimple" + testName + "Test()");
            lines.Add("\t\t{");
            //Uses the NavLab NavigationLinkAttribute.
            lines.Add("\t\t\tSession.CurrentBlock<BusinessModePage>().GoTo<" + testName + ">()");

            foreach (UserAction action in actions)
            {
                if (action.Node.ToLower() == "select")
                {
                    lines.Add("\t\t\t\t." + action.Label + ".Options.Random().Click()");
                }
                else if (action.Node.ToLower() == "input" && action.Type.ToLower() == "checkbox")
                {
                    lines.Add("\t\t\t\t." + action.Label + ".Toggle()");
                }
                else if (action.Node.ToLower() == "input" && action.Type.ToLower() != "button" &&
                         action.Type.ToLower() != "submit")
                {
                    lines.Add("\t\t\t\t." + action.Label + ".EnterText(\"" + action.Text + "\")");
                }
                else
                {
                    lines.Add("\t\t\t\t." + action.Label + ".Click()");
                }
            }
            lines.Add(";");

            lines.Add("\t\t}");
            lines.Add("\t}");
            lines.Add("}");

            return lines.ToArray();
        }

        /// <summary>
        ///     Creates a test by building the lines and writing each one to a file. Also, this
        ///     method uses ProcessStartInfo to initialize Notepad and open the test.
        /// </summary>
        /// <param name="actions">
        ///     The actions to be used for the test.
        /// </param>
        /// <param name="testName">
        ///     The chosen name of the test.
        /// </param>
        private static void CreateTestRaw(List<UserAction> actions, string testName)
        {
            string path = OutputPath + "\\Tests\\" + testName + "Tests.cs";

            FileStream fileStream = File.Create(path);
            var writer = new StreamWriter(fileStream);

            IEnumerable<string> lines = BuildTest(actions, testName);

            foreach (string s in lines)
            {
                writer.WriteLine(s);
            }

            writer.Close();
            fileStream.Close();

            var pi = new ProcessStartInfo(path);
            pi.Arguments = Path.GetFileName(path);
            pi.UseShellExecute = true;
            pi.WorkingDirectory = Path.GetDirectoryName(path);
            pi.FileName = "C:\\Windows\\notepad.exe";
            pi.Verb = "OPEN";
            Process.Start(pi);
        }
    }
}