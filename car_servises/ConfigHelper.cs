using System;
using System.Configuration;

namespace car_servises
{
    public static class ConfigHelper
    {
        public static int GetInactivityTimeout()
        {
            try
            {
                string value = ConfigurationManager.AppSettings["InactivityTimeoutSeconds"];

                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int timeout))
                {
                    return timeout > 0 ? timeout : 30; // Не меньше 1 секунды
                }
            }
            catch (Exception)
            {
                // Если ошибка чтения - используем значение по умолчанию
            }

            return 30; // Значение по умолчанию
        }
    }
}