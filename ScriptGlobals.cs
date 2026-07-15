using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using SharedModels ;

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
        public string country;
        public string county;
        public string city;
        public ApiClient api ;

    }

    public class getBoData {
        public string boName ;
    }

    public class InstanceBoGlobals {
        public string TypeName { get ; set; } = default! ;
        public string[] data ;

    }
}
