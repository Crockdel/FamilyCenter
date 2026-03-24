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
using FamilyCenterApp.WinForms.Controls;
using FamilyCenterApp.DataAccess.Repositories;

namespace FamilyCenterApp.WinForms.Forms
{
    public partial class MainForm : Form
    {
        private readonly ChildRepository _childRepository;
        private readonly ParentRepository _parentRepository;
        private readonly FosterParentRepository _fosterParentRepository;

        private ComboBox cbTableSelector;
        private TextBox tbSearch;
        private Button btnSearch;
        private Button btnAdd;
        private FlowLayoutPanel flowPanelCards;
        private Label lblStatus;
        private Panel headerPanel;
        private Panel filterPanel;

        private enum TableType { Children, Parents, FosterParents }
        private TableType _currentTable = TableType.Children;

        public MainForm()
        {
            _childRepository = new ChildRepository();
            _parentRepository = new ParentRepository();
            _fosterParentRepository = new FosterParentRepository();

            // Включаем автоматическое масштабирование
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Семейный центр - Управление данными";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(895, 620);
            this.BackColor = Color.FromArgb(236, 240, 241);

            // Верхняя панель (заголовок)
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(52, 73, 94),
                MinimumSize = new Size(0, 70)
            };

            Label lblTitle = new Label
            {
                Text = "Семейный центр \"Мой семейный центр\"",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            headerPanel.Controls.Add(lblTitle);
            lblTitle.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            // Панель фильтров
            filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10),
                MinimumSize = new Size(0, 60)
            };

            // Выбор таблицы
            Label lblTable = new Label
            {
                Text = "Таблица:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(15, 20)
            };
            filterPanel.Controls.Add(lblTable);
            lblTable.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            cbTableSelector = new ComboBox
            {
                Size = new Size(150, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(80, 17)
            };
            cbTableSelector.Items.AddRange(new[] { "Дети", "Кровные родители", "Приёмные родители" });
            cbTableSelector.SelectedIndex = 0;
            cbTableSelector.SelectedIndexChanged += (s, e) =>
            {
                _currentTable = (TableType)cbTableSelector.SelectedIndex;
                LoadData();
            };
            filterPanel.Controls.Add(cbTableSelector);
            cbTableSelector.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            // Поиск
            Label lblSearch = new Label
            {
                Text = "Поиск:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(260, 20)
            };
            filterPanel.Controls.Add(lblSearch);
            lblSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            tbSearch = new TextBox
            {
                Size = new Size(200, 27),
                Font = new Font("Segoe UI", 10F),
                Location = new Point(320, 17)
            };
            tbSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(); };
            filterPanel.Controls.Add(tbSearch);
            tbSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            btnSearch = new Button
            {
                Text = "Найти",
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(530, 15)
            };
            btnSearch.Click += (s, e) => LoadData();
            filterPanel.Controls.Add(btnSearch);
            btnSearch.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            btnAdd = new Button
            {
                Text = "+ Добавить",
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(0, 15)
            };
            btnAdd.Click += BtnAdd_Click;
            filterPanel.Controls.Add(btnAdd);
            btnAdd.Anchor = AnchorStyles.Right | AnchorStyles.Top;

            // Настраиваем кнопку "Добавить" для привязки к правому краю
            btnAdd.Location = new Point(filterPanel.Width - 120, 15);
            filterPanel.Resize += (s, e) =>
            {
                btnAdd.Location = new Point(filterPanel.Width - 120, 15);
            };

            // Панель карточек
            flowPanelCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(236, 240, 241),
                WrapContents = true,
                AutoSize = false
            };
            // Настраиваем автоматическое перераспределение карточек при изменении размера
            flowPanelCards.Resize += (s, e) =>
            {
                // Обновляем расположение карточек
                flowPanelCards.PerformLayout();
            };

            // Статусная строка
            lblStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(189, 195, 199),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Font = new Font("Segoe UI", 9F)
            };

            this.Controls.Add(flowPanelCards);
            this.Controls.Add(filterPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(lblStatus);

            // Обработчик изменения размера формы
            this.Resize += MainForm_Resize;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Обновляем расположение карточек при изменении размера окна
            flowPanelCards.PerformLayout();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Form addForm = null;
            switch (_currentTable)
            {
                case TableType.Children:
                    addForm = new ChildForm(_childRepository);
                    ((ChildForm)addForm).DataSaved += (s, ev) => LoadData();
                    break;
                case TableType.Parents:
                    addForm = new ParentForm(_parentRepository);
                    ((ParentForm)addForm).DataSaved += (s, ev) => LoadData();
                    break;
                case TableType.FosterParents:
                    addForm = new FosterParentForm(_fosterParentRepository);
                    ((FosterParentForm)addForm).DataSaved += (s, ev) => LoadData();
                    break;
            }
            addForm?.ShowDialog();
        }

        private void LoadData()
        {
            Cursor = Cursors.WaitCursor;
            flowPanelCards.Controls.Clear();

            string searchText = tbSearch.Text.Trim();
            int count = 0;

            try
            {
                switch (_currentTable)
                {
                    case TableType.Children:
                        var children = string.IsNullOrEmpty(searchText)
                            ? _childRepository.GetAll()
                            : _childRepository.Search(searchText);
                        count = children.Count;
                        foreach (var child in children)
                        {
                            var card = CreateChildCard(child);
                            flowPanelCards.Controls.Add(card);
                        }
                        break;

                    case TableType.Parents:
                        var parents = string.IsNullOrEmpty(searchText)
                            ? _parentRepository.GetAll()
                            : _parentRepository.Search(searchText);
                        count = parents.Count;
                        foreach (var parent in parents)
                        {
                            var card = CreateParentCard(parent);
                            flowPanelCards.Controls.Add(card);
                        }
                        break;

                    case TableType.FosterParents:
                        var fosterParents = string.IsNullOrEmpty(searchText)
                            ? _fosterParentRepository.GetAll()
                            : _fosterParentRepository.Search(searchText);
                        count = fosterParents.Count;
                        foreach (var foster in fosterParents)
                        {
                            var card = CreateFosterParentCard(foster);
                            flowPanelCards.Controls.Add(card);
                        }
                        break;
                }

                lblStatus.Text = $"Найдено записей: {count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Ошибка загрузки данных";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private RecordCard CreateChildCard(Child child)
        {
            var card = new RecordCard();
            string title = child.FullName;

            // Получаем кровных родителей с обработкой ошибок
            string parentsInfo = "Родители: не указаны";
            string fosterInfo = "Приёмная семья: не устроен";

            try
            {
                var childParentRepo = new ChildParentRepository();
                var parents = childParentRepo.GetParentsByChildId(child.Id);
                if (parents != null && parents.Count > 0)
                {
                    var parentNames = parents.Select(p =>
                    {
                        string name = p.FullName;
                        return string.IsNullOrEmpty(name) ? "неизвестно" : name;
                    });
                    parentsInfo = $"Родители: {string.Join(", ", parentNames)}";
                }
            }
            catch (Exception ex)
            {
                parentsInfo = "Родители: ошибка загрузки";
            }

            try
            {
                var arrangementRepo = new FosterArrangementRepository();
                var arrangements = arrangementRepo.GetByChildId(child.Id);
                var activeArrangements = arrangements?.Where(a => a.Status == "действует").ToList();
                if (activeArrangements != null && activeArrangements.Count > 0)
                {
                    var fosterNames = activeArrangements.Select(a => a.FosterParentName ?? "неизвестно");
                    fosterInfo = $"Приёмная семья: {string.Join(", ", fosterNames)}";
                }
            }
            catch (Exception ex)
            {
                fosterInfo = "Приёмная семья: ошибка загрузки";
            }

            string subtitle = $"Возраст: {child.Age} лет | Статус: {child.LegalStatus}";
            string details = $"{parentsInfo}\n{fosterInfo}\nДата поступления: {child.AdmissionDate:dd.MM.yyyy}";

            if (!string.IsNullOrEmpty(child.Notes))
                details += $"\n{child.Notes}";

            card.SetContent(title, subtitle, details, Color.FromArgb(52, 152, 219));
            card.TagData = child;
            card.EditClicked += (s, e) => EditChild(child);
            card.DeleteClicked += (s, e) => DeleteChild(child);

            return card;
        }

        private RecordCard CreateParentCard(Parent parent)
        {
            var card = new RecordCard();
            string title = parent.FullName;
            string subtitle = $"Статус прав: {parent.ParentalRightsStatus}";
            string details = !string.IsNullOrEmpty(parent.Phone) ? $"Телефон: {parent.Phone}" : "Телефон не указан";

            card.SetContent(title, subtitle, details, Color.FromArgb(155, 89, 182));
            card.TagData = parent;
            card.EditClicked += (s, e) => EditParent(parent);
            card.DeleteClicked += (s, e) => DeleteParent(parent);

            return card;
        }

        private RecordCard CreateFosterParentCard(FosterParent foster)
        {
            var card = new RecordCard();
            string title = foster.FullName;
            string subtitle = $"Статус: {foster.Status}";
            string details = !string.IsNullOrEmpty(foster.Phone) ? $"Телефон: {foster.Phone}" : "Телефон не указан";

            Color barColor = foster.Status == "приёмный родитель"
                ? Color.FromArgb(46, 204, 113)
                : foster.Status == "кандидат"
                    ? Color.FromArgb(241, 196, 15)
                    : Color.FromArgb(149, 165, 166);

            card.SetContent(title, subtitle, details, barColor);
            card.TagData = foster;
            card.EditClicked += (s, e) => EditFosterParent(foster);
            card.DeleteClicked += (s, e) => DeleteFosterParent(foster);

            // Добавляем двойной клик для просмотра детей
            card.DoubleClick += (s, e) =>
            {
                var arrangementRepo = new FosterArrangementRepository();
                var children = arrangementRepo.GetByFosterParentId(foster.Id);
                if (children.Count > 0)
                {
                    string message = $"Дети в семье {foster.FullName}:\n\n";
                    foreach (var child in children)
                    {
                        message += $"• {child.ChildName} - {child.ArrangementType} (с {child.StartDate:dd.MM.yyyy})\n";
                    }
                    MessageBox.Show(message, "Дети в семье", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"В семье {foster.FullName} нет детей", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            return card;
        }

        private void EditChild(Child child)
        {
            var form = new ChildForm(_childRepository, child);
            form.DataSaved += (s, e) => LoadData();
            form.ShowDialog();
        }

        private void EditParent(Parent parent)
        {
            var form = new ParentForm(_parentRepository, parent);
            form.DataSaved += (s, e) => LoadData();
            form.ShowDialog();
        }

        private void EditFosterParent(FosterParent foster)
        {
            var form = new FosterParentForm(_fosterParentRepository, foster);
            form.DataSaved += (s, e) => LoadData();
            form.ShowDialog();
        }

        private void DeleteChild(Child child)
        {
            var result = MessageBox.Show($"Удалить ребёнка {child.FullName}?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_childRepository.Delete(child.Id))
                    {
                        MessageBox.Show("Запись удалена", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteParent(Parent parent)
        {
            var result = MessageBox.Show($"Удалить родителя {parent.FullName}?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_parentRepository.Delete(parent.Id))
                    {
                        MessageBox.Show("Запись удалена", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteFosterParent(FosterParent foster)
        {
            var result = MessageBox.Show($"Удалить приёмного родителя {foster.FullName}?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (_fosterParentRepository.Delete(foster.Id))
                    {
                        MessageBox.Show("Запись удалена", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}