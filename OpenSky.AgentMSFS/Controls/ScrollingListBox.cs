// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScrollingListBox.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.AgentMSFS.Controls
{
    using System.Collections.Specialized;
    using System.Windows.Controls;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Auto-scroll to new elements listbox.
    /// </summary>
    /// <remarks>
    /// sushi.at, 22/12/2021.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Controls.ListBox"/>
    /// -------------------------------------------------------------------------------------------------
    public class ScrollingListBox : ListBox
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the current selection when an item in the
        /// <see cref="T:System.Windows.Controls.Primitives.Selector" /> has changed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/12/2021.
        /// </remarks>
        /// <param name="e">
        /// The event data.
        /// </param>
        /// <seealso cref="M:System.Windows.Controls.Primitives.Selector.OnItemsChanged(NotifyCollectionChangedEventArgs)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems.Count > 0)
            {
                this.ScrollIntoView(e.NewItems[e.NewItems.Count - 1]);
            }

            base.OnItemsChanged(e);
        }
    }
}