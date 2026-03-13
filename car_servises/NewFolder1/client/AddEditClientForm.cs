using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class AddEditClientForm : Form
    {
        private int _clientId;
        private bool _isEditMode;
        private bool _isFormValid = false;
        private ErrorProvider errorProvider = new ErrorProvider();

        public AddEditClientForm()
        {
            InitializeComponent();
            _isEditMode = false;
            this.Text = "Добавление клиента";
            SetupValidation();
            SetupPlaceholderBehavior();
        }

        public AddEditClientForm(int clientId, string fullName, string phone, string email, string address)
        {
            InitializeComponent();
            _clientId = clientId;
            _isEditMode = true;
            this.Text = "Редактирование клиента";

            txtFullName.Text = fullName;
            txtPhone.Text = phone;
            txtEmail.Text = email;
            txtAddress.Text = address;

            SetupValidation();
            ValidateForm();
        }

        private void SetupValidation()
        {
            // Настраиваем валидацию при изменении текста
            txtFullName.TextChanged += ValidateForm;
            txtPhone.TextChanged += ValidateForm;
            txtEmail.TextChanged += ValidateForm;

            // Устанавливаем максимальные длины для текстовых полей
            txtFullName.MaxLength = 100;
            txtPhone.MaxLength = 20;
            txtEmail.MaxLength = 50;
            txtAddress.MaxLength = 200;

            // Настраиваем маску для телефона
            txtPhone.Text = "+7";

            // Настраиваем ErrorProvider
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.ContainerControl = this;
        }

        private void SetupPlaceholderBehavior()
        {
            // Обработчики для эффекта placeholder для ФИО
            txtFullName.Enter += (s, e) =>
            {
                if (txtFullName.Text == "Введите ФИО полностью")
                {
                    txtFullName.Text = "";
                    txtFullName.ForeColor = SystemColors.WindowText;
                }
            };

            txtFullName.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    txtFullName.Text = "Введите ФИО полностью";
                    txtFullName.ForeColor = SystemColors.GrayText;
                }
            };

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                txtFullName.Text = "Введите ФИО полностью";
                txtFullName.ForeColor = SystemColors.GrayText;
            }

            // Placeholder для email
            txtEmail.Enter += (s, e) =>
            {
                if (txtEmail.Text == "example@email.com")
                {
                    txtEmail.Text = "";
                    txtEmail.ForeColor = SystemColors.WindowText;
                }
            };

            txtEmail.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    txtEmail.Text = "example@email.com";
                    txtEmail.ForeColor = SystemColors.GrayText;
                }
            };

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.Text = "example@email.com";
                txtEmail.ForeColor = SystemColors.GrayText;
            }
        }

        private void ValidateForm(object sender = null, EventArgs e = null)
        {
            bool isValid = true;

            // Очищаем предыдущие ошибки
            errorProvider.Clear();

            // Валидация ФИО
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                txtFullName.Text == "Введите ФИО полностью")
            {
                errorProvider.SetError(txtFullName, "ФИО обязательно для заполнения");
                isValid = false;
                SetErrorStyle(txtFullName, true);
            }
            else if (txtFullName.Text.Trim().Length < 3)
            {
                errorProvider.SetError(txtFullName, "ФИО должно содержать не менее 3 символов");
                isValid = false;
                SetErrorStyle(txtFullName, true);
            }
            else if (!IsValidFullName(txtFullName.Text.Trim()))
            {
                errorProvider.SetError(txtFullName, "ФИО должно содержать только буквы и пробелы");
                isValid = false;
                SetErrorStyle(txtFullName, true);
            }
            else
            {
                SetErrorStyle(txtFullName, false);
            }

            // Валидация телефона
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                errorProvider.SetError(txtPhone, "Телефон обязателен для заполнения");
                isValid = false;
                SetErrorStyle(txtPhone, true);
            }
            else if (!IsValidPhoneNumber(txtPhone.Text.Trim()))
            {
                errorProvider.SetError(txtPhone, "Введите корректный номер телефона\nФормат: +7(XXX)XXX-XX-XX");
                isValid = false;
                SetErrorStyle(txtPhone, true);
            }
            else
            {
                SetErrorStyle(txtPhone, false);
            }

            // Валидация email (не обязательное поле, но если заполнено - проверяем формат)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) &&
                txtEmail.Text != "example@email.com")
            {
                if (!IsValidEmail(txtEmail.Text.Trim()))
                {
                    errorProvider.SetError(txtEmail, "Введите корректный email адрес");
                    isValid = false;
                    SetErrorStyle(txtEmail, true);
                }
                else
                {
                    SetErrorStyle(txtEmail, false);
                }
            }
            else
            {
                SetErrorStyle(txtEmail, false);
            }

            // Валидация адреса (не обязательное поле, но проверяем длину если заполнено)
            if (!string.IsNullOrWhiteSpace(txtAddress.Text.Trim()))
            {
                if (txtAddress.Text.Trim().Length > 200)
                {
                    errorProvider.SetError(txtAddress, "Адрес не должен превышать 200 символов");
                    isValid = false;
                    SetErrorStyle(txtAddress, true);
                }
                else
                {
                    SetErrorStyle(txtAddress, false);
                }
            }
            else
            {
                SetErrorStyle(txtAddress, false);
            }

            _isFormValid = isValid;
            button1.Enabled = isValid;

            // Обновляем статус формы в заголовке
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            string baseTitle = _isEditMode ? "Редактирование клиента" : "Добавление клиента";
            this.Text = baseTitle + (_isFormValid ? " ✓" : "");
        }

        private void SetErrorStyle(Control control, bool hasError)
        {
            if (hasError)
            {
                control.BackColor = Color.FromArgb(255, 240, 240);
                control.ForeColor = Color.DarkRed;
            }
            else
            {
                control.BackColor = SystemColors.Window;
                control.ForeColor = SystemColors.WindowText;
            }
        }

        private bool IsValidFullName(string fullName)
        {
            // Проверяем, что ФИО содержит только буквы, пробелы и дефисы
            return Regex.IsMatch(fullName, @"^[а-яА-ЯёЁa-zA-Z\s\-]+$");
        }

        private bool IsValidPhoneNumber(string phone)
        {
            // Упрощенная проверка российского номера телефона
            // Разрешаем форматы: +7XXXXXXXXXX, 8XXXXXXXXXX, +7(XXX)XXX-XX-XX и т.д.

            // Убираем все нецифровые символы кроме плюса в начале
            string digitsOnly = Regex.Replace(phone, @"[^\d+]", "");

            // Если начинается с +7, оставляем как есть
            if (phone.StartsWith("+7"))
            {
                digitsOnly = phone.Substring(1); // убираем +
            }
            // Если начинается с 8, заменяем на 7
            else if (phone.StartsWith("8"))
            {
                digitsOnly = "7" + phone.Substring(1);
            }

            // Проверяем, что остались только цифры
            digitsOnly = Regex.Replace(digitsOnly, @"\D", "");

            // Российский номер должен быть 11 цифр и начинаться с 7
            if (digitsOnly.Length == 11 && digitsOnly.StartsWith("7"))
            {
                return true;
            }

            return false;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                // Более строгая проверка email
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private bool IsPhoneExists(string phone, int? excludeClientId = null)
        {
            try
            {
                string query;
                MySqlParameter[] parameters;

                if (excludeClientId.HasValue)
                {
                    query = "SELECT COUNT(*) FROM clients WHERE phone_number = @phone AND client_id != @client_id";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@phone", phone),
                        new MySqlParameter("@client_id", excludeClientId.Value)
                    };
                }
                else
                {
                    query = "SELECT COUNT(*) FROM clients WHERE phone_number = @phone";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@phone", phone)
                    };
                }

                var result = DatabaseHelper.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        private bool IsEmailExists(string email, int? excludeClientId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                string query;
                MySqlParameter[] parameters;

                if (excludeClientId.HasValue)
                {
                    query = "SELECT COUNT(*) FROM clients WHERE email = @email AND client_id != @client_id";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@email", email),
                        new MySqlParameter("@client_id", excludeClientId.Value)
                    };
                }
                else
                {
                    query = "SELECT COUNT(*) FROM clients WHERE email = @email";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@email", email)
                    };
                }

                var result = DatabaseHelper.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Проверяем форму еще раз перед сохранением
            ValidateForm();

            if (!_isFormValid)
            {
                MessageBox.Show("Пожалуйста, исправьте ошибки в форме перед сохранением.",
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем очищенные значения
            string fullName = txtFullName.Text.Trim();
            string phone = FormatPhoneNumber(txtPhone.Text.Trim());
            string email = txtEmail.Text.Trim();
            string address = txtAddress.Text.Trim();

            // Убираем placeholder значения если они остались
            if (fullName == "Введите ФИО полностью")
                fullName = "";

            if (email == "example@email.com")
                email = "";

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Введите ФИО клиента.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            // Проверка уникальности телефона
            if (IsPhoneExists(phone, _isEditMode ? _clientId : (int?)null))
            {
                MessageBox.Show("Клиент с таким номером телефона уже существует.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }

            // Проверка уникальности email (если заполнен)
            if (!string.IsNullOrWhiteSpace(email) && IsEmailExists(email, _isEditMode ? _clientId : (int?)null))
            {
                MessageBox.Show("Клиент с таким email адресом уже существует.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            try
            {
                string query;
                MySqlParameter[] parameters;

                if (_isEditMode)
                {
                    query = @"UPDATE clients SET full_name = @full_name, phone_number = @phone, 
                             email = @email, address = @address WHERE client_id = @id";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@full_name", fullName),
                        new MySqlParameter("@phone", phone),
                        new MySqlParameter("@email", string.IsNullOrWhiteSpace(email) ? DBNull.Value : (object)email),
                        new MySqlParameter("@address", string.IsNullOrWhiteSpace(address) ? DBNull.Value : (object)address),
                        new MySqlParameter("@id", _clientId)
                    };
                }
                else
                {
                    query = @"INSERT INTO clients (full_name, phone_number, email, address) 
                             VALUES (@full_name, @phone, @email, @address)";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@full_name", fullName),
                        new MySqlParameter("@phone", phone),
                        new MySqlParameter("@email", string.IsNullOrWhiteSpace(email) ? DBNull.Value : (object)email),
                        new MySqlParameter("@address", string.IsNullOrWhiteSpace(address) ? DBNull.Value : (object)address)
                    };
                }

                int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                if (rowsAffected > 0)
                {
                    MessageBox.Show(_isEditMode ? "Клиент обновлен успешно!" : "Клиент добавлен успешно!",
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
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // Duplicate entry
                {
                    MessageBox.Show("Клиент с таким номером телефона или email уже существует.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatPhoneNumber(string phone)
        {
            // Форматируем телефон в единый формат
            string digitsOnly = Regex.Replace(phone, @"\D", "");

            if (digitsOnly.Length == 11)
            {
                if (digitsOnly.StartsWith("8"))
                    digitsOnly = "7" + digitsOnly.Substring(1);

                return "+" + digitsOnly;
            }

            return phone;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Дополнительные методы для улучшения UX
        private void txtFullName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только буквы, пробел, дефис и управляющие клавиши
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) &&
                e.KeyChar != ' ' && e.KeyChar != '-')
            {
                e.Handled = true;
            }
        }

        private void txtPhone_Enter(object sender, EventArgs e)
        {
            if (txtPhone.Text == "+7")
                txtPhone.SelectionStart = txtPhone.Text.Length;
        }

        private void txtPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только цифры, +, (, ), - и управляющие клавиши
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) &&
                e.KeyChar != '+' && e.KeyChar != '(' && e.KeyChar != ')' &&
                e.KeyChar != '-' && e.KeyChar != ' ')
            {
                e.Handled = true;
            }

            // + можно ввести только в начале
            if (e.KeyChar == '+' && txtPhone.SelectionStart > 0)
            {
                e.Handled = true;
            }
        }

        private void txtPhone_Leave(object sender, EventArgs e)
        {
            // Автоматически добавляем скобки и дефисы для лучшего форматирования
            string phone = txtPhone.Text;
            string digitsOnly = Regex.Replace(phone, @"\D", "");

            if (digitsOnly.Length >= 11 && digitsOnly.StartsWith("7"))
            {
                // Формат: +7(XXX)XXX-XX-XX
                string formatted = $"+7({digitsOnly.Substring(1, 3)}){digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, 2)}-{digitsOnly.Substring(9, 2)}";
                txtPhone.Text = formatted;
            }
            else if (digitsOnly.Length >= 10 && digitsOnly.StartsWith("8"))
            {
                // Формат: +7(XXX)XXX-XX-XX (заменяем 8 на 7)
                string formatted = $"+7({digitsOnly.Substring(1, 3)}){digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, 2)}-{digitsOnly.Substring(9, Math.Min(2, digitsOnly.Length - 9))}";
                txtPhone.Text = formatted;
            }
        }

        // События для очистки placeholder при фокусе
        private void txtFullName_Enter(object sender, EventArgs e)
        {
            if (txtFullName.Text == "Введите ФИО полностью")
            {
                txtFullName.Text = "";
                txtFullName.ForeColor = SystemColors.WindowText;
            }
        }

        private void txtFullName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                txtFullName.Text = "Введите ФИО полностью";
                txtFullName.ForeColor = SystemColors.GrayText;
            }
        }

        private void txtEmail_Enter(object sender, EventArgs e)
        {
            if (txtEmail.Text == "example@email.com")
            {
                txtEmail.Text = "";
                txtEmail.ForeColor = SystemColors.WindowText;
            }
        }

        private void txtEmail_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.Text = "example@email.com";
                txtEmail.ForeColor = SystemColors.GrayText;
            }
        }

        private void AddEditClientForm_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ClientCarsForm carsForm = new ClientCarsForm(_clientId, txtFullName.Text.Trim());
            if (carsForm.ShowDialog() == DialogResult.OK)
            {
                // Можно обновить что-то если нужно
            }
        }
    }
}
