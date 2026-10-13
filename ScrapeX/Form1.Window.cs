using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace ScrapeX
{
    public partial class Form1
    {
        private readonly Color accent = Color.FromArgb(71, 227, 192);
        private readonly Color muted = Color.FromArgb(137, 158, 184);
        private readonly System.Collections.Generic.Dictionary<string, Bitmap> smallFlags = new System.Collections.Generic.Dictionary<string, Bitmap>();
        private System.Drawing.Image SmallFlag(string code)
        {
            if (string.IsNullOrEmpty(code)) return emptyFlag;
            code = code.ToLowerInvariant(); Bitmap flag;
            if (smallFlags.TryGetValue(code, out flag)) return flag;
            var original = Properties.Resources.ResourceManager.GetObject(code) as Image;
            if (original == null) return emptyFlag;
            flag = new Bitmap(20, 13);
            using (var g = Graphics.FromImage(flag)) { g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; g.DrawImage(original, new Rectangle(0, 0, 20, 13)); }
            smallFlags.Add(code, flag); return flag;
        }
        private void DisabledButtonPaint(object sender, PaintEventArgs e)
        {
            var button = (Button)sender;
            if (button.Enabled) return;
            e.Graphics.Clear(Color.FromArgb(42, 53, 70));
            TextRenderer.DrawText(e.Graphics, button.Text, button.Font, button.ClientRectangle, Color.FromArgb(180, 192, 210), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            using (var border = new Pen(Color.FromArgb(65, 79, 98))) e.Graphics.DrawRectangle(border, 0, 0, button.Width - 1, button.Height - 1);
        }
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private void TitleMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (e.Clicks == 2) { MaximizeClick(sender, EventArgs.Empty); return; }
            ReleaseCapture(); SendMessage(Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
        }
        private void MinimizeClick(object sender, EventArgs e) { WindowState = FormWindowState.Minimized; }
        private void MaximizeClick(object sender, EventArgs e)
        {
            MaximizedBounds = Screen.FromHandle(Handle).WorkingArea;
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }
        private void CloseClick(object sender, EventArgs e) { Close(); }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x84 && WindowState == FormWindowState.Normal && !DesignMode)
            {
                var pt = PointToClient(new Point(unchecked((short)(long)m.LParam), unchecked((short)((long)m.LParam >> 16))));
                const int edge = 7;
                bool left = pt.X < edge, right = pt.X >= ClientSize.Width - edge, top = pt.Y < edge, bottom = pt.Y >= ClientSize.Height - edge;
                if (left && top) m.Result = new IntPtr(13);
                else if (right && top) m.Result = new IntPtr(14);
                else if (left && bottom) m.Result = new IntPtr(16);
                else if (right && bottom) m.Result = new IntPtr(17);
                else if (left) m.Result = new IntPtr(10);
                else if (right) m.Result = new IntPtr(11);
                else if (top) m.Result = new IntPtr(12);
                else if (bottom) m.Result = new IntPtr(15);
            }
        }
        private void ShowPage(Panel page)
        {
            resultPage.Visible = page == resultPage; sourcePage.Visible = page == sourcePage; activityPage.Visible = page == activityPage;
            resultsTabButton.ForeColor = page == resultPage ? accent : muted;
            sourcesTabButton.ForeColor = page == sourcePage ? accent : muted;
            activityTabButton.ForeColor = page == activityPage ? accent : muted;
        }
        private void ResultsTabClick(object sender, EventArgs e) { ShowPage(resultPage); }
        private void SourcesTabClick(object sender, EventArgs e) { ShowPage(sourcePage); }
        private void ActivityTabClick(object sender, EventArgs e) { ShowPage(activityPage); }
        private void GridMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        { if (e.Button == MouseButtons.Right && e.RowIndex >= 0) grid.CurrentCell = grid.Rows[e.RowIndex].Cells[1]; }
        private void GridFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= visible.Count || e.ColumnIndex != 4) return;
            var r = visible[e.RowIndex];
            e.CellStyle.ForeColor = r.HttpsVerified ? accent : r.HttpWorking ? Color.FromArgb(255, 192, 99) : Color.FromArgb(251, 126, 147);
        }
        private void GridPaint(object sender, PaintEventArgs e)
        {
            if (grid.RowCount == 0)
                TextRenderer.DrawText(e.Graphics, results.Count == 0 ? "Collect a list, then verify HTTP + HTTPS.\nOnly certificate-validated routes enter the HTTPS output." : "No results match this filter.", Font, new Rectangle(0, 55, grid.Width, Math.Max(40, grid.Height - 70)), muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
