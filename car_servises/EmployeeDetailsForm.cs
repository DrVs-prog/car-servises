using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class EmployeeDetailsForm : Form
    {
        private int _employeeId;

        public EmployeeDetailsForm(int employeeId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            LoadEmployeeDetails();
            ApplyStyles();
        }

        private void LoadEmployeeDetails()
        {
            try
            {
                string query = @"
                    SELECT 
                        e.full_name AS 'Полное ФИО',
                        e.job_title AS 'Должность',
                        DATE_FORMAT(e.hire_date, '%d.%m.%Y') AS 'Дата найма',
                        r.role_name AS 'Роль',
                        e.login AS 'Логин',
                        (SELECT COUNT(*) FROM orders WHERE employee_id = e.employee_id) AS 'Всего заказов'
                    FROM employees e
                    LEFT JOIN roles r ON e.role_id = r.role_id
                    WHERE e.employee_id = @id";

                MySqlParameter[] parameters = {
                    new MySqlParameter("@id", _employeeId)
                };

                DataTable data = DatabaseHelper.ExecuteQuery(query, parameters);

                if (data.Rows.Count > 0)
                {
                    DataRow row = data.Rows[0];

                    lblFullName.Text = row["Полное ФИО"].ToString();
                    lblJobTitle.Text = row["Должность"].ToString();
                    lblHireDate.Text = row["Дата найма"].ToString();
                    lblRole.Text = row["Роль"].ToString();
                    lblLogin.Text = row["Логин"].ToString();
                    lblTotalOrders.Text = row["Всего заказов"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void ApplyStyles()
        {
            this.Text = "Информация о сотруднике";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = AppStyles.BackgroundColor;

            CreateControls();
        }

        private void CreateControls()
        {
            int yPos = 30;
            int labelWidth = 120;
            int valueWidth = 250;
            int xLabel = 30;
            int xValue = 160;

            // Заголовок
            Label lblTitle = new Label();
            lblTitle.Text = "ДЕТАЛЬНАЯ ИНФОРМАЦИЯ";
            lblTitle.Font = AppStyles.HeaderFont;
            lblTitle.ForeColor = AppStyles.PrimaryColor;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(120, 10);
            this.Controls.Add(lblTitle);
            yPos = 50;

            // Создаем строки с данными
            CreateDetailRow("ФИО:", ref lblFullName, xLabel, xValue, ref yPos, labelWidth, valueWidth);
            CreateDetailRow("Должность:", ref lblJobTitle, xLabel, xValue, ref yPos, labelWidth, valueWidth);
            CreateDetailRow("Дата найма:", ref lblHireDate, xLabel, xValue, ref yPos, labelWidth, valueWidth);
            CreateDetailRow("Роль:", ref lblRole, xLabel, xValue, ref yPos, labelWidth, valueWidth);
            CreateDetailRow("Логин:", ref lblLogin, xLabel, xValue, ref yPos, labelWidth, valueWidth);
            CreateDetailRow("Всего заказов:", ref lblTotalOrders, xLabel, xValue, ref yPos, labelWidth, valueWidth);

            yPos += 20;

            // Кнопка закрытия
            Button btnClose = new Button();
            btnClose.Text = "ЗАКРЫТЬ";
            btnClose.Location = new Point(150, yPos);
            btnClose.Size = new Size(120, 35);
            btnClose.Click += (s, e) => this.Close();
            AppStyles.ApplyButtonStyle(btnClose, true);
            this.Controls.Add(btnClose);
        }

        private void CreateDetailRow(string labelText, ref Label valueLabel, int xLabel, int xValue, ref int yPos, int labelWidth, int valueWidth)
        {
            Label label = new Label();
            label.Text = labelText;
            label.Font = AppStyles.NormalFont;
            label.ForeColor = AppStyles.DarkColor;
            label.Location = new Point(xLabel, yPos);
            label.Size = new Size(labelWidth, 25);
            label.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(label);

            valueLabel = new Label();
            valueLabel.Font = new Font(AppStyles.NormalFont, FontStyle.Bold);
            valueLabel.ForeColor = AppStyles.PrimaryColor;
            valueLabel.Location = new Point(xValue, yPos);
            valueLabel.Size = new Size(valueWidth, 25);
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(valueLabel);

            yPos += 30;
        }

        // Поля для значений
        private Label lblFullName;
        private Label lblJobTitle;
        private Label lblHireDate;
        private Label lblRole;
        private Label lblLogin;
        private Label lblTotalOrders;
    }
}