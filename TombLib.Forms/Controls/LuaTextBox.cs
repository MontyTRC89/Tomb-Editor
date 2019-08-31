using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;

namespace TombLib.Controls
{
    public partial class LuaTextBox : UserControl
    {
        private static TextStyle whitespaceColor = new TextStyle(new SolidBrush(Color.Gray), null, FontStyle.Regular);
        private static TextStyle commentColor = new TextStyle(new SolidBrush(Color.Green), null, FontStyle.Regular);
        private static TextStyle regularColor = new TextStyle(null, null, FontStyle.Regular);
        private static TextStyle operatorsColor = new TextStyle(new SolidBrush(Color.Orange), null, FontStyle.Bold);
        private static TextStyle keywordsColor = new TextStyle(new SolidBrush(Color.CornflowerBlue), null, FontStyle.Bold);

        private static string[] keywords = new string[]
       {
            "if",
            "end",
            "function",
            "for",
            "true",
            "false",
            "return",
            "then"
       };

        private static string[] operators = new string[]
        {
            "==",
            "and",
            "or",
            "~=",
            ":",
            @"\."
        };

        public LuaTextBox()
        {
            InitializeComponent();
        }

        public static void DoSyntaxHighlighting(TextChangedEventArgs e)
        {
            // Clear styles
            e.ChangedRange.ClearStyle(
                    commentColor, regularColor);

            // Apply styles (THE ORDER IS IMPORTANT!)
            e.ChangedRange.SetStyle(whitespaceColor, "·");
            e.ChangedRange.SetStyle(commentColor, @"--.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(regularColor, @"[\[\],]");
            e.ChangedRange.SetStyle(operatorsColor, @"(" + string.Join("|", operators) + @")");
            e.ChangedRange.SetStyle(keywordsColor, @"(" + string.Join("|", keywords) + @")");
        }

        public string Code
        {
            get { return textEditor.Text; }
            set { textEditor.Text = value; }
        }

        private void textEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            DoSyntaxHighlighting(e);
            textEditor.Invalidate();
        }
    }
}
