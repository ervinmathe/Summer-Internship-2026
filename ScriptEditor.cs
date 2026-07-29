using Microsoft.Win32;
using ScintillaNET;
using ScintillaNET_FindReplaceDialog;
using ScriptRunner.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Script_runner {

    public partial class ScriptEditor : Form {
        private Scintilla editor;
        private Button btnNew;
        private Button btnSave;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnOpenExternal;
        private Panel buttonPanel;
        private FindReplace myFindReplace;
        private ComboBox cmbScripts;
        private List<ExternalEditor> _availableEditors = new();

        // Local folder where uncompiled temp/new scripts are stored
        private readonly string _localScriptsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "LocalScripts");

        public string ScriptText {
            get => editor.Text;
            set => editor.Text = value;
        }

        public ScriptEditor() {
            InitializeComponent();

            // Ensure local scripts directory exists
            Directory.CreateDirectory(_localScriptsFolder);

            // ----------------------------------------------------
            // 1. TOP PANEL (Script Dropdown)
            // ----------------------------------------------------
            var scriptPanel = new Panel {
                Dock = DockStyle.Fill ,
                Padding = new Padding(5 , 5 , 5 , 0)
            };

            cmbScripts = new ComboBox {
                Dock = DockStyle.Fill ,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbScripts.SelectedIndexChanged += CmbScripts_SelectedIndexChanged;
            scriptPanel.Controls.Add(cmbScripts);

            // ----------------------------------------------------
            // 2. BOTTOM PANEL (Action Buttons)
            // ----------------------------------------------------
            buttonPanel = new Panel {
                Dock = DockStyle.Fill
            };

            btnNew = new Button { Text = "New Script" , Width = 85 , Height = 30 , Location = new Point(10 , 5) };
            btnNew.Click += BtnNew_Click;

            btnSave = new Button { Text = "Save Local" , Width = 85 , Height = 30 , Location = new Point(100 , 5) };
            btnSave.Click += BtnSave_Click;

            btnUpdate = new Button { Text = "Update DB" , Width = 85 , Height = 30 , Location = new Point(190 , 5) };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button { Text = "Delete" , Width = 75 , Height = 30 , Location = new Point(280 , 5) };
            btnDelete.Click += BtnDelete_Click;

            btnOpenExternal = new Button { Text = "Open in External Editor" , Width = 150 , Height = 30 , Location = new Point(360 , 5) };
            btnOpenExternal.Click += btnOpenExternal_Click;

            buttonPanel.Controls.Add(btnNew);
            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnUpdate);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnOpenExternal);

            // ----------------------------------------------------
            // 3. MIDDLE EDITOR (ScintillaNET)
            // ----------------------------------------------------
            editor = new Scintilla {
                Dock = DockStyle.Fill ,
                Font = new Font("Consolas" , 10)
            };

            // ----------------------------------------------------
            // 4. LAYOUT GRID (Guarantees zero overlap)
            // ----------------------------------------------------
            var mainLayout = new TableLayoutPanel {
                Dock = DockStyle.Fill ,
                ColumnCount = 1 ,
                RowCount = 3 ,
                Margin = new Padding(0) ,
                Padding = new Padding(0)
            };

            // Row 0: Top Panel (35px fixed height)
            // Row 1: Editor (100% remaining space)
            // Row 2: Bottom Buttons (40px fixed height)
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent , 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute , 35F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent , 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute , 40F));

            mainLayout.Controls.Add(scriptPanel , 0 , 0);
            mainLayout.Controls.Add(editor , 0 , 1);
            mainLayout.Controls.Add(buttonPanel , 0 , 2);

            // Add only the layout grid to the Form
            Controls.Add(mainLayout);

            // Editor configuration & events
            ConfigureCSharpLexer();
            editor.Text = "// your .csx script here\n";

            myFindReplace = new FindReplace(editor);

            editor.KeyDown += Editor_KeyDown;
            Load += ScriptEditor_Load;
        }

        /// <summary>
        /// NEW BUTTON: Prompts for a script name, creates a local draft file, 
        /// updates the dropdown list, and opens it in the editor.
        /// </summary>
        private async void BtnNew_Click(object sender , EventArgs e) {
            string scriptName = PromptForInput("Create New Script" , "Enter script name:");

            if(string.IsNullOrWhiteSpace(scriptName))
                return;

            // Sanitize file name
            string cleanName = string.Concat(scriptName.Split(Path.GetInvalidFileNameChars())).Trim();
            if(string.IsNullOrWhiteSpace(cleanName)) {
                MessageBox.Show("Please enter a valid file name." , "Invalid Name" , MessageBoxButtons.OK , MessageBoxIcon.Warning);
                return;
            }

            string localPath = GetLocalScriptPath(cleanName);

            if(File.Exists(localPath)) {
                MessageBox.Show($"A local script named '{cleanName}' already exists." , "Script Exists" , MessageBoxButtons.OK , MessageBoxIcon.Information);
                await RefreshScriptListAsync(cleanName);
                return;
            }

            try {
                // Create starter code template
                string starterCode = $"// {cleanName}.csx\nusing System;\n\n// Script implementation\n\n";
                await File.WriteAllTextAsync(localPath , starterCode);

                // Refresh dropdown list and select the new script
                await RefreshScriptListAsync(cleanName);
                editor.Text = starterCode;

                MessageBox.Show($"New script '{cleanName}' created locally. Use 'Update DB' to compile and upload it to the database." , "Script Created" , MessageBoxButtons.OK , MessageBoxIcon.Information);
            } catch(Exception ex) {
                MessageBox.Show($"Could not create script file: {ex.Message}" , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// SAVE BUTTON: Saves the active script locally to disk as a .csx file
        /// </summary>
        private async void BtnSave_Click(object sender , EventArgs e) {
            string scriptName = GetCurrentScriptName();

            if(string.IsNullOrWhiteSpace(scriptName)) {
                MessageBox.Show("Please select or enter a valid script name." , "Warning" , MessageBoxButtons.OK , MessageBoxIcon.Warning);
                return;
            }

            try {
                string localFilePath = GetLocalScriptPath(scriptName);
                await File.WriteAllTextAsync(localFilePath , editor.Text);

                MessageBox.Show($"Script saved locally as '{Path.GetFileName(localFilePath)}'." , "Saved Locally" , MessageBoxButtons.OK , MessageBoxIcon.Information);
            } catch(Exception ex) {
                MessageBox.Show($"Could not save local script: {ex.Message}" , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// UPDATE BUTTON: Pulls all scripts from DB, merges with local edits/new scripts, 
        /// compiles DLL, uploads to DB via /api/update, and cleans up temp files.
        /// </summary>
        private async void BtnUpdate_Click(object sender , EventArgs e) {
            btnUpdate.Enabled = false;
            btnUpdate.Text = "Updating...";

            try {
                using var httpClient = new HttpClient();

                // 1. Auto-save current editor script locally if selected
                string currentScriptName = GetCurrentScriptName();
                if(!string.IsNullOrWhiteSpace(currentScriptName)) {
                    string localFilePath = GetLocalScriptPath(currentScriptName);
                    await File.WriteAllTextAsync(localFilePath , editor.Text);
                }

                // 2. Fetch all existing script names from the database
                var dbScriptNames = await httpClient.GetFromJsonAsync<List<string>>("http://localhost:5153/api/update/scriptNames")
                    ?? new List<string>();

                // 3. Download any script from DB that IS NOT already in LocalScripts
                foreach(var name in dbScriptNames) {
                    string localPath = GetLocalScriptPath(name);

                    if(!File.Exists(localPath)) {
                        string encodedName = Uri.EscapeDataString(name);
                        var response = await httpClient.GetFromJsonAsync<JsonElement>($"http://localhost:5153/api/update/getScriptContent/{encodedName}");

                        if(response.TryGetProperty("scriptContent" , out var contentProp)) {
                            string content = contentProp.GetString() ?? string.Empty;
                            await File.WriteAllTextAsync(localPath , content);
                        }
                    }
                }

                // 4. Get all script files in LocalScripts
                var allScriptFiles = Directory.GetFiles(_localScriptsFolder , "*.csx");
                if(allScriptFiles.Length == 0) {
                    MessageBox.Show("No scripts found to compile." , "Information" , MessageBoxButtons.OK , MessageBoxIcon.Information);
                    return;
                }

                // 5. Force re-compilation by clearing old DLL cache
                if(File.Exists("GeneratedScripts.dll")) {
                    File.Delete("GeneratedScripts.dll");
                }

                // Compile all local .csx files together into GeneratedScripts.dll
                bool success = await ScriptModule.InitializeScriptsAsync(_localScriptsFolder , httpClient);

                if(success) {
                    // 6. Insert/Update script contents in database via /api/update
                    foreach(var file in allScriptFiles) {
                        string name = Path.GetFileNameWithoutExtension(file);
                        string content = await File.ReadAllTextAsync(file);

                        var updatePayload = new {
                            ScriptName = name ,
                            ScriptContent = content
                        };

                        var updateResponse = await httpClient.PostAsJsonAsync("http://localhost:5153/api/update" , updatePayload);
                        updateResponse.EnsureSuccessStatusCode();
                    }

                    // 7. Clean up and delete all temp .csx files after successful compile & DB upload
                    foreach(var file in allScriptFiles) {
                        if(File.Exists(file)) {
                            File.Delete(file);
                        }
                    }

                    // Refresh script list from database
                    await RefreshScriptListAsync(currentScriptName);

                    MessageBox.Show("All scripts pulled, compiled into DLL, saved to database, and temp files cleaned up!" , "Success" , MessageBoxButtons.OK , MessageBoxIcon.Information);
                } else {
                    MessageBox.Show("Compilation skipped or no scripts processed." , "Notice" , MessageBoxButtons.OK , MessageBoxIcon.Warning);
                }
            } catch(Exception ex) {
                MessageBox.Show($"Failed to compile and update database: {ex.Message}" , "Build Error" , MessageBoxButtons.OK , MessageBoxIcon.Error);
            } finally {
                btnUpdate.Enabled = true;
                btnUpdate.Text = "Update DB";
            }
        }

        private async void CmbScripts_SelectedIndexChanged(object sender , EventArgs e) {
            string selectedName = GetCurrentScriptName();

            if(!string.IsNullOrWhiteSpace(selectedName)) {
                await LoadScriptContentAsync(selectedName);
            }
        }

        private async Task LoadScriptContentAsync(string scriptName) {
            if(string.IsNullOrWhiteSpace(scriptName))
                return;

            try {
                string localFilePath = GetLocalScriptPath(scriptName);

                // 1. Load from local temp folder if uncompiled local edits / new script exist
                if(File.Exists(localFilePath)) {
                    editor.Text = await File.ReadAllTextAsync(localFilePath);
                    return;
                }

                // 2. Otherwise load from database
                using var httpClient = new HttpClient();
                string encodedName = Uri.EscapeDataString(scriptName);
                var response = await httpClient.GetFromJsonAsync<JsonElement>($"http://localhost:5153/api/update/getScriptContent/{encodedName}");

                if(response.TryGetProperty("scriptContent" , out var contentProp)) {
                    editor.Text = contentProp.GetString() ?? string.Empty;
                } else {
                    editor.Text = string.Empty;
                }
            } catch(Exception ex) {
                MessageBox.Show($"Could not load script content for '{scriptName}': {ex.Message}" , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Fetches script names from DB, combines them with local uncompiled .csx files,
        /// and refreshes the ComboBox dropdown.
        /// </summary>
        private async Task RefreshScriptListAsync(string selectScriptName = null) {
            List<string> scriptNames = new();

            try {
                using var httpClient = new HttpClient();
                var dbScripts = await httpClient.GetFromJsonAsync<List<string>>("http://localhost:5153/api/update/scriptNames");
                if(dbScripts != null)
                    scriptNames.AddRange(dbScripts);
            } catch {
                // Ignore API connection issues; local files will still be displayed
            }

            // Also check for local .csx files that haven't been pushed to DB yet
            if(Directory.Exists(_localScriptsFolder)) {
                var localFiles = Directory.GetFiles(_localScriptsFolder , "*.csx")
                    .Select(Path.GetFileNameWithoutExtension);
                scriptNames.AddRange(localFiles);
            }

            // Remove duplicates & sort
            var uniqueScripts = scriptNames.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s).ToList();

            cmbScripts.DataSource = uniqueScripts;

            // Restore/Set selected script index
            if(!string.IsNullOrWhiteSpace(selectScriptName) && uniqueScripts.Contains(selectScriptName , StringComparer.OrdinalIgnoreCase)) {
                cmbScripts.SelectedItem = uniqueScripts.First(s => s.Equals(selectScriptName , StringComparison.OrdinalIgnoreCase));
            } else if(uniqueScripts.Count > 0) {
                cmbScripts.SelectedIndex = 0;
            }
        }

        private string GetCurrentScriptName() {
            return cmbScripts.SelectedItem?.ToString() ?? cmbScripts.Text.Trim();
        }

        private string GetLocalScriptPath(string scriptName) {
            string cleanName = Path.GetFileNameWithoutExtension(scriptName);
            return Path.Combine(_localScriptsFolder , $"{cleanName}.csx");
        }

        private async void BtnDelete_Click(object sender , EventArgs e) {
            string scriptName = GetCurrentScriptName();

            if(string.IsNullOrWhiteSpace(scriptName)) {
                MessageBox.Show("No script is currently selected." , "Warning" , MessageBoxButtons.OK , MessageBoxIcon.Warning);
                return;
            }

            string localFilePath = GetLocalScriptPath(scriptName);

            if(!File.Exists(localFilePath)) {
                MessageBox.Show($"No local temporary file found for '{scriptName}'." , "Information" , MessageBoxButtons.OK , MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to delete local changes/file for '{scriptName}'?" ,
                "Confirm Delete" ,
                MessageBoxButtons.YesNo ,
                MessageBoxIcon.Warning);

            if(confirm == DialogResult.Yes) {
                try {
                    File.Delete(localFilePath);
                    await RefreshScriptListAsync();
                    MessageBox.Show($"Local file for '{scriptName}' deleted." , "Success" , MessageBoxButtons.OK , MessageBoxIcon.Information);
                } catch(Exception ex) {
                    MessageBox.Show($"Could not delete local file: {ex.Message}" , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Error);
                }
            }
        }

        private async void ScriptEditor_Load(object sender , EventArgs e) {
            _availableEditors = DetectExternalEditors();
            btnOpenExternal.Enabled = _availableEditors.Count > 0;
            btnOpenExternal.Text = _availableEditors.Count switch {
                0 => "No External Editor Found",
                1 => $"Open in {_availableEditors[0].Name}",
                _ => "Open in External Editor"
            };

            await RefreshScriptListAsync();

            string firstScriptName = GetCurrentScriptName();
            if(!string.IsNullOrWhiteSpace(firstScriptName)) {
                await LoadScriptContentAsync(firstScriptName);
            }
        }

        /// <summary>
        /// Lightweight modal input dialog to prompt the user for script name.
        /// </summary>
        private static string PromptForInput(string title , string promptText) {
            using Form prompt = new Form {
                Width = 350 ,
                Height = 160 ,
                FormBorderStyle = FormBorderStyle.FixedDialog ,
                Text = title ,
                StartPosition = FormStartPosition.CenterParent ,
                MaximizeBox = false ,
                MinimizeBox = false
            };

            Label textLabel = new Label { Left = 15 , Top = 15 , Text = promptText , AutoSize = true };
            TextBox textBox = new TextBox { Left = 15 , Top = 40 , Width = 300 };
            Button confirmation = new Button { Text = "OK" , Left = 150 , Width = 80 , Top = 75 , DialogResult = DialogResult.OK };
            Button cancel = new Button { Text = "Cancel" , Left = 235 , Width = 80 , Top = 75 , DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
        }

        private void btnOpenExternal_Click(object sender , EventArgs e) {
            if(_availableEditors.Count == 0) {
                MessageBox.Show("No supported external editor (VS Code or Notepad++) was found on this computer.");
                return;
            }

            if(_availableEditors.Count == 1) {
                _ = OpenInExternalEditorAsync(_availableEditors[0]);
                return;
            }

            var menu = new ContextMenuStrip();
            foreach(var ed in _availableEditors)
                menu.Items.Add(ed.Name , null , async (s , args) => await OpenInExternalEditorAsync(ed));

            menu.Show(btnOpenExternal , new Point(0 , btnOpenExternal.Height));
        }

        private async Task OpenInExternalEditorAsync(ExternalEditor ed) {
            string tempFile = Path.Combine(Path.GetTempPath() , $"script_{Guid.NewGuid()}.csx");
            editor.ReadOnly = true;
            btnOpenExternal.Enabled = false;
            try {
                await File.WriteAllTextAsync(tempFile , editor.Text);

                var psi = new ProcessStartInfo {
                    FileName = ed.Path ,
                    Arguments = string.Format(ed.ArgsTemplate , tempFile) ,
                    UseShellExecute = false
                };

                using var process = Process.Start(psi);
                if(process != null) {
                    await process.WaitForExitAsync();
                }

                string updatedContent = await File.ReadAllTextAsync(tempFile);
                editor.ReadOnly = false;
                editor.Text = updatedContent;
            } catch(Exception ex) {
                MessageBox.Show($"Could not open external editor: {ex.Message}");
            } finally {
                editor.ReadOnly = false;
                btnOpenExternal.Enabled = _availableEditors.Count > 0;
                if(File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        private static List<ExternalEditor> DetectExternalEditors() {
            var found = new List<ExternalEditor>();

            var vsCodePath = FindVsCodePath();
            if(vsCodePath != null)
                found.Add(new ExternalEditor("VS Code" , vsCodePath , "--wait \"{0}\""));

            var notepadPlusPath = FindNotepadPlusPlusPath();
            if(notepadPlusPath != null)
                found.Add(new ExternalEditor("Notepad++" , notepadPlusPath , "-multiInst -nosession \"{0}\""));

            return found;
        }

        private static string FindVsCodePath() {
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe"),
                @"C:\Program Files\Microsoft VS Code\Code.exe",
                @"C:\Program Files (x86)\Microsoft VS Code\Code.exe"
            };

            foreach(var path in candidates)
                if(File.Exists(path))
                    return path;

            return FindOnPath("Code.exe") ?? FindOnPath("code.cmd");
        }

        private static string FindNotepadPlusPlusPath() {
            var candidates = new[]
            {
                @"C:\Program Files\Notepad++\notepad++.exe",
                @"C:\Program Files (x86)\Notepad++\notepad++.exe"
            };

            foreach(var path in candidates)
                if(File.Exists(path))
                    return path;

            using(var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Notepad++")) {
                var installDir = key?.GetValue("")?.ToString();
                if(!string.IsNullOrEmpty(installDir)) {
                    var exe = Path.Combine(installDir , "notepad++.exe");
                    if(File.Exists(exe))
                        return exe;
                }
            }

            return FindOnPath("notepad++.exe");
        }

        private static string FindOnPath(string exeName) {
            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach(var dir in pathEnv.Split(Path.PathSeparator)) {
                try {
                    var fullPath = Path.Combine(dir , exeName);
                    if(File.Exists(fullPath))
                        return fullPath;
                } catch { }
            }
            return null;
        }

        private void ConfigureCSharpLexer() {
            editor.LexerName = "cpp";
            editor.StyleResetDefault();
            editor.Styles[Style.Default].Font = "Consolas";
            editor.Styles[Style.Default].Size = 10;
            editor.Styles[Style.Default].BackColor = Color.White;
            editor.Styles[Style.Default].ForeColor = Color.Black;

            editor.StyleClearAll();

            editor.Styles[Style.Cpp.Default].ForeColor = Color.Black;
            editor.Styles[Style.Cpp.Comment].ForeColor = Color.Green;
            editor.Styles[Style.Cpp.CommentLine].ForeColor = Color.Green;
            editor.Styles[Style.Cpp.Number].ForeColor = Color.DarkOrange;
            editor.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            editor.Styles[Style.Cpp.Word].Bold = true;
            editor.Styles[Style.Cpp.String].ForeColor = Color.Maroon;
            editor.Styles[Style.Cpp.Identifier].ForeColor = Color.Black;
            editor.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Purple;

            editor.SetKeywords(0 ,
                "using namespace class public private protected internal static void " +
                "int long double float decimal bool string char object var new return " +
                "if else for foreach while switch case break continue try catch finally " +
                "throw null true false this base virtual override abstract sealed " +
                "readonly const struct enum interface namespace get set async await");
        }

        private void Editor_KeyDown(object sender , KeyEventArgs e) {
            if(e.Control && e.KeyCode == Keys.F) {
                myFindReplace.ShowFind();
                e.SuppressKeyPress = true;
            } else if(e.Control && e.KeyCode == Keys.H) {
                myFindReplace.ShowReplace();
                e.SuppressKeyPress = true;
            } else if(e.KeyCode == Keys.F3) {
                myFindReplace.Window.FindNext();
                e.SuppressKeyPress = true;
            }
        }

        public record ExternalEditor(string Name , string Path , string ArgsTemplate);
    }

    public class ScriptListItem {
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;
    }
}