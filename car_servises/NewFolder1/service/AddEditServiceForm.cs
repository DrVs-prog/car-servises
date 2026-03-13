using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Drawing;

namespace car_servises
{
    public partial class AddEditServiceForm : Form
    {
        private int _serviceId;
        private bool _isEditMode;
        private bool _isFormValid = false;
        private ErrorProvider errorProvider = new ErrorProvider();

        public AddEditServiceForm()
        {
            InitializeComponent();
            _isEditMode = false;
            this.Text = "Добавление услуги";
            SetupValidation();
            SetupPlaceholderBehavior();
            ConfigureNumericUpDown();
        }

        public AddEditServiceForm(int serviceId, string serviceName, string description, decimal price)
        {
            InitializeComponent();
            _serviceId = serviceId;
            _isEditMode = true;
            this.Text = "Редактирование услуги";

            txtServiceName.Text = serviceName;
            txtDescription.Text = description;
            numPrice.Value = price;

            SetupValidation();
            ConfigureNumericUpDown();
            ValidateForm();
        }

        private void SetupValidation()
        {
            // Настраиваем валидацию при изменении текста
            txtServiceName.TextChanged += ValidateForm;
            txtDescription.TextChanged += ValidateForm;
            numPrice.ValueChanged += ValidateForm;

            // Устанавливаем максимальные длины для текстовых полей
            txtServiceName.MaxLength = 100;
            txtDescription.MaxLength = 500;

            // Настраиваем TextBox для многострочного описания
            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Height = 100;

            // Настраиваем ErrorProvider
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.ContainerControl = this;
        }

        private void SetupPlaceholderBehavior()
        {
            // Обработчики для эффекта placeholder для названия услуги
            txtServiceName.Enter += (s, e) =>
            {
                if (txtServiceName.Text == "Введите название услуги")
                {
                    txtServiceName.Text = "";
                    txtServiceName.ForeColor = SystemColors.WindowText;
                }
            };

            txtServiceName.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtServiceName.Text))
                {
                    txtServiceName.Text = "Введите название услуги";
                    txtServiceName.ForeColor = SystemColors.GrayText;
                }
            };

            if (string.IsNullOrWhiteSpace(txtServiceName.Text))
            {
                txtServiceName.Text = "Введите название услуги";
                txtServiceName.ForeColor = SystemColors.GrayText;
            }

            // Placeholder для описания
            txtDescription.Enter += (s, e) =>
            {
                if (txtDescription.Text == "Опишите услугу...")
                {
                    txtDescription.Text = "";
                    txtDescription.ForeColor = SystemColors.WindowText;
                }
            };

            txtDescription.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtDescription.Text))
                {
                    txtDescription.Text = "Опишите услугу...";
                    txtDescription.ForeColor = SystemColors.GrayText;
                }
            };

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                txtDescription.Text = "Опишите услугу...";
                txtDescription.ForeColor = SystemColors.GrayText;
            }
        }

        private void ConfigureNumericUpDown()
        {
            // Настраиваем NumericUpDown для цены
            numPrice.DecimalPlaces = 2;
            numPrice.Minimum = 0;
            numPrice.Maximum = 999999.99m;
            numPrice.Increment = 100;

            // Форматируем отображение цены
            numPrice.ThousandsSeparator = true;

            // Добавляем символ валюты
            Label lblCurrency = new Label();
            lblCurrency.Text = "₽";
            lblCurrency.Font = numPrice.Font;
            lblCurrency.ForeColor = SystemColors.GrayText;
            lblCurrency.AutoSize = true;
            lblCurrency.Location = new Point(numPrice.Right + 5, numPrice.Top + 3);
            this.Controls.Add(lblCurrency);
            lblCurrency.BringToFront();
        }

        private void ValidateForm(object sender = null, EventArgs e = null)
        {
            bool isValid = true;

            // Очищаем предыдущие ошибки
            errorProvider.Clear();

            // Валидация названия услуги
            if (string.IsNullOrWhiteSpace(txtServiceName.Text) ||
                txtServiceName.Text == "Введите название услуги")
            {
                errorProvider.SetError(txtServiceName, "Название услуги обязательно для заполнения");
                isValid = false;
                SetErrorStyle(txtServiceName, true);
            }
            else if (txtServiceName.Text.Trim().Length < 3)
            {
                errorProvider.SetError(txtServiceName, "Название услуги должно содержать не менее 3 символов");
                isValid = false;
                SetErrorStyle(txtServiceName, true);
            }
            else if (txtServiceName.Text.Trim().Length > 100)
            {
                errorProvider.SetError(txtServiceName, "Название услуги не должно превышать 100 символов");
                isValid = false;
                SetErrorStyle(txtServiceName, true);
            }
            else if (!IsValidServiceName(txtServiceName.Text.Trim()))
            {
                errorProvider.SetError(txtServiceName, "Название услуги содержит недопустимые символы");
                isValid = false;
                SetErrorStyle(txtServiceName, true);
            }
            else
            {
                SetErrorStyle(txtServiceName, false);
            }

            // Валидация описания (не обязательно, но проверяем если заполнено)
            if (!string.IsNullOrWhiteSpace(txtDescription.Text) &&
                txtDescription.Text != "Опишите услугу...")
            {
                if (txtDescription.Text.Trim().Length > 500)
                {
                    errorProvider.SetError(txtDescription, "Описание не должно превышать 500 символов");
                    isValid = false;
                    SetErrorStyle(txtDescription, true);
                }
                else if (ContainsInvalidSymbols(txtDescription.Text.Trim()))
                {
                    errorProvider.SetError(txtDescription, "Описание содержит недопустимые символы");
                    isValid = false;
                    SetErrorStyle(txtDescription, true);
                }
                else
                {
                    SetErrorStyle(txtDescription, false);
                }
            }
            else
            {
                SetErrorStyle(txtDescription, false);
            }

            // Валидация цены
            if (numPrice.Value <= 0)
            {
                errorProvider.SetError(numPrice, "Цена должна быть больше 0");
                isValid = false;
                SetErrorStyle(numPrice, true);
            }
            else if (numPrice.Value > 999999.99m)
            {
                errorProvider.SetError(numPrice, "Цена не должна превышать 999 999,99");
                isValid = false;
                SetErrorStyle(numPrice, true);
            }
            else if (numPrice.Value % 1 != 0 && numPrice.Value * 100 % 1 != 0)
            {
                // Проверка на корректность копеек (не более 2 знаков после запятой)
                errorProvider.SetError(numPrice, "Укажите цену с точностью до копеек (макс. 2 знака после запятой)");
                isValid = false;
                SetErrorStyle(numPrice, true);
            }
            else
            {
                SetErrorStyle(numPrice, false);
            }

            // Проверка уникальности названия услуги (только для новой услуги)
            if (!_isEditMode && isValid && !string.IsNullOrWhiteSpace(txtServiceName.Text.Trim()) &&
                txtServiceName.Text.Trim() != "Введите название услуги")
            {
                if (IsServiceNameExists(txtServiceName.Text.Trim()))
                {
                    errorProvider.SetError(txtServiceName, "Услуга с таким названием уже существует");
                    isValid = false;
                    SetErrorStyle(txtServiceName, true);
                }
            }

            _isFormValid = isValid;
            btnSave.Enabled = isValid;

            // Обновляем статус формы в заголовке
            UpdateFormTitle();

            // Обновляем счетчик символов для описания
            UpdateCharacterCount();
        }

        private void UpdateCharacterCount()
        {
            int currentLength = txtDescription.Text.Length;
            int maxLength = txtDescription.MaxLength;

            // Создаем или обновляем label для счетчика символов
            Label lblCharCount = null;
            foreach (Control control in this.Controls)
            {
                if (control.Name == "lblCharCount")
                {
                    lblCharCount = (Label)control;
                    break;
                }
            }

            if (lblCharCount == null)
            {
                lblCharCount = new Label();
                lblCharCount.Name = "lblCharCount";
                lblCharCount.Font = new Font(this.Font.FontFamily, 8);
                lblCharCount.ForeColor = SystemColors.GrayText;
                lblCharCount.AutoSize = true;
                lblCharCount.Location = new Point(txtDescription.Right - 50, txtDescription.Bottom + 5);
                this.Controls.Add(lblCharCount);
                lblCharCount.BringToFront();
            }

            lblCharCount.Text = $"{currentLength}/{maxLength}";
            lblCharCount.ForeColor = currentLength > maxLength * 0.9 ? Color.Red :
                                    currentLength > maxLength * 0.75 ? Color.Orange :
                                    SystemColors.GrayText;
        }

        private void UpdateFormTitle()
        {
            string baseTitle = _isEditMode ? "Редактирование услуги" : "Добавление услуги";
            this.Text = baseTitle + (_isFormValid ? " ✓" : "");
        }

        private void SetErrorStyle(Control control, bool hasError)
        {
            if (hasError)
            {
                control.BackColor = Color.FromArgb(255, 240, 240);

                if (control is TextBox)
                {
                    control.ForeColor = Color.DarkRed;
                }
                else if (control is NumericUpDown)
                {
                    // Для NumericUpDown меняем только цвет фона
                    control.BackColor = Color.FromArgb(255, 240, 240);
                }
            }
            else
            {
                control.BackColor = SystemColors.Window;

                if (control is TextBox)
                {
                    // Возвращаем цвет текста в зависимости от содержимого
                    if (control == txtServiceName && control.Text == "Введите название услуги")
                    {
                        control.ForeColor = SystemColors.GrayText;
                    }
                    else if (control == txtDescription && control.Text == "Опишите услугу...")
                    {
                        control.ForeColor = SystemColors.GrayText;
                    }
                    else
                    {
                        control.ForeColor = SystemColors.WindowText;
                    }
                }
                else if (control is NumericUpDown)
                {
                    control.BackColor = SystemColors.Window;
                }
            }
        }

        private bool IsValidServiceName(string serviceName)
        {
            // Проверяем, что название услуги содержит допустимые символы
            // Разрешаем буквы, цифры, пробелы, дефисы, скобки, запятые и точки
            return Regex.IsMatch(serviceName, @"^[а-яА-ЯёЁa-zA-Z0-9\s\-\(\)\.,:;!?]+$");
        }

        private bool ContainsInvalidSymbols(string text)
        {
            // Проверяем на наличие потенциально опасных символов
            return Regex.IsMatch(text, @"[<>$#@*^`|\\/]");
        }

        private bool IsServiceNameExists(string serviceName)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM services WHERE service_name = @service_name";
                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@service_name", serviceName)
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

            // Получаем очищенные значения
            string serviceName = txtServiceName.Text.Trim();
            string description = txtDescription.Text.Trim();
            decimal price = numPrice.Value;

            // Убираем placeholder значения если они остались
            if (serviceName == "Введите название услуги")
                serviceName = "";

            if (description == "Опишите услугу...")
                description = "";

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                MessageBox.Show("Введите название услуги.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServiceName.Focus();
                return;
            }

            if (price <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numPrice.Focus();
                return;
            }

            // Проверка уникальности названия услуги для новой услуги
            if (!_isEditMode && IsServiceNameExists(serviceName))
            {
                MessageBox.Show("Услуга с таким названием уже существует. Выберите другое название.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServiceName.Focus();
                return;
            }

            try
            {
                string query;
                MySqlParameter[] parameters;

                if (_isEditMode)
                {
                    query = @"UPDATE services SET service_name = @service_name, description = @description, 
                             price = @price WHERE service_id = @id";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@service_name", serviceName),
                        new MySqlParameter("@description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : (object)description),
                        new MySqlParameter("@price", price),
                        new MySqlParameter("@id", _serviceId)
                    };
                }
                else
                {
                    query = @"INSERT INTO services (service_name, description, price) 
                             VALUES (@service_name, @description, @price)";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@service_name", serviceName),
                        new MySqlParameter("@description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : (object)description),
                        new MySqlParameter("@price", price)
                    };
                }

                int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                if (rowsAffected > 0)
                {
                    MessageBox.Show(_isEditMode ? "Услуга обновлена успешно!" : "Услуга добавлена успешно!",
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
                    MessageBox.Show("Услуга с таким названием уже существует.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtServiceName.Focus();
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Дополнительные методы для улучшения UX
        private void txtServiceName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем буквы, цифры, пробелы, дефисы, скобки, запятые, точки и управляющие клавиши
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) &&
                e.KeyChar != ' ' && e.KeyChar != '-' && e.KeyChar != '(' && e.KeyChar != ')' &&
                e.KeyChar != ',' && e.KeyChar != '.' && e.KeyChar != ':' && e.KeyChar != ';' &&
                e.KeyChar != '!' && e.KeyChar != '?')
            {
                e.Handled = true;
            }
        }

        private void txtDescription_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Блокируем потенциально опасные символы
            if (e.KeyChar == '<' || e.KeyChar == '>' || e.KeyChar == '$' ||
                e.KeyChar == '#' || e.KeyChar == '@' || e.KeyChar == '*' ||
                e.KeyChar == '^' || e.KeyChar == '`' || e.KeyChar == '|' ||
                e.KeyChar == '\\' || e.KeyChar == '/')
            {
                e.Handled = true;
            }
        }

        private void numPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только цифры, запятую (для десятичных) и управляющие клавиши
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && e.KeyChar != ',')
            {
                e.Handled = true;
            }

            // Запятую можно вводить только одну
            if (e.KeyChar == ',' && numPrice.Text.Contains(","))
            {
                e.Handled = true;
            }
        }

        private void numPrice_Enter(object sender, EventArgs e)
        {
            // Выделяем все содержимое при фокусе для удобного редактирования
            numPrice.Select(0, numPrice.Text.Length);
        }

        // События для очистки placeholder при фокусе
        private void txtServiceName_Enter(object sender, EventArgs e)
        {
            if (txtServiceName.Text == "Введите название услуги")
            {
                txtServiceName.Text = "";
                txtServiceName.ForeColor = SystemColors.WindowText;
            }
        }

        private void txtServiceName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServiceName.Text))
            {
                txtServiceName.Text = "Введите название услуги";
                txtServiceName.ForeColor = SystemColors.GrayText;
            }
        }

        private void txtDescription_Enter(object sender, EventArgs e)
        {
            if (txtDescription.Text == "Опишите услугу...")
            {
                txtDescription.Text = "";
                txtDescription.ForeColor = SystemColors.WindowText;
            }
        }

        private void txtDescription_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                txtDescription.Text = "Опишите услугу...";
                txtDescription.ForeColor = SystemColors.GrayText;
            }
        }

        // Автоматический рост TextBox для описания при вводе многострочного текста
        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            // Автоподстройка высоты только если текст занимает больше одной строки
            if (txtDescription.Text.Length > 0 && txtDescription.Lines.Length > 1)
            {
                int lineCount = Math.Min(txtDescription.Lines.Length, 10); // Максимум 10 строк
                txtDescription.Height = txtDescription.Font.Height * lineCount + 10;

                // Обновляем положение кнопок
                btnSave.Top = txtDescription.Bottom + 20;
                btnCancel.Top = txtDescription.Bottom + 20;
            }
        }

        private void AddEditServiceForm_Load(object sender, EventArgs e)
        {

        }
    }
}