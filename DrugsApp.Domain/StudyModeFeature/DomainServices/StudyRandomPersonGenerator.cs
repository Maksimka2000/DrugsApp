namespace DrugsApp.Domain.StudyModeFeature.DomainServices;

/// <summary>
/// Random personal data suggested for a new study patient.
/// </summary>
/// <param name="FirstName">First name matching <paramref name="Gender"/>.</param>
/// <param name="LastName">Surname in the form matching <paramref name="Gender"/>.</param>
/// <param name="DateOfBirth">Birth date making the person 18–90 years old.</param>
/// <param name="Gender">Profile gender token: <c>female</c> or <c>male</c>.</param>
public sealed record StudyRandomPersonProfile(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender);

/// <summary>
/// Stateless generator of realistic Polish personal data for study patients; pure domain logic without I/O.
/// </summary>
public static class StudyRandomPersonGenerator
{
    private const int MIN_AGE_YEARS = 18;
    private const int MAX_AGE_YEARS = 90;
    private const string FEMALE_GENDER = "female";
    private const string MALE_GENDER = "male";

    private static readonly string[] FemaleFirstNames =
    [
        "Anna", "Maria", "Katarzyna", "Małgorzata", "Agnieszka", "Barbara", "Ewa", "Krystyna", "Elżbieta",
        "Magdalena", "Joanna", "Zofia", "Teresa", "Jadwiga", "Danuta", "Halina", "Irena", "Beata", "Aleksandra",
        "Monika", "Natalia", "Karolina", "Paulina", "Justyna", "Dorota", "Marta", "Renata", "Izabela", "Weronika",
        "Julia", "Alicja", "Wiktoria", "Oliwia", "Patrycja", "Sylwia", "Urszula", "Lidia", "Grażyna", "Helena",
        "Emilia", "Klaudia", "Martyna", "Daria", "Iwona", "Bożena", "Wioletta", "Kinga", "Hanna", "Laura", "Zuzanna",
    ];

    private static readonly string[] MaleFirstNames =
    [
        "Jan", "Piotr", "Andrzej", "Krzysztof", "Stanisław", "Tomasz", "Paweł", "Józef", "Marcin", "Marek",
        "Michał", "Grzegorz", "Jerzy", "Tadeusz", "Adam", "Łukasz", "Zbigniew", "Ryszard", "Dariusz", "Henryk",
        "Mariusz", "Kazimierz", "Wojciech", "Robert", "Mateusz", "Rafał", "Jacek", "Janusz", "Maciej", "Kamil",
        "Artur", "Sebastian", "Bartosz", "Dawid", "Jakub", "Szymon", "Filip", "Kacper", "Adrian", "Damian",
        "Patryk", "Norbert", "Wiesław", "Roman", "Leszek", "Czesław", "Stefan", "Antoni", "Franciszek", "Mikołaj",
    ];

    private static readonly (string Male, string Female)[] Surnames =
    [
        ("Kowalski", "Kowalska"), ("Nowak", "Nowak"), ("Wiśniewski", "Wiśniewska"), ("Wójcik", "Wójcik"),
        ("Kowalczyk", "Kowalczyk"), ("Kamiński", "Kamińska"), ("Lewandowski", "Lewandowska"),
        ("Zieliński", "Zielińska"), ("Szymański", "Szymańska"), ("Woźniak", "Woźniak"),
        ("Dąbrowski", "Dąbrowska"), ("Kozłowski", "Kozłowska"), ("Jankowski", "Jankowska"), ("Mazur", "Mazur"),
        ("Kwiatkowski", "Kwiatkowska"), ("Krawczyk", "Krawczyk"), ("Piotrowski", "Piotrowska"),
        ("Grabowski", "Grabowska"), ("Nowakowski", "Nowakowska"), ("Pawłowski", "Pawłowska"),
        ("Michalski", "Michalska"), ("Król", "Król"), ("Wieczorek", "Wieczorek"), ("Jabłoński", "Jabłońska"),
        ("Wróbel", "Wróbel"), ("Majewski", "Majewska"), ("Olszewski", "Olszewska"), ("Stępień", "Stępień"),
        ("Malinowski", "Malinowska"), ("Jaworski", "Jaworska"), ("Adamczyk", "Adamczyk"), ("Dudek", "Dudek"),
        ("Zawadzki", "Zawadzka"), ("Górski", "Górska"), ("Sikora", "Sikora"), ("Walczak", "Walczak"),
        ("Baran", "Baran"), ("Rutkowski", "Rutkowska"), ("Michalak", "Michalak"), ("Szewczyk", "Szewczyk"),
        ("Ostrowski", "Ostrowska"), ("Tomaszewski", "Tomaszewska"), ("Pietrzak", "Pietrzak"), ("Duda", "Duda"),
        ("Zalewski", "Zalewska"), ("Wróblewski", "Wróblewska"), ("Jasiński", "Jasińska"),
        ("Marciniak", "Marciniak"), ("Bąk", "Bąk"), ("Zając", "Zając"), ("Jakubowski", "Jakubowska"),
        ("Sadowski", "Sadowska"), ("Włodarczyk", "Włodarczyk"), ("Wilk", "Wilk"), ("Kubiak", "Kubiak"),
        ("Chmielewski", "Chmielewska"), ("Borkowski", "Borkowska"), ("Wysocki", "Wysocka"),
        ("Sokołowski", "Sokołowska"), ("Czarnecki", "Czarnecka"), ("Kaczmarek", "Kaczmarek"), ("Lis", "Lis"),
        ("Sobczak", "Sobczak"), ("Gajewski", "Gajewska"),
    ];

    /// <summary>
    /// Generates a random patient profile: gender, a first name and surname matching it, and a birth date that makes
    /// the person 18–90 years old on <paramref name="todayUtc"/>.
    /// </summary>
    /// <param name="todayUtc">
    /// Current UTC date used to derive the birth date.
    /// </param>
    /// <returns>
    /// Suggested personal data; nothing is persisted.
    /// </returns>
    public static StudyRandomPersonProfile GeneratePatientProfile(DateOnly todayUtc)
    {
        bool isFemale = Random.Shared.Next(2) == 0;
        string firstName = isFemale
            ? FemaleFirstNames[Random.Shared.Next(FemaleFirstNames.Length)]
            : MaleFirstNames[Random.Shared.Next(MaleFirstNames.Length)];

        (string maleSurname, string femaleSurname) = Surnames[Random.Shared.Next(Surnames.Length)];
        string lastName = isFemale ? femaleSurname : maleSurname;

        DateOnly youngestBirthDate = todayUtc.AddYears(-MIN_AGE_YEARS);
        DateOnly oldestBirthDate = todayUtc.AddYears(-MAX_AGE_YEARS);
        int rangeInDays = youngestBirthDate.DayNumber - oldestBirthDate.DayNumber;
        DateOnly dateOfBirth = oldestBirthDate.AddDays(Random.Shared.Next(rangeInDays + 1));

        return new StudyRandomPersonProfile(
            firstName,
            lastName,
            dateOfBirth,
            isFemale ? FEMALE_GENDER : MALE_GENDER);
    }
}
