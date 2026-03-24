using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FamilyCenterApp.WinForms.Controls
{
    public partial class RecordCard : UserControl
    {
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblDetails;
        private Panel colorBar;
        private Button btnEdit;
        private Button btnDelete;

        public event EventHandler EditClicked;
        public event EventHandler DeleteClicked;

        public object TagData { get; set; }

        public RecordCard()
        {
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Size = new Size(410, 95);

            colorBar = new Panel
            {
                BackColor = Color.FromArgb(52, 152, 219),
                Dock = DockStyle.Left,
                Width = 8
            };

            lblTitle = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(20, 12),
                Size = new Size(280, 25)
            };

            lblSubtitle = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(20, 40),
                Size = new Size(280, 20)
            };

            lblDetails = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(20, 65),
                Size = new Size(240, 45)
            };

            btnEdit = new Button
            {
                BackColor = Color.FromArgb(52, 152, 219),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.White,
                Location = new Point(280, 55),
                Size = new Size(55, 25),
                Text = "✎",
                UseVisualStyleBackColor = false
            };
            btnEdit.Click += (s, e) => EditClicked?.Invoke(this, EventArgs.Empty);

            btnDelete = new Button
            {
                BackColor = Color.FromArgb(231, 76, 60),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.White,
                Location = new Point(340, 55),
                Size = new Size(55, 25),
                Text = "🗑",
                UseVisualStyleBackColor = false
            };
            btnDelete.Click += (s, e) => DeleteClicked?.Invoke(this, EventArgs.Empty);

            this.Controls.Add(colorBar);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblSubtitle);
            this.Controls.Add(lblDetails);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
        }

        public void SetContent(string title, string subtitle, string details, Color barColor)
        {
            lblTitle.Text = title;
            lblSubtitle.Text = subtitle;
            lblDetails.Text = details;
            colorBar.BackColor = barColor;
        }

        public void SetButtonsVisible(bool visible)
        {
            btnEdit.Visible = visible;
            btnDelete.Visible = visible;
        }
    }
}