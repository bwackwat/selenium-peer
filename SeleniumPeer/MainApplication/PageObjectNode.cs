using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SeleniumPeer.MainApplication
{
    /// <summary>
    ///     This class represents a whole PageObject which contains WebElements (defined by selectors).
    /// </summary>
    internal class PageObjectNode : FolderNode
    {
        //A string used for the XML tree.
        public static string PageObjectString = "PageObject";

        public new List<WebElementNode> Children = new List<WebElementNode>();
        public string Name;

        public PageObjectNode(FolderNode parent, string name)
            : base(parent, PageObjectString)
        {
            Name = name;
        }

        /// <summary>
        ///     Writes the PageObjectString as the start of an XML element, and then fills it
        ///     with its children and an attribute for it's name.
        /// </summary>
        /// <param name="writer">
        ///     Accepts the primary writer.
        /// </param>
        public override void WriteToXml(XmlTextWriter writer)
        {
            writer.WriteStartElement(Title);
            writer.WriteAttributeString("Name", Name);
            foreach (WebElementNode element in Children)
            {
                element.WriteToXml(writer);
            }
            writer.WriteEndElement();
        }

        /// <summary>
        ///     This method updates the given user action by passing it onto its children.
        /// </summary>
        /// <param name="userAction">
        ///     A user action which requires an update check.
        /// </param>
        public override void UpdateAction(ref UserAction userAction)
        {
            foreach (WebElementNode element in Children)
            {
                element.UpdateAction(ref userAction);
            }
        }

        /// <summary>
        ///     This method updates the tree given a user action. If necessary, a new child will
        ///     be added because the UserAction's path doesn't have a corresponding WebElementNode.
        ///     Otherwise, the WebElementNode with a matching patch has its values updated.
        /// </summary>
        /// <param name="userAction">
        ///     The UserAction to update the tree with.
        /// </param>
        /// <returns>
        ///     Should only return true unless there was an error (the action's page should always
        ///     have the name of this page object!).
        /// </returns>
        public override bool Update(UserAction userAction)
        {
            if (Name == userAction.Page)
            {
                foreach (WebElementNode element in Children)
                {
                    if (element.Path == userAction.Path)
                    {
                        element.Label = userAction.Label;
                        element.Name = userAction.Name;
                        element.Id = userAction.Id;
                        element.ClassName = userAction.ClassName;
                        element.Node = userAction.Node;
                        element.Type = userAction.Type;
                        element.ToPage = userAction.ToPage;
                        element.IsEnumerable = userAction.IsEnumerable;
                        return true;
                    }
                }
                Children.Add(new WebElementNode(this, userAction.Label, userAction.Name, userAction.Id, userAction.ClassName, userAction.Node,
                                                userAction.Type, userAction.Path,
                                                userAction.ToPage, userAction.IsEnumerable));
                return true;
            }
            return false;
        }

        /// <summary>
        ///     This PageObjectNode should or does contain the given UserAction if they share the
        ///     same page's name.
        /// </summary>
        /// <param name="userAction">
        ///     A UserAction to check containment.
        /// </param>
        /// <returns>
        ///     True if the page should or does contain the action.
        /// </returns>
        public override bool Contains(UserAction userAction)
        {
            return Name == userAction.Page;
        }

        /// <summary>
        ///     This is a convenience method to get a list of strings (each representing a line of
        ///     code) for the current page object.
        /// </summary>
        /// <returns>
        ///     An enumerable, reusable ordered set of strings.
        /// </returns>
        private IEnumerable<string> Build()
        {
            var lines = new List<string>();

            lines.Add("using OpenQA.Selenium;");
            lines.Add("");
            lines.Add("namespace " + Exporter.PageObjectLibraryName);
            lines.Add("{");
            lines.Add("\tpublic class " + Name);
            lines.Add("\t{");
            
            foreach (WebElementNode node in Children)
            {
                lines.Add("\t\tpublic IWebElement " + node.Label + ";");
            }

            lines.Add("");
            lines.Add("\t\tpublic " + Name + "(IWebDriver driver)");
            lines.Add("\t\t{");
            lines.Add("\t\t\tdriver.Navigate().GoToUrl(\"INSERT PAGE URL HERE\");");
            lines.Add("");

            foreach (WebElementNode node in Children)
            {
                lines.Add(node.Build());
            }

            lines.Add("\t\t}");

            lines.Add("\t}");
            lines.Add("}");

            return lines.ToArray();
        }

    /// <summary>
    ///     Builds the content of this PageObjectNode into raw .cs class files. Uses the
    ///     "PageObjects" output folder and ProcessStartInfo to open them after.
    /// </summary>
    public override void BuildRaw()
        {
            if (Name == null)
            {
                return;
            }

            string path = Exporter.OutputPath + "\\PageObjects\\" + Name + ".cs";

            FileStream fileStream = File.Create(path);
            var writer = new StreamWriter(fileStream);

            IEnumerable<string> build = Build();

            foreach (string s in build)
            {
                writer.WriteLine(s);
            }

            writer.Close();
            fileStream.Close();

            if (PageObjectCreatorGui.TestName == Name || Exporter.PagesToOpen.Contains(Name))
            {
                ProcessStartInfo pi = new ProcessStartInfo(path);
                pi.Arguments = Path.GetFileName(path);
                pi.UseShellExecute = true;
                pi.WorkingDirectory = Path.GetDirectoryName(path);
                pi.FileName = "C:\\Windows\\notepad.exe";
                pi.Verb = "OPEN";
                Process.Start(pi);
            }
        }
    }
}