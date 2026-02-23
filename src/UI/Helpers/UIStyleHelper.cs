using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace test.src.UI.Helpers
{
    public static class UIStyleHelper
    {
        // 现代黑色风格配色
        public static readonly Color MainBackgroundColor = Color.FromArgb(18, 18, 18); // 深黑背景
        public static readonly Color CardBackgroundColor = Color.FromArgb(30, 30, 30); // 卡片背景
        public static readonly Color TextColor = Color.FromArgb(224, 224, 224); // 浅灰文本
        public static readonly Color AccentColor = Color.FromArgb(0, 120, 215); // Windows 蓝色强调色
        public static readonly Color SuccessColor = Color.FromArgb(76, 175, 80); // 绿色
        public static readonly Color ErrorColor = Color.FromArgb(244, 67, 54); // 红色
        public static readonly Color WarningColor = Color.FromArgb(255, 152, 0); // 橙色

        // 圆角半径
        public const int CornerRadius = 15;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        public static void ApplyModernStyle(Form form)
        {
            form.BackColor = MainBackgroundColor;
            form.ForeColor = TextColor;
            form.FormBorderStyle = FormBorderStyle.None; // 无边框，需要自定义标题栏

            // 启用拖拽
            EnableDrag(form);

            // 递归应用样式
            ApplyStyleToControls(form.Controls);
        }

        public static void EnableDrag(Form form)
        {
            form.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        public static void ApplyStyleToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = CardBackgroundColor;
                    btn.ForeColor = TextColor;
                    btn.Cursor = Cursors.Hand;

                    // 为按钮添加圆角区域（简单实现）
                    btn.Paint += (s, e) =>
                    {
                        var g = e.Graphics;
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        var rect = new Rectangle(0, 0, btn.Width, btn.Height);
                        using (var path = GetRoundedPath(rect, 8))
                        {
                            // 填充背景
                            using (var brush = new SolidBrush(btn.BackColor))
                            {
                                g.FillPath(brush, path);
                            }
                            // 绘制文字 (简单居中)
                            TextRenderer.DrawText(g, btn.Text, btn.Font, rect, btn.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    };
                }
                else if (control is Label lbl)
                {
                    lbl.ForeColor = TextColor;
                    lbl.BackColor = Color.Transparent;
                }
                else if (control is Panel pnl)
                {
                    pnl.BackColor = Color.Transparent; // 默认透明，除非是卡片
                }

                if (control.HasChildren)
                {
                    ApplyStyleToControls(control.Controls);
                }
            }
        }

        public static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = radius * 2F;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        // 创建卡片样式的Panel
        public static Panel CreateCardPanel()
        {
            Panel panel = new Panel();
            panel.BackColor = Color.Transparent;
            panel.Padding = new Padding(10);

            panel.Paint += (s, e) =>
            {
                var p = s as Panel;
                if (p == null) return;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                using (var path = GetRoundedPath(rect, CornerRadius))
                {
                    using (var brush = new SolidBrush(CardBackgroundColor))
                    {
                        g.FillPath(brush, path);
                    }
                    using (var pen = new Pen(Color.FromArgb(50, 50, 50), 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            };

            return panel;
        }
    }
}
