using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace car_servises
{
    public partial class Order : BaseForm
    {
        private string _userLogin;
        private int? _currentEmployeeId;
        private string _userRole;
        private DataTable originalOrdersData; // Сохраняем оригинальные данные

        // Добавляем объявления для контролов поиска
        private Panel pnlSearch;
        private TextBox txtSearch;
        private ComboBox cmbSearchColumn;
        private ComboBox cmbSortBy;
        private RadioButton rbAsc;
        private RadioButton rbDesc;
        private Button btnResetFilters;
        private Label lblSearch;
        private Label lblSort;
        private Label lblInColumn;

        public string UserLogin
        {
            get => _userLogin;
            set => _userLogin = value;
        }


        public Order()
        {
            InitializeComponent();
            ConnectSearchEvents();
            SubscribeToDataGridViewEvents();
            LoadOrders();
        }

        public Order(string userRole = "")
        {
            SubscribeToDataGridViewEvents();
            InitializeComponent();
            _userRole = userRole;
            ConfigureFormForRole();
            ConnectSearchEvents();
            LoadOrders();
        }

        public void SetUserRole(string userRole)
        {
            ConfigureFormForRole();
        }

        //private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    // Проверяем, что это механик и кликнули на колонку "Статус"
        //    if (CurrentUser.Role == "Механик" &&
        //        e.RowIndex >= 0 &&
        //        e.ColumnIndex >= 0 &&
        //        dataGridView1.Columns[e.ColumnIndex].HeaderText == "Статус")
        //    {
        //        // Создаем выпадающий список для статусов
        //        DataGridViewComboBoxCell comboBoxCell = new DataGridViewComboBoxCell();

        //        // Добавляем возможные статусы
        //        comboBoxCell.Items.AddRange("В работе", "Завершен", "Отменен", "Ожидает запчасти");

        //        // Устанавливаем текущее значение
        //        comboBoxCell.Value = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

        //        // Заменяем ячейку на выпадающий список
        //        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex] = comboBoxCell;

        //        // Сразу переводим в режим редактирования
        //        dataGridView1.BeginEdit(true);
        //    }
        //}

        // Обработчик для отслеживания изменения статуса через выпадающий список
        //private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        //{
        //    if (dataGridView1.IsCurrentCellDirty &&
        //        dataGridView1.CurrentCell is DataGridViewComboBoxCell &&
        //        dataGridView1.CurrentCell.ColumnIndex == GetStatusColumnIndex())
        //    {
        //        dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        //    }
        //}

        // Вспомогательный метод для получения индекса колонки "Статус"
        //private int GetStatusColumnIndex()
        //{
        //    foreach (DataGridViewColumn col in dataGridView1.Columns)
        //    {
        //        if (col.HeaderText == "Статус")
        //        {
        //            return col.Index;
        //        }
        //    }
        //    return -1;
        //}

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Если текущая ячейка "грязная" (изменена) и это комбобокс
            if (dataGridView1.IsCurrentCellDirty)
            {
                // Завершаем редактирование ячейки
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // Обработчик для события изменения значения в комбобоксе
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что это колонка "Статус" и индекс строки валидный
            if (e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex].HeaderText == "Статус")
            {
                try
                {
                    // Получаем ID заказа из текущей строки
                    int orderId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["Номер заказа"].Value);
                    string newStatus = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();

                    if (!string.IsNullOrEmpty(newStatus))
                    {
                        // Обновляем статус в базе данных
                        string updateQuery = "UPDATE orders SET status = @status WHERE order_id = @orderId";

                        // Добавляем проверку, что механик может менять статус только своих заказов
                        if (CurrentUser.Role == "Механик" && _currentEmployeeId.HasValue)
                        {
                            updateQuery += " AND employee_id = @employeeId";
                        }

                        List<MySqlParameter> parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@status", newStatus),
                    new MySqlParameter("@orderId", orderId)
                };

                        if (CurrentUser.Role == "Механик" && _currentEmployeeId.HasValue)
                        {
                            parameters.Add(new MySqlParameter("@employeeId", _currentEmployeeId.Value));
                        }

                        int rowsAffected = DatabaseHelper.ExecuteNonQuery(updateQuery, parameters.ToArray());

                        if (rowsAffected > 0)
                        {
                            // Если статус изменился на "завершен", устанавливаем дату завершения
                            if (newStatus.ToLower() == "завершен")
                            {
                                UpdateCompletionDate(orderId);
                            }

                            // Обновляем отображение в DataGridView
                            dataGridView1.Refresh();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить статус. Возможно, у вас нет прав для изменения этого заказа.");
                            LoadOrders(); // Перезагружаем данные
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении статуса: {ex.Message}");
                    LoadOrders(); // Перезагружаем данные при ошибке
                }
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Просто подавляем ошибку
            e.ThrowException = false;

            // Для отладки можно посмотреть ошибку
            // MessageBox.Show($"Ошибка: {e.Exception.Message}");
        }

        private void ConnectSearchEvents()
        {
            // Убедимся, что обработчики подключены
            txtSearch.TextChanged -= txtSearch_TextChanged; // Сначала отключаем
            txtSearch.TextChanged += txtSearch_TextChanged;

            cmbSearchColumn.SelectedIndexChanged -= cmbSearchColumn_SelectedIndexChanged;
            cmbSearchColumn.SelectedIndexChanged += cmbSearchColumn_SelectedIndexChanged;

            cmbSortBy.SelectedIndexChanged -= cmbSortBy_SelectedIndexChanged;
            cmbSortBy.SelectedIndexChanged += cmbSortBy_SelectedIndexChanged;

            rbAsc.CheckedChanged -= rbSortDirection_CheckedChanged;
            rbAsc.CheckedChanged += rbSortDirection_CheckedChanged;

            rbDesc.CheckedChanged -= rbSortDirection_CheckedChanged;
            rbDesc.CheckedChanged += rbSortDirection_CheckedChanged;

            btnResetFilters.Click -= btnResetFilters_Click;
            btnResetFilters.Click += btnResetFilters_Click;
        }
        private void ConfigureFormForRole()
        {
            if (CurrentUser.Role == "Механик")
            {
                button2.Visible = false;
                button3.Visible = false;

                dataGridView1.ReadOnly = false;

                // Разрешаем редактировать ТОЛЬКО колонку "Статус"
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    col.ReadOnly = col.HeaderText != "Статус";
                }

                // Настраиваем выпадающий список для статуса
                SetupStatusColumnForMechanic(); // <-- Добавь эту строку

                // Получаем ID текущего механика
                GetCurrentMechanicId();
            }
            else if (CurrentUser.Role == "Менеджер" || CurrentUser.Role == "Администратор")
            {
                button2.Visible = true;
                button3.Visible = true;
                this.Text = "Управление заказами";
                dataGridView1.ReadOnly = false;
            }
        }

        private void GetCurrentMechanicId()
        {
            try
            {
                string query = "SELECT employee_id FROM employees WHERE login = @login";
                MySqlParameter[] parameters = { new MySqlParameter("@login", CurrentUser.Login) };

                DataTable result = DatabaseHelper.ExecuteQuery(query, parameters);

                if (result.Rows.Count > 0)
                {
                    _currentEmployeeId = Convert.ToInt32(result.Rows[0]["employee_id"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения ID сотрудника: {ex.Message}");
            }
        }

        private void SetupStatusColumnForMechanic()
        {
            if (CurrentUser.Role != "Механик") return;

            // Находим колонку статуса
            DataGridViewColumn statusColumn = null;
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                if (col.HeaderText == "Статус")
                {
                    statusColumn = col;
                    break;
                }
            }

            if (statusColumn == null) return;

            // Запоминаем позицию колонки
            int columnIndex = statusColumn.Index;

            // Создаем новую колонку-комбобокс
            DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
            comboBoxColumn.HeaderText = "Статус";
            comboBoxColumn.Name = "Статус";
            comboBoxColumn.Items.AddRange("В работе", "Завершен", "Отменен", "Ожидает запчасти");
            comboBoxColumn.DataPropertyName = "Статус"; // Привязка к данным
            comboBoxColumn.ReadOnly = false; // Разрешаем редактирование

            // Удаляем старую колонку и вставляем новую на её место
            dataGridView1.Columns.Remove(statusColumn);
            dataGridView1.Columns.Insert(columnIndex, comboBoxColumn);

            // Настраиваем остальные колонки как только для чтения
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                if (col.HeaderText != "Статус")
                {
                    col.ReadOnly = true;
                }
            }
        }


        private void LoadOrders()
        {
            try
            {
                string query = @"
        SELECT 
            o.order_id AS 'Номер заказа',
            e.full_name AS 'Сотрудник',
            o.order_date AS 'Дата заказа',
            o.completion_date AS 'Дата завершения',
            o.total_cost AS 'Общая стоимость',
            c.full_name AS 'Клиент',
            CONCAT(car.brand, ' ', car.model, ' (', car.registration_number, ')') AS 'Автомобиль',
            s.service_name AS 'Услуга',
            p.part_name AS 'Использованная запчасть',
            o.status AS 'Статус',
            o.problem_description AS 'Описание проблемы',
            o.recommendations AS 'Рекомендации'
        FROM orders o
        LEFT JOIN employees e ON o.employee_id = e.employee_id
        LEFT JOIN clients c ON o.client_id = c.client_id
        LEFT JOIN cars car ON o.car_id = car.car_id
        LEFT JOIN parts p ON o.part_id = p.part_id  
        LEFT JOIN services s ON o.service_id = s.service_id
        ";

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                //            // 🔴 ВАЖНО: если механик — фильтруем заказы только для него
                if (CurrentUser.Role == "Механик")
                {
                    // Получаем ID сотрудника, если еще не получили
                    if (!_currentEmployeeId.HasValue)
                    {
                        GetCurrentMechanicId();
                    }

                    if (_currentEmployeeId.HasValue)
                    {
                        query += @"
        WHERE o.employee_id = @employeeId";

                        parameters.Add(new MySqlParameter("@employeeId", _currentEmployeeId.Value));
                    }
                    else
                    {
                        MessageBox.Show("Не удалось определить ID сотрудника");
                        return;
                    }
                }

                query += " ORDER BY o.order_date DESC";

                DataTable orders = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());

                originalOrdersData = orders.Copy();
                dataGridView1.DataSource = orders;

                FillSearchColumns();
                ApplySearchStyles();

                // После загрузки данных повторно настраиваем права редактирования
                ConfigureFormForRole();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что изменялась колонка "Статус"
            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Статус")
            {
                try
                {
                    // Получаем ID заказа из текущей строки
                    int orderId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["Номер заказа"].Value);
                    string newStatus = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();

                    if (!string.IsNullOrEmpty(newStatus))
                    {
                        // Обновляем статус в базе данных
                        string updateQuery = "UPDATE orders SET status = @status WHERE order_id = @orderId";

                        // Добавляем проверку, что механик может менять статус только своих заказов
                        if (CurrentUser.Role == "Механик" && _currentEmployeeId.HasValue)
                        {
                            updateQuery += " AND employee_id = @employeeId";
                        }

                        List<MySqlParameter> parameters = new List<MySqlParameter>
                        {
                            new MySqlParameter("@status", newStatus),
                            new MySqlParameter("@orderId", orderId)
                        };

                        if (CurrentUser.Role == "Механик" && _currentEmployeeId.HasValue)
                        {
                            parameters.Add(new MySqlParameter("@employeeId", _currentEmployeeId.Value));
                        }

                        int rowsAffected = DatabaseHelper.ExecuteNonQuery(updateQuery, parameters.ToArray());

                        if (rowsAffected > 0)
                        {
                            // Обновляем отображение
                            dataGridView1.Refresh();

                            // Если статус изменился на "завершен", можно автоматически установить дату завершения
                            if (newStatus.ToLower() == "завершен")
                            {
                                UpdateCompletionDate(orderId);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить статус. Возможно, у вас нет прав для изменения этого заказа.");
                            // Отменяем изменение в DataGridView
                            LoadOrders();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении статуса: {ex.Message}");
                    // Отменяем изменение в DataGridView
                    LoadOrders();
                }
            }
        }

        private void UpdateCompletionDate(int orderId)
        {
            try
            {
                string query = "UPDATE orders SET completion_date = NOW() WHERE order_id = @orderId";

                if (CurrentUser.Role == "Механик" && _currentEmployeeId.HasValue)
                {
                    query += " AND employee_id = @employeeId";
                }

                List<MySqlParameter> parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@orderId", orderId)
                };

                if (CurrentUser.Role == "Механик" && _currentEmployeeId.HasValue)
                {
                    parameters.Add(new MySqlParameter("@employeeId", _currentEmployeeId.Value));
                }

                DatabaseHelper.ExecuteNonQuery(query, parameters.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении даты завершения: {ex.Message}");
            }
        }

        private void SubscribeToDataGridViewEvents()
        {
            // Убираем CellEndEdit, добавляем новые обработчики
            dataGridView1.CurrentCellDirtyStateChanged += dataGridView1_CurrentCellDirtyStateChanged;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            dataGridView1.DataError += dataGridView1_DataError;
        }



        private void FillSearchColumns()
        {
            cmbSearchColumn.Items.Clear();
            cmbSortBy.Items.Clear();

            if (originalOrdersData != null)
            {
                foreach (DataColumn column in originalOrdersData.Columns)
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
            if (originalOrdersData == null)
            {
                MessageBox.Show("Нет данных для поиска");
                return;
            }

            string searchText = txtSearch.Text.Trim();
            string searchColumn = cmbSearchColumn.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(searchColumn))
            {
                MessageBox.Show("Не выбрана колонка для поиска");
                return;
            }

            DataTable filteredData;

            // Применяем фильтрацию
            if (string.IsNullOrEmpty(searchText))
            {
                filteredData = originalOrdersData.Copy();
            }
            else
            {
                try
                {
                    // ПРОСТОЙ И НАДЕЖНЫЙ СПОСОБ
                    filteredData = originalOrdersData.Clone();

                    foreach (DataRow row in originalOrdersData.Rows)
                    {
                        string cellValue = row[searchColumn].ToString();
                        if (cellValue.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            filteredData.ImportRow(row);
                        }
                    }

                    // Если ничего не найдено
                    if (filteredData.Rows.Count == 0)
                    {
                        // Можно показать сообщение или оставить пустую таблицу
                        MessageBox.Show("Ничего не найдено", "Результат поиска",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка поиска: {ex.Message}");
                    filteredData = originalOrdersData.Copy();
                }
            }

            // Применяем сортировку
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
            string sortDirection = rbAsc.Checked ? "ASC" : "DESC";

            try
            {
                data.DefaultView.Sort = $"{sortColumn} {sortDirection}";
                dataGridView1.DataSource = data.DefaultView;

                // Обновляем отображение
                dataGridView1.Refresh();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Невозможно отсортировать колонку '{sortColumn}': {ex.Message}");
                dataGridView1.DataSource = data;
            }
        }

        private void ResetSearchFilters()
        {
            txtSearch.Text = "";
            if (cmbSearchColumn.Items.Count > 0) cmbSearchColumn.SelectedIndex = 0;
            if (cmbSortBy.Items.Count > 0) cmbSortBy.SelectedIndex = 0;
            rbAsc.Checked = true;

            if (originalOrdersData != null)
            {
                dataGridView1.DataSource = originalOrdersData.Copy();
                dataGridView1.Refresh();
            }

            MessageBox.Show("Фильтры сброшены", "Сброс",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ApplySearchStyles()
        {
            // Применяем стили к элементам поиска
            AppStyles.ApplyTextBoxStyle(txtSearch);
            AppStyles.ApplyButtonStyle(btnResetFilters);

            // Стили для комбобоксов
            cmbSearchColumn.Font = AppStyles.NormalFont;
            cmbSortBy.Font = AppStyles.NormalFont;

            // Стили для меток
            AppStyles.ApplyLabelStyle(lblSearch);
            AppStyles.ApplyLabelStyle(lblInColumn);
            AppStyles.ApplyLabelStyle(lblSort);
        }


        // Обработчики для поиска и фильтрации
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"Поиск: {txtSearch.Text}");
            ApplySearch();
        }

        private void cmbSearchColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"Колонка поиска: {cmbSearchColumn.SelectedItem}");
            ApplySearch();
        }

        private void cmbSortBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"Сортировка по: {cmbSortBy.SelectedItem}");
            ApplySearch();
        }

        private void rbSortDirection_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"Направление сортировки: {(rbAsc.Checked ? "ASC" : "DESC")}");
            ApplySearch();
        }

        private void btnResetFilters_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Сброс фильтров");
            ResetSearchFilters();
        }
        private void button2_Click(object sender, EventArgs e) // Оформление заказа
        {
            if (CurrentUser.Role == "Механик")
            {
                MessageBox.Show("У вас нет прав для оформления заказов.",
                    "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (AddEditOrderForm form = new AddEditOrderForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadOrders();
                }
            }
        }
        private void button3_Click(object sender, EventArgs e) // Чек
        {
            if (CurrentUser.Role == "Механик")
            {
                MessageBox.Show("У вас нет прав для создания чеков.", "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridView1.CurrentRow != null)
            {
                int orderId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Номер заказа"].Value);
                GenerateWordReceipt(orderId);
            }
            else
            {
                MessageBox.Show("Выберите заказ для создания чека.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void GenerateWordReceipt(int orderId)
        {
            try
            {
                // Получаем данные заказа
                string orderQuery = @"
            SELECT 
                o.order_id,
                o.order_date,
                o.completion_date,
                o.total_cost,
                c.full_name AS client_name,
                c.phone_number,
                c.email,
                car.brand,
                car.model,
                car.registration_number,
                s.service_name,
                e.full_name AS employee_name,
                o.problem_description,
                o.recommendations,
                o.status,
                o.part_id  -- Добавляем прямую ссылку на запчасть
            FROM orders o
            LEFT JOIN clients c ON o.client_id = c.client_id
            LEFT JOIN cars car ON o.car_id = car.car_id
            LEFT JOIN services s ON o.service_id = s.service_id
            LEFT JOIN employees e ON o.employee_id = e.employee_id
            WHERE o.order_id = @order_id";

                MySqlParameter[] parameters = { new MySqlParameter("@order_id", orderId) };
                DataTable orderData = DatabaseHelper.ExecuteQuery(orderQuery, parameters);

                if (orderData.Rows.Count == 0)
                {
                    MessageBox.Show("Заказ не найден.");
                    return;
                }

                DataRow order = orderData.Rows[0];

                // ПОЛУЧАЕМ ЗАПЧАСТИ - ОБЪЕДИНЯЕМ ДВА ВАРИАНТА
                string partsQuery = @"
            -- Вариант 1: Запчасти из order_details (старые заказы)
            SELECT 
                p.part_name,
                p.manufacturer,
                p.cost,
                od.quantity,
                (p.cost * od.quantity) as total_cost
            FROM order_details od
            JOIN parts p ON od.part_id = p.part_id
            WHERE od.order_id = @order_id
            
            UNION ALL
            
            -- Вариант 2: Прямая запчасть из orders (новые заказы)
            SELECT 
                p.part_name,
                p.manufacturer,
                p.cost,
                1 as quantity,
                p.cost as total_cost
            FROM orders o
            JOIN parts p ON o.part_id = p.part_id
            WHERE o.order_id = @order_id AND o.part_id IS NOT NULL
              AND NOT EXISTS (SELECT 1 FROM order_details od WHERE od.order_id = o.order_id)";

                DataTable partsData = DatabaseHelper.ExecuteQuery(partsQuery, parameters);

                // Создаем Word документ
                CreateWordDocument(order, partsData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания чека: {ex.Message}");
            }
        }

        private void CreateWordDocument(DataRow order, DataTable partsData)
        {
            try
            {
                // Создаем имя файла
                string fileName = $"Чек_заказ_{order["order_id"]}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                // Рассчитываем стоимости
                decimal totalCost = Convert.ToDecimal(order["total_cost"]);
                decimal partsTotal = 0;
                decimal serviceCost = totalCost;

                using (WordprocessingDocument doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Заголовок
                    AddFormattedParagraph(body, "ЧЕК АВТОСЕРВИС", true, 20, JustificationValues.Center);
                    AddEmptyParagraph(body);

                    // ИНФОРМАЦИЯ О ЗАКАЗЕ
                    AddFormattedParagraph(body, "ИНФОРМАЦИЯ О ЗАКАЗЕ", true, 14, JustificationValues.Left);
                    AddSimpleParagraph(body, $"Номер заказа: {order["order_id"]}");
                    AddSimpleParagraph(body, $"Дата создания: {Convert.ToDateTime(order["order_date"]):dd.MM.yyyy HH:mm}");
                    if (order["completion_date"] != DBNull.Value)
                        AddSimpleParagraph(body, $"Дата завершения: {Convert.ToDateTime(order["completion_date"]):dd.MM.yyyy HH:mm}");
                    AddSimpleParagraph(body, $"Статус: {order["status"]}");
                    AddEmptyParagraph(body);

                    // ДАННЫЕ КЛИЕНТА
                    AddFormattedParagraph(body, "ДАННЫЕ КЛИЕНТА", true, 14, JustificationValues.Left);
                    AddSimpleParagraph(body, $"Клиент: {order["client_name"]}");
                    AddSimpleParagraph(body, $"Телефон: {order["phone_number"]}");
                    if (!string.IsNullOrEmpty(order["email"]?.ToString()))
                        AddSimpleParagraph(body, $"Email: {order["email"]}");
                    AddEmptyParagraph(body);

                    // АВТОМОБИЛЬ
                    AddFormattedParagraph(body, "АВТОМОБИЛЬ", true, 14, JustificationValues.Left);
                    AddSimpleParagraph(body, $"Марка: {order["brand"]}");
                    AddSimpleParagraph(body, $"Модель: {order["model"]}");
                    AddSimpleParagraph(body, $"Гос. номер: {order["registration_number"]}");
                    AddEmptyParagraph(body);

                    // УСЛУГА
                    AddFormattedParagraph(body, "ВЫПОЛНЕННЫЕ РАБОТЫ", true, 14, JustificationValues.Left);
                    AddSimpleParagraph(body, $"Услуга: {order["service_name"]}");
                    if (!string.IsNullOrEmpty(order["problem_description"]?.ToString()))
                    {
                        AddSimpleParagraph(body, "Описание проблемы:");
                        AddSimpleParagraph(body, $"{order["problem_description"]}");
                    }
                    AddEmptyParagraph(body);

                    // ИСПОЛЬЗОВАННЫЕ ЗАПЧАСТИ
                    if (partsData.Rows.Count > 0)
                    {
                        AddFormattedParagraph(body, "ИСПОЛЬЗОВАННЫЕ ЗАПЧАСТИ", true, 14, JustificationValues.Left);
                        AddEmptyParagraph(body);

                        // Создаем таблицу
                        Table table = new Table();
                        TableProperties tableProperties = new TableProperties();
                        TableWidth tableWidth = new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct };
                        tableProperties.Append(tableWidth);

                        // Границы
                        TableBorders borders = new TableBorders();
                        borders.TopBorder = new TopBorder() { Val = BorderValues.Single, Size = 1 };
                        borders.BottomBorder = new BottomBorder() { Val = BorderValues.Single, Size = 1 };
                        borders.LeftBorder = new LeftBorder() { Val = BorderValues.Single, Size = 1 };
                        borders.RightBorder = new RightBorder() { Val = BorderValues.Single, Size = 1 };
                        borders.InsideHorizontalBorder = new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 1 };
                        borders.InsideVerticalBorder = new InsideVerticalBorder() { Val = BorderValues.Single, Size = 1 };
                        tableProperties.Append(borders);
                        table.AppendChild(tableProperties);

                        // Заголовок таблицы
                        TableRow headerRow = new TableRow();
                        headerRow.Append(CreateTableCell("Наименование", true, 2500));
                        headerRow.Append(CreateTableCell("Производитель", true, 1500));
                        headerRow.Append(CreateTableCell("Кол-во", true, 1000));
                        headerRow.Append(CreateTableCell("Цена", true, 1200));
                        headerRow.Append(CreateTableCell("Сумма", true, 1200));
                        table.Append(headerRow);

                        // Строки с запчастями
                        foreach (DataRow part in partsData.Rows)
                        {
                            TableRow row = new TableRow();

                            string partName = part["part_name"].ToString();
                            string manufacturer = part["manufacturer"].ToString();
                            int quantity = Convert.ToInt32(part["quantity"]);
                            decimal cost = Convert.ToDecimal(part["cost"]);
                            decimal partTotal = Convert.ToDecimal(part["total_cost"]);

                            row.Append(CreateTableCell(partName, false, 2500));
                            row.Append(CreateTableCell(string.IsNullOrEmpty(manufacturer) ? "-" : manufacturer, false, 1500));
                            row.Append(CreateTableCell($"{quantity} шт.", false, 1000));
                            row.Append(CreateTableCell($"{cost:C2}", false, 1200));
                            row.Append(CreateTableCell($"{partTotal:C2}", false, 1200));

                            table.Append(row);
                            partsTotal += partTotal;
                        }

                        body.Append(table);
                        AddEmptyParagraph(body);
                        AddFormattedParagraph(body, $"ИТОГО ПО ЗАПЧАСТЯМ: {partsTotal:C2}", true, 12, JustificationValues.Right);
                        AddEmptyParagraph(body);

                        serviceCost = totalCost - partsTotal;
                    }
                    else
                    {
                        // Если запчастей нет, показываем сообщение
                        AddSimpleParagraph(body, "Запчасти не использовались");
                        AddEmptyParagraph(body);
                    }

                    // ИСПОЛНИТЕЛЬ
                    AddFormattedParagraph(body, "ИСПОЛНИТЕЛЬ", true, 14, JustificationValues.Left);
                    AddSimpleParagraph(body, $"Сотрудник: {order["employee_name"]}");
                    AddEmptyParagraph(body);

                    // СТОИМОСТЬ
                    AddFormattedParagraph(body, "РАСЧЕТ СТОИМОСТИ", true, 14, JustificationValues.Left);
                    AddSimpleParagraph(body, $"Стоимость работ: {serviceCost:C2}");
                    if (partsData.Rows.Count > 0)
                    {
                        AddSimpleParagraph(body, $"Стоимость запчастей: {partsTotal:C2}");
                    }
                    AddSimpleParagraph(body, "----------------------------------------");
                    AddFormattedParagraph(body, $"ИТОГО К ОПЛАТЕ: {totalCost:C2}", true, 16, JustificationValues.Right);
                    AddEmptyParagraph(body);

                    // РЕКОМЕНДАЦИИ
                    if (!string.IsNullOrEmpty(order["recommendations"]?.ToString()))
                    {
                        AddFormattedParagraph(body, "РЕКОМЕНДАЦИИ МЕХАНИКА", true, 14, JustificationValues.Left);
                        AddSimpleParagraph(body, $"{order["recommendations"]}");
                        AddEmptyParagraph(body);
                    }

                    // ПОДПИСЬ
                    AddSimpleParagraph(body, "_________________________");
                    AddSimpleParagraph(body, "Подпись сотрудника");
                    AddEmptyParagraph(body);
                    AddSimpleParagraph(body, $"Дата печати чека: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    AddSimpleParagraph(body, "Спасибо за обращение в наш автосервис!");

                    mainPart.Document.Save();
                }

                MessageBox.Show($"Чек успешно создан!\nФайл сохранен на рабочем столе: {fileName}",
                              "Чек создан", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании Word документа: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Вспомогательный метод для создания ячейки таблицы
        // ЗАМЕНИ полностью этот метод в твоем файле Order.cs
        private TableCell CreateTableCell(string text, bool isHeader, int width)
        {
            TableCell cell = new TableCell();

            // Свойства ячейки
            TableCellProperties cellProperties = new TableCellProperties();

            // Ширина ячейки
            TableCellWidth tableCellWidth = new TableCellWidth() { Width = width.ToString(), Type = TableWidthUnitValues.Dxa };
            cellProperties.Append(tableCellWidth);

            // Границы ячейки
            cellProperties.Append(new TopBorder() { Val = BorderValues.Single, Size = 1 });
            cellProperties.Append(new BottomBorder() { Val = BorderValues.Single, Size = 1 });
            cellProperties.Append(new LeftBorder() { Val = BorderValues.Single, Size = 1 });
            cellProperties.Append(new RightBorder() { Val = BorderValues.Single, Size = 1 });

            cell.Append(cellProperties);

            // Параграф с текстом
            Paragraph para = new Paragraph();
            Run run = new Run();

            // Если это заголовок - делаем жирным и центрируем
            if (isHeader)
            {
                RunProperties runProps = new RunProperties();
                runProps.Append(new Bold());
                run.RunProperties = runProps;

                ParagraphProperties paraProps = new ParagraphProperties();
                paraProps.Append(new Justification() { Val = JustificationValues.Center });
                para.Append(paraProps);
            }

            run.Append(new Text(text));
            para.Append(run);
            cell.Append(para);

            return cell;
        }

        private void AddFormattedParagraph(Body body, string text, bool bold, int fontSize, JustificationValues alignment)
        {
            Paragraph paragraph = new Paragraph();
            ParagraphProperties paragraphProperties = new ParagraphProperties();
            Justification justification = new Justification() { Val = alignment };
            paragraphProperties.Append(justification);

            Run run = new Run();
            RunProperties runProperties = new RunProperties();

            if (bold)
                runProperties.Append(new Bold());

            runProperties.Append(new FontSize() { Val = (fontSize * 2).ToString() });

            run.RunProperties = runProperties;
            run.Append(new Text(text));

            paragraph.Append(paragraphProperties);
            paragraph.Append(run);
            body.Append(paragraph);
        }

        private void AddSimpleParagraph(Body body, string text)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            Text textElement = new Text(text);
            run.Append(textElement);
            paragraph.Append(run);
            body.Append(paragraph);
        }

        private void AddEmptyParagraph(Body body)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            run.Append(new Text(""));
            paragraph.Append(run);
            body.Append(paragraph);
        }

        private void button1_Click(object sender, EventArgs e) // Назад
        {
            this.Close();

            switch (CurrentUser.Role)
            {
                case "Менеджер":
                    new ManagerForm().Show();
                    break;

                case "Механик":
                    new MechanicForm().Show();
                    break;

                case "Администратор":
                default:
                    new AdminForm().Show();
                    break;
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
             DataTable currentData = (DataTable)dataGridView1.DataSource;
             SearchHelper.OpenAdvancedSearch(currentData, "Поиск заказов");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DataTable currentData = (DataTable)dataGridView1.DataSource;
            SearchHelper.OpenSimpleSearch(currentData, "Быстрый поиск заказов");
        }

        private void Order_Load(object sender, EventArgs e)
        {


        }
    }
}
