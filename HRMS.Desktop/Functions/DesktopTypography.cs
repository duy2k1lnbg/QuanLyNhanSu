using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraGrid.Views.Grid;
namespace QLyNSu.Functions
{
    internal static class DesktopTypography
    {
        private static readonly Font Body = new Font("Segoe UI", 9.5f);
        private static readonly Font Bold = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        internal static void Apply(Control root, BarManager manager = null)
        {
            root.Font = root.Font.Bold ? Bold : Body;
            if (root is Label label) label.AutoSize = true;
            foreach (Control child in root.Controls) Apply(child);
            if (manager != null) {
                foreach (Bar bar in manager.Bars) {
                    bar.BarAppearance.Normal.Font = Body; bar.BarAppearance.Normal.Options.UseFont = true;
                    bar.BarAppearance.Disabled.Font = Body; bar.BarAppearance.Disabled.Options.UseFont = true;
                    bar.BarAppearance.Hovered.Font = Body; bar.BarAppearance.Hovered.Options.UseFont = true;
                    bar.BarAppearance.Pressed.Font = Body; bar.BarAppearance.Pressed.Options.UseFont = true;
                }
                foreach (BarItem item in manager.Items) Item(item);
            }
        }
        internal static void Ribbon(RibbonControl ribbon)
        {
            ribbon.Font = Body;
            var appearance = ribbon.Controller?.AppearancesRibbon;
            if (appearance != null) {
                foreach (var state in new[] { appearance.Item, appearance.ItemDisabled, appearance.ItemHovered, appearance.ItemPressed,
                    appearance.PageHeader, appearance.PageHeaderHovered, appearance.PageHeaderSelected, appearance.PageGroupCaption }) {
                    state.Font = Body; state.Options.UseFont = true;
                }
            }
            ribbon.AutoSizeItems = true;
            foreach (BarItem item in ribbon.Items) {
                Item(item); item.LargeWidth = 0;
                if (item is BarButtonItem || item is BarSubItem)
                    item.RibbonStyle = RibbonItemStyles.Large | RibbonItemStyles.SmallWithText;
            }
            foreach (RibbonPage page in ribbon.Pages)
                foreach (RibbonPageGroup group in page.Groups) {
                    group.State = RibbonPageGroupState.Auto;
                    group.AllowMinimize = true;
                    group.AllowTextClipping = false;
                    foreach (BarItemLink link in group.ItemLinks) link.BeginGroup = false;
                }
            ribbon.LayoutChanged();
        }
        private static void Item(BarItem item)
        {
            item.ItemAppearance.Normal.Font = Body; item.ItemAppearance.Normal.Options.UseFont = true;
            item.ItemAppearance.Disabled.Font = Body; item.ItemAppearance.Disabled.Options.UseFont = true;
            item.ItemAppearance.Hovered.Font = Body; item.ItemAppearance.Hovered.Options.UseFont = true;
            item.ItemAppearance.Pressed.Font = Body; item.ItemAppearance.Pressed.Options.UseFont = true;
            item.ItemAppearance.Normal.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        }
        internal static void Grid(GridView view)
        {
            view.Appearance.Row.Font = Body; view.Appearance.Row.Options.UseFont = true;
            view.Appearance.HeaderPanel.Font = Bold; view.Appearance.HeaderPanel.Options.UseFont = true;
            view.Appearance.HeaderPanel.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            view.RowHeight = 32; view.ColumnPanelRowHeight = 38;
            foreach (DevExpress.XtraGrid.Columns.GridColumn c in view.Columns) {
                c.AppearanceHeader.Font = Bold; c.AppearanceHeader.Options.UseFont = true;
                c.AppearanceCell.Font = Body; c.AppearanceCell.Options.UseFont = true;
            }
        }
    }
}
