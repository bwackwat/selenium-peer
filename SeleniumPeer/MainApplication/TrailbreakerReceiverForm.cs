using System.Collections.Generic;
using System.Windows.Forms;

namespace SeleniumPeer.MainApplication
{
    /// <summary>
    ///     This is an awesome abstract class for forms which can receive user actions and
    ///     characters from a receiver. Incomplete (research into which elements can be shared
    ///     is being held).
    /// </summary>
    public abstract class SeleniumPeerReceiverForm : Form
    {
        internal readonly List<UserAction> elements = new List<UserAction>();
        public abstract void AddAction(UserAction userAction);
        public abstract void AddCharacter(char c);
        /// <summary>
        ///     Checks if an action exists in the set of elements.
        /// </summary>
        /// <param name="action">
        ///     Given action to check.
        /// </param>
        /// <returns>
        ///     True if the element is in the list.
        /// </returns>
        internal bool ActionExists(UserAction action)
        {
            foreach (UserAction userAction in elements)
            {
                if (userAction.Path == action.Path && userAction.Page == action.Page)
                {
                    return true;
                }
            }
            return false;
        }
    }
}