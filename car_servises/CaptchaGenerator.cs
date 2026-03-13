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

        // Генерация случайной строки из 4 символов
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

        // Обновить CAPTCHA
        public void RefreshCaptcha()
        {
            GenerateCaptchaText();
        }
    }
}