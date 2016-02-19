using System.Collections.Generic;
using System.Xml;

namespace SeleniumPeer.MainApplication
{
    /// <summary>
    ///     Represents a Folder within a hierarchy of page objects. There is currently no way for
    ///     more of these to be generated, and this is only used for the root node (head).
    /// </summary>
    public class FolderNode
    {
        public readonly string Title;
        public List<FolderNode> Children = new List<FolderNode>();
        public FolderNode Parent;

        public FolderNode(FolderNode parent, string title)
        {
            Parent = parent;
            Title = title;
        }

        /// <summary>
        ///     Writes the title of this folder as the start of an XML element, and then fills it
        ///     with its children.
        /// </summary>
        /// <param name="writer">
        ///     Accepts the primary writer.
        /// </param>
        public virtual void WriteToXml(XmlTextWriter writer)
        {
            writer.WriteStartElement(Title);
            foreach (FolderNode element in Children)
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
        public virtual void UpdateAction(ref UserAction userAction)
        {
            foreach (FolderNode element in Children)
            {
                element.UpdateAction(ref userAction);
            }
        }

        /// <summary>
        ///     This method updates the tree given a user action. If necessary, a new child will
        ///     be added because the UserAction's page doesn't have a corresponding PageObjectNode.
        /// </summary>
        /// <param name="userAction">
        ///     The UserAction to update the tree with.
        /// </param>
        /// <returns>
        ///     Should only return true unless there was an error.
        /// </returns>
        public virtual bool Update(UserAction userAction)
        {
            //If this folder doesn't contain the action's page and this is the root then
            if (Contains(userAction) == false && Parent == null)
            {
                //Create a new page object element to suit this page
                Children.Add(new PageObjectNode(this, userAction.Page));
            }

            //Should always find an element to add the action to.
            foreach (FolderNode element in Children)
            {
                if (element.Update(userAction))
                {
                    return true;
                }
            }
            //Should be unreachable!
            return false;
        }

        /// <summary>
        ///     Checks if this node's children contains the given UserAction.
        /// </summary>
        /// <param name="userAction">
        ///     The UserAction to check.
        /// </param>
        /// <returns>
        ///     True of this node contains a corresponding WebElementNode already.
        /// </returns>
        public virtual bool Contains(UserAction userAction)
        {
            foreach (FolderNode element in Children)
            {
                if (element.Contains(userAction))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Builds the children of this folder to raw files.
        /// </summary>
        public virtual void BuildRaw()
        {
            foreach (FolderNode node in Children)
            {
                node.BuildRaw();
            }
        }
    }
}