using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace car_servises
{
    public partial class AdminForm : BaseForm
    {
        private Label lblUserInfo; // Добавляем Label для отображения
        private Panel headerPanel;

        public AdminForm()
        {
            InitializeComponent();
            SetupUserInfo();

        }

        private void SetupUserInfo()
        {
            // Создаем или находим Label для отображения информации о пользователе
            lblUserInfo = new Label();
            lblUserInfo.AutoSize = true;
            lblUserInfo.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular);
            lblUserInfo.ForeColor = Color.DarkBlue;
            lblUserInfo.Location = new Point(10, 10);
            lblUserInfo.Text = $"{CurrentUser.Role}: {CurrentUser.FullName}";


            // Добавляем на форму
            this.Controls.Add(lblUserInfo);

            // Поднимаем на передний план
            lblUserInfo.BringToFront();
        }

        // Обновляем заголовок формы
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Text = $"Панель администратора - {CurrentUser.FullName}";

            // Обновляем текст Label
            if (lblUserInfo != null)
                lblUserInfo.Text = $"{CurrentUser.Role}: {CurrentUser.FullName}";
        }

        private void OpenForm(Form form)
        {
            form.Show();
            this.Hide();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Services servicesForm = new Services();
            servicesForm.Show();
            this.Hide();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Roles rolesForm = new Roles();
            rolesForm.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Employes employesForm = new Employes();
            employesForm.Show();
            this.Hide();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 loginForm = new Form1();
            loginForm.Show();
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {

        }
    }
}
