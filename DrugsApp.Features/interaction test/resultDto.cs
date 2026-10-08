public class ResultDto{
    public int drugId {get; private set;}
    public int drugformId {get; private set;}
    public List<int> activeSubId {get; private set;}


    public List<Pair> pairs {get; private set;}

    public List<string> Notes {get; private set;}

    public string uniqueAlcoholText {get; private set;}

    public string basicAlcoholText {get; private set;}

    public ResultDto(int drugId, int drugformId, List<int> activeSubId, List<Pair> pairs){
        this.drugId = drugId;
        this.drugformId = drugformId;
        this.activeSubId = activeSubId;
        this.pairs = pairs;
    }

    public void AddNote(string note){
        this.Notes.Add(note);
    }

    public void SetUniqueAlcoholText(string uniqueAlcoholText){
        this.uniqueAlcoholText = uniqueAlcoholText;
    }

    public void SetBasicAlcoholText(string basicAlcoholText){
        this.basicAlcoholText = basicAlcoholText;
    }
}