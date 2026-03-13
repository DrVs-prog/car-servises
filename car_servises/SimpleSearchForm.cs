using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace car_servises
{
    public partial class SimpleSearchForm : Form
    {
        private DataTable originalData;
        private DataGridView dataGridView;
        private TextBox txtSearch;
        private Button btnReset;
        private Label lblPlaceholder;

        public SimpleSearchForm(DataTable data, string title = "Быстрый поиск")
        {
            InitializeComponent();
            SetupSimpleSearch(data, title);
        }

        private void SetupSimpleSearch(DataTable data, string title)
        {
            this.Text = title;
            this.originalData = data.Copy();

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(10);

            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 50;
            searchPanel.Padding = new Padding(5);

            txtSearch = new TextBox();
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Font = AppStyles.NormalFont;
            txtSearch.TextChanged += (s, e) => ApplyQuickSearch();

            // Добавляем подсказку вместо PlaceholderText
            lblPlaceholder = new Label();
            lblPlaceholder.Text = "Введите текст для поиска...";
            lblPlaceholder.ForeColor = Color.Gray;
            lblPlaceholder.Font = AppStyles.NormalFont;
            lblPlaceholder.Location = new Point(5, 8);
            lblPlaceholder.AutoSize = true;
            lblPlaceholder.Visible = string.IsNullOrEmpty(txtSearch.Text);

            // Обработчики для подсказки
            txtSearch.TextChanged += (s, e) =>
            {
                lblPlaceholder.Visible = string.IsNullOrEmpty(txtSearch.Text);
            };

            txtSearch.Enter += (s, e) => lblPlaceholder.Visible = false;
            txtSearch.Leave += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(txtSearch.Text);

            btnReset = new Button();
            btnReset.Text = "✕";
            btnReset.Dock = DockStyle.Right;
            btnReset.Width = 40;
            btnReset.Click += (s, e) =>
            {
                txtSearch.Text = "";
                lblPlaceholder.Visible = true;
                ResetSearch();
            };

            Label lblHint = new Label();
            lblHint.Text = "Поиск по всем колонкам";
            lblHint.Dock = DockStyle.Bottom;
            lblHint.Height = 15;
            lblHint.Font = AppStyles.SmallFont;
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.TextAlign = ContentAlignment.MiddleLeft;

            searchPanel.Controls.Add(lblHint);
            searchPanel.Controls.Add(lblPlaceholder);
            searchPanel.Controls.Add(btnReset);
            searchPanel.Controls.Add(txtSearch);

            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.DataSource = originalData.Copy();
            dataGridView.ReadOnly = true;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            mainPanel.Controls.Add(dataGridView);
            mainPanel.Controls.Add(searchPanel);

            this.Controls.Add(mainPanel);

            ApplyStyles();
        }

        private void ApplyQuickSearch()
        {
            string searchText = txtSearch.Text.Trim();
            var dataView = originalData.DefaultView;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                dataView.RowFilter = "";
            }
            else
            {
                string filter = "";
                foreach (DataColumn column in originalData.Columns)
                {
                    if (filter.Length > 0) filter += " OR ";
                    string columnName = column.ColumnName;
                    try
                    {
                        filter += $"CONVERT([{columnName}], 'System.String') LIKE '%{searchText.Replace("'", "''")}%'";
                    }
                    catch
                    {
                        filter += $"[{columnName}] LIKE '%{searchText.Replace("'", "''")}%'";
                    }
                }
                dataView.RowFilter = filter;
            }
        }

        private void ResetSearch()
        {
            txtSearch.Text = "";
            originalData.DefaultView.RowFilter = "";
        }

        private void ApplyStyles()
        {
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppStyles.BackgroundColor;

            AppStyles.ApplyTextBoxStyle(txtSearch);
            AppStyles.ApplyButtonStyle(btnReset);
            AppStyles.ApplyDataGridViewStyle(dataGridView);
        }
    }
}
