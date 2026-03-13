using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class Clients : BaseForm
    {

        public Clients(string userRole = "")
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients()
        {
            try
            {
                string query = @"
                    SELECT 
                        c.client_id AS 'ID',
                        c.full_name AS 'ФИО',
                        c.phone_number AS 'Телефон',
                        c.email AS 'Email',
                        c.address AS 'Адрес',
                        COUNT(car.car_id) AS 'Количество автомобилей',
                        GROUP_CONCAT(CONCAT(car.brand, ' ', car.model) SEPARATOR '; ') AS 'Автомобили'
                    FROM clients c
                    LEFT JOIN cars car ON c.client_id = car.client_id
                    GROUP BY c.client_id, c.full_name, c.phone_number, c.email, c.address
                    ORDER BY c.full_name";

                DataTable clients = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = clients;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e) // Добавление
        {
            using (AddEditClientForm form = new AddEditClientForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadClients();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) // Редактирование
        {
            if (dataGridView1.CurrentRow != null)
            {
                int clientId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string fullName = dataGridView1.CurrentRow.Cells["ФИО"].Value.ToString();
                string phone = dataGridView1.CurrentRow.Cells["Телефон"].Value?.ToString() ?? "";
                string email = dataGridView1.CurrentRow.Cells["Email"].Value?.ToString() ?? "";
                string address = dataGridView1.CurrentRow.Cells["Адрес"].Value?.ToString() ?? "";

                using (AddEditClientForm form = new AddEditClientForm(clientId, fullName, phone, email, address))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadClients();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для редактирования.");
            }
        }

        private void button4_Click(object sender, EventArgs e) // Удаление
        {
            if (dataGridView1.CurrentRow != null)
            {
                int clientId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string clientName = dataGridView1.CurrentRow.Cells["ФИО"].Value.ToString();
                int carCount = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Количество автомобилей"].Value);

                if (carCount > 0)
                {
                    MessageBox.Show($"Невозможно удалить клиента '{clientName}'. У клиента есть привязанные автомобили.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить клиента '{clientName}'?",
                    "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        string query = "DELETE FROM clients WHERE client_id = @id";
                        MySqlParameter[] parameters = {
                            new MySqlParameter("@id", clientId)
                        };

                        int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Клиент удален успешно!");
                            LoadClients();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления клиента: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для удаления.");
            }
        }

        private void button1_Click(object sender, EventArgs e) // Выход
        {
            this.Close();

            switch (CurrentUser.Role)
            {
                case "Менеджер":
                    ManagerForm managerForm = new ManagerForm();
                    managerForm.Show();
                    break;
                case "Администратор":
                    AdminForm adminForm = new AdminForm();
                    adminForm.Show();
                    break;
                default:
                    Form1 loginForm = new Form1();
                    loginForm.Show();
                    break;
            }
        }

        private void Clients_Load(object sender, EventArgs e)
        {

        }
    }
}
