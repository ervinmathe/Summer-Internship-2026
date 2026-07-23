using ScintillaNET;
using ScintillaNET_FindReplaceDialog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Script_runner {
    public partial class ScriptEditor : Form
    {
    private Scintilla editor;
    private Button btnSave;
    private Button btnDelete;
    private Panel buttonPanel;
    private FindReplace myFindReplace;
        private Button btnOpenExternal;

        public string ScriptText
    {
        get => editor.Text;
        set => editor.Text = value;
    }

    public ScriptEditor()
    {
        InitializeComponent();

        // Button panel docked at the bottom
        buttonPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40
        };

        btnSave = new Button
        {
            Text = "Save",
            Width = 80,
            Height = 30,
            Location = new Point(10, 5)
        };
        btnSave.Click += BtnSave_Click;

        btnDelete = new Button
        {
            Text = "Delete",
            Width = 80,
            Height = 30,
            Location = new Point(100, 5)
        };
        btnDelete.Click += BtnDelete_Click;
        
        btnOpenExternal = new Button
        {
            Text = "Open in External Editor",
            Width = 150,
            Height = 30,
            Location = new Point(190, 5)
        };
        btnOpenExternal.Click += btnOpenExternal_Click;




        buttonPanel.Controls.Add(btnSave);
        buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnOpenExternal);

            // Editor fills the remaining space — add panel BEFORE editor
            // so Dock.Fill doesn't get overridden
            Controls.Add(buttonPanel);

        editor = new Scintilla
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10)
        };
        Controls.Add(editor);

        ConfigureCSharpLexer();
        editor.Text = "// your .csx script here\n";

        myFindReplace = new FindReplace(editor);

        editor.KeyDown += Editor_KeyDown;
    }

    private async void BtnSave_Click(object sender, EventArgs e)
    {
        MessageBox.Show(ScriptText) ;
    }

    private async void BtnDelete_Click(object sender, EventArgs e)
    {
        var confirm = MessageBox.Show(
            "Delete this script?", "Confirm Delete",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (confirm == DialogResult.Yes)
        {
            // await scriptService.DeleteAsync(scriptId);
            this.Close();
        }
    }
        private async void btnOpenExternal_Click(object sender, EventArgs e)
        {
    string tempFile = Path.Combine(Path.GetTempPath(), $"script_{Guid.NewGuid()}.csx");
    editor.ReadOnly = true; // lock the internal editor
    btnOpenExternal.Enabled = false;
    try
    {
        await File.WriteAllTextAsync(tempFile, editor.Text);

        var psi = new ProcessStartInfo
        {
            FileName = "code", // VS Code CLI; must be on PATH ("code" command)
            Arguments = $"--wait \"{tempFile}\"",
            UseShellExecute = true
        };

        using (var process = Process.Start(psi))
        {
            await process.WaitForExitAsync(); // waits until the VS Code tab is closed
        }
        editor.ReadOnly = false; // unlock again
        btnOpenExternal.Enabled = true;
        string editedText = await File.ReadAllTextAsync(tempFile);
        editor.Text = editedText;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Could not open external editor: {ex.Message}");
    }
    finally
    {
        if (File.Exists(tempFile))
            File.Delete(tempFile);

        
    }
}


    private void ConfigureCSharpLexer()
{
            editor.LexerName = "cpp"; // set lexer first

            // 1. Reset & configure the DEFAULT style first
            editor.StyleResetDefault();
    editor.Styles[Style.Default].Font = "Consolas";
    editor.Styles[Style.Default].Size = 10;
    editor.Styles[Style.Default].BackColor = Color.White;
    editor.Styles[Style.Default].ForeColor = Color.Black;

    // 2. THEN clear all styles — this propagates the default to every style slot
    editor.StyleClearAll();

    // 3. NOW set your per-token colors — these must come AFTER StyleClearAll
    editor.Styles[Style.Cpp.Default].ForeColor = Color.Black;
    editor.Styles[Style.Cpp.Comment].ForeColor = Color.Green;
    editor.Styles[Style.Cpp.CommentLine].ForeColor = Color.Green;
    editor.Styles[Style.Cpp.Number].ForeColor = Color.DarkOrange;
    editor.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
    editor.Styles[Style.Cpp.Word].Bold = true;
    editor.Styles[Style.Cpp.String].ForeColor = Color.Maroon;
    editor.Styles[Style.Cpp.Identifier].ForeColor = Color.Black;
    editor.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Purple;

    // 4. Keywords AFTER lexer + styles are set
    editor.SetKeywords(0,
        "using namespace class public private protected internal static void " +
        "int long double float decimal bool string char object var new return " +
        "if else for foreach while switch case break continue try catch finally " +
        "throw null true false this base virtual override abstract sealed " +
        "readonly const struct enum interface namespace get set async await");
        }
private void Editor_KeyDown(object sender, KeyEventArgs e)
{
    if (e.Control && e.KeyCode == Keys.F)
    {
        myFindReplace.ShowFind();
        e.SuppressKeyPress = true;
    }
    else if (e.Control && e.KeyCode == Keys.H)
    {
        myFindReplace.ShowReplace();
        e.SuppressKeyPress = true;
    }
    else if (e.KeyCode == Keys.F3)
    {
        myFindReplace.Window.FindNext();
        e.SuppressKeyPress = true;
    }
}
}

}
