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
    /// Форма для просмотра детей кровного родителя
    /// </summary>
    public partial class ParentChildrenForm : Form
    {
        private readonly Parent _parent;
        private readonly ChildParentRepository _childParentRepository;
        private readonly ChildRepository _childRepository;

        private ListBox lbChildren;
        private Button btnViewChild;
        private Button btnRemoveRelation;
        private Button btnAddChild;
        private Button btnClose;

        public ParentChildrenForm(ParentRepository parentRepo, Parent parent)
        {
            _parent = parent;
            _childParentRepository = new ChildParentRepository();
            _childRepository = new ChildRepository();

            InitializeComponent();
            LoadChildren();
        }

        private void InitializeComponent()
        {
            this.Text = $"Дети родителя: {_parent.FullName}";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(550, 500);
            this.MinimumSize = new Size(550, 400);
            this.BackColor = Color.White;

            // Заголовок
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(155, 89, 182)
            };

            Label lblTitle = new Label
            {
                Text = $"Родитель: {_parent.FullName}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            string rightsStatus = GetParentalRightsStatusText(_parent.ParentalRightsStatus);
            Label lblSubtitle = new Label
            {
                Text = $"Статус прав: {rightsStatus} | Телефон: {_parent.Phone ?? "не указан"}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(230, 230, 230),
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

            // Список детей
            Label lblChildren = new Label
            {
                Text = "Дети:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblChildren);

            lbChildren = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(510, 280),
                Font = new Font("Segoe UI", 10F),
                DisplayMember = "DisplayText"
            };
            mainPanel.Controls.Add(lbChildren);

            // Кнопки
            btnViewChild = new Button
            {
                Text = "👁 Просмотреть ребёнка",
                Location = new Point(10, 330),
                Size = new Size(160, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnViewChild.Click += BtnViewChild_Click;
            mainPanel.Controls.Add(btnViewChild);

            btnRemoveRelation = new Button
            {
                Text = "✖ Удалить связь",
                Location = new Point(180, 330),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnRemoveRelation.Click += BtnRemoveRelation_Click;
            mainPanel.Controls.Add(btnRemoveRelation);

            btnAddChild = new Button
            {
                Text = "+ Добавить ребёнка",
                Location = new Point(320, 330),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddChild.Click += BtnAddChild_Click;
            mainPanel.Controls.Add(btnAddChild);

            btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(430, 400),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnClose);

            // Подписка на выбор в списке
            lbChildren.SelectedIndexChanged += (s, e) =>
            {
                bool hasSelection = lbChildren.SelectedItem != null;
                btnViewChild.Enabled = hasSelection;
                btnRemoveRelation.Enabled = hasSelection;
            };

            this.Controls.Add(mainPanel);
            this.Controls.Add(headerPanel);
        }

        private void LoadChildren()
        {
            var children = _childParentRepository.GetChildrenByParentId(_parent.Id);

            lbChildren.Items.Clear();
            foreach (var child in children)
            {
                string relationship = GetRelationshipFromChild(child);
                string ageText = $"{GetAge(child.BirthDate)} лет";

                lbChildren.Items.Add(new ChildListItem
                {
                    Child = child,
                    DisplayText = $"{child.FullName} | {ageText} | {child.LegalStatus} | {relationship}"
                });
            }

            if (children.Count == 0)
            {
                lbChildren.Items.Add(new ChildListItem
                {
                    Child = null,
                    DisplayText = "--- Нет детей ---"
                });
            }
        }

        private string GetRelationshipFromChild(Child child)
        {
            // Получаем тип родства из базы (нужно доработать)
            // Пока определяем по полу
            if (child.Patronymic != null && child.Patronymic.EndsWith("вна"))
                return "дочь";
            else if (child.Patronymic != null && child.Patronymic.EndsWith("вич"))
                return "сын";
            return "ребёнок";
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

        private int GetAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        private void BtnViewChild_Click(object sender, EventArgs e)
        {
            if (lbChildren.SelectedItem is ChildListItem item && item.Child != null)
            {
                // Открываем форму редактирования ребёнка
                var form = new ChildForm(_childRepository, item.Child);
                form.DataSaved += (s, ev) => LoadChildren(); // Обновляем список после изменений
                form.ShowDialog();
            }
        }

        private void BtnRemoveRelation_Click(object sender, EventArgs e)
        {
            if (lbChildren.SelectedItem is ChildListItem item && item.Child != null)
            {
                var result = MessageBox.Show($"Удалить связь с {item.Child.FullName}?\n\nСам ребёнок не будет удалён из базы.",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _childParentRepository.DeleteRelation(item.Child.Id, _parent.Id);
                    LoadChildren();
                    MessageBox.Show("Связь удалена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnAddChild_Click(object sender, EventArgs e)
        {
            // Форма выбора ребёнка
            var selectForm = new SelectChildForParentForm(_childRepository, _parent.Id);
            selectForm.ChildSelected += (child, relationshipType) =>
            {
                _childParentRepository.AddRelation(child.Id, _parent.Id, relationshipType);
                LoadChildren();
                MessageBox.Show($"Ребёнок {child.FullName} добавлен", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            selectForm.ShowDialog();
        }

        private class ChildListItem
        {
            public Child Child { get; set; }
            public string DisplayText { get; set; }
        }
    }

    /// <summary>
    /// Форма выбора ребёнка для добавления к родителю
    /// </summary>
    public partial class SelectChildForParentForm : Form
    {
        private readonly ChildRepository _childRepository;
        private readonly int _excludeParentId;
        private ListBox lbChildren;
        private ComboBox cbRelationshipType;
        private Button btnSelect;

        public event Action<Child, string> ChildSelected;

        public SelectChildForParentForm(ChildRepository childRepository, int excludeParentId)
        {
            _childRepository = childRepository;
            _excludeParentId = excludeParentId;
            InitializeComponent();
            LoadChildren();
        }

        private void InitializeComponent()
        {
            this.Text = "Выбрать ребёнка";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 450);
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
                Size = new Size(460, 250),
                Font = new Font("Segoe UI", 10F),
                DisplayMember = "DisplayText"
            };
            mainPanel.Controls.Add(lbChildren);
            yPos += 265;

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
            cbRelationshipType.Items.AddRange(new[] { "сын", "дочь", "пасынок/падчерица", "опекаемый" });
            cbRelationshipType.SelectedIndex = 0;
            mainPanel.Controls.Add(cbRelationshipType);
            yPos += 50;

            btnSelect = new Button
            {
                Text = "Выбрать",
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
                ChildSelected?.Invoke(item.Child, cbRelationshipType.SelectedItem.ToString());
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