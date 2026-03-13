using System.Drawing;
using System.Windows.Forms;

namespace car_servises
{
    public static class AppStyles
    {
        // Цветовая палитра
        public static Color PrimaryColor = Color.FromArgb(41, 128, 185);    // Синий
        public static Color SecondaryColor = Color.FromArgb(52, 152, 219);  // Светло-синий
        public static Color AccentColor = Color.FromArgb(46, 204, 113);     // Зеленый
        public static Color DangerColor = Color.FromArgb(231, 76, 60);      // Красный
        public static Color WarningColor = Color.FromArgb(241, 196, 15);    // Желтый
        public static Color DarkColor = Color.FromArgb(44, 62, 80);         // Темно-синий
        public static Color LightColor = Color.FromArgb(236, 240, 241);     // Светло-серый
        public static Color BackgroundColor = Color.White;

        // Шрифты
        public static Font HeaderFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static Font TitleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static Font NormalFont = new Font("Segoe UI", 10F);
        public static Font SmallFont = new Font("Segoe UI", 9F);

        // Стиль для кнопок
        public static void ApplyButtonStyle(Button button, bool isPrimary = false)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = isPrimary ? PrimaryColor : LightColor;
            button.ForeColor = isPrimary ? Color.White : DarkColor;
            button.Font = NormalFont;
            button.Padding = new Padding(10, 5, 10, 5);
            button.Cursor = Cursors.Hand;

            // Эффекты при наведении
            button.FlatAppearance.MouseOverBackColor = isPrimary ? SecondaryColor : Color.FromArgb(220, 220, 220);
            button.FlatAppearance.MouseDownBackColor = isPrimary ? DarkColor : Color.FromArgb(200, 200, 200);
        }

        // Стиль для текстовых полей
        public static void ApplyTextBoxStyle(TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
            textBox.ForeColor = DarkColor;
            textBox.Font = NormalFont;
            textBox.Margin = new Padding(3);
        }

        // Стиль для меток
        public static void ApplyLabelStyle(Label label, bool isTitle = false)
        {
            label.Font = isTitle ? TitleFont : NormalFont;
            label.ForeColor = DarkColor;
            label.BackColor = Color.Transparent;
        }

        // Стиль для DataGridView
        public static void ApplyDataGridViewStyle(DataGridView dataGrid)
        {
            dataGrid.BackgroundColor = BackgroundColor;
            dataGrid.BorderStyle = BorderStyle.None;
            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGrid.EnableHeadersVisualStyles = false;

            // Заголовки столбцов
            dataGrid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryColor;
            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGrid.ColumnHeadersDefaultCellStyle.Font = TitleFont;
            dataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGrid.ColumnHeadersHeight = 50;
            

            // Строки
            dataGrid.DefaultCellStyle.Font = NormalFont;
            dataGrid.DefaultCellStyle.BackColor = BackgroundColor;
            dataGrid.DefaultCellStyle.ForeColor = DarkColor;
            dataGrid.AlternatingRowsDefaultCellStyle.BackColor = LightColor;

            // Сетка
            dataGrid.GridColor = Color.FromArgb(200, 200, 200);
            dataGrid.RowHeadersVisible = false;

            // Выделение
            dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGrid.DefaultCellStyle.SelectionBackColor = SecondaryColor;
            dataGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        // Стиль для панелей
        public static void ApplyPanelStyle(Panel panel)
        {
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Padding = new Padding(10);
        }

        // Стиль для группы элементов
        public static void ApplyGroupBoxStyle(GroupBox groupBox)
        {
            groupBox.Font = TitleFont;
            groupBox.ForeColor = PrimaryColor;
            groupBox.BackColor = Color.Transparent;
        }
    }
}