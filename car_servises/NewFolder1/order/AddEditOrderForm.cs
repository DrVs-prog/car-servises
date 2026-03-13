using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class AddEditOrderForm : Form
    {
        private int _orderId;
        private bool _isEditMode;
        private int? _selectedPartId = null; // Для хранения выбранной запчасти

        public AddEditOrderForm()
        {
            InitializeComponent();
            _isEditMode = false;
            labelHeader.Text = "Оформление заказа";

            SetDateConstraints();
            LoadComboBoxData();
            LoadPartsComboBox(); // Загружаем запчасти в ComboBox

            cmbStatus.SelectedIndexChanged += cmbStatus_SelectedIndexChanged;
            dtpCompletionDate.Validating += dtpCompletionDate_Validating;
        }

        public AddEditOrderForm(int orderId, int clientId, int carId, int serviceId, int employeeId,
                              string problemDescription, string recommendations, string status,
                              DateTime? completionDate = null, int? partId = null)
        {
            InitializeComponent();
            _orderId = orderId;
            _isEditMode = true;
            labelHeader.Text = "Редактирование заказа";

            SetDateConstraints();
            LoadComboBoxData();
            LoadPartsComboBox(); // Загружаем запчасти в ComboBox

            cmbStatus.SelectedIndexChanged += cmbStatus_SelectedIndexChanged;
            dtpCompletionDate.Validating += dtpCompletionDate_Validating;

            // Заполняем данные
            if (clientId > 0) cmbClient.SelectedValue = clientId;
            if (carId > 0) cmbCar.SelectedValue = carId;
            if (serviceId > 0) cmbService.SelectedValue = serviceId;
            if (employeeId > 0) cmbEmployee.SelectedValue = employeeId;
            txtProblemDescription.Text = problemDescription;
            txtRecommendations.Text = recommendations;

            // Заполняем дату завершения
            if (completionDate.HasValue)
            {
                DateTime minDate = DateTime.Today;
                if (completionDate.Value.Date < minDate)
                {
                    dtpCompletionDate.Value = DateTime.Now;
                }
                else
                {
                    dtpCompletionDate.Value = completionDate.Value;
                }
            }
            else
            {
                dtpCompletionDate.Value = DateTime.Now;
            }

            // Заполняем запчасть если есть
            if (partId.HasValue && partId.Value > 0)
            {
                _selectedPartId = partId.Value;
                cmbParts.SelectedValue = partId.Value;
            }

            // Статус
            if (!string.IsNullOrEmpty(status))
            {
                foreach (var item in cmbStatus.Items)
                {
                    if (item.ToString() == status)
                    {
                        cmbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        // ЗАГРУЗКА ЗАПЧАСТЕЙ В КОМБОБОКС
        private void LoadPartsComboBox()
        {
            try
            {
                string query = @"SELECT part_id, 
                               CONCAT(part_name, ' (', manufacturer, ') - ', cost, ' руб.') as display_name
                               FROM parts 
                               WHERE stock_qty > 0 
                               ORDER BY part_name";

                DataTable parts = DatabaseHelper.ExecuteQuery(query);

                cmbParts.DataSource = parts;
                cmbParts.DisplayMember = "display_name";
                cmbParts.ValueMember = "part_id";

                // Добавляем пустой элемент
                DataRow emptyRow = parts.NewRow();
                emptyRow["part_id"] = 0;
                emptyRow["display_name"] = "(не выбрано)";
                parts.Rows.InsertAt(emptyRow, 0);

                cmbParts.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки запчастей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ОБНОВЛЕННЫЙ МЕТОД СОХРАНЕНИЯ
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                Cursor = Cursors.WaitCursor;

                string query;
                MySqlParameter[] parameters;

                string selectedStatus = cmbStatus.SelectedItem?.ToString() ?? "";

                // Определяем, нужно ли сохранять дату завершения
                object completionDateValue;
                if (selectedStatus.ToLower() == "завершен")
                {
                    // Для завершенных заказов сохраняем дату завершения
                    completionDateValue = dtpCompletionDate.Value;
                }
                else
                {
                    // Для незавершенных заказов сохраняем NULL
                    completionDateValue = dtpCompletionDate.Value;
                }
                // Получаем ID выбранной запчасти
                int? partId = null;
                if (cmbParts.SelectedValue != null && cmbParts.SelectedValue is int selectedId && selectedId > 0)
                {
                    partId = selectedId;
                }

                if (_isEditMode)
                {
                    query = @"UPDATE orders SET 
                             client_id = @client_id, 
                             car_id = @car_id, 
                             service_id = @service_id,
                             employee_id = @employee_id, 
                             problem_description = @problem_description,
                             recommendations = @recommendations, 
                             status = @status,
                             completion_date = @completion_date,
                             part_id = @part_id
                             WHERE order_id = @id";

                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@client_id", cmbClient.SelectedValue),
                        new MySqlParameter("@car_id", cmbCar.SelectedValue),
                        new MySqlParameter("@service_id", cmbService.SelectedValue),
                        new MySqlParameter("@employee_id", cmbEmployee.SelectedValue),
                        new MySqlParameter("@problem_description", txtProblemDescription.Text.Trim()),
                        new MySqlParameter("@recommendations", txtRecommendations.Text.Trim()),
                        new MySqlParameter("@status", selectedStatus),
                        new MySqlParameter("@completion_date", completionDateValue),
                        new MySqlParameter("@part_id", partId.HasValue ? (object)partId.Value : DBNull.Value),
                        new MySqlParameter("@id", _orderId)
                    };
                }
                else
                {
                    query = @"INSERT INTO orders 
                             (client_id, car_id, service_id, employee_id, 
                              problem_description, recommendations, status,
                              completion_date, order_date, part_id) 
                             VALUES 
                             (@client_id, @car_id, @service_id, @employee_id, 
                              @problem_description, @recommendations, @status,
                              @completion_date, NOW(), @part_id);
                             SELECT LAST_INSERT_ID();";

                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@client_id", cmbClient.SelectedValue),
                        new MySqlParameter("@car_id", cmbCar.SelectedValue),
                        new MySqlParameter("@service_id", cmbService.SelectedValue),
                        new MySqlParameter("@employee_id", cmbEmployee.SelectedValue),
                        new MySqlParameter("@problem_description", txtProblemDescription.Text.Trim()),
                        new MySqlParameter("@recommendations", txtRecommendations.Text.Trim()),
                        new MySqlParameter("@status", selectedStatus),
                        new MySqlParameter("@completion_date", completionDateValue),
                        new MySqlParameter("@part_id", partId.HasValue ? (object)partId.Value : DBNull.Value)
                    };
                }

                int rowsAffected;
                if (_isEditMode)
                {
                    rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                }
                else
                {
                    object result = DatabaseHelper.ExecuteScalar(query, parameters);
                    _orderId = Convert.ToInt32(result);
                    rowsAffected = _orderId > 0 ? 1 : 0;
                }

                if (rowsAffected > 0)
                {
                    // Рассчитываем и обновляем стоимость заказа
                    CalculateOrderCost(_orderId);

                    MessageBox.Show(_isEditMode ? "Заказ обновлен успешно!" : "Заказ оформлен успешно!",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // РАСЧЕТ СТОИМОСТИ ЗАКАЗА
        private void CalculateOrderCost(int orderId)
        {
            try
            {
                // Получаем стоимость услуги
                decimal serviceCost = 0;
                if (cmbService.SelectedValue != null)
                {
                    string serviceQuery = "SELECT price FROM services WHERE service_id = @service_id";
                    object result = DatabaseHelper.ExecuteScalar(serviceQuery,
                        new MySqlParameter[] { new MySqlParameter("@service_id", cmbService.SelectedValue) });

                    if (result != null)
                        serviceCost = Convert.ToDecimal(result);
                }

                // Получаем стоимость запчасти
                decimal partCost = 0;
                if (cmbParts.SelectedValue != null && cmbParts.SelectedValue is int partId && partId > 0)
                {
                    string partQuery = "SELECT cost FROM parts WHERE part_id = @part_id";
                    object result = DatabaseHelper.ExecuteScalar(partQuery,
                        new MySqlParameter[] { new MySqlParameter("@part_id", partId) });

                    if (result != null)
                        partCost = Convert.ToDecimal(result);
                }

                // Общая стоимость
                decimal totalCost = serviceCost + partCost;

                // Обновляем заказ
                string updateQuery = "UPDATE orders SET total_cost = @total_cost WHERE order_id = @order_id";
                DatabaseHelper.ExecuteNonQuery(updateQuery,
                    new MySqlParameter[]
                    {
                        new MySqlParameter("@total_cost", totalCost),
                        new MySqlParameter("@order_id", orderId)
                    });
            }
            catch (Exception ex)
            {
                // Не прерываем сохранение если ошибка расчета стоимости
                Console.WriteLine($"Ошибка расчета стоимости: {ex.Message}");
            }
        }

        // ОБНОВЛЕННАЯ ВАЛИДАЦИЯ
        private bool ValidateForm()
        {
            // Существующие проверки...
            if (cmbClient.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbClient.Focus();
                return false;
            }

            if (cmbCar.SelectedValue == null)
            {
                MessageBox.Show("Выберите автомобиль.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCar.Focus();
                return false;
            }

            if (cmbService.SelectedValue == null)
            {
                MessageBox.Show("Выберите услугу.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbService.Focus();
                return false;
            }

            if (cmbEmployee.SelectedValue == null)
            {
                MessageBox.Show("Выберите сотрудника.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbEmployee.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtProblemDescription.Text))
            {
                MessageBox.Show("Введите описание проблемы.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProblemDescription.Focus();
                return false;
            }

            // Проверка даты для завершенных заказов
            string selectedStatus = cmbStatus.SelectedItem?.ToString() ?? "";
            if (selectedStatus.ToLower() == "завершен")
            {
                if (dtpCompletionDate.Value > DateTime.Now.AddDays(3))
                {
                    DialogResult result = MessageBox.Show(
                        "Для завершенного заказа дата завершения установлена более чем на 3 дня вперед. Это может быть ошибкой. Продолжить?",
                        "Предупреждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        dtpCompletionDate.Focus();
                        return false;
                    }
                }

                if (dtpCompletionDate.Value.Date < DateTime.Today)
                {
                    MessageBox.Show("Для завершенного заказа дата завершения не может быть в прошлом.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dtpCompletionDate.Focus();
                    return false;
                }
            }
            else
            {
                if (dtpCompletionDate.Value.Date < DateTime.Today)
                {
                    MessageBox.Show("Дата завершения не может быть в прошлом.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dtpCompletionDate.Focus();
                    return false;
                }
            }

            return true;
        }

        // Остальные методы без изменений
        private void SetDateConstraints()
        {
            DateTime today = DateTime.Today;
            dtpCompletionDate.MinDate = today;
            dtpCompletionDate.MaxDate = today.AddYears(1);
            dtpCompletionDate.Value = DateTime.Now;
        }

        private void dtpCompletionDate_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DateTime selectedDate = dtpCompletionDate.Value;
            DateTime today = DateTime.Today;

            if (selectedDate.Date < today)
            {
                MessageBox.Show("Дата завершения не может быть в прошлом.",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpCompletionDate.Value = DateTime.Now;
                e.Cancel = true;
            }

            if (selectedDate > today.AddYears(1))
            {
                MessageBox.Show("Дата завершения не может быть более чем на год вперед.",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpCompletionDate.Value = today.AddYears(1);
                e.Cancel = true;
            }
        }

        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedStatus = cmbStatus.SelectedItem?.ToString() ?? "";

            if (selectedStatus.ToLower() == "завершен")
            {
                dtpCompletionDate.BackColor = System.Drawing.Color.LightYellow;

                if (dtpCompletionDate.Value > DateTime.Now)
                {
                    DialogResult result = MessageBox.Show(
                        "Для завершенного заказа рекомендуется установить текущую дату. Установить сейчас?",
                        "Дата завершения",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        dtpCompletionDate.Value = DateTime.Now;
                    }
                }
            }
            else
            {
                dtpCompletionDate.BackColor = System.Drawing.Color.AliceBlue;
            }
        }

        private void LoadComboBoxData()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // Загрузка клиентов
                string clientsQuery = "SELECT client_id, CONCAT(full_name, ' (', phone_number, ')') as display_name FROM clients ORDER BY full_name";
                DataTable clients = DatabaseHelper.ExecuteQuery(clientsQuery);
                cmbClient.DataSource = clients;
                cmbClient.DisplayMember = "display_name";
                cmbClient.ValueMember = "client_id";

                // Загрузка услуг
                string servicesQuery = "SELECT service_id, CONCAT(service_name, ' - ', price, ' руб.') as display_name FROM services ORDER BY service_name";
                DataTable services = DatabaseHelper.ExecuteQuery(servicesQuery);
                cmbService.DataSource = services;
                cmbService.DisplayMember = "display_name";
                cmbService.ValueMember = "service_id";

                // Загрузка сотрудников
                string employeesQuery = "SELECT employee_id, CONCAT(full_name, ' (', job_title, ')') as display_name FROM employees ORDER BY full_name";
                DataTable employees = DatabaseHelper.ExecuteQuery(employeesQuery);
                cmbEmployee.DataSource = employees;
                cmbEmployee.DisplayMember = "display_name";
                cmbEmployee.ValueMember = "employee_id";

                // Статусы
                cmbStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void cmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbClient.SelectedValue != null && cmbClient.SelectedValue is int clientId)
            {
                LoadClientCars(clientId);
            }
        }

        private void LoadClientCars(int clientId)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                string query = @"SELECT car_id, 
                               CONCAT(brand, ' ', model, ' (', registration_number, ', ', year, ' год)') as car_info 
                               FROM cars WHERE client_id = @client_id 
                               ORDER BY brand, model";

                MySqlParameter[] parameters = { new MySqlParameter("@client_id", clientId) };
                DataTable cars = DatabaseHelper.ExecuteQuery(query, parameters);

                cmbCar.DataSource = cars;
                cmbCar.DisplayMember = "car_info";
                cmbCar.ValueMember = "car_id";

                if (cars.Rows.Count == 0)
                {
                    MessageBox.Show("У выбранного клиента нет зарегистрированных автомобилей.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки автомобилей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void AddEditOrderForm_Load(object sender, EventArgs e) { }
    }
}