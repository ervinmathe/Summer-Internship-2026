

//City.Country = "Hu" ;



using System.Threading.Tasks;

async Task modify(CityData City) {
    string name2 = string.Empty;
    try {
        int i = 0;
        int? i2 = null ;
        string name = null;
        int i3 = i2!.Value + i ;
        Form.resultLabel.Text = "Elotte" ;
        await Task.Delay(2000) ;
        Form.resultLabel.Text = "Utana" ; 
        name2 = name ;
    } catch(Exception ex) {
        Form.resultLabel.Text = ex.Message ;
    }
               
        
    await Task.Delay(2000) ;
    City.County = "Budapest" ;
    City.City = "Budapest" ;
    Form.resultLabel.Text = name2 ;
}

await modify(City) ;

//Form.resultLabel.Text = City.ToString() ;
