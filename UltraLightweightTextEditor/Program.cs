using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class SimpleEditor : Form
{
    private RichTextBox editor;

    private FindForm findForm;  // Embedded find control

    public SimpleEditor()
    {
        // Basic form setup
        this.Text = "Mini VS Code";
        this.Size = new Size(800, 600);
        this.BackColor = Color.FromArgb(30, 30, 30);
        this.KeyPreview = true; // Allows the form to capture key events
        this.KeyDown += SimpleEditor_KeyDown;

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

        // Initialize and embed the find form
        findForm = new FindForm(this);
        findForm.TopLevel = false; // Make it a child control
        findForm.FormBorderStyle = FormBorderStyle.None; // Optional: Remove border for a seamless look
        findForm.Size = new Size(300, 40); // Adjust size as needed
        this.Controls.Add(findForm);

        // Position it in the upper-right corner of the main form's client area
        findForm.Location = new Point(this.ClientSize.Width - findForm.Width, 0);
        // Anchor to top-right so it moves with the form
        findForm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        findForm.Hide();  // Start hidden; show when Ctrl+F is pressed
    }

    private void SimpleEditor_KeyDown(object sender, KeyEventArgs e)
    {
        // Toggle the visibility of the find control when Ctrl+F is pressed
        if (e.Control && e.KeyCode == Keys.F)
        {
            if (findForm.Visible)
                findForm.Hide();
            else
            {
                findForm.Show();
                findForm.BringToFront();
                findForm.Focus(); // Optionally, set focus to the textbox in the find form
            }
        }
    }

    // This method searches for the next occurrence of searchText
    public void FindNext(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
            return;

        // Start searching from the end of the current selection
        int startIndex = editor.SelectionStart + editor.SelectionLength;
        int index = editor.Text.IndexOf(searchText, startIndex, StringComparison.CurrentCultureIgnoreCase);

        // If not found, try from the beginning of the text
        if (index == -1 && startIndex > 0)
        {
            index = editor.Text.IndexOf(searchText, 0, StringComparison.CurrentCultureIgnoreCase);
        }

        if (index != -1)
        {
            editor.Select(index, searchText.Length);
            editor.ScrollToCaret();
            editor.Focus();
        }
        else
        {
            MessageBox.Show("Text not found", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
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
                string content = "";
                using (var stream = new FileStream(files[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }

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

public class FindForm : Form
{
    private TextBox txtFind;
    private Button btnFind;
    private SimpleEditor mainEditor;

    public FindForm(SimpleEditor editor)
    {
        mainEditor = editor;
        this.Text = "Find";
        this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        this.StartPosition = FormStartPosition.Manual;
        this.Size = new Size(320, 100);

        // Position the find form at the upper right corner of the main editor's client area.
        // This calculates the screen coordinates of the top-right corner.
        Point upperRight = mainEditor.PointToScreen(new Point(mainEditor.ClientSize.Width - this.Width, 0));
        this.Location = upperRight;

        txtFind = new TextBox();
        txtFind.Location = new Point(10, 10);
        txtFind.Width = 200;
        txtFind.KeyDown += TxtFind_KeyDown;
        this.Controls.Add(txtFind);

        btnFind = new Button();
        btnFind.Text = "Find Next";
        btnFind.Location = new Point(220, 10);
        btnFind.Click += BtnFind_Click;
        this.Controls.Add(btnFind);
    }


    private void BtnFind_Click(object sender, EventArgs e)
    {
        mainEditor.FindNext(txtFind.Text);
    }

    private void TxtFind_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            mainEditor.FindNext(txtFind.Text);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }
}
