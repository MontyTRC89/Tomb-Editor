using DarkUI.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.LevelData;

namespace TombLib.Controls
{
    [DefaultEvent(nameof(SelectedIndexChanged))]
    public partial class DarkSearchableComboBox : UserControl
    {
        public TRVersion.Game GameVersion = TRVersion.Game.TR4;
        public DarkComboBox Control { get { return combo; } }

        public ComboBox.ObjectCollection Items => combo.Items;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(106)]
        public int DropDownHeight { get { return combo.DropDownHeight; } set { combo.DropDownHeight = value; } }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(236)]
        public int DropDownWidth { get { return combo.DropDownWidth; } set { combo.DropDownWidth = value; } }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(true)]
        public bool IntegralHeight { get { return combo.IntegralHeight; } set { combo.IntegralHeight = value; } }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(15)]
        public int ItemHeight { get { return combo.ItemHeight; } set { combo.ItemHeight = value; } }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(true)]
        public bool FormattingEnabled { get { return combo.FormattingEnabled; } set { combo.FormattingEnabled = value; } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem { get { return combo.SelectedItem; } set { combo.SelectedItem = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex { get { return combo.SelectedIndex; } set { combo.SelectedIndex = value; } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedValue { get { return combo.SelectedValue; } set { combo.SelectedValue = value; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource { get { return combo.DataSource; } set { combo.DataSource = value; } }

        [Browsable(true)]
        public event ListControlConvertEventHandler Format { add { combo.Format += value; } remove { combo.Format -= value; } }
        [Browsable(true)]
        public event EventHandler SelectedIndexChanged { add { combo.SelectedIndexChanged += value; } remove { combo.SelectedIndexChanged -= value; } }
        [Browsable(true)]
        public event EventHandler SelectedValueChanged { add { combo.SelectedValueChanged += value; } remove { combo.SelectedValueChanged -= value; } }
        [Browsable(true)]
        public new event EventHandler Validated { add { combo.Validated += value; } remove { combo.Validated -= value; } }
        [Browsable(true)]
        public event EventHandler SelectionChangeCommitted { add { combo.SelectionChangeCommitted += value; } remove { combo.SelectionChangeCommitted -= value; } }

        public DarkSearchableComboBox()
        {
            InitializeComponent();
        }

        private void combo_Resize(object sender, EventArgs e)
        {
            button.Size = new Size(combo.Size.Height, combo.Size.Height);
            Size = new Size(Size.Width, combo.Size.Height);
        }

        private void DarkSearchableComboBox_Resize(object sender, EventArgs e)
        {
            Size = new Size(Size.Width, combo.Size.Height);
        }

        public void Search()
        {
            var searchPopUp = new PopUpSearch(combo, GameVersion);
            searchPopUp.Show(this);
        }

        private void button_Click(object sender, EventArgs e) => Search();
    }
}
