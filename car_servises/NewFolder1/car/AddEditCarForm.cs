using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Data;

namespace car_servises
{
    public partial class AddEditCarForm : Form
    {
        private int _carId;
        private int _clientId;
        private bool _isEditMode;
        private bool _isFormValid = false;
        private ErrorProvider errorProvider = new ErrorProvider();

        public AddEditCarForm(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
            _isEditMode = false;
            this.Text = "Добавление автомобиля";
            SetupValidation();
            SetupPlaceholders();
            LoadBrands();
        }

        private void SetupPlaceholders()
        {
            SetupTextBoxPlaceholder(txtBrand, "Например: Toyota");
            SetupTextBoxPlaceholder(txtModel, "Например: Camry");
            SetupTextBoxPlaceholder(txtVIN, "17 символов (без I, O, Q)");
            SetupTextBoxPlaceholder(txtLicensePlate, "Например: А123ВС777");
            // ... для других TextBox
        }

        private void SetupTextBoxPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Enter += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = SystemColors.WindowText;
                }
            };

            textBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = SystemColors.GrayText;
                }
            };

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = placeholder;
                textBox.ForeColor = SystemColors.GrayText;
            }
        }

        public AddEditCarForm(int carId, int clientId, string brand, string model,
                              int year, string vin, string licensePlate)
        {
            InitializeComponent();
            _carId = carId;
            _clientId = clientId;
            _isEditMode = true;
            this.Text = "Редактирование автомобиля";

            txtBrand.Text = brand;
            txtModel.Text = model;
            numYear.Value = year;
            txtVIN.Text = vin;
            txtLicensePlate.Text = licensePlate;

            SetupValidation();
            LoadBrands();
            ValidateForm();
        }

        private void SetupValidation()
        {
            txtBrand.TextChanged += ValidateForm;
            txtModel.TextChanged += ValidateForm;
            txtLicensePlate.TextChanged += ValidateForm;
            txtVIN.TextChanged += ValidateForm;
            numYear.ValueChanged += ValidateForm;

            txtBrand.MaxLength = 50;
            txtModel.MaxLength = 50;
            txtVIN.MaxLength = 17;
            txtLicensePlate.MaxLength = 20;

            // Настраиваем ErrorProvider
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.ContainerControl = this;
        }

        private void LoadBrands()
        {
            try
            {
                // Можно загрузить популярные марки из базы или использовать статический список
                txtBrand.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txtBrand.AutoCompleteSource = AutoCompleteSource.CustomSource;

                AutoCompleteStringCollection brands = new AutoCompleteStringCollection();
                brands.AddRange(new string[] {
                    "Toyota", "Lexus", "Honda", "BMW", "Mercedes-Benz", "Audi",
                    "Volkswagen", "Skoda", "Hyundai", "Kia", "Nissan", "Ford",
                    "Renault", "Lada", "Chevrolet", "Mazda", "Subaru", "Volvo",
                    "Mitsubishi", "Land Rover", "Jeep", "Peugeot", "Citroen",
                    "Opel", "Infiniti", "Acura", "Cadillac", "Chrysler", "Dodge",
                    "Fiat", "Genesis", "Jaguar", "Mini", "Porsche", "Saab",
                    "Suzuki", "Tesla", "Alfa Romeo", "Aston Martin", "Bentley",
                    "Bugatti", "Ferrari", "Lamborghini", "Maserati", "McLaren",
                    "Rolls-Royce"
                });

                txtBrand.AutoCompleteCustomSource = brands;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки марок: {ex.Message}");
            }
        }

        private void ValidateForm(object sender = null, EventArgs e = null)
        {
            bool isValid = true;
            errorProvider.Clear();

            // Валидация марки
            if (string.IsNullOrWhiteSpace(txtBrand.Text))
            {
                errorProvider.SetError(txtBrand, "Марка автомобиля обязательна");
                isValid = false;
                SetErrorStyle(txtBrand, true);
            }
            else if (txtBrand.Text.Trim().Length < 2)
            {
                errorProvider.SetError(txtBrand, "Марка должна содержать не менее 2 символов");
                isValid = false;
                SetErrorStyle(txtBrand, true);
            }
            else
            {
                SetErrorStyle(txtBrand, false);
            }

            // Валидация модели
            if (string.IsNullOrWhiteSpace(txtModel.Text))
            {
                errorProvider.SetError(txtModel, "Модель автомобиля обязательна");
                isValid = false;
                SetErrorStyle(txtModel, true);
            }
            else if (txtModel.Text.Trim().Length < 1)
            {
                errorProvider.SetError(txtModel, "Модель должна содержать не менее 1 символа");
                isValid = false;
                SetErrorStyle(txtModel, true);
            }
            else
            {
                SetErrorStyle(txtModel, false);
            }

            // Валидация года выпуска
            int currentYear = DateTime.Now.Year;
            if (numYear.Value < 1900 || numYear.Value > currentYear + 1)
            {
                errorProvider.SetError(numYear, $"Год должен быть от 1900 до {currentYear + 1}");
                isValid = false;
                SetErrorStyle(numYear, true);
            }
            else
            {
                SetErrorStyle(numYear, false);
            }

            // Валидация госномера
            if (string.IsNullOrWhiteSpace(txtLicensePlate.Text))
            {
                errorProvider.SetError(txtLicensePlate, "Госномер обязателен");
                isValid = false;
                SetErrorStyle(txtLicensePlate, true);
            }
            else if (!IsValidLicensePlate(txtLicensePlate.Text.Trim()))
            {
                errorProvider.SetError(txtLicensePlate, "Неверный формат госномера (пример: A123BC777)");
                isValid = false;
                SetErrorStyle(txtLicensePlate, true);
            }
            else
            {
                SetErrorStyle(txtLicensePlate, false);
            }

            // Валидация VIN (не обязателен, но если заполнен - проверяем)
            if (!string.IsNullOrWhiteSpace(txtVIN.Text))
            {
                if (txtVIN.Text.Trim().Length != 17)
                {
                    errorProvider.SetError(txtVIN, "VIN должен содержать ровно 17 символов");
                    isValid = false;
                    SetErrorStyle(txtVIN, true);
                }
                else if (ContainsInvalidVINCharacters(txtVIN.Text.Trim()))
                {
                    errorProvider.SetError(txtVIN, "VIN содержит недопустимые символы (I, O, Q не допускаются)");
                    isValid = false;
                    SetErrorStyle(txtVIN, true);
                }
                else
                {
                    SetErrorStyle(txtVIN, false);
                }
            }
            else
            {
                SetErrorStyle(txtVIN, false);
            }

            // Проверка уникальности госномера
            if (!_isEditMode && isValid && !string.IsNullOrWhiteSpace(txtLicensePlate.Text.Trim()))
            {
                if (IsLicensePlateExists(txtLicensePlate.Text.Trim()))
                {
                    errorProvider.SetError(txtLicensePlate, "Автомобиль с таким госномером уже существует");
                    isValid = false;
                    SetErrorStyle(txtLicensePlate, true);
                }
            }

            _isFormValid = isValid;
            btnSave.Enabled = isValid;
        }

        private bool IsValidLicensePlate(string plate)
        {
            // Проверка российских госномеров (пример: A123BC777)
            return Regex.IsMatch(plate.ToUpper(), @"^[АВЕКМНОРСТУХ]\d{3}[АВЕКМНОРСТУХ]{2}\d{2,3}$");
        }

        private bool ContainsInvalidVINCharacters(string vin)
        {
            // VIN не должен содержать буквы I, O, Q
            return Regex.IsMatch(vin.ToUpper(), @"[IOQ]");
        }

        private bool IsLicensePlateExists(string licensePlate)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM cars WHERE registration_number = @license_plate";
                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@license_plate", licensePlate)
                };

                var result = DatabaseHelper.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
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
            }
            else
            {
                control.BackColor = SystemColors.Window;
                if (control is TextBox)
                {
                    control.ForeColor = SystemColors.WindowText;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ValidateForm();

            if (!_isFormValid)
            {
                MessageBox.Show("Пожалуйста, исправьте ошибки в форме перед сохранением.",
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string brand = txtBrand.Text.Trim();
            string model = txtModel.Text.Trim();
            int year = (int)numYear.Value;
            string vin = string.IsNullOrWhiteSpace(txtVIN.Text) ? null : txtVIN.Text.Trim();
            string licensePlate = txtLicensePlate.Text.Trim().ToUpper();

            try
            {
                string query;
                MySqlParameter[] parameters;

                if (_isEditMode)
                {
                    query = @"UPDATE cars SET brand = @brand, model = @model, year = @year, 
                             vin = @vin, registration_number = @license_plate 
                             WHERE car_id = @car_id";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@brand", brand),
                        new MySqlParameter("@model", model),
                        new MySqlParameter("@year", year),
                        new MySqlParameter("@vin", string.IsNullOrEmpty(vin) ? DBNull.Value : (object)vin),
                        new MySqlParameter("@license_plate", licensePlate),
                        new MySqlParameter("@car_id", _carId)
                    };
                }
                else
                {
                    query = @"INSERT INTO cars (client_id, brand, model, year, vin, registration_number) 
                             VALUES (@client_id, @brand, @model, @year, @vin, @license_plate)";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@client_id", _clientId),
                        new MySqlParameter("@brand", brand),
                        new MySqlParameter("@model", model),
                        new MySqlParameter("@year", year),
                        new MySqlParameter("@vin", string.IsNullOrEmpty(vin) ? DBNull.Value : (object)vin),
                        new MySqlParameter("@license_plate", licensePlate)
                    };
                }

                int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
                if (rowsAffected > 0)
                {
                    MessageBox.Show(_isEditMode ? "Автомобиль обновлен!" : "Автомобиль добавлен!",
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
                    MessageBox.Show("Автомобиль с таким госномером уже существует.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtLicensePlate.Focus();
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

        // Дополнительные методы для UX
        private void txtLicensePlate_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Преобразуем русские буквы в верхний регистр автоматически
            if (char.IsLetter(e.KeyChar) && char.IsLower(e.KeyChar))
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }

            // Разрешаем буквы, цифры и управляющие клавиши
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtVIN_KeyPress(object sender, KeyPressEventArgs e)
        {
            // VIN только заглавные буквы и цифры
            if (char.IsLetter(e.KeyChar))
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }

            // Блокируем I, O, Q
            if (e.KeyChar == 'I' || e.KeyChar == 'O' || e.KeyChar == 'Q' ||
                e.KeyChar == 'i' || e.KeyChar == 'o' || e.KeyChar == 'q')
            {
                e.Handled = true;
            }
        }

        private void AddEditCarForm_Load(object sender, EventArgs e)
        {

        }

        private void AddEditCarForm_Load_1(object sender, EventArgs e)
        {

        }
    }
}
