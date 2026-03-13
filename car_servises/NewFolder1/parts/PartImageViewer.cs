using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace car_servises
{
    public partial class PartImageViewer : Form
    {
        private int _partId;
        private string _partName;
        private byte[] _currentImageData;

        public PartImageViewer(int partId, string partName)
        {
            InitializeComponent();
            _partId = partId;
            _partName = partName;
            this.Text = $"Фото запчасти: {_partName}";
            LoadImage();
        }

        private void LoadImage()
        {
            try
            {
                string query = @"SELECT image_data, image_name, image_size 
                               FROM part_images 
                               WHERE part_id = @part_id 
                               ORDER BY upload_date DESC LIMIT 1";

                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@part_id", _partId)
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    _currentImageData = (byte[])dt.Rows[0]["image_data"];
                    string imageName = dt.Rows[0]["image_name"]?.ToString() ?? "photo.jpg";
                    int imageSize = dt.Rows[0]["image_size"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["image_size"]) : 0;

                    using (MemoryStream ms = new MemoryStream(_currentImageData))
                    {
                        pictureBox1.Image = Image.FromStream(ms);
                    }

                    lblImageName.Text = $"Файл: {imageName} ({imageSize / 1024} КБ)";
                    lblImageName.Visible = true;
                    btnSave.Enabled = true;
                    btnDelete.Enabled = true;
                }
                else
                {
                    // Загружаем заглушку
                    SetPlaceholderImage();
                    lblImageName.Text = "Фото отсутствует";
                    lblImageName.Visible = true;
                    btnSave.Enabled = false;
                    btnDelete.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetPlaceholderImage();
            }
        }

        private void SetPlaceholderImage()
        {
            try
            {
                // Создаем заглушку "Нет фото"
                Bitmap placeholder = new Bitmap(400, 300);
                using (Graphics g = Graphics.FromImage(placeholder))
                {
                    // Заливка фона
                    g.Clear(Color.FromArgb(240, 240, 240));

                    // Рамка
                    Pen pen = new Pen(Color.FromArgb(200, 200, 200), 2);
                    g.DrawRectangle(pen, 1, 1, placeholder.Width - 2, placeholder.Height - 2);

                    // Иконка фото
                    Pen dashPen = new Pen(Color.FromArgb(180, 180, 180), 3);
                    dashPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                    // Рисуем стилизованную камеру
                    g.DrawRectangle(dashPen, placeholder.Width / 2 - 40, placeholder.Height / 2 - 30, 80, 60);
                    g.DrawEllipse(dashPen, placeholder.Width / 2 - 20, placeholder.Height / 2 - 10, 40, 40);

                    // Текст
                    Font font = new Font("Arial", 14, FontStyle.Bold);
                    Brush brush = new SolidBrush(Color.FromArgb(150, 150, 150));
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    g.DrawString("НЕТ ФОТО", font, brush,
                        new RectangleF(0, placeholder.Height - 50, placeholder.Width, 30), sf);

                    g.DrawString(_partName, new Font("Arial", 10, FontStyle.Regular),
                        new SolidBrush(Color.FromArgb(120, 120, 120)),
                        new RectangleF(0, placeholder.Height - 30, placeholder.Width, 20), sf);
                }

                pictureBox1.Image = placeholder;
                pictureBox1.BackColor = Color.FromArgb(240, 240, 240);
                _currentImageData = null;
            }
            catch
            {
                // Если не удалось создать заглушку, просто очищаем
                pictureBox1.Image = null;
                pictureBox1.BackColor = Color.FromArgb(240, 240, 240);
            }
        }

        private Image ResizeImage(Image image, int maxWidth, int maxHeight)
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

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Выберите изображение запчасти";
                ofd.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Все файлы|*.*";
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;

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
                            Image resizedImg = ResizeImage(img, 800, 600);
                            pictureBox1.Image = resizedImg;

                            using (MemoryStream ms = new MemoryStream())
                            {
                                resizedImg.Save(ms, ImageFormat.Jpeg);
                                _currentImageData = ms.ToArray();
                            }
                        }

                        lblImageName.Text = $"Файл: {Path.GetFileName(ofd.FileName)} ({fi.Length / 1024} КБ)";
                        btnSave.Enabled = true;
                        btnDelete.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_currentImageData == null)
            {
                MessageBox.Show("Нет изображения для сохранения.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string deleteQuery = "DELETE FROM part_images WHERE part_id = @part_id";
                MySqlParameter[] deleteParams = new MySqlParameter[]
                {
                    new MySqlParameter("@part_id", _partId)
                };
                DatabaseHelper.ExecuteNonQuery(deleteQuery, deleteParams);

                string insertQuery = @"INSERT INTO part_images (part_id, image_data, image_name, image_size, content_type) 
                                     VALUES (@part_id, @image_data, @image_name, @image_size, @content_type)";

                string imageName = lblImageName.Text.Replace("Файл: ", "");
                if (imageName.Contains("("))
                    imageName = imageName.Substring(0, imageName.IndexOf("(")).Trim();

                MySqlParameter[] parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@part_id", _partId),
                    new MySqlParameter("@image_data", _currentImageData),
                    new MySqlParameter("@image_name", imageName),
                    new MySqlParameter("@image_size", _currentImageData.Length),
                    new MySqlParameter("@content_type", "image/jpeg")
                };

                int result = DatabaseHelper.ExecuteNonQuery(insertQuery, parameters);

                string updateFlagQuery = "UPDATE parts SET has_image = 1 WHERE part_id = @part_id";
                MySqlParameter[] flagParams = new MySqlParameter[]
                {
                    new MySqlParameter("@part_id", _partId)
                };
                DatabaseHelper.ExecuteNonQuery(updateFlagQuery, flagParams);

                if (result > 0)
                {
                    MessageBox.Show("Изображение успешно сохранено!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения изображения: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Удалить фотографию запчасти '{_partName}'?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM part_images WHERE part_id = @part_id";
                    MySqlParameter[] parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@part_id", _partId)
                    };

                    int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);

                    string updateFlagQuery = "UPDATE parts SET has_image = 0 WHERE part_id = @part_id";
                    MySqlParameter[] flagParams = new MySqlParameter[]
                    {
                        new MySqlParameter("@part_id", _partId)
                    };
                    DatabaseHelper.ExecuteNonQuery(updateFlagQuery, flagParams);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Изображение удалено!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SetPlaceholderImage();
                        lblImageName.Text = "Фото отсутствует";
                        btnSave.Enabled = false;
                        btnDelete.Enabled = false;
                        this.DialogResult = DialogResult.OK;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления изображения: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}