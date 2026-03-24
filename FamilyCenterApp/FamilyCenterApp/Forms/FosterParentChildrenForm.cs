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
    /// Форма для просмотра детей в приёмной семье
    /// </summary>
    public partial class FosterParentChildrenForm : Form
    {
        private readonly FosterParent _fosterParent;
        private readonly FosterArrangementRepository _arrangementRepository;
        private readonly ChildRepository _childRepository;

        private ListBox lbArrangements;
        private Button btnViewChild;
        private Button btnEndArrangement;
        private Button btnAddChild;
        private Button btnClose;

        public FosterParentChildrenForm(FosterParentRepository fosterRepo, FosterParent fosterParent)
        {
            _fosterParent = fosterParent;
            _arrangementRepository = new FosterArrangementRepository();
            _childRepository = new ChildRepository();

            InitializeComponent();
            LoadArrangements();
        }

        private void InitializeComponent()
        {
            this.Text = $"Дети в семье: {_fosterParent.FullName}";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(650, 550);
            this.MinimumSize = new Size(600, 450);
            this.BackColor = Color.White;

            // Заголовок
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(52, 152, 219)
            };

            Label lblTitle = new Label
            {
                Text = $"Приёмная семья: {_fosterParent.FullName}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            string statusIcon = _fosterParent.Status == "приёмный родитель" ? "✅" :
                                _fosterParent.Status == "кандидат" ? "📝" : "⭕";

            Label lblSubtitle = new Label
            {
                Text = $"{statusIcon} Статус: {_fosterParent.Status} | Телефон: {_fosterParent.Phone ?? "не указан"}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(230, 230, 230),
                Location = new Point(20, 50),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblSubtitle);

            Label lblInfo = new Label
            {
                Text = "Двойное нажатие по записи для просмотра ребёнка",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(20, 75),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblInfo);

            // Основная панель
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            // Список устройств
            Label lblArrangements = new Label
            {
                Text = "Дети, устроенные в семью:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblArrangements);

            lbArrangements = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(610, 320),
                Font = new Font("Segoe UI", 10F),
                DisplayMember = "DisplayText"
            };
            lbArrangements.DoubleClick += (s, e) => BtnViewChild_Click(s, e);
            mainPanel.Controls.Add(lbArrangements);

            // Кнопки
            btnViewChild = new Button
            {
                Text = "👁 Просмотреть ребёнка",
                Location = new Point(10, 370),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnViewChild.Click += BtnViewChild_Click;
            mainPanel.Controls.Add(btnViewChild);

            btnEndArrangement = new Button
            {
                Text = "✖ Завершить устройство",
                Location = new Point(170, 370),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnEndArrangement.Click += BtnEndArrangement_Click;
            mainPanel.Controls.Add(btnEndArrangement);

            btnAddChild = new Button
            {
                Text = "+ Устроить ребёнка",
                Location = new Point(330, 370),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddChild.Click += BtnAddChild_Click;
            mainPanel.Controls.Add(btnAddChild);

            btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(530, 450),
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
                bool hasSelection = lbArrangements.SelectedItem != null;
                btnViewChild.Enabled = hasSelection;
                if (hasSelection)
                {
                    var selected = lbArrangements.SelectedItem as ArrangementListItem;
                    btnEndArrangement.Enabled = selected?.Arrangement.Status == "действует";
                }
                else
                {
                    btnEndArrangement.Enabled = false;
                }
            };

            this.Controls.Add(mainPanel);
            this.Controls.Add(headerPanel);
        }

        private void LoadArrangements()
        {
            var arrangements = _arrangementRepository.GetByFosterParentId(_fosterParent.Id);

            lbArrangements.Items.Clear();
            foreach (var arr in arrangements)
            {
                string statusIcon = arr.Status == "действует" ? "🟢" :
                                    arr.Status == "завершено" ? "✅" : "🔴";
                string dateInfo = arr.EndDate.HasValue
                    ? $"{arr.StartDate:dd.MM.yyyy} - {arr.EndDate:dd.MM.yyyy}"
                    : $"с {arr.StartDate:dd.MM.yyyy} по настоящее время";
                string typeIcon = arr.ArrangementType == "усыновление/удочерение" ? "👨‍👩‍👧" :
                                  arr.ArrangementType == "опека" ? "👨‍👧" : "🏠";

                lbArrangements.Items.Add(new ArrangementListItem
                {
                    Arrangement = arr,
                    DisplayText = $"{statusIcon} {arr.ChildName} | {typeIcon} {arr.ArrangementType} | {dateInfo}"
                });
            }

            if (arrangements.Count == 0)
            {
                lbArrangements.Items.Add(new ArrangementListItem
                {
                    Arrangement = null,
                    DisplayText = "--- В семье нет детей ---"
                });
            }
        }

        private void BtnViewChild_Click(object sender, EventArgs e)
        {
            if (lbArrangements.SelectedItem is ArrangementListItem item && item.Arrangement != null)
            {
                var child = _childRepository.GetById(item.Arrangement.ChildId);
                if (child != null)
                {
                    var form = new ChildForm(_childRepository, child);
                    form.ShowDialog();
                }
            }
        }

        private void BtnEndArrangement_Click(object sender, EventArgs e)
        {
            if (lbArrangements.SelectedItem is ArrangementListItem item && item.Arrangement != null)
            {
                var result = MessageBox.Show($"Завершить устройство {item.Arrangement.ChildName}?\n\nУкажите дату завершения:",
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

        private void BtnAddChild_Click(object sender, EventArgs e)
        {
            if (_fosterParent.Status != "приёмный родитель" && _fosterParent.Status != "кандидат")
            {
                MessageBox.Show("Этот родитель не может принимать детей", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Форма выбора ребёнка для устройства
            var selectForm = new SelectChildForFamilyForm(_childRepository, _fosterParent.Id);
            selectForm.ChildSelected += (child, arrangementType, startDate) =>
            {
                var arrangement = new FosterArrangement
                {
                    ChildId = child.Id,
                    FosterParentId = _fosterParent.Id,
                    StartDate = startDate,
                    EndDate = null,
                    ArrangementType = arrangementType,
                    Status = "действует"
                };

                _arrangementRepository.Add(arrangement);
                LoadArrangements();
                MessageBox.Show($"Ребёнок {child.FullName} устроен в семью", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            selectForm.ShowDialog();
        }

        private class ArrangementListItem
        {
            public FosterArrangement Arrangement { get; set; }
            public string DisplayText { get; set; }
        }
    }

    /// <summary>
    /// Форма выбора ребёнка для устройства в приёмную семью
    /// </summary>
    public partial class SelectChildForFamilyForm : Form
    {
        private readonly ChildRepository _childRepository;
        private readonly int _excludeFosterParentId;
        private ListBox lbChildren;
        private ComboBox cbArrangementType;
        private DateTimePicker dtpStartDate;
        private Button btnSelect;

        public event Action<Child, string, DateTime> ChildSelected;

        public SelectChildForFamilyForm(ChildRepository childRepository, int excludeFosterParentId)
        {
            _childRepository = childRepository;
            _excludeFosterParentId = excludeFosterParentId;
            InitializeComponent();
            LoadChildren();
        }

        private void InitializeComponent()
        {
            this.Text = "Выбрать ребёнка для устройства";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 480);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            int yPos = 10;

            Label lblTitle = new Label
            {
                Text = "Выберите ребёнка:",
                Location = new Point(10, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            mainPanel.Controls.Add(lblTitle);
            yPos += 35;

            lbChildren = new ListBox
            {
                Location = new Point(10, yPos),
                Size = new Size(460, 220),
                Font = new Font("Segoe UI", 10F),
                DisplayMember = "DisplayText"
            };
            mainPanel.Controls.Add(lbChildren);
            yPos += 235;

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

        private void LoadChildren()
        {
            var allChildren = _childRepository.GetAll();

            lbChildren.Items.Clear();
            foreach (var child in allChildren)
            {
                int age = GetAge(child.BirthDate);
                lbChildren.Items.Add(new ChildListItem
                {
                    Child = child,
                    DisplayText = $"{child.FullName} | Возраст: {age} лет | Статус: {child.LegalStatus}"
                });
            }

            if (allChildren.Count == 0)
            {
                lbChildren.Items.Add(new ChildListItem
                {
                    Child = null,
                    DisplayText = "--- Нет детей в базе ---"
                });
            }
        }

        private int GetAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            if (lbChildren.SelectedItem is ChildListItem item && item.Child != null)
            {
                ChildSelected?.Invoke(item.Child, cbArrangementType.SelectedItem.ToString(), dtpStartDate.Value);
                this.Close();
            }
            else
            {
                MessageBox.Show("Выберите ребёнка", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private class ChildListItem
        {
            public Child Child { get; set; }
            public string DisplayText { get; set; }
        }
    }
}