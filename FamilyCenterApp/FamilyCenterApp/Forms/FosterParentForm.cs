using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FamilyCenterApp.DataAccess.Models;
using FamilyCenterApp.DataAccess.Repositories;

namespace FamilyCenterApp.WinForms.Forms
{
    public partial class FosterParentForm : Form
    {
        private readonly FosterParentRepository _repository;
        private readonly FosterParent _editingFoster;

        private TextBox tbLastName;
        private TextBox tbFirstName;
        private TextBox tbPatronymic;
        private DateTimePicker dtpBirthDate;
        private TextBox tbPhone;
        private ComboBox cbStatus;
        private Button btnSave;
        private Button btnCancel;
        private CheckBox chkNoBirthDate;

        public event EventHandler DataSaved;

        public FosterParentForm(FosterParentRepository repository, FosterParent foster = null)
        {
            _repository = repository;
            _editingFoster = foster;
            InitializeComponent();
            LoadDataToForm();
        }

        private void InitializeComponent()
        {
            this.Text = _editingFoster == null ? "Добавление приёмного родителя" : "Редактирование приёмного родителя";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(450, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            int yPos = 10;
            int labelWidth = 120;
            int controlWidth = 260;

            // Фамилия
            Label lblLastName = new Label { Text = "Фамилия:", Location = new Point(10, yPos + 8), AutoSize = true };
            mainPanel.Controls.Add(lblLastName);
            tbLastName = new TextBox { Location = new Point(labelWidth + 10, yPos), Size = new Size(controlWidth, 27) };
            mainPanel.Controls.Add(tbLastName);
            yPos += 40;

            // Имя
            Label lblFirstName = new Label { Text = "Имя:", Location = new Point(10, yPos + 8), AutoSize = true };
            mainPanel.Controls.Add(lblFirstName);
            tbFirstName = new TextBox { Location = new Point(labelWidth + 10, yPos), Size = new Size(controlWidth, 27) };
            mainPanel.Controls.Add(tbFirstName);
            yPos += 40;

            // Отчество
            Label lblPatronymic = new Label { Text = "Отчество:", Location = new Point(10, yPos + 8), AutoSize = true };
            mainPanel.Controls.Add(lblPatronymic);
            tbPatronymic = new TextBox { Location = new Point(labelWidth + 10, yPos), Size = new Size(controlWidth, 27) };
            mainPanel.Controls.Add(tbPatronymic);
            yPos += 40;

            // Дата рождения
            Label lblBirthDate = new Label { Text = "Дата рождения:", Location = new Point(10, yPos + 8), AutoSize = true };
            mainPanel.Controls.Add(lblBirthDate);
            dtpBirthDate = new DateTimePicker
            {
                Location = new Point(labelWidth + 10, yPos),
                Size = new Size(controlWidth - 100, 27),
                Format = DateTimePickerFormat.Short,
                MaxDate = DateTime.Today
            };
            mainPanel.Controls.Add(dtpBirthDate);

            chkNoBirthDate = new CheckBox
            {
                Text = "не указана",
                Location = new Point(labelWidth + controlWidth - 85, yPos + 5),
                AutoSize = true
            };
            chkNoBirthDate.CheckedChanged += (s, e) => dtpBirthDate.Enabled = !chkNoBirthDate.Checked;
            mainPanel.Controls.Add(chkNoBirthDate);
            yPos += 40;

            // Телефон
            Label lblPhone = new Label { Text = "Телефон:", Location = new Point(10, yPos + 8), AutoSize = true };
            mainPanel.Controls.Add(lblPhone);
            tbPhone = new TextBox { Location = new Point(labelWidth + 10, yPos), Size = new Size(controlWidth, 27) };
            mainPanel.Controls.Add(tbPhone);
            yPos += 40;

            // Статус
            Label lblStatus = new Label { Text = "Статус:", Location = new Point(10, yPos + 8), AutoSize = true };
            mainPanel.Controls.Add(lblStatus);
            cbStatus = new ComboBox
            {
                Location = new Point(labelWidth + 10, yPos),
                Size = new Size(controlWidth, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbStatus.Items.AddRange(new[] { "кандидат", "приёмный родитель", "освобождён от обязанностей" });
            mainPanel.Controls.Add(cbStatus);
            yPos += 70;

            // В методе InitializeComponent, после добавления всех полей ввода
            // Добавляем кнопку просмотра детей
            Button btnViewChildren = new Button
            {
                Text = "👪 Дети в семье",
                Location = new Point(labelWidth + 10, yPos),
                Size = new Size(controlWidth, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F)
            };
            btnViewChildren.Click += (s, e) =>
            {
                if (_editingFoster != null)
                {
                    var form = new FosterParentChildrenForm(_repository, _editingFoster);
                    form.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Сначала сохраните приёмного родителя", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            mainPanel.Controls.Add(btnViewChildren);
            yPos += 50;

            // Кнопки сохранения и отмены
            btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(labelWidth + 10, yPos),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            mainPanel.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(labelWidth + 140, yPos),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnCancel);

            // Кнопки
            btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(labelWidth + 10, yPos),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            mainPanel.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(labelWidth + 140, yPos),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnCancel);

            this.Controls.Add(mainPanel);
        }

        private void LoadDataToForm()
        {
            if (_editingFoster != null)
            {
                tbLastName.Text = _editingFoster.LastName;
                tbFirstName.Text = _editingFoster.FirstName;
                tbPatronymic.Text = _editingFoster.Patronymic;

                if (_editingFoster.BirthDate.HasValue)
                {
                    dtpBirthDate.Value = _editingFoster.BirthDate.Value;
                    chkNoBirthDate.Checked = false;
                }
                else
                {
                    chkNoBirthDate.Checked = true;
                    dtpBirthDate.Enabled = false;
                }

                tbPhone.Text = _editingFoster.Phone;
                cbStatus.SelectedItem = _editingFoster.Status;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbLastName.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbLastName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tbFirstName.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbFirstName.Focus();
                return;
            }

            if (cbStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var foster = new FosterParent
                {
                    Id = _editingFoster?.Id ?? 0,
                    LastName = tbLastName.Text.Trim(),
                    FirstName = tbFirstName.Text.Trim(),
                    Patronymic = string.IsNullOrWhiteSpace(tbPatronymic.Text) ? null : tbPatronymic.Text.Trim(),
                    BirthDate = chkNoBirthDate.Checked ? (DateTime?)null : dtpBirthDate.Value,
                    Phone = string.IsNullOrWhiteSpace(tbPhone.Text) ? null : tbPhone.Text.Trim(),
                    Status = cbStatus.SelectedItem.ToString()
                };

                if (_editingFoster == null)
                {
                    _repository.Add(foster);
                    MessageBox.Show("Приёмный родитель добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _repository.Update(foster);
                    MessageBox.Show("Данные обновлены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DataSaved?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}