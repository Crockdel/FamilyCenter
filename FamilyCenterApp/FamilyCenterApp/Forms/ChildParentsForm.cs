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
    /// Форма для управления кровными родителями ребёнка
    /// </summary>
    public partial class ChildParentsForm : Form
    {
        private readonly Child _child;
        private readonly ChildParentRepository _childParentRepository;
        private readonly ParentRepository _parentRepository;

        private ListBox lbParents;
        private Button btnAddParent;
        private Button btnRemoveParent;
        private Button btnClose;

        public ChildParentsForm(Child child)
        {
            _child = child;
            _childParentRepository = new ChildParentRepository();
            _parentRepository = new ParentRepository();

            InitializeComponent();
            LoadParents();
        }

        private void InitializeComponent()
        {
            this.Text = $"Кровные родители: {_child.FullName}";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 450);
            this.MinimumSize = new Size(500, 400);
            this.BackColor = Color.White;

            // Заголовок
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
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
                Text = $"Дата рождения: {_child.BirthDate:dd.MM.yyyy} | Статус: {_child.LegalStatus}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(20, 38),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblSubtitle);

            // Основная панель
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            // Список родителей
            Label lblParents = new Label
            {
                Text = "Кровные родители:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblParents);

            lbParents = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(465, 250),
                Font = new Font("Segoe UI", 10F),
                DisplayMember = "DisplayText"
            };
            mainPanel.Controls.Add(lbParents);

            // Кнопки
            btnAddParent = new Button
            {
                Text = "+ Добавить родителя",
                Location = new Point(10, 300),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F)
            };
            btnAddParent.Click += BtnAddParent_Click;
            mainPanel.Controls.Add(btnAddParent);

            btnRemoveParent = new Button
            {
                Text = "✖ Удалить",
                Location = new Point(170, 300),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Enabled = false
            };
            btnRemoveParent.Click += BtnRemoveParent_Click;
            mainPanel.Controls.Add(btnRemoveParent);

            btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(400, 360),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnClose);

            // Подписка на событие выбора
            lbParents.SelectedIndexChanged += (s, e) =>
            {
                btnRemoveParent.Enabled = lbParents.SelectedItem != null;
            };

            this.Controls.Add(mainPanel);
            this.Controls.Add(headerPanel);
        }

        private void LoadParents()
        {
            var parents = _childParentRepository.GetParentsByChildId(_child.Id);

            lbParents.Items.Clear();
            foreach (var parent in parents)
            {
                string relationship = GetRelationshipFromParent(parent);
                string statusText = GetParentalRightsStatusText(parent.ParentalRightsStatus);

                lbParents.Items.Add(new ParentListItem
                {
                    Parent = parent,
                    DisplayText = $"{parent.FullName} ({relationship}) - {statusText}"
                });
            }
        }

        private string GetRelationshipFromParent(Parent parent)
        {
            // Получаем тип родства из базы (нужно доработать)
            // Пока определяем по полу
            if (parent.Patronymic != null && parent.Patronymic.EndsWith("вна"))
                return "мать";
            else if (parent.Patronymic != null && parent.Patronymic.EndsWith("вич"))
                return "отец";
            return "родитель";
        }

        private string GetParentalRightsStatusText(string status)
        {
            switch (status)
            {
                case "в правах": return "✅ в правах";
                case "ограничены в правах": return "⚠️ ограничены";
                case "лишены прав": return "❌ лишены";
                case "восстановлены в правах": return "🔄 восстановлены";
                default: return status;
            }
        }

        private void BtnAddParent_Click(object sender, EventArgs e)
        {
            // Форма выбора родителя
            var selectForm = new SelectParentForm(_parentRepository, _child.Id);
            selectForm.ParentSelected += (parent, relationshipType) =>
            {
                _childParentRepository.AddRelation(_child.Id, parent.Id, relationshipType);
                LoadParents();
            };
            selectForm.ShowDialog();
        }

        private void BtnRemoveParent_Click(object sender, EventArgs e)
        {
            if (lbParents.SelectedItem is ParentListItem item)
            {
                var result = MessageBox.Show($"Удалить связь с {item.Parent.FullName}?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _childParentRepository.DeleteRelation(_child.Id, item.Parent.Id);
                    LoadParents();
                }
            }
        }

        private class ParentListItem
        {
            public Parent Parent { get; set; }
            public string DisplayText { get; set; }
        }
    }

    /// <summary>
    /// Форма выбора родителя из списка
    /// </summary>
    public partial class SelectParentForm : Form
    {
        private readonly ParentRepository _parentRepository;
        private readonly int _excludeChildId;
        private ListBox lbParents;
        private ComboBox cbRelationshipType;
        private Button btnSelect;

        public event Action<Parent, string> ParentSelected;

        public SelectParentForm(ParentRepository parentRepository, int excludeChildId)
        {
            _parentRepository = parentRepository;
            _excludeChildId = excludeChildId;
            InitializeComponent();
            LoadParents();
        }

        private void InitializeComponent()
        {
            this.Text = "Выбрать родителя";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(450, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            int yPos = 10;

            Label lblTitle = new Label
            {
                Text = "Выберите родителя:",
                Location = new Point(10, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            mainPanel.Controls.Add(lblTitle);
            yPos += 30;

            lbParents = new ListBox
            {
                Location = new Point(10, yPos),
                Size = new Size(410, 200),
                Font = new Font("Segoe UI", 10F),
                DisplayMember = "DisplayText"
            };
            mainPanel.Controls.Add(lbParents);
            yPos += 210;

            Label lblType = new Label
            {
                Text = "Тип родства:",
                Location = new Point(10, yPos + 8),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblType);

            cbRelationshipType = new ComboBox
            {
                Location = new Point(120, yPos),
                Size = new Size(150, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbRelationshipType.Items.AddRange(new[] { "мать", "отец", "опекун (временный)", "иной родственник" });
            cbRelationshipType.SelectedIndex = 0;
            mainPanel.Controls.Add(cbRelationshipType);
            yPos += 50;

            btnSelect = new Button
            {
                Text = "Выбрать",
                Location = new Point(320, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSelect.Click += BtnSelect_Click;
            mainPanel.Controls.Add(btnSelect);

            Button btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(210, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnCancel);

            this.Controls.Add(mainPanel);
        }

        private void LoadParents()
        {
            var parents = _parentRepository.GetAll();

            lbParents.Items.Clear();
            foreach (var parent in parents)
            {
                lbParents.Items.Add(new ParentListItem
                {
                    Parent = parent,
                    DisplayText = $"{parent.FullName} | {parent.Phone ?? "нет телефона"} | {parent.ParentalRightsStatus}"
                });
            }
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            if (lbParents.SelectedItem is ParentListItem item)
            {
                ParentSelected?.Invoke(item.Parent, cbRelationshipType.SelectedItem.ToString());
                this.Close();
            }
            else
            {
                MessageBox.Show("Выберите родителя", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private class ParentListItem
        {
            public Parent Parent { get; set; }
            public string DisplayText { get; set; }
        }
    }
}