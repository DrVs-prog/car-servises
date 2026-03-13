
namespace car_servises
{
    partial class Employes
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button4 = new System.Windows.Forms.Button();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblInColumn = new System.Windows.Forms.Label();
            this.cmbSearchColumn = new System.Windows.Forms.ComboBox();
            this.lblSort = new System.Windows.Forms.Label();
            this.cmbSortBy = new System.Windows.Forms.ComboBox();
            this.rbAsc = new System.Windows.Forms.RadioButton();
            this.rbDesc = new System.Windows.Forms.RadioButton();
            this.btnResetFilters = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.pnlSearch.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(148, 394);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(133, 46);
            this.button1.TabIndex = 0;
            this.button1.Text = "Удаление";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(287, 394);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(178, 46);
            this.button2.TabIndex = 1;
            this.button2.Text = "Редактирование";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(0, 394);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(133, 46);
            this.button3.TabIndex = 2;
            this.button3.Text = "Добавление";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(0, 60);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(1016, 328);
            this.dataGridView1.TabIndex = 3;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(882, 394);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(134, 46);
            this.button4.TabIndex = 5;
            this.button4.Text = "Выход";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // pnlSearch
            // 
            this.pnlSearch.Controls.Add(this.lblSearch);
            this.pnlSearch.Controls.Add(this.txtSearch);
            this.pnlSearch.Controls.Add(this.lblInColumn);
            this.pnlSearch.Controls.Add(this.cmbSearchColumn);
            this.pnlSearch.Controls.Add(this.lblSort);
            this.pnlSearch.Controls.Add(this.cmbSortBy);
            this.pnlSearch.Controls.Add(this.rbAsc);
            this.pnlSearch.Controls.Add(this.rbDesc);
            this.pnlSearch.Controls.Add(this.btnResetFilters);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(0, 0);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new System.Drawing.Size(1016, 60);
            this.pnlSearch.TabIndex = 8;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(10, 20);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(42, 13);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Поиск:";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(60, 17);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(182, 20);
            this.txtSearch.TabIndex = 1;
            // 
            // lblInColumn
            // 
            this.lblInColumn.AutoSize = true;
            this.lblInColumn.Location = new System.Drawing.Point(248, 20);
            this.lblInColumn.Name = "lblInColumn";
            this.lblInColumn.Size = new System.Drawing.Size(61, 13);
            this.lblInColumn.TabIndex = 2;
            this.lblInColumn.Text = "в колонке:";
            // 
            // cmbSearchColumn
            // 
            this.cmbSearchColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSearchColumn.FormattingEnabled = true;
            this.cmbSearchColumn.Location = new System.Drawing.Point(315, 17);
            this.cmbSearchColumn.Name = "cmbSearchColumn";
            this.cmbSearchColumn.Size = new System.Drawing.Size(150, 21);
            this.cmbSearchColumn.TabIndex = 3;
            // 
            // lblSort
            // 
            this.lblSort.AutoSize = true;
            this.lblSort.Location = new System.Drawing.Point(471, 18);
            this.lblSort.Name = "lblSort";
            this.lblSort.Size = new System.Drawing.Size(70, 13);
            this.lblSort.TabIndex = 4;
            this.lblSort.Text = "Сортировка:";
            // 
            // cmbSortBy
            // 
            this.cmbSortBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSortBy.FormattingEnabled = true;
            this.cmbSortBy.Location = new System.Drawing.Point(547, 18);
            this.cmbSortBy.Name = "cmbSortBy";
            this.cmbSortBy.Size = new System.Drawing.Size(150, 21);
            this.cmbSortBy.TabIndex = 5;
            // 
            // rbAsc
            // 
            this.rbAsc.AutoSize = true;
            this.rbAsc.Checked = true;
            this.rbAsc.Location = new System.Drawing.Point(703, 14);
            this.rbAsc.Name = "rbAsc";
            this.rbAsc.Size = new System.Drawing.Size(91, 17);
            this.rbAsc.TabIndex = 6;
            this.rbAsc.TabStop = true;
            this.rbAsc.Text = "Возрастание";
            this.rbAsc.UseVisualStyleBackColor = true;
            // 
            // rbDesc
            // 
            this.rbDesc.AutoSize = true;
            this.rbDesc.Location = new System.Drawing.Point(809, 17);
            this.rbDesc.Name = "rbDesc";
            this.rbDesc.Size = new System.Drawing.Size(77, 17);
            this.rbDesc.TabIndex = 7;
            this.rbDesc.Text = "Убывание";
            this.rbDesc.UseVisualStyleBackColor = true;
            // 
            // btnResetFilters
            // 
            this.btnResetFilters.Location = new System.Drawing.Point(892, 7);
            this.btnResetFilters.Name = "btnResetFilters";
            this.btnResetFilters.Size = new System.Drawing.Size(100, 40);
            this.btnResetFilters.TabIndex = 8;
            this.btnResetFilters.Text = "Сбросить";
            this.btnResetFilters.UseVisualStyleBackColor = true;
            // 
            // Employes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 453);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pnlSearch);
            this.Name = "Employes";
            this.Text = "Employes";
            this.Load += new System.EventHandler(this.Employes_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button4;
    }
}