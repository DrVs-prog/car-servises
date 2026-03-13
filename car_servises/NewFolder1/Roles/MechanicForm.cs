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
    public partial class MechanicForm : BaseForm
    {
        private Label lblUserInfo;

        private void OpenForm(Form form)
        {
            form.Show();
            this.Hide();
        }

        public MechanicForm()
        {
            InitializeComponent();
            SetupUserInfo();
        }

        private void SetupUserInfo()
        {
            lblUserInfo = new Label();
            lblUserInfo.AutoSize = true;
            lblUserInfo.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular);
            lblUserInfo.ForeColor = Color.DarkOrange;
            lblUserInfo.Location = new Point(10, 10);
            lblUserInfo.Text = $"{CurrentUser.Role}: {CurrentUser.FullName}";

            this.Controls.Add(lblUserInfo);
            lblUserInfo.BringToFront();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Text = $"Панель механика - {CurrentUser.FullName}";

            if (lblUserInfo != null)
                lblUserInfo.Text = $"{CurrentUser.Role}: {CurrentUser.FullName}";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 loginForm = new Form1();
            loginForm.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Order orderForm = new Order();
            orderForm.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Parts partsForm = new Parts();
            partsForm.Show();
            this.Hide();
        }

        private void MechanicForm_Load(object sender, EventArgs e)
        {

        }
    }
}
