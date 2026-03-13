using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace car_servises
{
    public class CaptchaGenerator
    {
        private Random random = new Random();
        private string currentCaptchaText;

        public string CurrentCaptchaText => currentCaptchaText;

        public string GenerateCaptchaText()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] stringChars = new char[4];

            for (int i = 0; i < 4; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            currentCaptchaText = new string(stringChars);
            return currentCaptchaText;
        }

        // Добавьте этот метод
        public Bitmap CreateCaptchaImage(int width = 200, int height = 60)
        {
            if (string.IsNullOrEmpty(currentCaptchaText))
            {
                GenerateCaptchaText();
            }

            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Фоновый шум (линии)
                Pen noisePen = new Pen(Color.LightGray);
                for (int i = 0; i < 20; i++)
                {
                    int x1 = random.Next(width);
                    int y1 = random.Next(height);
                    int x2 = random.Next(width);
                    int y2 = random.Next(height);
                    graphics.DrawLine(noisePen, x1, y1, x2, y2);
                }

                // Фоновый шум (точки)
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(width);
                    int y = random.Next(height);
                    bitmap.SetPixel(x, y, Color.FromArgb(random.Next(100, 200),
                                                         random.Next(100, 200),
                                                         random.Next(100, 200)));
                }

                // Рисуем символы с разными углами наклона
                int startX = 20;
                int[] yOffsets = { 5, -5, 10, 0 };
                float[] rotations = { -5f, 8f, -8f, 5f };

                for (int i = 0; i < currentCaptchaText.Length; i++)
                {
                    string charStr = currentCaptchaText[i].ToString();
                    Font font = new Font("Arial", random.Next(20, 26), FontStyle.Bold);

                    Color charColor = Color.FromArgb(
                        random.Next(50, 200),
                        random.Next(50, 200),
                        random.Next(50, 200)
                    );

                    using (Brush brush = new SolidBrush(charColor))
                    {
                        GraphicsState state = graphics.Save();

                        graphics.TranslateTransform(startX + i * 35, height / 2 + yOffsets[i]);
                        graphics.RotateTransform(rotations[i]);

                        graphics.DrawString(charStr, font, brush, 0, 0);

                        graphics.Restore(state);
                    }
                }

                // Перечеркивающие линии
                Pen crossPen = new Pen(Color.FromArgb(100, Color.Gray));
                for (int i = 0; i < 3; i++)
                {
                    int y = random.Next(20, height - 20);
                    graphics.DrawLine(crossPen, 10, y, width - 10, y + random.Next(-10, 10));
                }
            }

            return bitmap;
        }

        public void RefreshCaptcha()
        {
            GenerateCaptchaText();
        }
    }
}