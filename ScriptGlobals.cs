using System;
using System.Collections.Generic;
using System.Text;

namespace Script_runner {
    public class ScriptGlobals
    {
        public Form1 MainForm;
        public Dictionary<string, object> Variables;
        public CityData City ;
    }

    public class PrintGlobals {
        public CityData City ;
        public Form1 Form;
        
    }

    public class InsertGlobals {
        public CityData City ;
        public ApiClient api ;
    }
}
