using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace car_servises
{
    public partial class Form1 : Form
    {


        private bool isPasswordVisible = false;
        private int failedAttempts = 0;
        private bool showCaptcha = false;
        private CaptchaGenerator captchaGenerator;
        private Timer lockTimer;
        private int lockSecondsRemaining = 0;

        // Элементы управления
        private PictureBox pictureBoxCaptcha;
        private TextBox textBoxCaptcha;
        private Button btnRefreshCaptcha;
        private Label lblTimer;
        public Form1()
        {
            InitializeComponent();
            SetupPasswordVisibility();
            ApplyStyles();

            InitializeCaptchaControls();
            InitializeLockTimer();

            // Проверка подключения к БД при запуске
            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте настройки подключения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeCaptchaControls()
        {
            // Инициализация генератора CAPTCHA
            captchaGenerator = new CaptchaGenerator();

            // PictureBox для CAPTCHA
            pictureBoxCaptcha = new PictureBox();
            pictureBoxCaptcha.Location = new Point(50, 170);
            pictureBoxCaptcha.Size = new Size(200, 60);
            pictureBoxCaptcha.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxCaptcha.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxCaptcha.Visible = false;
            this.Controls.Add(pictureBoxCaptcha);

            // TextBox для ввода CAPTCHA
            textBoxCaptcha = new TextBox();
            textBoxCaptcha.Location = new Point(50, 240);
            textBoxCaptcha.Size = new Size(200, 30);
            textBoxCaptcha.Font = AppStyles.NormalFont;
            textBoxCaptcha.Visible = false;
            textBoxCaptcha.MaxLength = 4;
            textBoxCaptcha.CharacterCasing = CharacterCasing.Upper;
            this.Controls.Add(textBoxCaptcha);

            // Кнопка обновления CAPTCHA
            btnRefreshCaptcha = new Button();
            btnRefreshCaptcha.Location = new Point(260, 170);
            btnRefreshCaptcha.Size = new Size(90, 60);
            btnRefreshCaptcha.Text = "Обновить";
            btnRefreshCaptcha.Font = AppStyles.NormalFont;
            btnRefreshCaptcha.Visible = false;
            btnRefreshCaptcha.Click += btnRefreshCaptcha_Click;
            AppStyles.ApplyButtonStyle(btnRefreshCaptcha);
            this.Controls.Add(btnRefreshCaptcha);

            // Метка для таймера
            lblTimer = new Label();
            lblTimer.Location = new Point(50, 310);
            lblTimer.Size = new Size(300, 30);
            lblTimer.Font = AppStyles.NormalFont;
            lblTimer.ForeColor = Color.Red;
            lblTimer.TextAlign = ContentAlignment.MiddleCenter;
            lblTimer.Visible = false;
            this.Controls.Add(lblTimer);
        }

        private void ShowCaptchaControls(bool show)
        {
            showCaptcha = show;
            pictureBoxCaptcha.Visible = show;
            textBoxCaptcha.Visible = show;
            btnRefreshCaptcha.Visible = show;

            if (show)
            {
                GenerateNewCaptcha();
                // Сдвигаем кнопки ниже
                button1.Location = new Point(50, 340);
                button2.Location = new Point(50, 390);
                // Увеличиваем размер формы
                this.Size = new Size(400, 500);
            }
            else
            {
                // Возвращаем кнопки на место
                button1.Location = new Point(50, 200);
                button2.Location = new Point(50, 250);
                this.Size = new Size(400, 350);
            }
        }

        private void GenerateNewCaptcha()
        {
            captchaGenerator.RefreshCaptcha();
            Bitmap captchaImage = captchaGenerator.CreateCaptchaImage(200, 60);
            pictureBoxCaptcha.Image = captchaImage;
            textBoxCaptcha.Clear();
            textBoxCaptcha.Focus();
        }

        private void btnRefreshCaptcha_Click(object sender, EventArgs e)
        {
            if (lockSecondsRemaining <= 0)
            {
                GenerateNewCaptcha();
            }
        }


        private void ApplyStyles()
        {
            // Стиль формы
            this.BackColor = AppStyles.BackgroundColor;
            this.Font = AppStyles.NormalFont;
            this.Padding = new Padding(20);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(400, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Заголовок
            Label lblTitle = new Label();
            lblTitle.Text = "АВТОСЕРВИС";
            lblTitle.Font = AppStyles.HeaderFont;
            lblTitle.ForeColor = AppStyles.PrimaryColor;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(120, 20);
            this.Controls.Add(lblTitle);

            // Метки для полей ввода
            Label lblLogin = new Label();
            lblLogin.Text = "Логин:";
            lblLogin.Font = AppStyles.NormalFont;
            lblLogin.ForeColor = AppStyles.DarkColor;
            lblLogin.AutoSize = true;
            lblLogin.Location = new Point(50, 60);
            this.Controls.Add(lblLogin);

            Label lblPassword = new Label();
            lblPassword.Text = "Пароль:";
            lblPassword.Font = AppStyles.NormalFont;
            lblPassword.ForeColor = AppStyles.DarkColor;
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(50, 110);
            this.Controls.Add(lblPassword);

            // Стиль для полей ввода
            AppStyles.ApplyTextBoxStyle(textBox1);
            AppStyles.ApplyTextBoxStyle(textBox2);

            // Позиционирование
            textBox1.Location = new Point(50, 80);
            textBox1.Size = new Size(250, 30);

            textBox2.Location = new Point(50, 130);
            textBox2.Size = new Size(250, 30);

            // Стиль для кнопок
            AppStyles.ApplyButtonStyle(button1, true);
            AppStyles.ApplyButtonStyle(button2);

            button1.Location = new Point(50, 200);
            button1.Size = new Size(300, 35);
            button1.Text = "ВОЙТИ";

            button2.Location = new Point(50, 250);
            button2.Size = new Size(300, 35);
            button2.Text = "ВЫХОД";

            // Кнопка показа пароля
            AppStyles.ApplyButtonStyle(btnTogglePassword);
            btnTogglePassword.Location = new Point(56, 156);
            btnTogglePassword.Size = new Size(40, 40);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Устанавливаем фокус на поле логина
            textBox1.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text;

            // Проверка на пустые значения
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }

            // Хеширование пароля
            string hashedPassword = HashPassword(password);

            // Проверка пользователя в базе данных
            if (AuthenticateUser(username, hashedPassword))
            {
                string role = GetRole(username);
                MessageBox.Show($"Авторизация успешна! Ваша роль: {role}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Открыть нужную форму в зависимости от роли (передаем username)
                OpenRoleForm(role, username);
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox2.Clear();
                textBox1.Focus();
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool AuthenticateUser(string username, string hashedPassword)
        {
            string query = "SELECT COUNT(*) FROM employees WHERE login = @username AND password_hash = @hashedPassword";

            MySqlParameter[] parameters = {
                new MySqlParameter("@username", username),
                new MySqlParameter("@hashedPassword", hashedPassword)
            };

            object result = DatabaseHelper.ExecuteScalar(query, parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        private string GetRole(string username)
        {
            string query = "SELECT job_title FROM employees WHERE login = @username";

            MySqlParameter[] parameters = {
                new MySqlParameter("@username", username)
            };

            object result = DatabaseHelper.ExecuteScalar(query, parameters);
            return result?.ToString() ?? "Неизвестная роль";
        }

        private void OpenRoleForm(string role, string username)
        {
            // получаем ФИО
            string fullName = GetUserFullName(username);

            // ✅ ЗАПОЛНЯЕМ ГЛОБАЛЬНУЮ СЕССИЮ
            CurrentUser.Role = role;
            CurrentUser.FullName = fullName;
            CurrentUser.Login = username;

            switch (role)
            {
                case "Администратор":
                    new AdminForm().Show();
                    break;

                case "Менеджер":
                    new ManagerForm().Show();
                    break;

                case "Механик":
                    new MechanicForm().Show();
                    break;
            }

            this.Hide();
        }


        private string GetUserFullName(string username)
        {
            string query = "SELECT full_name FROM employees WHERE login = @username";

            MySqlParameter[] parameters = {
        new MySqlParameter("@username", username)
    };

            object result = DatabaseHelper.ExecuteScalar(query, parameters);
            return result?.ToString() ?? "Неизвестный пользователь";
        }


        private void button2_Click(object sender, EventArgs e)
        {
            // Подтверждение выхода
            DialogResult result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        // Обработчики для удобства использования
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                textBox2.Focus();
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                button1_Click(sender, e);
                e.Handled = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Можно добавить валидацию логина
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // Можно добавить валидацию пароля
        }

        private void SetupPasswordVisibility()
        {
            // Настройка поля пароля
            textBox2.UseSystemPasswordChar = true; // Пароль скрыт звездочками

            // Настройка кнопки показа пароля
            btnTogglePassword.Text = "👁"; // Или установите изображение
            btnTogglePassword.BackColor = Color.Transparent;
            btnTogglePassword.FlatStyle = FlatStyle.Flat;
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            btnTogglePassword.Cursor = Cursors.Hand;
        }
        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            TogglePasswordVisibility();
        }

        private void TogglePasswordVisibility()
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                // Показываем пароль
                textBox2.UseSystemPasswordChar = false;
                btnTogglePassword.Text = "🔒"; // Иконка "скрыть"
                btnTogglePassword.BackColor = Color.LightBlue;
            }
            else
            {
                // Скрываем пароль
                textBox2.UseSystemPasswordChar = true;
                btnTogglePassword.Text = "👁"; // Иконка "показать"
                btnTogglePassword.BackColor = Color.Transparent;
            }

            // Фокус остается в поле пароля
            textBox2.Focus();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
