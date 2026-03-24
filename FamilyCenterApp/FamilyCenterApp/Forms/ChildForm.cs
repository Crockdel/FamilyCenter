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
    public partial class ChildForm : Form
    {
        private readonly ChildRepository _repository;
        private readonly Child _editingChild;

        private TextBox tbLastName;
        private TextBox tbFirstName;
        private TextBox tbPatronymic;
        private DateTimePicker dtpBirthDate;
        private ComboBox cbLegalStatus;
        private DateTimePicker dtpAdmissionDate;
        private TextBox tbNotes;
        private Button btnSave;
        private Button btnCancel;

        public event EventHandler DataSaved;

        public ChildForm(ChildRepository repository, Child child = null)
        {
            _repository = repository;
            _editingChild = child;
            InitializeComponent();
            LoadDataToForm();
        }

        private void InitializeComponent()
        {
            this.Text = _editingChild == null ? "Добавление ребёнка" : "Редактирование ребёнка";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(450, 480);
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
                Size = new Size(controlWidth, 27),
                Format = DateTimePickerFormat.Short,
                MaxDate = DateTime.Today
            };
            mainPanel.Controls.Add(dtpBirthDate);
            yPos += 40;

            // Правовой статус
            Label lblLegalStatus = new Label { Text = "Правовой статус:", Location = new Point(10, yPos + 8), AutoSize = true };
            mainPanel.Controls.Add(lblLegalStatus);
            cbLegalStatus = new ComboBox
            {
                Location = new Point(labelWidth + 10, yPos),
                Size = new Size(controlWidth, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbLegalStatus.Items.AddRange(new[] { "сирота", "оставшийся без попечения родителей", "временное отсутствие попечения" });
            mainPanel.Controls.Add(cbLegalStatus);
            yPos += 40;

            // Дата поступления
            Label lblAdmissionDate = new Label { Text = "Дата поступления:", Location = new Point(10, yPos + 8), AutoSize = true };
            mainPanel.Controls.Add(lblAdmissionDate);
            dtpAdmissionDate = new DateTimePicker
            {
                Location = new Point(labelWidth + 10, yPos),
                Size = new Size(controlWidth, 27),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            mainPanel.Controls.Add(dtpAdmissionDate);
            yPos += 40;

            // Примечания
            Label lblNotes = new Label { Text = "Примечания:", Location = new Point(10, yPos + 8), AutoSize = true };
            mainPanel.Controls.Add(lblNotes);
            tbNotes = new TextBox
            {
                Location = new Point(labelWidth + 10, yPos),
                Size = new Size(controlWidth, 80),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            mainPanel.Controls.Add(tbNotes);
            yPos += 95;

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
            if (_editingChild != null)
            {
                tbLastName.Text = _editingChild.LastName;
                tbFirstName.Text = _editingChild.FirstName;
                tbPatronymic.Text = _editingChild.Patronymic;
                dtpBirthDate.Value = _editingChild.BirthDate;
                cbLegalStatus.SelectedItem = _editingChild.LegalStatus;
                dtpAdmissionDate.Value = _editingChild.AdmissionDate;
                tbNotes.Text = _editingChild.Notes;
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

            if (cbLegalStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите правовой статус", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var child = new Child
                {
                    Id = _editingChild?.Id ?? 0,
                    LastName = tbLastName.Text.Trim(),
                    FirstName = tbFirstName.Text.Trim(),
                    Patronymic = string.IsNullOrWhiteSpace(tbPatronymic.Text) ? null : tbPatronymic.Text.Trim(),
                    BirthDate = dtpBirthDate.Value,
                    LegalStatus = cbLegalStatus.SelectedItem.ToString(),
                    AdmissionDate = dtpAdmissionDate.Value,
                    Notes = string.IsNullOrWhiteSpace(tbNotes.Text) ? null : tbNotes.Text.Trim()
                };

                if (_editingChild == null)
                {
                    _repository.Add(child);
                    MessageBox.Show("Ребёнок добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _repository.Update(child);
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