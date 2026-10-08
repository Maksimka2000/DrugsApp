public class drugFroms{
    public int drugId {get; private set;}
    public int drugformId {get; private set;}
    public List<int> activeSubIds {get; private set;}

    public List<string> notes {get; set;} = new List<string>();

    public drugFroms(int drugId, int drugformId, List<int> activeSubIds){
        this.drugId = drugId;
        this.drugformId = drugformId;
        this.activeSubIds = activeSubIds;

    }
}