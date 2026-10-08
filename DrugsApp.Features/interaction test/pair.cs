

public class Pair{
    public form form1 {get; private set;}
    public int activeSubId1 {get; private set;}
    public form form2 {get; private set;}
    public int activeSubId2 {get; private set;}

    public int idofinteraction {get; private set;}

    public string text {get; private set;}

    public string stomachText {get; private set;}

    public Pair(form form1, form form2, int idofinteraction){
        this.form1 = form1;
        this.form2 = form2;
        this.idofinteraction = idofinteraction;
    }

    public void SetStomachText(string stomachText){
        this.stomachText = stomachText;
    }
    public void SetText(string text){
        this.text = text;
    }


}