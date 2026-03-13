using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class Roles : BaseForm
    {
        public Roles()
        {
            InitializeComponent();
            LoadRoles();
        }

        private void LoadRoles()
        {
            try
            {
                string query = @"
                    SELECT 
                        r.role_id AS 'ID',
                        r.role_name AS 'Название роли',
                        COUNT(e.employee_id) AS 'Количество сотрудников',
                        GROUP_CONCAT(e.full_name SEPARATOR '; ') AS 'Сотрудники'
                    FROM roles r
                    LEFT JOIN employees e ON r.role_id = e.role_id
                    GROUP BY r.role_id, r.role_name
                    ORDER BY r.role_name";

                DataTable roles = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = roles;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Автоподбор высоты строк для текстовых полей
                dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Добавление роли
            string roleName = Microsoft.VisualBasic.Interaction.InputBox("Введите название новой роли:", "Добавление роли");

            if (!string.IsNullOrEmpty(roleName))
            {
                try
                {
                    string query = "INSERT INTO roles (role_name) VALUES (@role_name)";
                    MySqlParameter[] parameters = {
                        new MySqlParameter("@role_name", roleName)
                    };

                    int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Роль добавлена успешно!");
                        LoadRoles();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления роли: {ex.Message}");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Удаление роли
            if (dataGridView1.CurrentRow != null)
            {
                int roleId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string roleName = dataGridView1.CurrentRow.Cells["Название роли"].Value.ToString();
                int employeeCount = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Количество сотрудников"].Value);

                if (employeeCount > 0)
                {
                    MessageBox.Show($"Невозможно удалить роль '{roleName}'. К этой роли привязаны сотрудники.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить роль '{roleName}'?",
                    "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string query = "DELETE FROM roles WHERE role_id = @id";
                    MySqlParameter[] parameters = {
                        new MySqlParameter("@id", roleId)
                    };

                    int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Роль удалена успешно!");
                        LoadRoles();
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Редактирование роли
            if (dataGridView1.CurrentRow != null)
            {
                int roleId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string currentRoleName = dataGridView1.CurrentRow.Cells["Название роли"].Value.ToString();

                string newRoleName = Microsoft.VisualBasic.Interaction.InputBox("Введите новое название роли:", "Редактирование роли", currentRoleName);

                if (!string.IsNullOrEmpty(newRoleName) && newRoleName != currentRoleName)
                {
                    try
                    {
                        string query = "UPDATE roles SET role_name = @role_name WHERE role_id = @id";
                        MySqlParameter[] parameters = {
                            new MySqlParameter("@role_name", newRoleName),
                            new MySqlParameter("@id", roleId)
                        };

                        int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Роль обновлена успешно!");
                            LoadRoles();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка обновления роли: {ex.Message}");
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
            AdminForm adminForm = new AdminForm();
            adminForm.Show();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Просмотр детальной информации при двойном клике
            if (e.RowIndex >= 0)
            {
                string roleName = dataGridView1.Rows[e.RowIndex].Cells["Название роли"].Value.ToString();
                string employeeCount = dataGridView1.Rows[e.RowIndex].Cells["Количество сотрудников"].Value.ToString();
                string employees = dataGridView1.Rows[e.RowIndex].Cells["Сотрудники"].Value.ToString();

                string message = $"Роль: {roleName}\n\n" +
                               $"Количество сотрудников: {employeeCount}\n\n" +
                               $"Сотрудники:\n{employees}";

                MessageBox.Show(message, "Детальная информация о роли", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Roles_Load(object sender, EventArgs e)
        {

        }
    }
}
