using System;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace car_servises
{
    public partial class AdvancedSearchForm : Form
    {
        private DataTable originalData;
        private DataTable filteredData;

        // Контролы
        private TextBox txtSearch;
        private ComboBox cmbSearchColumns;
        private ComboBox cmbSortColumns;
        private RadioButton rbAsc;
        private RadioButton rbDesc;
        private DataGridView dataGridView;
        private Button btnReset;

        public AdvancedSearchForm(DataTable data, string formTitle = "Расширенный поиск")
        {
            InitializeComponent();
            InitializeAdvancedSearch(data, formTitle);
        }

        private void InitializeAdvancedSearch(DataTable data, string formTitle)
        {
            this.Text = formTitle;
            this.originalData = data.Copy();
            this.filteredData = data.Copy();

            SetupSearchControls();
            ApplyStyles();
            ApplySorting(); // Применяем начальную сортировку
        }

        private void SetupSearchControls()
        {
            // Главный контейнер
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(10);

            // Панель поиска
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 100;
            searchPanel.BorderStyle = BorderStyle.FixedSingle;
            searchPanel.Padding = new Padding(10);

            // Поле поиска
            Label lblSearch = new Label();
            lblSearch.Text = "Поиск:";
            lblSearch.Location = new Point(10, 15);
            lblSearch.AutoSize = true;

            txtSearch = new TextBox();
            txtSearch.Location = new Point(60, 12);
            txtSearch.Size = new Size(200, 25);
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // Выбор колонки для поиска
            Label lblSearchColumn = new Label();
            lblSearchColumn.Text = "в колонке:";
            lblSearchColumn.Location = new Point(270, 15);
            lblSearchColumn.AutoSize = true;

            cmbSearchColumns = new ComboBox();
            cmbSearchColumns.Location = new Point(350, 12);
            cmbSearchColumns.Size = new Size(150, 25);
            cmbSearchColumns.DropDownStyle = ComboBoxStyle.DropDownList;

            // Выбор колонки для сортировки
            Label lblSort = new Label();
            lblSort.Text = "Сортировка:";
            lblSort.Location = new Point(10, 50);
            lblSort.AutoSize = true;

            cmbSortColumns = new ComboBox();
            cmbSortColumns.Location = new Point(90, 47);
            cmbSortColumns.Size = new Size(150, 25);
            cmbSortColumns.DropDownStyle = ComboBoxStyle.DropDownList;

            // Заполняем колонки
            foreach (DataColumn column in originalData.Columns)
            {
                cmbSearchColumns.Items.Add(column.ColumnName);
                cmbSortColumns.Items.Add(column.ColumnName);
            }
            cmbSearchColumns.SelectedIndex = 0;
            cmbSortColumns.SelectedIndex = 0;
            cmbSortColumns.SelectedIndexChanged += CmbSortColumns_SelectedIndexChanged;

            // Радио-кнопки для направления сортировки
            rbAsc = new RadioButton();
            rbAsc.Text = "По возрастанию";
            rbAsc.Location = new Point(250, 50);
            rbAsc.AutoSize = true;
            rbAsc.Checked = true;
            rbAsc.CheckedChanged += SortDirection_Changed;

            rbDesc = new RadioButton();
            rbDesc.Text = "По убыванию";
            rbDesc.Location = new Point(380, 50);
            rbDesc.AutoSize = true;
            rbDesc.CheckedChanged += SortDirection_Changed;

            // Кнопка сброса
            btnReset = new Button();
            btnReset.Text = "Сбросить все";
            btnReset.Location = new Point(520, 40);
            btnReset.Size = new Size(100, 30);
            btnReset.Click += BtnReset_Click;

            // Добавляем элементы на панель поиска
            searchPanel.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, lblSearchColumn, cmbSearchColumns,
                lblSort, cmbSortColumns, rbAsc, rbDesc, btnReset
            });

            // DataGridView
            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.DataSource = filteredData;
            dataGridView.ReadOnly = true;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Добавляем на главную панель
            mainPanel.Controls.Add(dataGridView);
            mainPanel.Controls.Add(searchPanel);

            this.Controls.Add(mainPanel);
        }

        // Обработчики событий
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void CmbSortColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySorting();
        }

        private void SortDirection_Changed(object sender, EventArgs e)
        {
            ApplySorting();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            ResetAllFilters();
        }

        private void ApplyFilters()
        {
            string searchText = txtSearch.Text.Trim();
            string searchColumn = cmbSearchColumns.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(searchText))
            {
                filteredData = originalData.Copy();
            }
            else
            {
                try
                {
                    var query = originalData.AsEnumerable()
                        .Where(row => row[searchColumn].ToString()
                        .IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (query.Any())
                    {
                        filteredData = query.CopyToDataTable();
                    }
                    else
                    {
                        filteredData = originalData.Clone(); // Пустая таблица с такой же структурой
                    }
                }
                catch (Exception ex)
                {
                    filteredData = originalData.Copy();
                    MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
                }
            }

            ApplySorting();
        }

        private void ApplySorting()
        {
            if (filteredData.Rows.Count == 0) return;

            string sortColumn = cmbSortColumns.SelectedItem?.ToString();
            string sortDirection = rbAsc.Checked ? "ASC" : "DESC";

            try
            {
                filteredData.DefaultView.Sort = $"{sortColumn} {sortDirection}";
                dataGridView.DataSource = filteredData.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Невозможно отсортировать колонку '{sortColumn}': {ex.Message}");
            }
        }

        private void ResetAllFilters()
        {
            txtSearch.Text = "";
            cmbSearchColumns.SelectedIndex = 0;
            cmbSortColumns.SelectedIndex = 0;
            rbAsc.Checked = true;

            filteredData = originalData.Copy();
            ApplySorting();
        }

        private void ApplyStyles()
        {
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppStyles.BackgroundColor;

            // Стили для контролов
            AppStyles.ApplyTextBoxStyle(txtSearch);
            AppStyles.ApplyButtonStyle(btnReset);
            AppStyles.ApplyDataGridViewStyle(dataGridView);

            // Стили для комбобоксов
            cmbSearchColumns.Font = AppStyles.NormalFont;
            cmbSortColumns.Font = AppStyles.NormalFont;

            // Стили для меток
            foreach (Control control in this.Controls)
            {
                if (control is Label label)
                {
                    AppStyles.ApplyLabelStyle(label);
                }
            }
        }
    }
}
