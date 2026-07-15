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
            
            string bo = "CityData" ;

            var globals = new getBoData {
                boName = bo,
            } ;

            var options = ScriptOptions.Default
                    .WithReferences(typeof(BoUI).Assembly , typeof(Control).Assembly)
                    .WithImports("System" , "System.Windows.Forms" , "Script_runner");

            //script locationje
            string scriptPath = Path.Combine(Application.StartupPath , "Scripts" , "getBoData.csx");
            string code = File.ReadAllText(scriptPath);

            //futtatas es response
            var res = await CSharpScript.RunAsync(code , options , globals: globals , globalsType: typeof(getBoData));
            
            label1.Text = res.ReturnValue?.ToString() ?? "No return value";
        }
        private async void BoUI_Load(object sender , EventArgs e) {
        } 
    }
}
