using Microsoft.ClearScript.V8 ;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;


namespace Script_runner {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        public ApiClient Api { get; } = new ApiClient();

        private async void button1_Click(object sender , EventArgs e) {

            try
            {
                CityData city = new CityData();
                city.Country = textBox1.Text;
                city.County = textBox2.Text;
                city.City = textBox3.Text;
                

                var globals = new PrintGlobals
                {
                    Form = this,
                    City = city,
                };

                var options = ScriptOptions.Default
                    .WithReferences(typeof(Form1).Assembly, typeof(Control).Assembly)
                    .WithImports("System", "System.Windows.Forms" , "Script_runner");

                string scriptPath = Path.Combine(Application.StartupPath, "Scripts", "printobject.csx");
                string code = File.ReadAllText(scriptPath);

                await CSharpScript.RunAsync(code , options , globals: globals , globalsType: typeof(PrintGlobals));




            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void createbutton_Click(object sender , EventArgs e) {

            try {
                

                ///definialni egy valtozot a globalis hozzafereshez
                var globals = new InsertGlobals {
                    api = this.Api,
                    country = textBox1.Text,
                    county = textBox2.Text, 
                    city = textBox3.Text,
              
                };

                //a globalis hozzaferest biztositja a tipusokhoz
                var options = ScriptOptions.Default
                    .WithReferences(typeof(Form1).Assembly, typeof(Control).Assembly)
                    .WithImports("System", "System.Windows.Forms" , "Script_runner");

                //script locationje
                string scriptPath = Path.Combine(Application.StartupPath, "Scripts", "insertObject.csx");
                string code = File.ReadAllText(scriptPath);

                //futtatas es response
                var res = await CSharpScript.RunAsync(code , options , globals: globals , globalsType: typeof(InsertGlobals));


                MessageBox.Show(res.ReturnValue.ToString()) ;
                //MessageBox.Show(city.ToString()) ;

            } catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
            

        }
           

        private void deletbutton_Click(object sender , EventArgs e) {
            
        }
    }

    public class CityData {
        public string Country { get; set; }
        public string County { get; set; }
        public string City { get; set; }

        /*public override string ToString()
        {
            return $"{Country}, {County}, {City}";
        }*/

    }
}
