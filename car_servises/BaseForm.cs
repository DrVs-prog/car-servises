using System.Drawing;
using System.Windows.Forms;

namespace car_servises
{
    public class BaseForm : Form
    {
        public BaseForm()
        {
            BackColor = AppStyles.BackgroundColor;
            //Font = AppStyles.NormalFont;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            AutoScaleMode = AutoScaleMode.None;


            Load += BaseForm_Load;
        }

        private void BaseForm_Load(object sender, System.EventArgs e)
        {
            ApplyStyles(this);
        }

        private void ApplyStyles(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                // Кнопки
                if (c is Button btn)
                {
                    // По умолчанию НЕ primary
                    AppStyles.ApplyButtonStyle(btn);
                }
                // TextBox
                else if (c is TextBox tb)
                {
                    AppStyles.ApplyTextBoxStyle(tb);
                }
                // Label
                else if (c is Label lbl)
                {
                    AppStyles.ApplyLabelStyle(lbl);
                }
                // DataGridView
                else if (c is DataGridView dgv)
                {
                    AppStyles.ApplyDataGridViewStyle(dgv);
                }
                // Panel
                else if (c is Panel panel)
                {
                    panel.BackColor = Color.White;
                    panel.BorderStyle = BorderStyle.FixedSingle;
                    // Padding НЕ трогаем
                }

                // GroupBox
                else if (c is GroupBox gb)
                {
                    AppStyles.ApplyGroupBoxStyle(gb);
                }

                // Рекурсия
                if (c.HasChildren)
                    ApplyStyles(c);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // BaseForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "BaseForm";
            this.Load += new System.EventHandler(this.BaseForm_Load_1);
            this.ResumeLayout(false);

        }

        private void BaseForm_Load_1(object sender, System.EventArgs e)
        {

        }
    }
}

