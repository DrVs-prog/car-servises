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
    public partial class ManagerForm : BaseForm
    {

        private Label lblUserInfo;
        private void OpenForm(Form form)
        {
            form.Show();
            this.Hide();
        }

        public ManagerForm()
        {
            InitializeComponent();
            SetupUserInfo();
        }

        private void SetupUserInfo()
        {
            lblUserInfo = new Label();
            lblUserInfo.AutoSize = true;
            lblUserInfo.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular);
            lblUserInfo.ForeColor = Color.DarkGreen;
            lblUserInfo.Location = new Point(10, 10);
            lblUserInfo.Text = $"{CurrentUser.Role}: {CurrentUser.FullName}";

            this.Controls.Add(lblUserInfo);
            lblUserInfo.BringToFront();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Text = $"Панель менеджера - {CurrentUser.FullName}";

            if (lblUserInfo != null)
                lblUserInfo.Text = $"{CurrentUser.Role}: {CurrentUser.FullName}";
        }
        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 form1 = new Form1();
            form1.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clients clientsForm = new Clients();
            clientsForm.Show();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Order orderForm = new Order();
            orderForm.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Parts parts = new Parts();
            parts.Show();
            this.Hide();
        }

        private void ManagerForm_Load(object sender, EventArgs e)
        {

        }
    }
}
