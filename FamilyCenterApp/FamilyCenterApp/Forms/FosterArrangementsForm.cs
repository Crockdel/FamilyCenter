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
    /// <summary>
    /// Форма для управления приёмными семьями и устройствами детей
    /// </summary>
    public partial class FosterArrangementsForm : Form
    {
        private readonly Child _child;
        private readonly FosterArrangementRepository _arrangementRepository;
        private readonly FosterParentRepository _fosterParentRepository;

        private ListBox lbArrangements;
        private Button btnAdd;
        private Button btnEnd;
        private Button btnClose;

        public FosterArrangementsForm(Child child)
        {
            _child = child;
            _arrangementRepository = new FosterArrangementRepository();
            _fosterParentRepository = new FosterParentRepository();

            InitializeComponent();
            LoadArrangements();
        }

        private void InitializeComponent()
        {
            this.Text = $"Приёмные семьи - {_child.FullName}";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(600, 500);
            this.BackColor = Color.White;

            // Заголовок
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(52, 73, 94)
            };

            Label lblTitle = new Label
            {
                Text = $"Ребёнок: {_child.FullName}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            Label lblSubtitle = new Label
            {
                Text = $"Дата рождения: {_child.BirthDate:dd.MM.yyyy} | Возраст: {_child.Age} лет",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(20, 45),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblSubtitle);

            // Основная панель
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            Label lblArrangements = new Label
            {
                Text = "Текущие устроенные семьи:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblArrangements);

            lbArrangements = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(565, 300),
                Font = new Font("Segoe UI", 10F),
                DisplayMember = "DisplayText"
            };
            mainPanel.Controls.Add(lbArrangements);

            // Кнопки
            btnAdd = new Button
            {
                Text = "+ Устроить в семью",
                Location = new Point(10, 350),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F)
            };
            btnAdd.Click += BtnAdd_Click;
            mainPanel.Controls.Add(btnAdd);

            btnEnd = new Button
            {
                Text = "✖ Завершить устройство",
                Location = new Point(170, 350),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            btnEnd.Click += BtnEnd_Click;
            mainPanel.Controls.Add(btnEnd);

            btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(480, 410),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnClose);

            // Подписка на выбор
            lbArrangements.SelectedIndexChanged += (s, e) =>
            {
                var selected = lbArrangements.SelectedItem as ArrangementListItem;
                btnEnd.Enabled = selected != null && selected.Arrangement.Status == "действует";
            };

            this.Controls.Add(mainPanel);
            this.Controls.Add(headerPanel);
        }

        private void LoadArrangements()
        {
            var arrangements = _arrangementRepository.GetByChildId(_child.Id);

            lbArrangements.Items.Clear();
            foreach (var arr in arrangements)
            {
                string statusIcon = arr.Status == "действует" ? "🟢" : arr.Status == "завершено" ? "✅" : "🔴";
                string dateInfo = arr.EndDate.HasValue
                    ? $"{arr.StartDate:dd.MM.yyyy} - {arr.EndDate:dd.MM.yyyy}"
                    : $"с {arr.StartDate:dd.MM.yyyy} по настоящее время";

                lbArrangements.Items.Add(new ArrangementListItem
                {
                    Arrangement = arr,
                    DisplayText = $"{statusIcon} {arr.FosterParentName} | {arr.ArrangementType} | {dateInfo}"
                });
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new SelectFosterParentForm(_fosterParentRepository);
            form.FosterParentSelected += (fosterParent, arrangementType, startDate) =>
            {
                var arrangement = new FosterArrangement
                {
                    ChildId = _child.Id,
                    FosterParentId = fosterParent.Id,
                    StartDate = startDate,
                    EndDate = null,
                    ArrangementType = arrangementType,
                    Status = "действует"
                };

                _arrangementRepository.Add(arrangement);
                LoadArrangements();
                MessageBox.Show($"Ребёнок устроен в семью {fosterParent.FullName}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            form.ShowDialog();
        }

        private void BtnEnd_Click(object sender, EventArgs e)
        {
            if (lbArrangements.SelectedItem is ArrangementListItem item)
            {
                var result = MessageBox.Show($"Завершить устройство в семью {item.Arrangement.FosterParentName}?\n\nУкажите дату завершения:",
                    "Завершение устройства", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    using (var dateDialog = new DatePickerDialog())
                    {
                        if (dateDialog.ShowDialog() == DialogResult.OK)
                        {
                            _arrangementRepository.EndArrangement(item.Arrangement.Id, dateDialog.SelectedDate);
                            LoadArrangements();
                            MessageBox.Show("Устройство завершено", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private class ArrangementListItem
        {
            public FosterArrangement Arrangement { get; set; }
            public string DisplayText { get; set; }
        }
    }

    /// <summary>
    /// Форма выбора приёмного родителя
    /// </summary>
    public partial class SelectFosterParentForm : Form
    {
        private readonly FosterParentRepository _repository;
        private ListBox lbFosterParents;
        private ComboBox cbArrangementType;
        private DateTimePicker dtpStartDate;
        private Button btnSelect;

        public event Action<FosterParent, string, DateTime> FosterParentSelected;

        public SelectFosterParentForm(FosterParentRepository repository)
        {
            _repository = repository;
            InitializeComponent();
            LoadFosterParents();
        }

        private void InitializeComponent()
        {
            this.Text = "Выбрать приёмную семью";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            int yPos = 10;

            Label lblTitle = new Label
            {
                Text = "Выберите приёмного родителя/семью:",
                Location = new Point(10, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            mainPanel.Controls.Add(lblTitle);
            yPos += 35;

            lbFosterParents = new ListBox
            {
                Location = new Point(10, yPos),
                Size = new Size(460, 200),
                Font = new Font("Segoe UI", 10F),
                DisplayMember = "DisplayText"
            };
            mainPanel.Controls.Add(lbFosterParents);
            yPos += 215;

            Label lblType = new Label
            {
                Text = "Форма устройства:",
                Location = new Point(10, yPos + 8),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblType);

            cbArrangementType = new ComboBox
            {
                Location = new Point(150, yPos),
                Size = new Size(150, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbArrangementType.Items.AddRange(new[] { "приёмная семья", "опека", "усыновление/удочерение" });
            cbArrangementType.SelectedIndex = 0;
            mainPanel.Controls.Add(cbArrangementType);
            yPos += 45;

            Label lblDate = new Label
            {
                Text = "Дата устройства:",
                Location = new Point(10, yPos + 8),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblDate);

            dtpStartDate = new DateTimePicker
            {
                Location = new Point(150, yPos),
                Size = new Size(150, 27),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            mainPanel.Controls.Add(dtpStartDate);
            yPos += 50;

            btnSelect = new Button
            {
                Text = "Устроить",
                Location = new Point(360, yPos),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSelect.Click += BtnSelect_Click;
            mainPanel.Controls.Add(btnSelect);

            Button btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(240, yPos),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnCancel);

            this.Controls.Add(mainPanel);
        }

        private void LoadFosterParents()
        {
            var fosterParents = _repository.GetAll();

            lbFosterParents.Items.Clear();
            foreach (var fp in fosterParents)
            {
                string statusIcon = fp.Status == "приёмный родитель" ? "✅" : fp.Status == "кандидат" ? "📝" : "⭕";
                lbFosterParents.Items.Add(new FosterParentListItem
                {
                    FosterParent = fp,
                    DisplayText = $"{statusIcon} {fp.FullName} | {fp.Status} | {fp.Phone ?? "нет телефона"}"
                });
            }
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            if (lbFosterParents.SelectedItem is FosterParentListItem item)
            {
                if (item.FosterParent.Status != "приёмный родитель" && item.FosterParent.Status != "кандидат")
                {
                    MessageBox.Show("Этот родитель не может принимать детей", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                FosterParentSelected?.Invoke(item.FosterParent, cbArrangementType.SelectedItem.ToString(), dtpStartDate.Value);
                this.Close();
            }
            else
            {
                MessageBox.Show("Выберите приёмного родителя", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private class FosterParentListItem
        {
            public FosterParent FosterParent { get; set; }
            public string DisplayText { get; set; }
        }
    }

    /// <summary>
    /// Диалог выбора даты
    /// </summary>
    public partial class DatePickerDialog : Form
    {
        public DateTime SelectedDate { get; private set; }
        private DateTimePicker dtpDate;

        public DatePickerDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Выберите дату";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(280, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            dtpDate = new DateTimePicker
            {
                Location = new Point(20, 20),
                Size = new Size(220, 27),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            this.Controls.Add(dtpDate);

            Button btnOk = new Button
            {
                Text = "OK",
                Location = new Point(100, 70),
                Size = new Size(70, 30),
                DialogResult = DialogResult.OK
            };
            btnOk.Click += (s, e) => { SelectedDate = dtpDate.Value; };
            this.Controls.Add(btnOk);

            Button btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(180, 70),
                Size = new Size(70, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnCancel);
        }
    }
}