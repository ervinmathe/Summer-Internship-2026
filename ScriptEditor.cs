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
        private Button btnSave;
        private Button btnUpdate; // New Update Button
        private Button btnDelete;
        private Button btnOpenExternal;
        private Panel buttonPanel;
        private FindReplace myFindReplace;
        private ComboBox cmbScripts;
        private List<ExternalEditor> _availableEditors = new();

        // Local folder where uncompiled temp scripts are stored
        private readonly string _localScriptsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "LocalScripts");

        public string ScriptText {
            get => editor.Text;
            set => editor.Text = value;
        }

        public ScriptEditor() {
            InitializeComponent();

            // Ensure the local scripts directory exists
            Directory.CreateDirectory(_localScriptsFolder);

            // Button panel docked at the bottom
            buttonPanel = new Panel {
                Dock = DockStyle.Bottom ,
                Height = 40
            };

            // 1. SAVE BUTTON (Local Disk Save)
            btnSave = new Button {
                Text = "Save Local" ,
                Width = 90 ,
                Height = 30 ,
                Location = new Point(10 , 5)
            };
            btnSave.Click += BtnSave_Click;

            // 2. UPDATE BUTTON (Compile DLL & Upload to DB)
            btnUpdate = new Button {
                Text = "Update DB" ,
                Width = 90 ,
                Height = 30 ,
                Location = new Point(110 , 5)
            };
            btnUpdate.Click += BtnUpdate_Click;

            // 3. DELETE BUTTON
            btnDelete = new Button {
                Text = "Delete" ,
                Width = 80 ,
                Height = 30 ,
                Location = new Point(210 , 5)
            };
            btnDelete.Click += BtnDelete_Click;

            // 4. EXTERNAL EDITOR BUTTON
            btnOpenExternal = new Button {
                Text = "Open in External Editor" ,
                Width = 150 ,
                Height = 30 ,
                Location = new Point(300 , 5)
            };
            btnOpenExternal.Click += btnOpenExternal_Click;

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnUpdate);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnOpenExternal);

            // Script picker panel docked at the top
            var scriptPanel = new Panel {
                Dock = DockStyle.Top ,
                Height = 30
            };

            cmbScripts = new ComboBox {
                Dock = DockStyle.Fill ,
                DropDownStyle = ComboBoxStyle.DropDownList ,
                DisplayMember = nameof(ScriptListItem.Name)
            };
            cmbScripts.SelectedIndexChanged += CmbScripts_SelectedIndexChanged;
            scriptPanel.Controls.Add(cmbScripts);

            Controls.Add(buttonPanel);
            Controls.Add(scriptPanel);

            editor = new Scintilla {
                Dock = DockStyle.Fill ,
                Font = new Font("Consolas" , 10)
            };
            Controls.Add(editor);

            ConfigureCSharpLexer();
            editor.Text = "// your .csx script here\n";

            myFindReplace = new FindReplace(editor);

            editor.KeyDown += Editor_KeyDown;
            Load += ScriptEditor_Load;
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
        /// UPDATE BUTTON: Auto-saves current script locally, compiles all local scripts to DLL,
        /// uploads DLL to DB via ScriptModule, and deletes local temp scripts upon success.
        /// </summary>
        private async void BtnUpdate_Click(object sender , EventArgs e) {
            btnUpdate.Enabled = false;
            btnUpdate.Text = "Updating...";

            try {
                using var httpClient = new HttpClient();

                // 1. Auto-save current editor script locally if a name is selected/entered
                string currentScriptName = GetCurrentScriptName();
                if(!string.IsNullOrWhiteSpace(currentScriptName)) {
                    string localFilePath = GetLocalScriptPath(currentScriptName);
                    await File.WriteAllTextAsync(localFilePath , editor.Text);
                }

                // 2. Fetch all existing script names from the database
                var dbScriptNames = await httpClient.GetFromJsonAsync<List<string>>("http://localhost:5153/api/update/scriptNames")
                    ?? new List<string>();

                // 3. Download any script from the DB that IS NOT already in LocalScripts
                foreach(var name in dbScriptNames) {
                    string localPath = GetLocalScriptPath(name);

                    // If the local file doesn't exist yet, download from DB.
                    // If it DOES exist, leave it alone so your local edits are preserved!
                    if(!File.Exists(localPath)) {
                        string encodedName = Uri.EscapeDataString(name);
                        var response = await httpClient.GetFromJsonAsync<JsonElement>($"http://localhost:5153/api/update/getScriptContent/{encodedName}");

                        if(response.TryGetProperty("scriptContent" , out var contentProp)) {
                            string content = contentProp.GetString() ?? string.Empty;
                            await File.WriteAllTextAsync(localPath , content);
                        }
                    }
                }

                // 4. Get all script files in LocalScripts (now contains ALL DB scripts + local edits)
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

                    // 7. Clean up and delete all temp .csx files after successful compilation & DB upload
                    foreach(var file in allScriptFiles) {
                        if(File.Exists(file)) {
                            File.Delete(file);
                        }
                    }

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

        /// <summary>
        /// LOAD SCRIPT: Checks local folder first. If file exists, loads from disk. 
        /// Otherwise, loads script from Database.
        /// </summary>
        private async void CmbScripts_SelectedIndexChanged(object sender , EventArgs e) {
            // Get the selected script name safely regardless of underlying item type
            string selectedName = cmbScripts.SelectedItem?.ToString();

            if(!string.IsNullOrWhiteSpace(selectedName)) {
                await LoadScriptContentAsync(selectedName);
            }
        }

        private async Task LoadScriptContentAsync(string scriptName) {
            if(string.IsNullOrWhiteSpace(scriptName))
                return;

            try {
                string localFilePath = GetLocalScriptPath(scriptName);

                // 1. Load from local temp folder if uncompiled local edits exist
                if(File.Exists(localFilePath)) {
                    editor.Text = await File.ReadAllTextAsync(localFilePath);
                    return;
                }

                // 2. Otherwise load from the database
                using var httpClient = new HttpClient();
                var response = await httpClient.GetFromJsonAsync<JsonElement>($"http://localhost:5153/api/update/getScriptContent/{scriptName}");

                if(response.TryGetProperty("scriptContent" , out var contentProp)) {
                    editor.Text = contentProp.GetString() ?? string.Empty;
                } else {
                    editor.Text = string.Empty;
                }
            } catch(Exception ex) {
                MessageBox.Show($"Could not load script content for '{scriptName}': {ex.Message}" , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Error);
            }
        }

        private string GetCurrentScriptName() {
            return (cmbScripts.SelectedItem as ScriptListItem)?.Name ?? cmbScripts.Text;
        }

        private string GetLocalScriptPath(string scriptName) {
            string fileName = scriptName.EndsWith(".csx" , StringComparison.OrdinalIgnoreCase)
                ? scriptName
                : $"{scriptName}.csx";

            return Path.Combine(_localScriptsFolder , fileName);
        }

        // --- Standard form loading, lexer setup, external editor handlers below ---

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
                $"Are you sure you want to delete the local unsaved changes for '{scriptName}'?\n\nThis will revert the editor back to the version stored in the database." ,
                "Confirm Delete Local File" ,
                MessageBoxButtons.YesNo ,
                MessageBoxIcon.Warning);

            if(confirm == DialogResult.Yes) {
                try {
                    // 1. Delete the local temp .csx file
                    File.Delete(localFilePath);

                    // 2. Reload original code from DB to discard local edits
                    await LoadScriptContentAsync(scriptName);

                    MessageBox.Show($"Local file for '{scriptName}' deleted. Reverted to database version." , "Success" , MessageBoxButtons.OK , MessageBoxIcon.Information);
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

            try {
                using var httpClient = new HttpClient();
                var scripts = await httpClient.GetFromJsonAsync<List<string>>("http://localhost:5153/api/update/scriptNames");

                cmbScripts.DataSource = scripts;

                // Force-load the content for the initially selected script when opening the form
                if(cmbScripts.SelectedItem is string firstScriptName) {
                    await LoadScriptContentAsync(firstScriptName);
                }
            } catch(Exception ex) {
                MessageBox.Show($"Could not load scripts list: {ex.Message}" , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Error);
            }
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