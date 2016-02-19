using System.Runtime.Serialization;

namespace SeleniumPeer.MainApplication
{
    /// <summary>
    ///     This class represents a click by a user on an element of a web page. It essentially
    ///     contains a good deal of metadata regarding the clicked element. Most of the class is
    ///     defined by serializable elements, but some of the data is altered as other information
    ///     is determined or found regarding the action's element.
    /// </summary>
    [DataContract]
    public class UserAction
    {
        public bool IsEnumerable = false;
        public string Text = "";
        public string ToPage;

        [DataMember(Name = "Label", IsRequired = true)]
        public string Label { get; set; }

        [DataMember(Name = "Name", IsRequired = true)]
        public string Name { get; set; }

        [DataMember(Name = "Id", IsRequired = true)]
        public string Id { get; set; }

        [DataMember(Name = "ClassName", IsRequired = true)]
        public string ClassName { get; set; }

        [DataMember(Name = "Page", IsRequired = true)]
        public string Page { get; set; }

        [DataMember(Name = "Node", IsRequired = true)]
        public string Node { get; set; }

        [DataMember(Name = "Type", IsRequired = true)]
        public string Type { get; set; }

        [DataMember(Name = "Path", IsRequired = true)]
        public string Path { get; set; }

        /// <summary>
        ///     This is not used, but is intended to be completed and solve the problem regarding
        ///     the recorder using multiple class names in the By.ClassName selector.
        /// </summary>
        public void ResolveMultipleClassNames()
        {
            string[] classNames = ClassName.Split(new[] {' '});
            if (Id == "null" && Name == "null")
            {
                if (classNames.Length > 0)
                {
//                    MessageBox.Show("The clicked element has more than one class name ascribed to it. You must select one to use!", "Selector Selector", new MessageBoxButtons())
                }
            }
        }

        /// <summary>
        ///     A method to get the best Label for a user's action (uses the most valuable metadata).
        /// </summary>
        /// <returns>
        ///     A string to be the label of this UserAction.
        /// </returns>
        public string GetBestLabel()
        {
            if (Id != "null")
            {
                return Id;
            }
            else if (Name != "null")
            {
                return Name;
            }
            else if (ClassName != "null")
            {
                return ClassName;
            }
            else
            {
                return Node + "/" + Type;
            }
        }

        /// <summary>
        ///     A method to find the best Selenium By selector for this action.
        /// </summary>
        /// <returns>
        ///     A string for the selector.
        /// </returns>
        public override string ToString()
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
            return by;
        }
    }
}