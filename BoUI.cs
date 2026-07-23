using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Script_runner {
    public partial class BoUI : Form {
        public BoUI() {
            InitializeComponent();

            this.Shown += BoUI_afterload ;
        }
        private async void BoUI_afterload(object sender , EventArgs e) { 
            
            string bo = "CityData";

var globals = new getBoData
{
    boName = bo,
};

object result;

try
{
    // 1. Try to fetch the pre-compiled bytecode from the DB and run it instantly
    result = await PreCompiledScriptRunner.RunFromApiAsync("getBoData", globals);
}
catch (Exception ex) when (ex.Message.Contains("404") || ex.Message.Contains("not found"))
{
    // 2. FALLBACK: If the DB returns a 404, it means it's empty!
    var uploader = new ScriptUploaderService("http://localhost:5153/api/update");
    string scriptPath = Path.Combine(Application.StartupPath, "Scripts", "getBoData.csx");

    // Compile the local .csx file and insert it into the database
    await uploader.UploadScriptAsync(
        scriptName: "getBoData",
        scriptPath: scriptPath,
        globalsType: typeof(getBoData)
    );

    // 3. Try running it again now that it has been successfully uploaded
    result = await PreCompiledScriptRunner.RunFromApiAsync("getBoData", globals);
}

label1.Text = result?.ToString() ?? "No return value";
        }
        private async void BoUI_Load(object sender , EventArgs e) {
        } 
    }
}
