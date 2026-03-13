using System.Drawing;
using System.Windows.Forms;
using System; // Добавьте эту строку

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
            FormClosing += BaseForm_FormClosing; // ДОБАВИТЬ
        }

        private void BaseForm_Load(object sender, System.EventArgs e)
        {
            ApplyStyles(this);

            // ДОБАВИТЬ: Не отслеживаем активность на форме авторизации
            if (!(this is Form1))
            {
                // Подписываемся на событие бездействия
                ActivityTracker.Instance.UserInactive += OnUserInactive;

                // Запускаем отслеживание для этой формы
                ActivityTracker.Instance.StartTracking(this);
            }
        }

        // ДОБАВИТЬ: Обработчик закрытия формы
        private void BaseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Отписываемся от события
            ActivityTracker.Instance.UserInactive -= OnUserInactive;
        }

        // ДОБАВИТЬ: Обработчик бездействия
        private void OnUserInactive(object sender, EventArgs e)
        {
            // Этот метод выполняется в потоке таймера, поэтому используем Invoke
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleInactivity()));
            }
            else
            {
                HandleInactivity();
            }
        }

        // ДОБАВИТЬ: Метод обработки бездействия
        private void HandleInactivity()
        {
            // Показываем сообщение (опционально)
            MessageBox.Show("Вы были заблокированы за бездействие.", "Блокировка",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Закрываем все формы, кроме Form1
            foreach (Form form in Application.OpenForms)
            {
                if (form is Form1)
                {
                    form.Show();
                }
                else
                {
                    form.Close();
                }
            }
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