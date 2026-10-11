namespace DrugsApp.Domain.StudyModeFeature;

/// <summary>
/// Curated, realistic Polish values used when generating random sample medical data for study patients in sections
/// that have no catalog (other allergies, own supplement names, meal times) and for free-text notes.
/// Every list contains distinct entries (names and meal times are unique) so that duplicate checks of the client
/// handlers rarely skip a candidate.
/// </summary>
public static class StudyRandomMedicalDataCatalog
{
    /// <summary>
    /// Maximum number of random items a teacher can request in one call.
    /// </summary>
    public const int MAX_ITEMS_PER_REQUEST = 10;

    /// <summary>
    /// Gets environmental, contact and occupational allergy names.
    /// </summary>
    public static IReadOnlyList<string> OtherAllergyNames { get; } =
    [
        "Pyłki traw",
        "Pyłek brzozy",
        "Pyłek olchy",
        "Pyłek leszczyny",
        "Pyłek bylicy",
        "Pyłek ambrozji",
        "Pyłek dębu",
        "Pyłek topoli",
        "Pyłek jesionu",
        "Pyłek babki lancetowatej",
        "Pyłek pokrzywy",
        "Pyłek żyta",
        "Roztocza kurzu domowego",
        "Roztocza przechowalniane",
        "Sierść kota",
        "Sierść psa",
        "Sierść konia",
        "Sierść królika",
        "Sierść świnki morskiej",
        "Sierść chomika",
        "Pierze i puch",
        "Zarodniki pleśni Alternaria",
        "Pleśń Aspergillus",
        "Pleśń Cladosporium",
        "Pleśń Penicillium",
        "Lateks",
        "Jad pszczoły",
        "Jad osy",
        "Jad szerszenia",
        "Nikiel",
        "Kobalt",
        "Chrom",
        "Kosmetyki zapachowe",
        "Perfumy",
        "Środki czyszczące",
        "Lanolina",
        "Karaluchy",
        "Dym tytoniowy",
        "Proszek do prania",
        "Płyny do mycia naczyń",
        "Guma i kauczuk",
        "Barwniki do włosów (PPD)",
        "Konserwanty w kosmetykach",
        "Kleje cyjanoakrylowe",
        "Żywica epoksydowa",
        "Formaldehyd",
        "Cement i beton",
        "Wełna owcza",
        "Naftalina",
        "Farby i lakiery",
    ];

    /// <summary>
    /// Gets allergy reaction notes.
    /// </summary>
    public static IReadOnlyList<string> AllergyNotes { get; } =
    [
        "Wysypka i świąd skóry w ciągu godziny.",
        "Pokrzywka na ramionach i szyi.",
        "Obrzęk ust i powiek.",
        "Kichanie i łzawienie oczu.",
        "Duszność; pacjent nosi przy sobie inhalator.",
        "Nudności i bóle brzucha.",
        "Reakcja łagodna, ustąpiła bez leczenia.",
        "Wstrząs anafilaktyczny w przeszłości; nosi autowstrzykiwacz z adrenaliną.",
        "Obrzęk gardła po kontakcie, konieczna pomoc lekarska.",
        "Swędzenie jamy ustnej tuż po spożyciu.",
        "Biegunka kilka godzin po kontakcie.",
        "Nasilony katar i zatkany nos.",
        "Zaczerwienienie i pieczenie skóry.",
        "Ból głowy i zawroty głowy po ekspozycji.",
        "Kaszel i świszczący oddech.",
        "Reakcja potwierdzona testami skórnymi.",
        "Reakcja potwierdzona badaniem IgE.",
        "Objawy nasilają się wiosną.",
        "Objawy nasilają się jesienią.",
        "Objawy występują głównie w nocy.",
        "Reakcja pojawia się po kilku minutach od kontaktu.",
        "Reakcja opóźniona, pojawia się po kilku godzinach.",
        "Wymaga unikania kontaktu, zalecono leki przeciwhistaminowe.",
        "Stosuje leki przeciwhistaminowe w razie potrzeby.",
        "Uczulenie zdiagnozowane w dzieciństwie.",
        "Uczulenie ujawniło się w wieku dorosłym.",
        "Zaostrzenie wyprysku skóry rąk.",
        "Łzawienie i zaczerwienienie spojówek.",
        "Pacjent zgłasza objawy po każdej ekspozycji.",
        "Łagodne objawy, bez konieczności leczenia.",
    ];

    /// <summary>
    /// Gets health condition notes.
    /// </summary>
    public static IReadOnlyList<string> HealthConditionNotes { get; } =
    [
        "Rozpoznanie postawione kilka lat temu, stan stabilny.",
        "Pod stałą opieką specjalisty.",
        "Objawy nasilają się zimą.",
        "Choroba kontrolowana lekami.",
        "Świeże rozpoznanie, leczenie jest dobierane.",
        "Choroba występuje także u członków rodziny.",
        "Kontrola co sześć miesięcy.",
        "Okresowe zaostrzenia, ostatnie w zeszłym roku.",
        "Pacjent stosuje dietę zalecaną przez lekarza.",
        "Wymaga regularnych badań kontrolnych.",
        "Objawy łagodne, bez wpływu na codzienne funkcjonowanie.",
        "Objawy znacznie utrudniają codzienne funkcjonowanie.",
        "Po leczeniu szpitalnym, obecnie w remisji.",
        "Stan pogorszył się po przebytej infekcji.",
        "Pacjent monitoruje parametry w domu.",
        "Zalecona zmiana stylu życia i aktywność fizyczna.",
        "Leczony od wielu lat, bez powikłań.",
        "Konieczne ograniczenie soli w diecie.",
        "Choroba współistnieje z nadciśnieniem.",
        "Choroba współistnieje z cukrzycą.",
        "Wymaga unikania stresu i niedoboru snu.",
        "Pacjent zgłasza sezonowe pogorszenie samopoczucia.",
        "Leczenie przerwane, planowany powrót do terapii.",
        "Rokowanie dobre przy regularnym leczeniu.",
        "Pod opieką poradni specjalistycznej.",
    ];

    /// <summary>
    /// Gets drug problem descriptions.
    /// </summary>
    public static IReadOnlyList<string> DrugProblemNotes { get; } =
    [
        "Nudności po każdej dawce.",
        "Zawroty głowy przy wstawaniu.",
        "Uporczywy suchy kaszel.",
        "Ból głowy w pierwszym tygodniu stosowania.",
        "Ból brzucha po zażyciu na czczo.",
        "Senność w ciągu dnia.",
        "Trudności z zasypianiem.",
        "Suchość w ustach.",
        "Kołatanie serca.",
        "Utrata apetytu.",
        "Bóle mięśni.",
        "Obrzęki kostek.",
        "Zgaga po zażyciu leku.",
        "Biegunka w pierwszych dniach stosowania.",
        "Zaparcia podczas kuracji.",
        "Wysypka skórna po kilku dniach.",
        "Świąd skóry po zażyciu.",
        "Nadwrażliwość skóry na słońce.",
        "Drżenie rąk.",
        "Pobudzenie i niepokój.",
        "Osłabienie i uczucie zmęczenia.",
        "Metaliczny posmak w ustach.",
        "Szum w uszach.",
        "Problemy z koncentracją.",
        "Zwiększone pragnienie.",
        "Częstsze oddawanie moczu.",
        "Bóle stawów.",
        "Nieprzyjemny zapach moczu.",
        "Trudności w połykaniu tabletki.",
        "Lek nie przynosi oczekiwanej poprawy.",
        "Pacjent zapomina o dawkach.",
        "Wahania nastroju.",
        "Zaburzenia widzenia po zażyciu.",
        "Podwyższone ciśnienie po rozpoczęciu leczenia.",
        "Spadek ciśnienia po zażyciu.",
        "Pieczenie w żołądku.",
    ];

    /// <summary>
    /// Gets own supplement mix names (up to 200 characters each, unique).
    /// </summary>
    public static IReadOnlyList<string> OwnSupplementNames { get; } =
    [
        "Poranny zastrzyk energii",
        "Wieczorny zestaw na sen",
        "Wzmocnienie odporności",
        "Zestaw na stawy",
        "Zdrowe serce",
        "Mocne kości",
        "Pamięć i koncentracja",
        "Regeneracja po treningu",
        "Zimowa tarcza ochronna",
        "Komfort trawienia",
        "Skóra, włosy i paznokcie",
        "Redukcja stresu",
        "Codzienna multiwitamina",
        "Zdrowe oczy",
        "Wsparcie w ciąży",
        "Zestaw dla aktywnych",
        "Energia na cały dzień",
        "Spokojny sen",
        "Odporność jesienią",
        "Mięśnie i regeneracja",
        "Wsparcie wątroby",
        "Zdrowe jelita",
        "Układ nerwowy",
        "Witaminy z grupy B",
        "Antyoksydanty",
        "Wsparcie tarczycy",
        "Mocne zęby i dziąsła",
        "Zestaw dla seniora",
        "Wsparcie krążenia",
        "Detoks po zimie",
        "Wzmocnienie włosów",
        "Zestaw na nastrój",
        "Poprawa wchłaniania żelaza",
        "Wsparcie dla wegetarian",
        "Koncentracja w pracy",
        "Zestaw na jesienną chandrę",
        "Odnowa po chorobie",
        "Witamina D i wapń",
        "Zestaw na odporność dzieci",
        "Piękna cera",
    ];

    /// <summary>
    /// Gets meal names with their typical intake times; names and times are both unique.
    /// </summary>
    public static IReadOnlyList<(string MealName, TimeOnly IntakeTime)> MealTimes { get; } =
    [
        ("Wczesne śniadanie", new TimeOnly(6, 0)),
        ("Śniadanie", new TimeOnly(7, 30)),
        ("Drugie śniadanie", new TimeOnly(10, 0)),
        ("Obiad", new TimeOnly(13, 0)),
        ("Podwieczorek", new TimeOnly(16, 0)),
        ("Kolacja", new TimeOnly(19, 0)),
        ("Późna kolacja", new TimeOnly(21, 30)),
        ("Pierwsze śniadanie", new TimeOnly(6, 30)),
        ("Śniadanie w pracy", new TimeOnly(8, 30)),
        ("Przekąska przedpołudniowa", new TimeOnly(9, 30)),
        ("Brunch", new TimeOnly(11, 30)),
        ("Lekki lunch", new TimeOnly(12, 15)),
        ("Obiad w pracy", new TimeOnly(14, 0)),
        ("Herbata z kanapką", new TimeOnly(15, 0)),
        ("Przekąska przed treningiem", new TimeOnly(17, 15)),
        ("Posiłek po treningu", new TimeOnly(18, 15)),
        ("Wczesna kolacja", new TimeOnly(18, 30)),
        ("Kolacja rodzinna", new TimeOnly(19, 45)),
        ("Lekka kolacja", new TimeOnly(20, 15)),
        ("Wieczorna przekąska", new TimeOnly(22, 30)),
        ("Nocna przekąska", new TimeOnly(23, 45)),
        ("Szklanka mleka na dobranoc", new TimeOnly(22, 0)),
        ("Koktajl poranny", new TimeOnly(7, 0)),
        ("Owsianka", new TimeOnly(8, 0)),
        ("Zupa", new TimeOnly(12, 45)),
        ("Owoce", new TimeOnly(10, 45)),
        ("Jogurt", new TimeOnly(15, 45)),
        ("Sałatka", new TimeOnly(17, 45)),
        ("Posiłek nocny dla zmianowych", new TimeOnly(1, 30)),
        ("Śniadanie weekendowe", new TimeOnly(9, 0)),
    ];

    /// <summary>
    /// Gets typical intake times for regular drugs, supplements and food.
    /// </summary>
    public static IReadOnlyList<TimeOnly> IntakeTimes { get; } =
    [
        new TimeOnly(6, 30),
        new TimeOnly(7, 0),
        new TimeOnly(7, 30),
        new TimeOnly(8, 0),
        new TimeOnly(8, 30),
        new TimeOnly(9, 0),
        new TimeOnly(10, 0),
        new TimeOnly(12, 0),
        new TimeOnly(12, 30),
        new TimeOnly(13, 0),
        new TimeOnly(14, 0),
        new TimeOnly(16, 0),
        new TimeOnly(17, 0),
        new TimeOnly(18, 0),
        new TimeOnly(19, 0),
        new TimeOnly(20, 0),
        new TimeOnly(21, 0),
        new TimeOnly(22, 0),
        new TimeOnly(22, 30),
    ];
}
