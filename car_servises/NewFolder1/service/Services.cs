using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class Services : BaseForm
    {
        private string _userRole;

        public Services(string userRole = "")
        {
            InitializeComponent();
            _userRole = userRole;
            LoadServices();
        }

        private void LoadServices()
        {
            try
            {
                string query = @"
                    SELECT 
                        service_id AS 'ID',
                        service_name AS 'Название услуги',
                        description AS 'Описание',
                        price AS 'Цена',
                        (SELECT COUNT(*) FROM orders WHERE service_id = services.service_id) AS 'Количество заказов'
                    FROM services 
                    ORDER BY service_name";

                DataTable services = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = services;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.Columns["Цена"].DefaultCellStyle.Format = "C2";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e) // Добавление
        {
            using (AddEditServiceForm form = new AddEditServiceForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadServices();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) // Удаление
        {
            if (dataGridView1.CurrentRow != null)
            {
                int serviceId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string serviceName = dataGridView1.CurrentRow.Cells["Название услуги"].Value.ToString();
                int orderCount = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Количество заказов"].Value);

                if (orderCount > 0)
                {
                    MessageBox.Show($"Невозможно удалить услугу '{serviceName}'. Она используется в {orderCount} заказах.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult result = MessageBox.Show($"Удалить услугу '{serviceName}'?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string query = "DELETE FROM services WHERE service_id = @id";
                    MySqlParameter[] parameters = { new MySqlParameter("@id", serviceId) };

                    int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Услуга удалена!");
                        LoadServices();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу для удаления.");
            }
        }

        private void button2_Click(object sender, EventArgs e) // Редактирование
        {
            if (dataGridView1.CurrentRow != null)
            {
                int serviceId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string serviceName = dataGridView1.CurrentRow.Cells["Название услуги"].Value.ToString();
                string description = dataGridView1.CurrentRow.Cells["Описание"].Value?.ToString() ?? "";
                decimal price = Convert.ToDecimal(dataGridView1.CurrentRow.Cells["Цена"].Value);

                using (AddEditServiceForm form = new AddEditServiceForm(serviceId, serviceName, description, price))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadServices();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу для редактирования.");
            }
        }

        private void button4_Click(object sender, EventArgs e) // Выход
        {
            this.Close();

            switch (_userRole)
            {
                case "Менеджер":
                    ManagerForm managerForm = new ManagerForm();
                    managerForm.Show();
                    break;
                case "Администратор":
                default:
                    AdminForm adminForm = new AdminForm();
                    adminForm.Show();
                    break;
            }
        }

        public void SetUserRole(string userRole)
        {
            _userRole = userRole;
        }
        private void Services_Load(object sender, EventArgs e)
        {

        }
    }
}
