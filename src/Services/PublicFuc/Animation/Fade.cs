namespace test.src.Services.PublicFuc.Animation
{
    public static class FadeExtensions
    {
        #region 窗口效果
        // 淡入效果
        public static async Task WinFadeIn(this Form form, int duration = 300)
        {
            form.Opacity = 0;
            form.Show();
            //form.Visible = true;

            int steps = 20;
            float increment = 1f / steps;
            int delay = duration / steps;

            for (int i = 0; i <= steps; i++)
            {
                form.Opacity = i * increment;
                await Task.Delay(delay);
            }
            form.Opacity = 1;
        }

        // 淡出效果
        public static async Task WinFadeOut(this Form form, int duration = 300)
        {
            int steps = 20;
            float decrement = 1f / steps;
            int delay = duration / steps;

            for (int i = steps; i >= 0; i--)
            {
                form.Opacity = i * decrement;
                await Task.Delay(delay);
            }
            form.Opacity = 0;
            form.Hide();
        }

        #endregion 窗口效果

        #region 控件效果

        // 淡入效果 - 适用于所有Control派生类
        public static async Task FadeIn(this Control control, int duration = 300)
        {
            // 检查控件是否支持Opacity（只有Form支持）
            if (control is Form form)
            {
                form.Opacity = 0;
                form.Visible = true;
            }
            else
            {
                // 对于普通控件，使用透明度效果
                control.Visible = true;
                control.BackColor = Color.FromArgb(0, control.BackColor);
            }

            int steps = 20;
            float increment = 1f / steps;
            int delay = duration / steps;

            for (int i = 0; i <= steps; i++)
            {
                float opacity = i * increment;

                if (control is Form formControl)
                {
                    formControl.Opacity = opacity;
                }
                else
                {
                    // 通过BackColor的Alpha通道模拟透明度
                    Color originalColor = control.BackColor;
                    int alpha = (int)(opacity * 255);
                    control.BackColor = Color.FromArgb(alpha, originalColor);

                    // 递归处理子控件的透明度（如果需要）
                    SetChildControlsOpacity(control, opacity);
                }

                await Task.Delay(delay);
            }
        }

        // 设置子控件透明度
        private static void SetChildControlsOpacity(Control parent, float opacity)
        {
            foreach (Control child in parent.Controls)
            {
                if (child.BackColor != Color.Transparent)
                {
                    Color originalColor = child.BackColor;
                    int alpha = (int)(opacity * 255);
                    child.BackColor = Color.FromArgb(alpha, originalColor);
                }

                // 递归设置子控件的子控件
                if (child.HasChildren)
                {
                    SetChildControlsOpacity(child, opacity);
                }
            }
        }

        #endregion 控件效果
    }
}
