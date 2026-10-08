using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ErrorOr;
using DrugsApp.Features.SystemTextFeature.AlcoholInteractionDefaultText;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;

// waht we got from the front end 
public sealed record FrontRequest{
    public List<int> drugFroms {get; set;}
    public bool getUserData {get; set;}
}


// and teh data taken form the front 
var fromTheFrontEnt = new FrontRequest
{
    drugFroms = new List<int> { 1, 2, 3, 4 },
    getUserData = true
};

// if the getuserData true get the drug forms from the user account db qeury which will return this 
// make the db call and get data 
var drugFromUserData = new List<int> {2, 5}; // IF THE SWITTCH IS TURNED ON TO GET THE USER DATA. 



// then we just 
// connect two lists
var drugFromsALl = drugFromUserData.Concat(FrontRequest.drugFroms).ToList();
 // we have teh {1, 2, 3, 4, 2, 5}

// instatly check for the duplication on the drug from id.  and remember that number
var duplicatedNumbersOnTheDrugFromId = drugFromsALl
    .GroupBy(id => id)
    .Where(g => g.Count() > 1)
    .Select(g => g.Key)
    .ToHashSet(); 
// result we got {2}. that means the drug from with the id 2 is duplicated.

// cut the duplicaitons
drugFromsALl.Distinct(); 
// result we got {1, 2, 3, 4, 5}



// make the following db calls:

// 1. get the drug id based on the drug form id. take the all list of the drug froms and make the db call to get the drug id for each drug form id.
// first value drug form id  , second value drug id 
// but for example we can get this date from teh db reques on the beging becasue we already know the drug from id 
var fakeDatabaseOfDrug = new Dictionary<int, int>
{
   {4, 1},
   {1, 1},
   {2, 2},
   {3, 3},
};

// 2. get the active sub id based on the drug id. take the all list of the drug froms and make the db call to get the active sub id for each drug id.
// second number is the active sub id 
// we can make the db queery to get all the active substance for the specific drug form id based on the drug id 
// first number is the drug id , second number is the active sub id 
var fakeDatabaseForTheDrugForms = new Dictionary<int, List<int>>
{
    { 1, new List<int> { 1, 2, 3 } },
    { 2, new List<int> { 4, 1 } },
    { 3, new List<int> { 8, 4 } },
    { 4, new List<int> { 1, 2, 3 } }
};
// based on this we have that 






// step one form the list of the drugfrom objects 
// we do the query to the db to get the data  basically its like the using the joins to get this kind of thing 

var newListOFtheSeparateDrugFomrsObjs = new List<form>();

// so the newListOFtheSeparateDrugFomrsObjs gives us this thing: 
// as result we have a list : newListOFtheSeparateDrugFomrsObjs
// form(drugId: 1, drugformId: 1, activeSubId: 1)
// form(drugId: 1, drugformId: 1, activeSubId: 2)
// form(drugId: 1, drugformId: 1, activeSubId: 3)

// form(drugId: 2, drugformId: 2, activeSubId: 4)
// form(drugId: 2, drugformId: 2, activeSubId: 1)

// form(drugId: 3, drugformId: 3, activeSubId: 8)
// form(drugId: 3, drugformId: 3, activeSubId: 4)

// form(drugId: 1, drugformId: 4, activeSubId: 1)
// form(drugId: 1, drugformId: 4, activeSubId: 2)
// form(drugId: 1, drugformId: 4, activeSubId: 3)


// no duplication on the drug form as we cut that before.
// form(drugId: 2, drugformId: 2, activeSubId: 4)
// form(drugId: 2, drugformId: 2, activeSubId: 1)   






// the story behind how we can get the interaction records intead of doing the call each time int the loop 
        var listOfAllActiveSubstances = new HashSet<int>();
        foreach (var obj in newListOFtheSeparateDrugFomrsObjs){
            listOfAllActiveSubstances.Add(obj.activeSubId);
        }

        // make the hard copy of the hashset 
        var listOfAllActiveSubstancesHardCopy = listOfAllActiveSubstances.ToList();

        // so in the db we have the right id and the left id and lets just put to the right the listOfAllActiveSubstances and to the left the listOfAllActiveSubstancesHardCopy        
        // so basically the dictuiary its the result of the db call

// thats how we got this :

// the pair interaction 
var fakeIntercatonOnActiveSubsIds = new Dictionary<(int, int), string>
{
    { (1, 2), "1" },
    { (1, 3), "2" },
    { (2, 4), "3" },
    { (2, 1), "3" },
    { (3, 8), "4" },
    { (3, 4), "5" },
    { (4, 1), "6" },
    { (4, 2), "7" },
    { (4, 3), "8" },
};


// generate alcohol info 
// we have the list of the all actie substance 
// so lets make a query to the database aobu the alcotho , just ask if the specidfic active sub has the alcohol checkbbox marked and if there si spesial text 
// as return we can get smth like this:

// first number in the list is the activ sub id the seond is the list so the true faluse if the default text exist and the unique text 

var alcoholInfoFromTheDb = new Dictionary<int, List<object>>
{
    {1, new List<object> {true, "alcohol text 1"}},
    {2, new List<object> {false, "alcohol text 2"}},
    {3, new List<object> {true, "alcohol text 3"}},
    {4, new List<object> {false, "alcohol text 4"}},
    {5, new List<object> {true, "alcohol text 5"}},
    {6, new List<object> {false, "alcohol text 6"}},
    {7, new List<object> {true, "alcohol text 7"}},
    {8, new List<object> {false, "alcohol text 8"}},
};

// get also basic text in the cache 
string basicAlcoholTextFromTheDb = "new Dictionary<int, string>";


// step two interaction forming . (check the duplication on the active subs id, duplication indrug id )
var theSameDrug = new HashSet<form>();
// ass result we have list like this :
// form(drugId: 1, drugformId: 1, activeSubId: 1)
// form(drugId: 1, drugformId: 1, activeSubId: 2)
// form(drugId: 1, drugformId: 1, activeSubId: 3)
// form(drugId: 1, drugformId: 4, activeSubId: 1)
// form(drugId: 1, drugformId: 4, activeSubId: 2)
// form(drugId: 1, drugformId: 4, activeSubId: 3)
// later just group by the drug id 

var theSameDrugForm = new HashSet<form>();
// 
// the list will be like this:
// form(drugId: 2, drugformId: 2, activeSubId: 4)
// form(drugId: 2, drugformId: 2, activeSubId: 1)
// later just gropu by the drug form 

var theSameActiveSubId = new HashSet<form>();
// examples result ;
// form(drugId: 1, drugformId: 1, activeSubId: 1)
// form(drugId: 2, drugformId: 2, activeSubId: 1)
// form(drugId: 1, drugformId: 4, activeSubId: 1)
// basically later group by the active sub

var interactionList = new HashSet<Pair>();

        // after taht we have the objects pair whihc look like this :
        // pair(form1(d1,f1,s1), form2(d2,f2,s4), "… 4 and 1")
        // pair(form1(d2,f2,s4), form2(d1,f1,s1), "… 4 and 1")
        // pair(form1(d1,f1,s1), form2(d3,f3,s4), "… 4 and 1")
        // pair(form1(d3,f3,s4), form2(d1,f1,s1), "… 4 and 1")
        // pair(form1(d1,f1,s2), form2(d2,f2,s4), "… 2 and 4")
        // pair(form1(d2,f2,s4), form2(d1,f1,s2), "… 2 and 4")
        // pair(form1(d1,f1,s2), form2(d2,f2,s1), "… 2 and 1")
        // pair(form1(d2,f2,s1), form2(d1,f1,s2), "… 2 and 1")
        // pair(form1(d1,f1,s2), form2(d3,f3,s4), "… 2 and 4")
        // pair(form1(d3,f3,s4), form2(d1,f1,s2), "… 2 and 4")
        // pair(form1(d1,f1,s3), form2(d2,f2,s4), "… 3 and 4")
        // pair(form1(d2,f2,s4), form2(d1,f1,s3), "… 3 and 4")
        // pair(form1(d1,f1,s3), form2(d2,f2,s1), "… 1 and 3")
        // pair(form1(d2,f2,s1), form2(d1,f1,s3), "… 1 and 3")
        // pair(form1(d1,f1,s3), form2(d3,f3,s8), "… 3 and 8")
        // pair(form1(d3,f3,s8), form2(d1,f1,s3), "… 3 and 8")
        // pair(form1(d1,f1,s3), form2(d3,f3,s4), "… 3 and 4")
        // pair(form1(d3,f3,s4), form2(d1,f1,s3), "… 3 and 4")
        // pair(form1(d1,f4,s1), form2(d2,f2,s4), "… 4 and 1")
        // pair(form1(d2,f2,s4), form2(d1,f4,s1), "… 4 and 1")
        // pair(form1(d1,f4,s1), form2(d3,f3,s4), "… 4 and 1")
        // pair(form1(d3,f3,s4), form2(d1,f4,s1), "… 4 and 1")
        // pair(form1(d1,f4,s2), form2(d2,f2,s4), "… 2 and 4")
        // pair(form1(d2,f2,s4), form2(d1,f4,s2), "… 2 and 4")
        // pair(form1(d1,f4,s2), form2(d2,f2,s1), "… 2 and 1")
        // pair(form1(d2,f2,s1), form2(d1,f4,s2), "… 2 and 1")
        // pair(form1(d1,f4,s2), form2(d3,f3,s4), "… 2 and 4")
        // pair(form1(d3,f3,s4), form2(d1,f4,s2), "… 2 and 4")


 

for (int i = 0; i < newListOFtheSeparateDrugFomrsObjs.Count; i++)
{
    var obj1 = newListOFtheSeparateDrugFomrsObjs[i];

    for (int j = i + 1; j < newListOFtheSeparateDrugFomrsObjs.Count; j++)
    {
        var obj2 = newListOFtheSeparateDrugFomrsObjs[j];
        



        // check duplication on the same drug form 
        if (obj1.drugformId == obj2.drugformId) {
            continue;
        }



        var isDuplicatedOnTheDrugFormId = false;
        // check duplication on the drug form id . no one more loop because tis a hash so tis okay 
        if (duplicatedNumbersOnTheDrugFromId.Contains(obj1.drugformId)){
            theSameDrugForm.Add(obj1); 
            isDuplicatedOnTheDrugFormId = true;
        }
        if (duplicatedNumbersOnTheDrugFromId.Contains(obj2.drugformId)){
            theSameDrugForm.Add(obj2); 
            isDuplicatedOnTheDrugFormId = true;
        }
        if (isDuplicatedOnTheDrugFormId){
            continue;
        }


        // check duplication on the drug id 
        if (obj1.drugId == obj2.drugId){
            theSameDrug.Add(obj1); 
            theSameDrug.Add(obj2); 
            continue;
             // we store two values because the user can use two drugs with two diffrenet drug forms 
        }



        // check duplication on the active sub id 
        if (obj1.activeSubId == obj2.activeSubId){
            theSameActiveSubId.Add(obj1); 
            theSameActiveSubId.Add(obj2); 
            continue;
        }



       


        // form the pair  with instant call to the db reques (fake db in this case) 
        if (fakeIntercatonOnActiveSubsIds.ContainsKey((obj1.activeSubId, obj2.activeSubId)) || fakeIntercatonOnActiveSubsIds.ContainsKey((obj2.activeSubId, obj1.activeSubId))){
            interactionList.Add(new Pair(obj1, obj2, fakeIntercatonOnActiveSubsIds[(obj1.activeSubId, obj2.activeSubId)]));
            interactionList.Add(new Pair(obj2, obj1, fakeIntercatonOnActiveSubsIds[(obj1.activeSubId, obj2.activeSubId)]));
            continue;
        }
        // same two version so because after that we can simply grup by the drug form the first row and the prepared text will be ready for the response dto 

    


 

    }
}








// collect the interaction table from the interactiondi list in the pair
// as teh result returend the list of hte interaction with teh text both stomach and simpe;
var listOfInteractionWithTextBothStomachAndSimple = new dictionary<int, List<string>>
{
    {1, {"both stomach and simple","stomach"}},
    {2, {"both stomach and simple","stomach"}},
    {3, {"both stomach and simple","stomach"}},
    {4, {"both stomach and simple","stomach"}},
    {5, {"both stomach and simple","stomach"}},
    {6, {"both stomach and simple","stomach"}},
    {7, {"both stomach and simple","stomach"}},
    {8, {"both stomach and simple","stomach"}},
}; 
// so this is an example of the test based on the return form the dababase we simply take the id of the interaction and return the text based on the id . from the pair 


foreach (var pair in interactionList){
    if (listOfInteractionWithTextBothStomachAndSimple.ContainsKey(pair.idofinteraction)){
        if(pair.form11.isStomach && pair.form2.isStomach){
            pair.setStomachText(listOfInteractionWithTextBothStomachAndSimple[pair.idofinteraction][1]);
        } 
         pair.setText(listOfInteractionWithTextBothStomachAndSimple[pair.idofinteraction][0]);
    }

}





var result = new List<ResultDto>();
// collect the response 
foreach (var drugFormId in drugFromsALl){

    var newDto = new ResultDto(
        fakeDatabaseOfDrug[drugFormId],
        drugFormId,
        fakeDatabaseForTheDrugForms[drugFormId],
        interactionList.Where(x => x.form1.drugformId == drugFormId).ToList() // dont need to check both sides because the left side contains all needed . 
        
    );

    result.Add(newDto);

    if(drugFromUserData.Contains(drugFormId)){
        newDto.AddNote("teken regulary");
    } else if (FrontRequest.drugFroms.Contains(drugFormId)) {
        newDto.AddNote("selected ");
    }


    // add the alcohol info to the dto :

    foreach (var activeSubId in newDto.activeSubId){
        if (alcoholInfoFromTheDb.ContainsKey(activeSubId)){
            if (alcoholInfoFromTheDb[activeSubId][0] as bool){
                newDto.SetBasicAlcoholText(basicAlcoholTextFromTheDb);
            } 
            if (alcoholInfoFromTheDb[activeSubId][1] is not null){
                newDto.SetUniqueAlcoholText(alcoholInfoFromTheDb[activeSubId][1] as string);
            }
        }

    // here also collect the food and the supplemnt interaction for each of hte acitve substance 

    foreach (var activeSubId in newDto.activeSubId){


        // for the food and supplment  interaction with the active substance where user regularly itake . 

        // here we take the list returned form the db about hte fakeInteractionOnFoodAndSupplementOnActiveSubsIds where the second nuber is the food or supplment id and the first is the active substance number 
        // so we take the returned data and 


        // section food intreaction on all possiible food 
        // we take all the interaction from the db about the food id any , and the active sub id. 
        // so basically we take all of the record where the activeSubId is lets say name list 
        var allFoddInteractionsOnActiveSubId = new List<Pair>();


        // section food intreactio on the user intake 
        // from the previously found all the records allFoddInteractionsOnActiveSubId we take that list and just select form htat list only the record with the foodFromTheUserAccountCurrentIntake
        var foodIntreactionOnUserIntake = allFoddInteractionsOnActiveSubId.Where(x => foodFromTheUserAccountCurrentIntake.Contains(x.form1.foodId) || foodFromTheUserAccountCurrentIntake.Contains(x.form2.foodId)).ToList();
        
        // so we minus the foodIntreactionOnUserIntake from the allFoddInteractionsOnActiveSubId and we get the list of the food interactions on all possible food 
        var foodIntreactionOnAllPossibleFood = allFoddInteractionsOnActiveSubId.Where(x => !foodIntreactionOnUserIntake.Contains(x)).ToList();

        

        // do the same for the supplement 
        var allSupplementInteractionsOnActiveSubId = new List<Pair>();
        var supplementIntreactionOnUserIntake = allSupplementInteractionsOnActiveSubId.Where(x => supplementFromTheUserAccountCurrentIntake.Contains(x.form1.supplementId) || supplementFromTheUserAccountCurrentIntake.Contains(x.form2.supplementId)).ToList();
        var supplementIntreactionOnAllPossibleSupplement = allSupplementInteractionsOnActiveSubId.Where(x => !supplementIntreactionOnUserIntake.Contains(x)).ToList();

        // and return that to the dto. 

    }


    }

    // if this current drug form drug is in the list of hte problematicDrugsOVerlapInregularIntake or the  problematicDrugsOVerlapSelected

    if (problematicDrugsOVerlapInregularIntake.Contains(drugFormId) || problematicDrugsOVerlapSelected.Contains(drugFormId)){
        // add the infor baout taht

    // if this current drug form drug is in the list of the medicalAllergiesOVerlapInregularIntake or the medicalAllergiesOVerlapSelected
    if (medicalAllergiesOVerlapInregularIntake.Contains(drugFormId) || medicalAllergiesOVerlapSelected.Contains(drugFormId)){
        // add the infor baout taht

}




// if the Include my profile data is turned on we also take the queries about the health condtions both custom and the default ones  and just return in the return dto 
// main warnings the same just take form the profile and that all 


// for the problematicDrugs just also take the drug froms and output 
// also take the infor mation about hte regulary taken and seleted and calculate the overlap 
// as the return from hte database i got this : first value is the drug form id the second is hte text 
var problematicDrugsFromTheDb = new Dictionary<int, string>{
    {1, "problematic drug 1"},
    {2, "problematic drug 2"},
    {3, "problematic drug 3"},
    {4, "problematic drug 4"},
    {5, "problematic drug 5"},
    {6, "problematic drug 6"},
    {7, "problematic drug 7"},
    {8, "problematic drug 8"},
};

// then take the  fromTheFrontEnt and the drugFromUserData


var problematicDrugsOVerlapInregularIntake = new HashSet<int>();
var problematicDrugsOVerlapSelected = new HashSet<int>();

foreach (var problematicDrug in problematicDrugsFromTheDb){
    if (drugFromUserData.Contains(problematicDrug.Key)){
        problematicDrugsOVerlapInregularIntake.Add(problematicDrug.Key);
    }
    if (FrontRequest.drugFroms.Contains(problematicDrug.Key)){
        problematicDrugsOVerlapSelected.Add(problematicDrug.Key);
    }
}

// dont forget about the custom ones 
// then include that in the the response dto 


// the same with the medical allergies 

// as the return from the database i got this : first value is the drug form id the second is hte text 
var medicalAllergiesFromTheDb = new Dictionary<int, string>{
    {1, "medical allergy 1"},
    {2, "medical allergy 2"},
    {3, "medical allergy 3"},
    {4, "medical allergy 4"},
    {5, "medical allergy 5"},
    {6, "medical allergy 6"},
    {7, "medical allergy 7"},
    {8, "medical allergy 8"},
};
 

 // then take the  fromTheFrontEnt and the drugFromUserData

 var medicalAllergiesOVerlapInregularIntake = new HashSet<int>();
 var medicalAllergiesOVerlapSelected = new HashSet<int>();

 foreach (var medicalAllergy in medicalAllergiesFromTheDb){
    if (drugFromUserData.Contains(medicalAllergy.Key)){
        medicalAllergiesOVerlapInregularIntake.Add(medicalAllergy.Key);
    }
    if (FrontRequest.drugFroms.Contains(medicalAllergy.Key)){
        medicalAllergiesOVerlapSelected.Add(medicalAllergy.Key);
    }
 }


// dont forget about the custom ones 


// for the food and supplement similar 


// form the db we take the list of the reguar food and supplement and the list of the selected food and supplement

// first number is the id fo the food the second is the text 
var foodFromTheUserAccountDballergyTo = new Dictionary<int, string>{
    {1, "food 1"},
    {2, "food 2"},
    {3, "food 3"},
    {4, "food 4"},
    {5, "food 5"},
    {6, "food 6"},
    {7, "food 7"},
    {8, "food 8"},
};

// get the data of the current intake 

var foodFromTheUserAccountCurrentIntake = new Dictionary<int, string>{
    {1, "food 1"},
    {2, "food 2"},
    {3, "food 3"},
    {4, "food 4"},
    {5, "food 5"},
    {6, "food 6"},
    {7, "food 7"},
    {8, "food 8"},
};

var foodAllergyToOverlapInregularIntake = new HashSet<int>();

foreach (var food in foodFromTheUserAccountCurrentIntake){
    if (foodFromTheUserAccountDballergyTo.ContainsKey(food.Key)){
        foodAllergyToOverlapInregularIntake.Add(food.Key);
    }
}





// adh the suplment 
/// first number is the id fo the supplement the second is the text 
var supplementFromTheUserAccountDballergyTo = new Dictionary<int, string>{
    {1, "supplement 1"},
    {2, "supplement 2"},
    {3, "supplement 3"},
    {4, "supplement 4"},
    {5, "supplement 5"},
    {6, "supplement 6"},
    {7, "supplement 7"},
    {8, "supplement 8"},
};

// get the data of the current intake 

var supplementFromTheUserAccountCurrentIntake = new Dictionary<int, string>{
    {1, "supplement 1"},
    {2, "supplement 2"},
    {3, "supplement 3"},
    {4, "supplement 4"},
    {5, "supplement 5"},
    {6, "supplement 6"},
    {7, "supplement 7"},
    {8, "supplement 8"},
};

var supplementAllergyToOverlapInregularIntake = new HashSet<int>();

foreach (var supplement in supplementFromTheUserAccountCurrentIntake){
    if (supplementFromTheUserAccountDballergyTo.ContainsKey(supplement.Key)){
        supplementAllergyToOverlapInregularIntake.Add(supplement.Key);
    }
}

// the include the info to the response dto 


// dont forget about the custom ones 


// also simple get for the other allergy 


var simpleOtherAllrgies = new Dictionary<int, string>{ // both custom and not 


// and the alst one most important 


// the interaction about hte food and the supplment
// so we have teh list of all the active substances 


// now we got waht user takes  (list of the food and list of the supplement)

// foodFromTheUserAccountCurrentIntake, supplementFromTheUserAccountCurrentIntake we have teh list of that

//  and we have the listOfAllActiveSubstances 

// so we just do hte balk query about it.  we compare the listOfAllActiveSubstances with all the food and supplment interactions in result we got this :

// first number is the id of the food or the suppplment the second is the id of hte active substance and hte last id of hte interaction 
var fakeInteractionOnFoodAndSupplementOnActiveSubsIds = new Dictionary<(int, int), string>
{
    { (1, 2), "1" },
    { (1, 3), "2" },
    { (2, 4), "3" },
    { (2, 1), "3" },
    { (3, 8), "4" },
    { (3, 4), "5" },
    { (4, 1), "6" },
    { (4, 2), "7" },
    { (4, 3), "8" },
};

// then we got the  list o fhte interactions based on the id 

var listOftheinteractiontextonFoodAndSupplement = new dictionary<int, List<string>>
{
    {1, {"both stomach and simple","stomach"}},
    {2, {"both stomach and simple","stomach"}},
    {3, {"both stomach and simple","stomach"}},
    {4, {"both stomach and simple","stomach"}},
    {5, {"both stomach and simple","stomach"}},
    {6, {"both stomach and simple","stomach"}},
    {7, {"both stomach and simple","stomach"}},
    {8, {"both stomach and simple","stomach"}},
}; 


// then we have all the all possible interaction with active sub and the food or supplement also on the active sub we have information about hte 

// lets collect the food intreactions user intake. we have the list foodFromTheUserAccountCurrentIntake with the id of the food he is taking 




// also dont forget about the duplication check on the groups for the active substances.
