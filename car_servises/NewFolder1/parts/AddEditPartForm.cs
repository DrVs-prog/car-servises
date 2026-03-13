using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;

namespace car_servises
{
    public partial class AddEditPartForm : Form
    {
        private int _partId;
        private bool _isEditMode;
        private bool _isFormValid = false;
        private ErrorProvider errorProvider = new ErrorProvider();

        // Поля для работы с фото
        private Button btnSelectImage;
        private Button btnViewImage;
        private PictureBox pictureBoxPreview;
        private Label lblImageStatus;
        private byte[] _imageData;
        private bool _imageChanged = false;
        private GroupBox gbPhoto;

        public AddEditPartForm()
        {
            InitializeComponent();
            _isEditMode = false;
            this.Text = "Добавление запчасти";
            SetupValidation();
            SetupPlaceholderBehavior();
            ConfigureNumericControls();
            InitializePhotoComponents();
            this.Size = new Size(500, 400);
        }

        public AddEditPartForm(int partId, string partName, string manufacturer, decimal cost, int stockQty)
        {
            InitializeComponent();
            _partId = partId;
            _isEditMode = true;
            this.Text = "Редактирование запчасти";

            txtPartName.Text = partName;
            txtManufacturer.Text = manufacturer;
            numCost.Value = cost;
            numStockQty.Value = stockQty;

            SetupValidation();
            ConfigureNumericControls();
            InitializePhotoComponents();
            ValidateForm();
            this.Size = new Size(500, 400);
        }

        private void SetupValidation()
        {
            txtPartName.TextChanged += ValidateForm;
            txtManufacturer.TextChanged += ValidateForm;
            numCost.ValueChanged += ValidateForm;
            numStockQty.ValueChanged += ValidateForm;

            txtPartName.MaxLength = 100;
            txtManufacturer.MaxLength = 50;

            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.ContainerControl = this;
        }

        private void SetupPlaceholderBehavior()
        {
            // Placeholder для названия запчасти
            txtPartName.Enter += (s, e) =>
            {
                if (txtPartName.Text == "Например: Тормозной диск")
                {
                    txtPartName.Text = "";
                    txtPartName.ForeColor = SystemColors.WindowText;
                }
            };

            txtPartName.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtPartName.Text))
                {
                    txtPartName.Text = "Например: Тормозной диск";
                    txtPartName.ForeColor = SystemColors.GrayText;
                }
            };

            if (string.IsNullOrWhiteSpace(txtPartName.Text))
            {
                txtPartName.Text = "Например: Тормозной диск";
                txtPartName.ForeColor = SystemColors.GrayText;
            }

            // Placeholder для производителя
            txtManufacturer.Enter += (s, e) =>
            {
                if (txtManufacturer.Text == "Например: Bosch")
                {
                    txtManufacturer.Text = "";
                    txtManufacturer.ForeColor = SystemColors.WindowText;
                }
            };

            txtManufacturer.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtManufacturer.Text))
                {
                    txtManufacturer.Text = "Например: Bosch";
                    txtManufacturer.ForeColor = SystemColors.GrayText;
                }
            };

            if (string.IsNullOrWhiteSpace(txtManufacturer.Text))
            {
                txtManufacturer.Text = "Например: Bosch";
                txtManufacturer.ForeColor = SystemColors.GrayText;
            }
        }

        private void ConfigureNumericControls()
        {
            numCost.DecimalPlaces = 2;
            numCost.Minimum = 0;
            numCost.Maximum = 999999.99m;
            numCost.Increment = 100;
            numCost.ThousandsSeparator = true;

            Label lblCurrency = new Label();
            lblCurrency.Text = "₽";
            lblCurrency.Font = numCost.Font;
            lblCurrency.ForeColor = SystemColors.GrayText;
            lblCurrency.AutoSize = true;
            lblCurrency.Location = new Point(numCost.Right + 5, numCost.Top + 3);
            this.Controls.Add(lblCurrency);
            lblCurrency.BringToFront();

            numStockQty.Minimum = 0;
            numStockQty.Maximum = 10000;
            numStockQty.Increment = 1;
            numStockQty.ThousandsSeparator = true;

            Label lblUnits = new Label();
            lblUnits.Text = "шт.";
            lblUnits.Font = numStockQty.Font;
            lblUnits.ForeColor = SystemColors.GrayText;
            lblUnits.AutoSize = true;
            lblUnits.Location = new Point(numStockQty.Right + 5, numStockQty.Top + 3);
            this.Controls.Add(lblUnits);
            lblUnits.BringToFront();

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(numCost, "Стоимость одной единицы запчасти");
            toolTip.SetToolTip(numStockQty, "Количество на складе (0 означает отсутствие)");
            toolTip.SetToolTip(txtPartName, "Укажите полное название запчасти");
            toolTip.SetToolTip(txtManufacturer, "Производитель или бренд запчасти");
        }

        private void InitializePhotoComponents()
        {
            gbPhoto = new GroupBox();
            gbPhoto.Text = "Фотография запчасти";
            gbPhoto.Location = new Point(12, 135);
            gbPhoto.Size = new Size(460, 150);
            gbPhoto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            pictureBoxPreview = new PictureBox();
            pictureBoxPreview.Location = new Point(10, 20);
            pictureBoxPreview.Size = new Size(110, 110);
            pictureBoxPreview.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxPreview.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxPreview.BackColor = Color.FromArgb(240, 240, 240);

            // Убираем Label "Нет фото" - теперь будем показывать заглушку

            btnSelectImage = new Button();
            btnSelectImage.Text = "Загрузить фото...";
            btnSelectImage.Location = new Point(130, 30);
            btnSelectImage.Size = new Size(150, 30);
            btnSelectImage.UseVisualStyleBackColor = true;
            btnSelectImage.Click += BtnSelectImage_Click;

            btnViewImage = new Button();
            btnViewImage.Text = "Просмотр фото";
            btnViewImage.Location = new Point(130, 70);
            btnViewImage.Size = new Size(150, 30);
            btnViewImage.UseVisualStyleBackColor = true;
            btnViewImage.Click += BtnViewImage_Click;
            btnViewImage.Enabled = false;

            lblImageStatus = new Label();
            lblImageStatus.Location = new Point(130, 110);
            lblImageStatus.Size = new Size(250, 20);
            lblImageStatus.ForeColor = Color.Gray;
            lblImageStatus.Text = "Фото не загружено";

            gbPhoto.Controls.Add(pictureBoxPreview);
            gbPhoto.Controls.Add(btnSelectImage);
            gbPhoto.Controls.Add(btnViewImage);
            gbPhoto.Controls.Add(lblImageStatus);

            this.Controls.Add(gbPhoto);

            // Сдвигаем кнопки вниз
            btnSave.Location = new Point(94, 300);
            btnCancel.Location = new Point(204, 300);

            // Показываем заглушку, если нет фото
            SetPreviewPlaceholder();

            if (_isEditMode)
            {
                LoadExistingImage();
            }
        }

        private void SetPreviewPlaceholder()
        {
            try
            {
                // Создаем маленькую заглушку для предпросмотра
                Bitmap placeholder = new Bitmap(110, 110);
                using (Graphics g = Graphics.FromImage(placeholder))
                {
                    g.Clear(Color.FromArgb(240, 240, 240));

                    Pen pen = new Pen(Color.FromArgb(200, 200, 200), 1);
                    g.DrawRectangle(pen, 1, 1, placeholder.Width - 2, placeholder.Height - 2);

                    Pen dashPen = new Pen(Color.FromArgb(180, 180, 180), 2);
                    dashPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                    // Рисуем маленькую камеру
                    g.DrawRectangle(dashPen, placeholder.Width / 2 - 20, placeholder.Height / 2 - 15, 40, 30);
                    g.DrawEllipse(dashPen, placeholder.Width / 2 - 10, placeholder.Height / 2 - 5, 20, 20);

                    Font font = new Font("Arial", 7, FontStyle.Bold);
                    Brush brush = new SolidBrush(Color.FromArgb(150, 150, 150));
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    g.DrawString("НЕТ ФОТО", font, brush,
                        new RectangleF(0, placeholder.Height - 20, placeholder.Width, 15), sf);
                }

                pictureBoxPreview.Image = placeholder;
            }
            catch
            {
                pictureBoxPreview.Image = null;
            }
        }


        private void LoadExistingImage()
        {
            try
            {
                string query = @"SELECT image_data FROM part_images WHERE part_id = @part_id ORDER BY upload_date DESC LIMIT 1";
                MySqlParameter[] parameters = new MySqlParameter[]
                {
            new MySqlParameter("@part_id", _partId)
                };

                var result = DatabaseHelper.ExecuteScalar(query, parameters);
                if (result != null && result != DBNull.Value)
                {
                    _imageData = (byte[])result;
                    using (MemoryStream ms = new MemoryStream(_imageData))
                    {
                        pictureBoxPreview.Image = Image.FromStream(ms);
                    }
                    lblImageStatus.Text = "Фото загружено";
                    lblImageStatus.ForeColor = Color.Green;
                    btnViewImage.Enabled = true;
                }
                else
                {
                    SetPreviewPlaceholder();
                    lblImageStatus.Text = "Фото не загружено";
                    lblImageStatus.ForeColor = Color.Gray;
                    btnViewImage.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки фото: {ex.Message}");
                SetPreviewPlaceholder();
            }
        }

        private void BtnSelectImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Выберите изображение запчасти";
                ofd.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Все файлы|*.*";
                ofd.FilterIndex = 1;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(ofd.FileName);
                        if (fi.Length > 5 * 1024 * 1024)
                        {
                            MessageBox.Show("Размер файла не должен превышать 5 МБ.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        using (Image img = Image.FromFile(ofd.FileName))
                        {
                            Image resizedImg = ResizeImageForPreview(img, 110, 110);
                            pictureBoxPreview.Image = resizedImg;

                            using (MemoryStream ms = new MemoryStream())
                            {
                                resizedImg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                _imageData = ms.ToArray();
                                _imageChanged = true;
                            }
                        }

                        lblImageStatus.Text = $"Фото выбрано: {Path.GetFileName(ofd.FileName)}";
                        lblImageStatus.ForeColor = Color.Blue;
                        btnViewImage.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private Image ResizeImageForPreview(Image image, int maxWidth, int maxHeight)
        {
            float ratioX = (float)maxWidth / image.Width;
            float ratioY = (float)maxHeight / image.Height;
            float ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        private void BtnViewImage_Click(object sender, EventArgs e)
        {
            if (_isEditMode && _partId > 0)
            {
                using (PartImageViewer viewer = new PartImageViewer(_partId, txtPartName.Text))
                {
                    if (viewer.ShowDialog() == DialogResult.OK)
                    {
                        LoadExistingImage();
                    }
                }
            }
            else if (_imageData != null)
            {
                using (Form previewForm = new Form())
                {
                    previewForm.Text = "Предпросмотр изображения";
                    previewForm.Size = new Size(800, 600);
                    previewForm.StartPosition = FormStartPosition.CenterParent;
                    previewForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    previewForm.MaximizeBox = false;
                    previewForm.MinimizeBox = false;

                    PictureBox pb = new PictureBox();
                    pb.Dock = DockStyle.Fill;
                    pb.SizeMode = PictureBoxSizeMode.Zoom;

                    using (MemoryStream ms = new MemoryStream(_imageData))
                    {
                        pb.Image = Image.FromStream(ms);
                    }

                    previewForm.Controls.Add(pb);
                    previewForm.ShowDialog();
                }
            }
        }

        private void ValidateForm(object sender = null, EventArgs e = null)
        {
            bool isValid = true;
            errorProvider.Clear();

            // Валидация названия запчасти
            if (string.IsNullOrWhiteSpace(txtPartName.Text) ||
                txtPartName.Text == "Например: Тормозной диск")
            {
                errorProvider.SetError(txtPartName, "Название запчасти обязательно для заполнения");
                isValid = false;
                SetErrorStyle(txtPartName, true);
            }
            else if (txtPartName.Text.Trim().Length < 2)
            {
                errorProvider.SetError(txtPartName, "Название запчасти должно содержать не менее 2 символов");
                isValid = false;
                SetErrorStyle(txtPartName, true);
            }
            else if (txtPartName.Text.Trim().Length > 100)
            {
                errorProvider.SetError(txtPartName, "Название запчасти не должно превышать 100 символов");
                isValid = false;
                SetErrorStyle(txtPartName, true);
            }
            else if (!IsValidPartName(txtPartName.Text.Trim()))
            {
                errorProvider.SetError(txtPartName, "Название запчасти содержит недопустимые символы");
                isValid = false;
                SetErrorStyle(txtPartName, true);
            }
            else
            {
                SetErrorStyle(txtPartName, false);
            }

            // Валидация производителя
            if (!string.IsNullOrWhiteSpace(txtManufacturer.Text) &&
                txtManufacturer.Text != "Например: Bosch")
            {
                if (txtManufacturer.Text.Trim().Length > 50)
                {
                    errorProvider.SetError(txtManufacturer, "Название производителя не должно превышать 50 символов");
                    isValid = false;
                    SetErrorStyle(txtManufacturer, true);
                }
                else if (!IsValidManufacturerName(txtManufacturer.Text.Trim()))
                {
                    errorProvider.SetError(txtManufacturer, "Название производителя содержит недопустимые символы");
                    isValid = false;
                    SetErrorStyle(txtManufacturer, true);
                }
                else
                {
                    SetErrorStyle(txtManufacturer, false);
                }
            }
            else
            {
                SetErrorStyle(txtManufacturer, false);
            }

            // Валидация цены
            if (numCost.Value <= 0)
            {
                errorProvider.SetError(numCost, "Цена должна быть больше 0");
                isValid = false;
                SetErrorStyle(numCost, true);
            }
            else if (numCost.Value > 999999.99m)
            {
                errorProvider.SetError(numCost, "Цена не должна превышать 999 999,99");
                isValid = false;
                SetErrorStyle(numCost, true);
            }
            else
            {
                SetErrorStyle(numCost, false);
            }

            // Валидация количества
            if (numStockQty.Value < 0)
            {
                errorProvider.SetError(numStockQty, "Количество не может быть отрицательным");
                isValid = false;
                SetErrorStyle(numStockQty, true);
            }
            else if (numStockQty.Value > 10000)
            {
                errorProvider.SetError(numStockQty, "Количество не должно превышать 10 000 единиц");
                isValid = false;
                SetErrorStyle(numStockQty, true);
            }
            else if (numStockQty.Value != Math.Floor(numStockQty.Value))
            {
                errorProvider.SetError(numStockQty, "Количество должно быть целым числом");
                isValid = false;
                SetErrorStyle(numStockQty, true);
            }
            else
            {
                SetErrorStyle(numStockQty, false);
            }

            // Проверка уникальности названия
            if (!_isEditMode && isValid && !string.IsNullOrWhiteSpace(txtPartName.Text.Trim()) &&
                txtPartName.Text.Trim() != "Например: Тормозной диск")
            {
                if (IsPartNameExists(txtPartName.Text.Trim()))
                {
                    errorProvider.SetError(txtPartName, "Запчасть с таким названием уже существует");
                    isValid = false;
                    SetErrorStyle(txtPartName, true);
                }
            }

            _isFormValid = isValid;
            btnSave.Enabled = isValid;
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            string baseTitle = _isEditMode ? "Редактирование запчасти" : "Добавление запчасти";
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
            }
            else
            {
                control.BackColor = SystemColors.Window;
                if (control is TextBox)
                {
                    if (control == txtPartName && control.Text == "Например: Тормозной диск")
                    {
                        control.ForeColor = SystemColors.GrayText;
                    }
                    else if (control == txtManufacturer && control.Text == "Например: Bosch")
                    {
                        control.ForeColor = SystemColors.GrayText;
                    }
                    else
                    {
                        control.ForeColor = SystemColors.WindowText;
                    }
                }
            }
        }

        private bool IsValidPartName(string partName)
        {
            return Regex.IsMatch(partName, @"^[а-яА-ЯёЁa-zA-Z0-9\s\-\(\)\.,:;/]+$");
        }

        private bool IsValidManufacturerName(string manufacturer)
        {
            return Regex.IsMatch(manufacturer, @"^[а-яА-ЯёЁa-zA-Z0-9\s\-\.&]+$");
        }

        private bool IsPartNameExists(string partName)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM parts WHERE part_name = @part_name";
                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@part_name", partName)
                };
                var result = DatabaseHelper.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        private int GetLastInsertedId()
        {
            string query = "SELECT LAST_INSERT_ID()";
            var result = DatabaseHelper.ExecuteScalar(query);
            return Convert.ToInt32(result);
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

            string partName = txtPartName.Text.Trim();
            string manufacturer = txtManufacturer.Text.Trim();
            decimal cost = numCost.Value;
            int stockQty = (int)numStockQty.Value;

            if (partName == "Например: Тормозной диск")
                partName = "";
            if (manufacturer == "Например: Bosch")
                manufacturer = "";

            if (string.IsNullOrWhiteSpace(partName))
            {
                MessageBox.Show("Введите название запчасти.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPartName.Focus();
                return;
            }

            if (cost <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numCost.Focus();
                return;
            }

            if (!_isEditMode && IsPartNameExists(partName))
            {
                MessageBox.Show("Запчасть с таким названием уже существует. Выберите другое название.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPartName.Focus();
                return;
            }

            if (stockQty == 0)
            {
                DialogResult result = MessageBox.Show(
                    "Количество на складе равно 0. Это означает, что запчасть отсутствует.\nПродолжить сохранение?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                {
                    numStockQty.Focus();
                    return;
                }
            }

            if (cost > 100000)
            {
                DialogResult result = MessageBox.Show(
                    $"Стоимость запчасти очень высокая: {cost:N2} ₽\nПроверьте правильность введенной цены.\nПродолжить сохранение?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                {
                    numCost.Focus();
                    return;
                }
            }

            try
            {
                string query;
                MySqlParameter[] parameters;
                int savedPartId = _partId;

                if (_isEditMode)
                {
                    query = @"UPDATE parts SET part_name = @part_name, manufacturer = @manufacturer, 
                             cost = @cost, stock_qty = @stock_qty WHERE part_id = @id";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@part_name", partName),
                        new MySqlParameter("@manufacturer", string.IsNullOrWhiteSpace(manufacturer) ? DBNull.Value : (object)manufacturer),
                        new MySqlParameter("@cost", cost),
                        new MySqlParameter("@stock_qty", stockQty),
                        new MySqlParameter("@id", _partId)
                    };
                }
                else
                {
                    query = @"INSERT INTO parts (part_name, manufacturer, cost, stock_qty) 
                             VALUES (@part_name, @manufacturer, @cost, @stock_qty)";
                    parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@part_name", partName),
                        new MySqlParameter("@manufacturer", string.IsNullOrWhiteSpace(manufacturer) ? DBNull.Value : (object)manufacturer),
                        new MySqlParameter("@cost", cost),
                        new MySqlParameter("@stock_qty", stockQty)
                    };
                }

                int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    if (!_isEditMode)
                    {
                        savedPartId = GetLastInsertedId();
                    }

                    // Сохраняем фото, если оно было изменено
                    if (_imageChanged && _imageData != null)
                    {
                        SaveImage(savedPartId);
                    }

                    MessageBox.Show(_isEditMode ? "Запчасть обновлена успешно!" : "Запчасть добавлена успешно!",
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
                if (ex.Number == 1062)
                {
                    MessageBox.Show("Запчасть с таким названием уже существует.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPartName.Focus();
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

        private void SaveImage(int partId)
        {
            try
            {
                string deleteQuery = "DELETE FROM part_images WHERE part_id = @part_id";
                MySqlParameter[] deleteParams = new MySqlParameter[]
                {
                    new MySqlParameter("@part_id", partId)
                };
                DatabaseHelper.ExecuteNonQuery(deleteQuery, deleteParams);

                string insertQuery = @"INSERT INTO part_images (part_id, image_data, image_name, image_size, content_type) 
                                     VALUES (@part_id, @image_data, @image_name, @image_size, @content_type)";

                MySqlParameter[] imageParams = new MySqlParameter[]
                {
                    new MySqlParameter("@part_id", partId),
                    new MySqlParameter("@image_data", _imageData),
                    new MySqlParameter("@image_name", "part_image.jpg"),
                    new MySqlParameter("@image_size", _imageData.Length),
                    new MySqlParameter("@content_type", "image/jpeg")
                };

                DatabaseHelper.ExecuteNonQuery(insertQuery, imageParams);

                string updateFlagQuery = "UPDATE parts SET has_image = 1 WHERE part_id = @part_id";
                MySqlParameter[] flagParams = new MySqlParameter[]
                {
                    new MySqlParameter("@part_id", partId)
                };
                DatabaseHelper.ExecuteNonQuery(updateFlagQuery, flagParams);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Запчасть сохранена, но возникла ошибка при сохранении фото: {ex.Message}",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtPartName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) &&
                e.KeyChar != ' ' && e.KeyChar != '-' && e.KeyChar != '(' && e.KeyChar != ')' &&
                e.KeyChar != ',' && e.KeyChar != '.' && e.KeyChar != ':' && e.KeyChar != ';' &&
                e.KeyChar != '/')
            {
                e.Handled = true;
            }
        }

        private void txtManufacturer_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar) &&
                e.KeyChar != ' ' && e.KeyChar != '-' && e.KeyChar != '.' && e.KeyChar != '&')
            {
                e.Handled = true;
            }
        }

        private void numCost_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && e.KeyChar != ',')
            {
                e.Handled = true;
            }
            if (e.KeyChar == ',' && numCost.Text.Contains(","))
            {
                e.Handled = true;
            }
        }

        private void numStockQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void numCost_Enter(object sender, EventArgs e)
        {
            numCost.Select(0, numCost.Text.Length);
        }

        private void numStockQty_Enter(object sender, EventArgs e)
        {
            numStockQty.Select(0, numStockQty.Text.Length);
        }

        private void txtPartName_Enter(object sender, EventArgs e)
        {
            if (txtPartName.Text == "Например: Тормозной диск")
            {
                txtPartName.Text = "";
                txtPartName.ForeColor = SystemColors.WindowText;
            }
        }

        private void txtPartName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPartName.Text))
            {
                txtPartName.Text = "Например: Тормозной диск";
                txtPartName.ForeColor = SystemColors.GrayText;
            }
        }

        private void txtManufacturer_Enter(object sender, EventArgs e)
        {
            if (txtManufacturer.Text == "Например: Bosch")
            {
                txtManufacturer.Text = "";
                txtManufacturer.ForeColor = SystemColors.WindowText;
            }
        }

        private void txtManufacturer_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtManufacturer.Text))
            {
                txtManufacturer.Text = "Например: Bosch";
                txtManufacturer.ForeColor = SystemColors.GrayText;
            }
        }

        private void AddEditPartForm_Load(object sender, EventArgs e)
        {
            // Центрируем форму
            this.CenterToParent();
        }
    }
}