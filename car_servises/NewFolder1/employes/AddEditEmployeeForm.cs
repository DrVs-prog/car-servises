using System;
using System.Data;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class AddEditEmployeeForm : Form
    {
        private int _employeeId;
        private bool _isEditMode;
        private bool _isFormValid = false;

        public AddEditEmployeeForm()
        {
            InitializeComponent();
            _isEditMode = false;
            this.Text = "Добавление сотрудника";
            LoadRoles();
            SetupValidation();
        }

        public AddEditEmployeeForm(int employeeId, string fullName, string jobTitle, DateTime hireDate, string roleName, string login)
        {
            InitializeComponent();
            _employeeId = employeeId;
            _isEditMode = true;
            this.Text = "Редактирование сотрудника";

            txtFullName.Text = fullName;
            txtJobTitle.Text = jobTitle;
            dtpHireDate.Value = hireDate;
            txtLogin.Text = login;

            LoadRoles(); // Загружаем роли ДО установки выбранной

            // Устанавливаем выбранную роль
            if (cmbRole.DataSource != null)
            {
                DataTable dt = cmbRole.DataSource as DataTable;
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["role_name"].ToString() == roleName)
                        {
                            cmbRole.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }

            SetupValidation();
            ValidateForm();
        }

        private void SetupValidation()
        {
            // Настраиваем валидацию при изменении текста
            txtFullName.TextChanged += ValidateForm;
            txtJobTitle.TextChanged += ValidateForm;
            txtLogin.TextChanged += ValidateForm;
            cmbRole.SelectedIndexChanged += ValidateForm;

            // Устанавливаем максимальные длины для текстовых полей
            txtFullName.MaxLength = 100;
            txtJobTitle.MaxLength = 50;
            txtLogin.MaxLength = 30;

            // Настраиваем подсказки (placeholder-like behavior)
            SetupPlaceholderBehavior();
        }

        private void SetupPlaceholderBehavior()
        {
            // Обработчики для эффекта placeholder
            txtFullName.Enter += (s, e) =>
            {
                if (txtFullName.Text == "Введите ФИО полностью")
                {
                    txtFullName.Text = "";
                    txtFullName.ForeColor = System.Drawing.SystemColors.WindowText;
                }
            };

            txtFullName.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    txtFullName.Text = "Введите ФИО полностью";
                    txtFullName.ForeColor = System.Drawing.SystemColors.GrayText;
                }
            };

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                txtFullName.Text = "Введите ФИО полностью";
                txtFullName.ForeColor = System.Drawing.SystemColors.GrayText;
            }
        }

        private void LoadRoles()
        {
            try
            {
                string query = "SELECT role_id, role_name FROM roles ORDER BY role_name";
                DataTable roles = DatabaseHelper.ExecuteQuery(query);

                // Создаем DataTable для привязки
                DataTable dtRoles = new DataTable();
                dtRoles.Columns.Add("role_id", typeof(int));
                dtRoles.Columns.Add("role_name", typeof(string));

                // Добавляем пустой элемент для выбора
                dtRoles.Rows.Add(0, "-- Выберите роль --");

                // Добавляем роли из базы
                foreach (DataRow row in roles.Rows)
                {
                    dtRoles.Rows.Add(row["role_id"], row["role_name"]);
                }

                // Настраиваем DataSource
                cmbRole.DataSource = dtRoles;
                cmbRole.DisplayMember = "role_name";
                cmbRole.ValueMember = "role_id";

                if (!_isEditMode)
                    cmbRole.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ValidateForm(object sender = null, EventArgs e = null)
        {
            bool isValid = true;
            string errorMessage = "";

            // Валидация ФИО
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                txtFullName.Text == "Введите ФИО полностью" ||
                txtFullName.Text.Trim().Length < 5)
            {
                isValid = false;
                errorMessage += "• ФИО должно содержать не менее 5 символов\n";
                SetErrorStyle(txtFullName, true);
            }
            else if (!IsValidFullName(txtFullName.Text.Trim()))
            {
                isValid = false;
                errorMessage += "• ФИО должно содержать только буквы и пробелы\n";
                SetErrorStyle(txtFullName, true);
            }
            else
            {
                SetErrorStyle(txtFullName, false);
            }

            // Валидация должности
            if (string.IsNullOrWhiteSpace(txtJobTitle.Text.Trim()))
            {
                isValid = false;
                errorMessage += "• Должность не может быть пустой\n";
                SetErrorStyle(txtJobTitle, true);
            }
            else if (txtJobTitle.Text.Trim().Length > 50)
            {
                isValid = false;
                errorMessage += "• Должность не должна превышать 50 символов\n";
                SetErrorStyle(txtJobTitle, true);
            }
            else
            {
                SetErrorStyle(txtJobTitle, false);
            }

            // Валидация логина
            if (string.IsNullOrWhiteSpace(txtLogin.Text.Trim()))
            {
                isValid = false;
                errorMessage += "• Логин не может быть пустым\n";
                SetErrorStyle(txtLogin, true);
            }
            else if (!IsValidLogin(txtLogin.Text.Trim()))
            {
                isValid = false;
                errorMessage += "• Логин должен содержать только латинские буквы, цифры и символы ._-@\n";
                SetErrorStyle(txtLogin, true);
            }
            else if (txtLogin.Text.Trim().Length < 3 || txtLogin.Text.Trim().Length > 30)
            {
                isValid = false;
                errorMessage += "• Логин должен быть от 3 до 30 символов\n";
                SetErrorStyle(txtLogin, true);
            }
            else
            {
                SetErrorStyle(txtLogin, false);
            }

            // Валидация роли
            if (cmbRole.SelectedIndex <= 0 || GetSelectedRoleId() == 0)
            {
                isValid = false;
                errorMessage += "• Необходимо выбрать роль сотрудника\n";
                SetErrorStyle(cmbRole, true);
            }
            else
            {
                SetErrorStyle(cmbRole, false);
            }

            // Валидация даты приема
            if (dtpHireDate.Value > DateTime.Now)
            {
                isValid = false;
                errorMessage += "• Дата приема не может быть будущей датой\n";
                SetErrorStyle(dtpHireDate, true);
            }
            else if (dtpHireDate.Value.Year < 2000)
            {
                isValid = false;
                errorMessage += "• Дата приема не может быть раньше 2000 года\n";
                SetErrorStyle(dtpHireDate, true);
            }
            else
            {
                SetErrorStyle(dtpHireDate, false);
            }

            // Проверка уникальности логина (только для нового сотрудника)
            if (!_isEditMode && isValid && !string.IsNullOrWhiteSpace(txtLogin.Text.Trim()))
            {
                if (IsLoginExists(txtLogin.Text.Trim()))
                {
                    isValid = false;
                    errorMessage += "• Такой логин уже существует. Выберите другой логин\n";
                    SetErrorStyle(txtLogin, true);
                }
            }

            _isFormValid = isValid;
            btnSave.Enabled = isValid;

            // Показываем сообщения об ошибках
            if (!isValid && sender != null) // Показываем только если валидация вызвана пользователем
            {
                ShowValidationErrors(errorMessage);
            }
            else
            {
                ClearValidationMessage();
            }
        }

        private void SetErrorStyle(Control control, bool hasError)
        {
            if (hasError)
            {
                control.BackColor = System.Drawing.Color.LightPink;
                control.ForeColor = System.Drawing.Color.DarkRed;
            }
            else
            {
                control.BackColor = System.Drawing.SystemColors.Window;
                control.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private void ShowValidationErrors(string errorMessage)
        {
            // Можно реализовать отображение ошибок в специальном Label или Panel
            // В данном случае просто показываем в MessageBox при попытке сохранения
            // Но для интерактивной валидации лучше использовать ErrorProvider
        }

        private void ClearValidationMessage()
        {
            // Очистка сообщений об ошибках
        }

        private bool IsValidFullName(string fullName)
        {
            // Проверяем, что ФИО содержит только буквы и пробелы
            return Regex.IsMatch(fullName, @"^[а-яА-ЯёЁa-zA-Z\s]+$");
        }

        private bool IsValidLogin(string login)
        {
            // Проверяем, что логин содержит только разрешенные символы
            return Regex.IsMatch(login, @"^[a-zA-Z0-9._\-@]+$");
        }

        private bool IsLoginExists(string login)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM employees WHERE login = @login";
                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@login", login)
                };

                var result = DatabaseHelper.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Проверяем форму еще раз перед сохранением
            ValidateForm();

            if (!_isFormValid)
            {
                MessageBox.Show("Пожалуйста, исправьте ошибки в форме перед сохранением.",
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text) || txtFullName.Text == "Введите ФИО полностью")
            {
                MessageBox.Show("Введите ФИО сотрудника.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            // Проверка выбора роли
            if (cmbRole.SelectedIndex <= 0 || GetSelectedRoleId() == 0)
            {
                MessageBox.Show("Выберите роль сотрудника.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbRole.Focus();
                return;
            }

            // Проверка уникальности логина для нового сотрудника
            if (!_isEditMode && IsLoginExists(txtLogin.Text.Trim()))
            {
                MessageBox.Show("Такой логин уже существует. Выберите другой логин.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLogin.Focus();
                return;
            }

            try
            {
                string query;
                MySqlParameter[] parameters;

                if (_isEditMode)
                {
                    query = @"UPDATE employees SET full_name = @full_name, job_title = @job_title, 
                     hire_date = @hire_date, role_id = @role_id, login = @login 
                     WHERE employee_id = @id";
                    parameters = new MySqlParameter[]
                    {
                new MySqlParameter("@full_name", txtFullName.Text.Trim()),
                new MySqlParameter("@job_title", txtJobTitle.Text.Trim()),
                new MySqlParameter("@hire_date", dtpHireDate.Value),
                new MySqlParameter("@role_id", GetSelectedRoleId()), // ИСПРАВЛЕНО
                new MySqlParameter("@login", txtLogin.Text.Trim()),
                new MySqlParameter("@id", _employeeId)
                    };
                }
                else
                {
                    query = @"INSERT INTO employees (full_name, job_title, hire_date, role_id, login, password_hash) 
                     VALUES (@full_name, @job_title, @hire_date, @role_id, @login, @password_hash)";
                    parameters = new MySqlParameter[]
                    {
                new MySqlParameter("@full_name", txtFullName.Text.Trim()),
                new MySqlParameter("@job_title", txtJobTitle.Text.Trim()),
                new MySqlParameter("@hire_date", dtpHireDate.Value),
                new MySqlParameter("@role_id", GetSelectedRoleId()), // ИСПРАВЛЕНО
                new MySqlParameter("@login", txtLogin.Text.Trim()),
                new MySqlParameter("@password_hash", HashPassword("123456"))
                    };
                }

                int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                if (rowsAffected > 0)
                {
                    MessageBox.Show(_isEditMode ? "Сотрудник обновлен успешно!" : "Сотрудник добавлен успешно!",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить данные.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetSelectedRoleId()
        {
            if (cmbRole.SelectedValue != null && cmbRole.SelectedValue is int)
            {
                return (int)cmbRole.SelectedValue;
            }

            // Если SelectedValue не работает, пытаемся получить из DataRowView
            if (cmbRole.SelectedItem is DataRowView rowView)
            {
                return Convert.ToInt32(rowView["role_id"]);
            }

            return 0;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Дополнительные методы для улучшения UX
        private void txtFullName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только буквы, пробел и управляющие клавиши
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != '-')
            {
                e.Handled = true;
            }
        }

        private void txtLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только разрешенные для логина символы
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) &&
                e.KeyChar != '.' && e.KeyChar != '_' && e.KeyChar != '-' && e.KeyChar != '@')
            {
                e.Handled = true;
            }
        }

        private void dtpHireDate_ValueChanged(object sender, EventArgs e)
        {
            ValidateForm();
        }

        private void AddEditEmployeeForm_Load(object sender, EventArgs e)
        {

        }
    }
}