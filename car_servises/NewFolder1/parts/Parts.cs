using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class Parts : BaseForm
    {
        private string _userRole;

        public Parts(string userRole = "")
        {
            InitializeComponent();
            _userRole = userRole;
            ConfigureFormForRole();
            LoadParts();
            SetupDataGridView();
        }

        public void SetUserRole(string userRole)
        {
            _userRole = userRole;
            ConfigureFormForRole();
        }

        private void ConfigureFormForRole()
        {
            if (CurrentUser.Role == "Механик")
            {
                button2.Visible = false;
                button3.Visible = false;
                button4.Visible = false;
                this.Text = "Просмотр запчастей (Режим просмотра)";
                dataGridView1.ReadOnly = true;
            }
            else
            {
                button2.Visible = true;
                button3.Visible = true;
                button4.Visible = true;
                this.Text = "Управление запчастями";
                dataGridView1.ReadOnly = true;
            }
        }

        private void SetupDataGridView()
        {
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            dataGridView1.MouseClick += DataGridView1_MouseClick;
        }

        private void LoadParts()
        {
            try
            {
                string query = @"
                    SELECT 
                        p.part_id AS 'ID',
                        p.part_name AS 'Название запчасти',
                        p.manufacturer AS 'Производитель',
                        p.cost AS 'Цена',
                        p.stock_qty AS 'Количество на складе',
                        CASE 
                            WHEN p.stock_qty = 0 THEN 'Нет в наличии'
                            WHEN p.stock_qty < 5 THEN 'Мало'
                            ELSE 'В наличии'
                        END AS 'Статус',
                        CASE 
                            WHEN p.has_image = 1 OR EXISTS(SELECT 1 FROM part_images pi WHERE pi.part_id = p.part_id) 
                            THEN '✓' 
                            ELSE '' 
                        END AS 'Фото'
                    FROM parts p
                    ORDER BY p.part_name";

                DataTable parts = DatabaseHelper.ExecuteQuery(query);
                dataGridView1.DataSource = parts;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                if (dataGridView1.Columns["Цена"] != null)
                    dataGridView1.Columns["Цена"].DefaultCellStyle.Format = "C2";

                if (dataGridView1.Columns["Фото"] != null)
                {
                    //dataGridView1.Columns["Фото"].Width = 50;
                    dataGridView1.Columns["Фото"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                if (dataGridView1.Columns["ID"] != null)
                    dataGridView1.Columns["ID"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки запчастей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && dataGridView1.CurrentRow != null)
            {
                if (_userRole != "Механик")
                {
                    ContextMenuStrip cms = new ContextMenuStrip();

                    ToolStripMenuItem viewPhotoItem = new ToolStripMenuItem("Просмотреть фото");
                    viewPhotoItem.Click += ViewPhotoMenuItem_Click;
                    cms.Items.Add(viewPhotoItem);

                    ToolStripMenuItem addPhotoItem = new ToolStripMenuItem("Добавить/Изменить фото");
                    addPhotoItem.Click += AddPhotoMenuItem_Click;
                    cms.Items.Add(addPhotoItem);

                    ToolStripMenuItem deletePhotoItem = new ToolStripMenuItem("Удалить фото");
                    deletePhotoItem.Click += DeletePhotoMenuItem_Click;
                    cms.Items.Add(deletePhotoItem);

                    cms.Items.Add(new ToolStripSeparator());

                    ToolStripMenuItem editItem = new ToolStripMenuItem("Редактировать");
                    editItem.Click += button3_Click;
                    cms.Items.Add(editItem);

                    ToolStripMenuItem deleteItem = new ToolStripMenuItem("Удалить");
                    deleteItem.Click += button4_Click;
                    cms.Items.Add(deleteItem);

                    cms.Show(dataGridView1, e.Location);
                }
            }
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (dataGridView1.Columns[e.ColumnIndex].Name == "Фото")
                {
                    int partId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value);
                    string partName = dataGridView1.Rows[e.RowIndex].Cells["Название запчасти"].Value.ToString();

                    using (PartImageViewer viewer = new PartImageViewer(partId, partName))
                    {
                        viewer.ShowDialog();
                    }
                }
            }
        }

        private void ViewPhotoMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int partId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string partName = dataGridView1.CurrentRow.Cells["Название запчасти"].Value.ToString();

                using (PartImageViewer viewer = new PartImageViewer(partId, partName))
                {
                    viewer.ShowDialog();
                }
            }
        }

        private void AddPhotoMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int partId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string partName = dataGridView1.CurrentRow.Cells["Название запчасти"].Value.ToString();

                using (PartImageViewer viewer = new PartImageViewer(partId, partName))
                {
                    if (viewer.ShowDialog() == DialogResult.OK)
                    {
                        LoadParts();
                    }
                }
            }
        }

        private void DeletePhotoMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int partId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string partName = dataGridView1.CurrentRow.Cells["Название запчасти"].Value.ToString();

                DialogResult result = MessageBox.Show($"Удалить фото запчасти '{partName}'?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        string query = "DELETE FROM part_images WHERE part_id = @part_id";
                        MySqlParameter[] parameters = new MySqlParameter[]
                        {
                            new MySqlParameter("@part_id", partId)
                        };

                        int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);

                        string updateFlagQuery = "UPDATE parts SET has_image = 0 WHERE part_id = @part_id";
                        MySqlParameter[] flagParams = new MySqlParameter[]
                        {
                            new MySqlParameter("@part_id", partId)
                        };
                        DatabaseHelper.ExecuteNonQuery(updateFlagQuery, flagParams);

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Фото удалено!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadParts();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления фото: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) // Добавление
        {
            using (AddEditPartForm form = new AddEditPartForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadParts();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) // Редактирование
        {
            if (dataGridView1.CurrentRow != null)
            {
                int partId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string partName = dataGridView1.CurrentRow.Cells["Название запчасти"].Value.ToString();
                string manufacturer = dataGridView1.CurrentRow.Cells["Производитель"].Value?.ToString() ?? "";
                decimal cost = Convert.ToDecimal(dataGridView1.CurrentRow.Cells["Цена"].Value);
                int stockQty = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Количество на складе"].Value);

                using (AddEditPartForm form = new AddEditPartForm(partId, partName, manufacturer, cost, stockQty))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadParts();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите запчасть для редактирования.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e) // Удаление
        {
            if (dataGridView1.CurrentRow != null)
            {
                int partId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ID"].Value);
                string partName = dataGridView1.CurrentRow.Cells["Название запчасти"].Value.ToString();

                string checkQuery = "SELECT COUNT(*) FROM order_details WHERE part_id = @id";
                MySqlParameter[] checkParams = { new MySqlParameter("@id", partId) };
                object result = DatabaseHelper.ExecuteScalar(checkQuery, checkParams);
                int usageCount = Convert.ToInt32(result);

                if (usageCount > 0)
                {
                    MessageBox.Show($"Невозможно удалить запчасть '{partName}'. Она используется в {usageCount} заказах.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult resultDialog = MessageBox.Show($"Удалить запчасть '{partName}'?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resultDialog == DialogResult.Yes)
                {
                    try
                    {
                        string query = "DELETE FROM parts WHERE part_id = @id";
                        MySqlParameter[] parameters = { new MySqlParameter("@id", partId) };

                        int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Запчасть удалена!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadParts();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите запчасть для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e) // Назад
        {
            this.Close();

            if (CurrentUser.Role == "Менеджер")
            {
                new ManagerForm().Show();
            }
            else if (CurrentUser.Role == "Механик")
            {
                new MechanicForm().Show();
            }
            else
            {
                new AdminForm().Show();
            }
        }

        private void Parts_Load(object sender, EventArgs e)
        {
            this.CenterToScreen();
        }
    }
}