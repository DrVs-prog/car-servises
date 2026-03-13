using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class ClientCarsForm : Form
    {
        private int _clientId;
        private string _clientName;

        public ClientCarsForm(int clientId, string clientName)
        {
            InitializeComponent();
            _clientId = clientId;
            _clientName = clientName;
            this.Text = $"Автомобили клиента: {clientName}";
            LoadCars();
        }

        private void LoadCars()
        {
            try
            {
                string query = @"
                    SELECT car_id, brand, model, year, 
                           COALESCE(vin, 'Не указан') as vin, 
                           registration_number 
                    FROM cars 
                    WHERE client_id = @client_id 
                    ORDER BY brand, model";

                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@client_id", _clientId)
                };

                DataTable cars = DatabaseHelper.ExecuteQuery(query, parameters);
                dataGridViewCars.DataSource = cars;

                // Настраиваем DataGridView
                ConfigureDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки автомобилей: {ex.Message}");
            }
        }

        private void ConfigureDataGridView()
        {
            if (dataGridViewCars.Columns.Count > 0)
            {
                dataGridViewCars.Columns["car_id"].Visible = false;
                dataGridViewCars.Columns["brand"].HeaderText = "Марка";
                dataGridViewCars.Columns["model"].HeaderText = "Модель";
                dataGridViewCars.Columns["year"].HeaderText = "Год";
                dataGridViewCars.Columns["vin"].HeaderText = "VIN";
                dataGridViewCars.Columns["registration_number"].HeaderText = "Госномер";

                // Автоподбор ширины столбцов
                dataGridViewCars.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddEditCarForm addCarForm = new AddEditCarForm(_clientId);
            if (addCarForm.ShowDialog() == DialogResult.OK)
            {
                LoadCars(); // Обновляем список
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewCars.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridViewCars.SelectedRows[0];
                int carId = Convert.ToInt32(row.Cells["car_id"].Value);
                string brand = row.Cells["brand"].Value.ToString();
                string model = row.Cells["model"].Value.ToString();
                int year = Convert.ToInt32(row.Cells["year"].Value);
                string vin = row.Cells["vin"].Value.ToString();
                string licensePlate = row.Cells["registration_number"].Value.ToString();

                // Если VIN был "Не указан", передаем пустую строку
                if (vin == "Не указан") vin = "";

                AddEditCarForm editCarForm = new AddEditCarForm(carId, _clientId, brand, model, year, vin, licensePlate);
                if (editCarForm.ShowDialog() == DialogResult.OK)
                {
                    LoadCars(); // Обновляем список
                }
            }
            else
            {
                MessageBox.Show("Выберите автомобиль для редактирования.");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewCars.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridViewCars.SelectedRows[0];
                int carId = Convert.ToInt32(row.Cells["car_id"].Value);
                string licensePlate = row.Cells["registration_number"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Удалить автомобиль {licensePlate}?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Проверяем, есть ли заказы на этот автомобиль
                        string checkQuery = "SELECT COUNT(*) FROM orders WHERE car_id = @car_id";
                        MySqlParameter[] checkParams = new MySqlParameter[]
                        {
                            new MySqlParameter("@car_id", carId)
                        };

                        var orderCount = DatabaseHelper.ExecuteScalar(checkQuery, checkParams);
                        if (Convert.ToInt32(orderCount) > 0)
                        {
                            MessageBox.Show("Невозможно удалить автомобиль, так как на него есть заказы.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        string deleteQuery = "DELETE FROM cars WHERE car_id = @car_id";
                        MySqlParameter[] deleteParams = new MySqlParameter[]
                        {
                            new MySqlParameter("@car_id", carId)
                        };

                        int rowsAffected = DatabaseHelper.ExecuteNonQuery(deleteQuery, deleteParams);
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Автомобиль удален.");
                            LoadCars(); // Обновляем список
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите автомобиль для удаления.");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridViewCars_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnEdit_Click(sender, e);
            }
        }

        private void ClientCarsForm_Load(object sender, EventArgs e)
        {

        }
    }
}