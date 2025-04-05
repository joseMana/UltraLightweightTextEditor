using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class SimpleEditor : Form
{
    private RichTextBox editor;

    public SimpleEditor()
    {
        // Basic form setup
        this.Text = "Mini VS Code";
        this.Size = new Size(800, 600);
        this.BackColor = Color.FromArgb(30, 30, 30);

        // Editor setup
        editor = new RichTextBox();
        editor.Dock = DockStyle.Fill;
        editor.Font = new Font("Consolas", 12);
        editor.BackColor = Color.FromArgb(30, 30, 30);
        editor.ForeColor = Color.White;
        editor.BorderStyle = BorderStyle.None;
        editor.AcceptsTab = true;
        editor.WordWrap = false;
        editor.TextChanged += Editor_TextChanged;
        editor.SelectionColor = Color.DeepSkyBlue;

        editor.AllowDrop = true;
        editor.DragEnter += Editor_DragEnter;
        editor.DragDrop += Editor_DragDrop;

        this.Controls.Add(editor);
    }

    private void Editor_TextChanged(object sender, EventArgs e)
    {
        int selectionStart = editor.SelectionStart;
        int selectionLength = editor.SelectionLength;

        editor.TextChanged -= Editor_TextChanged;

        // Reset all text color and background
        editor.SelectAll();
        editor.SelectionColor = Color.White;
        editor.SelectionBackColor = editor.BackColor;

        string text = editor.Text;

        // Color palette
        Color keywordColor = ColorTranslator.FromHtml("#569CD6");
        Color typeColor = ColorTranslator.FromHtml("#4EC9B0");
        Color stringColor = ColorTranslator.FromHtml("#D69D85");
        Color commentColor = ColorTranslator.FromHtml("#6A9955");
        Color methodColor = ColorTranslator.FromHtml("#DCDCAA");
        Color numberColor = ColorTranslator.FromHtml("#B5CEA8");

        // Patterns
        string[] keywords = { "public", "private", "protected", "static", "void", "return", "class", "using", "namespace", "if", "else", "for", "while", "switch", "case", "break", "new", "try", "catch", "finally" };
        string[] types = { "int", "string", "bool", "double", "float", "char", "var", "object", "decimal" };

        // Keywords
        foreach (var word in keywords)
        {
            foreach (Match m in Regex.Matches(text, $@"\b{word}\b"))
            {
                editor.Select(m.Index, m.Length);
                editor.SelectionColor = keywordColor;
                editor.SelectionBackColor = editor.BackColor;
            }
        }

        // Types
        foreach (var word in types)
        {
            foreach (Match m in Regex.Matches(text, $@"\b{word}\b"))
            {
                editor.Select(m.Index, m.Length);
                editor.SelectionColor = typeColor;
                editor.SelectionBackColor = editor.BackColor;
            }
        }

        // Strings
        foreach (Match m in Regex.Matches(text, "\".*?\""))
        {
            editor.Select(m.Index, m.Length);
            editor.SelectionColor = stringColor;
            editor.SelectionBackColor = editor.BackColor;
        }

        // Single-line comments
        foreach (Match m in Regex.Matches(text, @"//.*?$", RegexOptions.Multiline))
        {
            editor.Select(m.Index, m.Length);
            editor.SelectionColor = commentColor;
            editor.SelectionBackColor = editor.BackColor;
        }

        // Numbers
        foreach (Match m in Regex.Matches(text, @"\b\d+\b"))
        {
            editor.Select(m.Index, m.Length);
            editor.SelectionColor = numberColor;
            editor.SelectionBackColor = editor.BackColor;
        }

        // Method names (simple: anything followed by `(`)
        foreach (Match m in Regex.Matches(text, @"\b\w+(?=\s*\()"))
        {
            editor.Select(m.Index, m.Length);
            editor.SelectionColor = methodColor;
            editor.SelectionBackColor = editor.BackColor;
        }

        editor.SelectionStart = selectionStart;
        editor.SelectionLength = selectionLength;
        editor.SelectionColor = Color.White;
        editor.SelectionBackColor = editor.BackColor;

        editor.TextChanged += Editor_TextChanged;
    }

    private void Editor_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    private void Editor_DragDrop(object sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files.Length > 0)
        {
            try
            {
                string content = File.ReadAllText(files[0]);
                editor.Text = content;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading file: " + ex.Message);
            }
        }
    }


    [STAThread]
    public static void Main()
    {
        Application.Run(new SimpleEditor());
    }
}
