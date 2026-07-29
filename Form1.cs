using Microsoft.ClearScript.V8 ;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp;
using SharedModels;
using System.Net.Http;
using System.Text.Json;
using static ScintillaNET.Style;
using ScriptRunner.Core;
using ScriptRunner.Engine;


namespace Script_runner {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private string[] GetAllTextBoxValues() {
            return this.Controls
                .OfType<TextBox>()
                .Select(tb => tb.Text)
                .ToArray();
        }
        public CityData city ;

        public ApiClient Api { get; } = new ApiClient();

        private async void button1_Click(object sender , EventArgs e) {
            try {
                city = new CityData();
                city.Country = textBox1.Text;
                city.County = textBox2.Text;
                city.City = textBox3.Text;
                /*
                var globals = new PrintGlobals {
                    Form = this ,
                    City = city ,
                };

                object result;

                try {
                    // 1. Try to fetch the pre-compiled bytecode from the DB and run it instantly
                    result = await PreCompiledScriptRunner.RunFromApiAsync("printobject" , globals);
                } catch(Exception ex) when(ex.Message.Contains("404") || ex.Message.Contains("not found")) {
                    // 2. FALLBACK: If the DB returns a 404, it means it's empty!
                    // Corrected the URL here to target the base POST route: /api/update
                    var uploader = new ScriptUploaderService("http://localhost:5153/api/update");
                    string scriptPath = Path.Combine(Application.StartupPath , "Scripts" , "printobject.csx");

                    // Compile the local .csx file and insert it into the database
                    await uploader.UploadScriptAsync(
                        scriptName: "printobject" ,
                        scriptPath: scriptPath ,
                        globalsType: typeof(PrintGlobals)
                    );

                    // 3. Try running it again now that it has been successfully uploaded
                    result = await PreCompiledScriptRunner.RunFromApiAsync("printobject" , globals);
                }
                MessageBox.Show(result?.ToString() ?? "Script executed successfully!");*/
                var context = new ScriptContext
                {
                    TargetBo = city,       // the CityData instance this script expects
                    PropertyName = "",  
                    OldValue = 0,
                    NewValue = 0,
                    EventType = ScriptEventType.After,
                    UpdateStatus = text =>
                    {
                        if (resultLabel.InvokeRequired)
                            resultLabel.Invoke(() => resultLabel.Text = text);
                        else
                            resultLabel.Text = text;
                    }
                };

                ScriptResult result = await Task.Run(() => ScriptModule.ExecuteScript("printobject", context));

                MessageBox.Show(result.ReturnValue as string);

                
            } catch(Exception ex) {
                MessageBox.Show($"Execution failed: {ex.Message}");
            }
        }

        private async void createbutton_Click(object sender , EventArgs e) {

            try {
                /*

                ///definialni egy valtozot a globalis hozzafereshez
                var globals = new InsertGlobals {
                    api = this.Api ,
                    country = textBox1.Text ,
                    county = textBox2.Text ,
                    city = textBox3.Text ,

                };

                //a globalis hozzaferest biztositja a tipusokhoz
                var options = ScriptOptions.Default
                    .WithReferences(typeof(Form1).Assembly , typeof(Control).Assembly)
                    .WithImports("System" , "System.Windows.Forms" , "Script_runner");

                //script locationje
                string scriptPath = Path.Combine(Application.StartupPath , "Scripts" , "insertObject.csx");
                string code = File.ReadAllText(scriptPath);

                //futtatas es response
                var res = await CSharpScript.RunAsync(code , options , globals: globals , globalsType: typeof(InsertGlobals));


                MessageBox.Show(res.ReturnValue.ToString());
                //MessageBox.Show(city.ToString()) ;
                */
                city = new CityData();
                city.Country = textBox1.Text;
                city.County = textBox2.Text;
                city.City = textBox3.Text;

                var context = new ScriptContext
                {
                    TargetBo = city,       // the CityData instance this script expects
                    PropertyName = "",  
                    OldValue = 0,
                    NewValue = 0,
                    EventType = ScriptEventType.After,
                    UpdateStatus = null
                };

                ScriptResult result = await Task.Run(() => ScriptModule.ExecuteScript("insertObject", context));

                MessageBox.Show(result.ReturnValue as string);
            } catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }


        }


        private void deletbutton_Click(object sender , EventArgs e) {

        }

        private void button2_Click(object sender , EventArgs e) {
            BoUI boui = new BoUI();
            boui.Show();
        }

        private async void button3_Click(object sender , EventArgs e) {
            try {
                /*var globals = new InstanceBoGlobals {
                    TypeName = "CityData" ,
                    data = GetAllTextBoxValues() ,

                };

                var options = ScriptOptions.Default
                    .WithReferences(typeof(Form1).Assembly , typeof(Control).Assembly)
                    .WithImports("System" , "System.Windows.Forms" , "Script_runner");

                //script locationje
                string scriptPath = Path.Combine(Application.StartupPath , "Scripts" , "createBoInstance.csx");
                string code = File.ReadAllText(scriptPath);

                //futtatas es response
                var res = await CSharpScript.RunAsync(code , options , globals: globals , globalsType: typeof(InstanceBoGlobals));


                MessageBox.Show(res.ReturnValue.ToString());*/
                city = new CityData();
                city.Country = textBox1.Text;
                city.County = textBox2.Text;
                city.City = textBox3.Text;

                var context = new ScriptContext
                {
                    TargetBo = city,       // the CityData instance this script expects
                    PropertyName = "",  
                    OldValue = 0,
                    NewValue = 0,
                    EventType = ScriptEventType.After,
                    UpdateStatus = null
                };

                ScriptResult result = await Task.Run(() => ScriptModule.ExecuteScript("createBoInstance", context));

                MessageBox.Show(result.ReturnValue as string);


            } catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private async void button4_Click(object sender , EventArgs e) {
            var client = new HttpClient();

            var response = await client.DeleteAsync("http://localhost:5153/api/data/clearStore").Result.Content.ReadAsStringAsync();

            MessageBox.Show(response);
        }


        // Add this to your Form1.cs

        private async void btnCompileAndUpload_Click(object sender , EventArgs e) {
            /*try {
                var btn = (Button)sender;
                btn.Enabled = false;

                var uploader = new ScriptUploaderService("http://localhost:5153/api/update/printobject");
                string scriptPath = Path.Combine(Application.StartupPath , "Scripts" , "printobject.csx");

                // Capture the server's response payload
                string apiResponse = await uploader.UploadScriptAsync(
                    scriptName: "printobject" ,
                    scriptPath: scriptPath ,
                    globalsType: typeof(PrintGlobals)
                );

                // This will tell us exactly which table the API picked!
                MessageBox.Show($"Server Response:\n{apiResponse}" , "Upload Result");
            } catch(Exception ex) {
                MessageBox.Show($"Failed to upload script:\n{ex.Message}" , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Error);
            } finally {
                ((Button)sender).Enabled = true;
            }*/
        }

        private async  void button5_Click(object sender , EventArgs e) {

            /*string scriptname = "printobject";
            var client = new HttpClient();
            var currentScript = string.Empty;
            try {
                var response = await client.GetStringAsync($"http://localhost:5153/api/update/getScriptContent/{scriptname}");

                if( response != null )
                {
                    using JsonDocument doc = JsonDocument.Parse(response);
                    string scriptContent = doc.RootElement.GetProperty("scriptContent").GetString();
                    currentScript = scriptContent;
                }
            } catch(Exception ex) {
                MessageBox.Show($"Failed to fetch script content:\n{ex.Message}" , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Error);
                return;
            }*/
            

            using (var editorForm = new ScriptEditor())
            {
                ///editorForm.ScriptText = currentScript; 

                if (editorForm.ShowDialog(this) == DialogResult.OK)
                {
                    //currentScript = editorForm.ScriptText;
                }
            }
        }
    }


}
