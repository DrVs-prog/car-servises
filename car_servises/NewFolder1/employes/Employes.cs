using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class Employes : BaseForm
    {
        private string _userRole;
        private DataTable originalEmployeesData;
        private System.Windows.Forms.Button btnDetails;
        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ComboBox cmbSearchColumn;
        private System.Windows.Forms.ComboBox cmbSortBy;
        private System.Windows.Forms.RadioButton rbAsc;
        private System.Windows.Forms.RadioButton rbDesc;
        private System.Windows.Forms.Button btnResetFilters;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Label lblSort;
        private System.Windows.Forms.Label lblInColumn;

        public Employes()
        {
            InitializeComponent();
            ConnectSearchEvents();
            LoadEmployees();
            InitializeDetailsButton();
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        public Employes(string userRole = "")
        {
            InitializeComponent();
            _userRole = userRole;
            ConnectSearchEvents();
            LoadEmployees();
        }

        private void ConnectSearchEvents()
        {
            txtSearch.TextChanged += txtSearch_TextChanged;
            cmbSearchColumn.SelectedIndexChanged += cmbSearchColumn_SelectedIndexChanged;
            cmbSortBy.SelectedIndexChanged += cmbSortBy_SelectedIndexChanged;
            rbAsc.CheckedChanged += rbSortDirection_CheckedChanged;
            rbDesc.CheckedChanged += rbSortDirection_CheckedChanged;
            btnResetFilters.Click += btnResetFilters_Click;
        }

        private void InitializeDetailsButton()
        {
            // Создаем кнопку "ПРОСМОТР"
            btnDetails = new Button();
            btnDetails.Text = "ПРОСМОТР";

            // РАСПОЛОЖЕНИЕ - подберите координаты под вашу форму!
            // Обычно кнопки находятся внизу формы, например:
            btnDetails.Location = new Point(538, 394); // X, Y

            btnDetails.Size = new Size(133, 46);
            btnDetails.Click += btnDetails_Click;
            AppStyles.ApplyButtonStyle(btnDetails);
            this.Controls.Add(btnDetails);
        }

        private void OpenDetailsForm()
        {
            if (dataGridView1.CurrentRow != null)
            {
                int employeeId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                EmployeeDetailsForm detailsForm = new EmployeeDetailsForm(employeeId);
                detailsForm.ShowDialog(); // Открываем как диалог
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для просмотра.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            OpenDetailsForm();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Не открываем при двойном клике на заголовке
            if (e.RowIndex >= 0)
            {
                OpenDetailsForm();
            }
        }

        private void LoadEmployees()
        {
            try
            {
                string query = @"
            SELECT 
                e.employee_id AS 'ID',
                
                -- Маскируем ФИО: фамилия + инициалы
                CONCAT(
                    SUBSTRING_INDEX(e.full_name, ' ', 1),
                    ' ',
                    LEFT(SUBSTRING_INDEX(SUBSTRING_INDEX(e.full_name, ' ', 2), ' ', -1), 1),
                    '.',
                    CASE 
                        WHEN LENGTH(e.full_name) - LENGTH(REPLACE(e.full_name, ' ', '')) >= 2 
                        THEN CONCAT(' ', LEFT(SUBSTRING_INDEX(SUBSTRING_INDEX(e.full_name, ' ', 3), ' ', -1), 1), '.')
                        ELSE ''
                    END
                ) AS 'ФИО',
                
                e.job_title AS 'Должность',
                DATE_FORMAT(e.hire_date, '%d.%m.%Y') AS 'Дата найма',
                r.role_name AS 'Роль',
                e.login AS 'Логин',
                
                (SELECT COUNT(*) FROM orders WHERE employee_id = e.employee_id) AS 'Количество заказов'
            FROM employees e
            LEFT JOIN roles r ON e.role_id = r.role_id
            ORDER BY e.full_name";

                DataTable employees = DatabaseHelper.ExecuteQuery(query);

                originalEmployeesData = employees;
                dataGridView1.DataSource = employees;

                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                FillSearchColumns();
                ApplySearchStyles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }



        private void FillSearchColumns()
        {
            cmbSearchColumn.Items.Clear();
            cmbSortBy.Items.Clear();

            if (originalEmployeesData != null)
            {
                foreach (DataColumn column in originalEmployeesData.Columns)
                {
                    cmbSearchColumn.Items.Add(column.ColumnName);
                    cmbSortBy.Items.Add(column.ColumnName);
                }

                if (cmbSearchColumn.Items.Count > 0)
                {
                    cmbSearchColumn.SelectedIndex = 0;
                    cmbSortBy.SelectedIndex = 0;
                }
            }
        }

        private void ApplySearch()
        {
            if (originalEmployeesData == null)
            {
                if (dataGridView1.DataSource is DataTable dataTable)
                {
                    originalEmployeesData = dataTable;
                }
                else if (dataGridView1.DataSource is DataView dataView)
                {
                    originalEmployeesData = dataView.Table;
                }
                else
                {
                    return;
                }
            }

            string searchText = txtSearch.Text.Trim();
            string searchColumn = cmbSearchColumn.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(searchColumn)) return;

            DataTable filteredData;

            if (string.IsNullOrEmpty(searchText))
            {
                filteredData = originalEmployeesData.Copy();
            }
            else
            {
                try
                {
                    filteredData = originalEmployeesData.Clone();

                    foreach (DataRow row in originalEmployeesData.Rows)
                    {
                        string cellValue = row[searchColumn].ToString();
                        if (cellValue.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            filteredData.ImportRow(row);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка поиска: {ex.Message}");
                    filteredData = originalEmployeesData.Copy();
                }
            }

            ApplySorting(filteredData);
        }

        private void ApplySorting(DataTable data)
        {
            if (data == null || data.Rows.Count == 0)
            {
                dataGridView1.DataSource = data;
                return;
            }

            string sortColumn = cmbSortBy.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(sortColumn)) return;

            string sortDirection = rbAsc.Checked ? "ASC" : "DESC";

            try
            {
                DataView dataView = new DataView(data);
                dataView.Sort = $"{sortColumn} {sortDirection}";
                dataGridView1.DataSource = dataView;
            }
            catch (Exception)
            {
                dataGridView1.DataSource = data;
            }
        }

        private void ResetSearchFilters()
        {
            txtSearch.Text = "";
            if (cmbSearchColumn.Items.Count > 0) cmbSearchColumn.SelectedIndex = 0;
            if (cmbSortBy.Items.Count > 0) cmbSortBy.SelectedIndex = 0;
            rbAsc.Checked = true;

            if (originalEmployeesData != null)
            {
                dataGridView1.DataSource = originalEmployeesData;
            }
        }

        private void ApplySearchStyles()
        {
            if (txtSearch != null) AppStyles.ApplyTextBoxStyle(txtSearch);
            if (btnResetFilters != null) AppStyles.ApplyButtonStyle(btnResetFilters);

            if (cmbSearchColumn != null) cmbSearchColumn.Font = AppStyles.NormalFont;
            if (cmbSortBy != null) cmbSortBy.Font = AppStyles.NormalFont;

            if (lblSearch != null) AppStyles.ApplyLabelStyle(lblSearch);
            if (lblInColumn != null) AppStyles.ApplyLabelStyle(lblInColumn);
            if (lblSort != null) AppStyles.ApplyLabelStyle(lblSort);
        }

        // Обработчики событий для поиска
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplySearch();
        }

        private void cmbSearchColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySearch();
        }

        private void cmbSortBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySearch();
        }

        private void rbSortDirection_CheckedChanged(object sender, EventArgs e)
        {
            ApplySearch();
        }

        private void btnResetFilters_Click(object sender, EventArgs e)
        {
            ResetSearchFilters();
        }


        private void button3_Click(object sender, EventArgs e) // Добавление
        {
            using (AddEditEmployeeForm form = new AddEditEmployeeForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadEmployees();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) // Редактирование
        {
            if (dataGridView1.CurrentRow != null)
            {
                int employeeId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string fullName = dataGridView1.CurrentRow.Cells["ФИО"].Value.ToString();
                string jobTitle = dataGridView1.CurrentRow.Cells["Должность"].Value?.ToString() ?? "";
                DateTime hireDate = Convert.ToDateTime(dataGridView1.CurrentRow.Cells["Дата найма"].Value);
                string roleName = dataGridView1.CurrentRow.Cells["Роль"].Value?.ToString() ?? "";
                string login = dataGridView1.CurrentRow.Cells["Логин"].Value?.ToString() ?? "";

                using (AddEditEmployeeForm form = new AddEditEmployeeForm(employeeId, fullName, jobTitle, hireDate, roleName, login))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadEmployees();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для редактирования.");
            }
        }

        private void button1_Click(object sender, EventArgs e) // Удаление
        {
            if (dataGridView1.CurrentRow != null)
            {
                int employeeId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string employeeName = dataGridView1.CurrentRow.Cells["ФИО"].Value.ToString();
                int orderCount = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Количество заказов"].Value);

                if (orderCount > 0)
                {
                    MessageBox.Show($"Невозможно удалить сотрудника '{employeeName}'. Он участвует в {orderCount} заказах.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить сотрудника {employeeName}?",
                    "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        string query = "DELETE FROM employees WHERE employee_id = @id";
                        MySqlParameter[] parameters = {
                            new MySqlParameter("@id", employeeId)
                        };

                        int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Сотрудник удален успешно!");
                            LoadEmployees();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления сотрудника: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для удаления.");
            }
        }

        private void button4_Click(object sender, EventArgs e) // Выход
        {
            this.Close();
            AdminForm adminForm = new AdminForm();
            adminForm.Show();
        }

        public void SetUserRole(string userRole)
        {
            _userRole = userRole;
        }
        private void Employes_Load(object sender, EventArgs e)
        {

        }
    }
}
