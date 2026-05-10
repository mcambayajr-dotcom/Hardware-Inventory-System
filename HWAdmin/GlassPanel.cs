using System.Drawing;
using System.Windows.Forms;

namespace ComputerHardwareStockMonitoringSystem
{
    public class GlassPanel : Panel
    {
        public Color FillColor { get; set; } = Color.FromArgb(210, 245, 255, 250);
        public Color BorderColor { get; set; } = Color.FromArgb(95, 150, 130);
        public int Radius { get; set; } = 18;

        public GlassPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var path = RoundedRect(ClientRectangle, Radius))
            using (var brush = new SolidBrush(FillColor))
            using (var pen = new Pen(BorderColor, 1))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
            base.OnPaint(e);
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d - 1, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d - 1, bounds.Bottom - d - 1, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d - 1, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
