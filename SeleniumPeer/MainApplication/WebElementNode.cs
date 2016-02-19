using System.Collections.Generic;
using System.Xml;

namespace SeleniumPeer.MainApplication
{
    /// <summary>
    ///     This class represents a single element within a page object class, primarily defined
    ///     by a unique XPath (path). It contains lots of metadata for logistics regarding the
    ///     creation of page object code and tests. It is different from the UserAction because it
    ///     cannot be serialized. This may change in the future.
    /// </summary>
    internal class WebElementNode : FolderNode
    {
        //A string used for the XML tree.
        public static string WebElementString = "WebElement";

        private readonly PageObjectNode ParentPageObjectNode;
        public string ClassName;
        public string Id;
        public bool IsEnumerable;

        public string Label;
        public string Name;
        public string Node;
        public string Path;
        public string ToPage;
        public string Type;

        public WebElementNode(PageObjectNode parent, string label, string name, string id, string cclass, string node,
                              string type,
                              string path, string toname, bool enumerable)
            : base(parent, WebElementString)
        {
            ParentPageObjectNode = parent;
            Label = label;
            Name = name;
            Id = id;
            ClassName = cclass;
            Node = node;
            Type = type;
            Path = path;
            ToPage = toname;
            IsEnumerable = enumerable;
        }

        /// <summary>
        ///     Writes the title of this element as the start of an XML element, and then fills it
        ///     with its metadata.
        /// </summary>
        /// <param name="writer">
        ///     Accepts the primary writer.
        /// </param>
        public override void WriteToXml(XmlTextWriter writer)
        {
            writer.WriteStartElement(base.Title);
            writer.WriteAttributeString("Label", Label);
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("Id", Id);
            writer.WriteAttributeString("ClassName", ClassName);
            writer.WriteAttributeString("Node", Node);
            writer.WriteAttributeString("Type", Type);
            writer.WriteAttributeString("Path", Path);
            writer.WriteAttributeString("ToPage", ToPage);
            writer.WriteAttributeString("IsEnumerable", IsEnumerable.ToString());
            writer.WriteEndElement();
        }

        /// <summary>
        ///     This method updates the given user action by checking if they have the same critical
        ///     metadata. If the given action has the same path and page as this web element then it's
        ///     label will update to the label which is already in the tree. This is important when a
        ///     user clicks on an element which has been named in the past.
        /// </summary>
        /// <param name="userAction">
        ///     A user action which requires an update check.
        /// </param>
        public override void UpdateAction(ref UserAction userAction)
        {
            if (Path == userAction.Path && ParentPageObjectNode.Name == userAction.Page)
            {
                userAction.Label = Label;
            }
        }

        /// <summary>
        ///     This is a convenience method to get a list of strings (each representing a line of
        ///     code) for the current page object element.
        /// </summary>
        /// <returns>
        ///     An enumerable, reusable ordered set of strings.
        /// </returns>
        public string Build()
        {
            string by;
            
            if (Id != "null")
            {
                by = "By.Id(\"" + Id + "\")";
            }
            else if (Name != "null")
            {
                by = "By.Name(\"" + Name + "\")";
            }
            else if (ClassName != "null")
            {
                by = "By.ClassName(\"" + ClassName + "\")";
            }
            else
            {
                by = "By.XPath(\"" + Path.Replace("\"", "\\\"") + "\")";
            }

            return "\t\t\t" + Label + " = driver.FindElement(" + by + ");";
        }
    }
}