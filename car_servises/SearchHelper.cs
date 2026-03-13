using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace car_servises
{
    public static class SearchHelper
    {
        public static void OpenAdvancedSearch(DataTable data, string title = "Расширенный поиск")
        {
            if (data == null || data.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для поиска");
                return;
            }

            AdvancedSearchForm form = new AdvancedSearchForm(data, title);
            form.Show();
        }

        public static void OpenSimpleSearch(DataTable data, string title = "Быстрый поиск")
        {
            if (data == null || data.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для поиска");
                return;
            }

            SimpleSearchForm form = new SimpleSearchForm(data, title);
            form.Show();
        }

        public static void AddSearchToExistingForm(Form existingForm, DataGridView dataGridView)
        {
            if (existingForm == null || dataGridView == null) return;

            // Создаем панель поиска
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 40;
            searchPanel.Padding = new Padding(5);

            TextBox txtSearch = new TextBox();
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Font = AppStyles.NormalFont;
            txtSearch.TextChanged += (s, e) => ApplySearchToGrid(dataGridView, txtSearch.Text);

            // Добавляем метку-подсказку вместо PlaceholderText
            Label lblPlaceholder = new Label();
            lblPlaceholder.Text = "Поиск...";
            lblPlaceholder.ForeColor = Color.Gray;
            lblPlaceholder.Font = AppStyles.NormalFont;
            lblPlaceholder.Location = new Point(5, 8);
            lblPlaceholder.AutoSize = true;
            lblPlaceholder.Visible = string.IsNullOrEmpty(txtSearch.Text);

            // Обработчики для показа/скрытия подсказки
            txtSearch.TextChanged += (s, e) =>
            {
                lblPlaceholder.Visible = string.IsNullOrEmpty(txtSearch.Text);
            };

            txtSearch.Enter += (s, e) => lblPlaceholder.Visible = false;
            txtSearch.Leave += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(txtSearch.Text);

            Button btnClear = new Button();
            btnClear.Text = "✕";
            btnClear.Dock = DockStyle.Right;
            btnClear.Width = 30;
            btnClear.Click += (s, e) =>
            {
                txtSearch.Text = "";
                lblPlaceholder.Visible = true;
            };

            searchPanel.Controls.Add(lblPlaceholder);
            searchPanel.Controls.Add(btnClear);
            searchPanel.Controls.Add(txtSearch);

            // Вставляем панель поиска перед DataGridView
            existingForm.Controls.Add(searchPanel);
            searchPanel.BringToFront();

            // Применяем стили
            AppStyles.ApplyTextBoxStyle(txtSearch);
            AppStyles.ApplyButtonStyle(btnClear);
        }

        private static void ApplySearchToGrid(DataGridView dataGridView, string searchText)
        {
            if (dataGridView.DataSource is DataTable dataTable)
            {
                var dataView = dataTable.DefaultView;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    dataView.RowFilter = "";
                }
                else
                {
                    string filter = "";
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        if (filter.Length > 0) filter += " OR ";
                        filter += $"[{column.ColumnName}] LIKE '%{searchText.Replace("'", "''")}%'";
                    }
                    dataView.RowFilter = filter;
                }
            }
        }
    }
}
