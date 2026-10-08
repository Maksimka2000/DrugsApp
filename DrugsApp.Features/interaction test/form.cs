


using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;  

public class form{

public int drugId { get; private set;}

public int drugformId { get; private set;}

public int activeSubId {get; private set;}

public bool isStomach {get; private set;} = true;




public form(int drugId, int drugformId, int activeSubId)
{
    this.drugId = drugId;
    this.drugformId = drugformId;
    this.activeSubId = activeSubId;
  
}

}