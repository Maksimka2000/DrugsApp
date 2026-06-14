#!/usr/bin/env python3
"""Generates Seed/seed.sql for dev catalog and multi-user fixtures."""
from __future__ import annotations

from pathlib import Path
from uuid import UUID

NOW = "(NOW() AT TIME ZONE 'utc')"
SEV = [
    "11111111-1111-1111-1111-111111110111",
    "22222222-2222-2222-2222-222222221222",
    "33333333-3333-3333-3333-333333332333",
    "44444444-4444-4444-4444-444444443444",
]
clients = [f"b1010000-0000-4000-8000-{i:012d}" for i in range(1, 6)]
providers = [f"b2010000-0000-4000-8000-{i:012d}" for i in range(1, 4)]

lines: list[str] = []


def w(text: str = "") -> None:
    lines.append(text)


def uid(prefix: str, n: int) -> str:
    """Build a UUID string; prefix must be 8 hex chars (0-9, a-f)."""
    full = f"{prefix}-{n:012d}"
    UUID(full)  # validates format and hex
    return full


# UUID prefix stems (8 hex chars + standard suffix); all characters must be 0-9 or a-f.
INT_SUB_I, INT_SUB_P, INT_SUB_E = "a3010000-0000-4000-8000", "a3110000-0000-4000-8000", "a3210000-0000-4000-8000"
INT_FOOD_I, INT_FOOD_P, INT_FOOD_E = "a4010000-0000-4000-8000", "a4110000-0000-4000-8000", "a4210000-0000-4000-8000"
INT_SUPP_I, INT_SUPP_P, INT_SUPP_E = "a5010000-0000-4000-8000", "a5110000-0000-4000-8000", "a5210000-0000-4000-8000"
SUPERVISION_REQUEST = "ab010000-0000-4000-8000"
SUPERVISION_CONNECTION = "ab020000-0000-4000-8000"
MEDICAL_PROPOSAL = "ab030000-0000-4000-8000"
MEDICAL_PROPOSAL_ITEM = "ab031000-0000-4000-8000"
NOTIFICATION = "b4010000-0000-4000-8000"
USER_HEALTH_CONDITION = "f0000000-0000-4000-8000"
DRUG_PROBLEM = "f1000000-0000-4000-8000"
VET_DRUG = "b5010000-0000-4000-8000"
TEMP_BATCH = "b6010000-0000-4000-8000"
TEMP_DRUG = "b6020000-0000-4000-8000"


def canonical_pair(short_type: str, id_a: str, id_b: str) -> tuple[str, str, str, str]:
    if UUID(id_a) <= UUID(id_b):
        return short_type, id_a, short_type, id_b
    return short_type, id_b, short_type, id_a


def sql_str(value: str) -> str:
    return value.replace("'", "''")


SUBSTANCES = [
    "Ibuprofen", "Paracetamol", "Metformin", "Atorvastatin", "Omeprazole",
    "Amoxicillin", "Lisinopril", "Levothyroxine sodium", "Salbutamol", "Sertraline",
    "Amlodipine", "Simvastatin", "Losartan", "Pantoprazole", "Cetirizine",
    "Metoprolol", "Warfarin", "Diazepam", "Insulin glargine", "Acetylsalicylic acid",
]
SUBSTANCE_ALCOHOL = [False] * 16 + [True, True] + [False] * 2

DRUGS = [
    ("Nurofen", "Standard"), ("Apap", "Standard"), ("Glucophage", "Standard"),
    ("Atorvasterol", "Standard"), ("Controloc", "Standard"), ("Amotaks", "Standard"),
    ("Lisinoratio", "Standard"), ("Euthyrox", "Standard"), ("Ventolin", "Standard"),
    ("Zoloft", "Standard"), ("Norvasc", "Standard"), ("Simvacard", "Standard"),
    ("Cozaar", "Standard"), ("Controloc 40", "Standard"), ("Zyrtec", "Standard"),
    ("Betaloc", "Standard"), ("Warfarina", "Standard"), ("Relanium", "Standard"),
    ("Lantus", "Standard"), ("Aspirin", "Standard"),
]

FORM_ROWS = [
    (1, "200 mg", True, "M01AE01"), (2, "500 mg", True, "N02BE01"), (3, "500 mg", True, "A10BA02"),
    (4, "20 mg", True, "C10AA05"), (5, "20 mg", True, "A02BC01"), (6, "500 mg", True, "J01CA04"),
    (7, "10 mg", True, "C09AA03"), (8, "50 mcg", True, "H03AA01"), (9, "2 mg", True, "R03AC02"),
    (10, "50 mg", True, "N06AB06"), (11, "5 mg", True, "C08CA01"), (12, "20 mg", True, "C10AA01"),
    (13, "50 mg", True, "C09CA01"), (14, "40 mg", True, "A02BC02"), (15, "10 mg", True, "R06AE07"),
    (16, "50 mg", True, "C07AB02"), (17, "5 mg", True, "B01AA03"), (18, "5 mg", True, "N05BA01"),
    (19, "100 IU/ml", True, "A10AE04"), (20, "100 mg", True, "N02BA01"),
]

ALT_FORMS = [
    (1, 101, "400 mg", True, "M01AE01"), (2, 102, "1000 mg", True, "N02BE01"),
    (3, 103, "850 mg", True, "A10BA02"), (4, 104, "40 mg", True, "C10AA05"),
    (5, 105, "40 mg", True, "A02BC01"), (6, 106, "875 mg", True, "J01CA04"),
    (7, 107, "20 mg", True, "C09AA03"), (8, 108, "75 mcg", True, "H03AA01"),
    (9, 109, "100 mcg/dose", False, "R03AC02"), (10, 110, "100 mg", True, "N06AB06"),
]

EXTRA_DRUGS = [
    ("Hylo-Comod", "Ophthalmic", 1, "10 ml", False, "S01XA20", 21),
    ("Xalatan", "Ophthalmic", 4, "0.005%", False, "S01EE01", 22),
    ("Tobrex", "Ophthalmic", 6, "0.3%", False, "S01AA12", 23),
    ("Allergodil", "Ophthalmic", 15, "0.05%", False, "S01GX07", 24),
    ("Artelac", "Ophthalmic", 2, "10 ml", False, "S01XA20", 25),
    ("Orkambi", "Special", 3, "150/200 mg", True, "R07FX02", 26),
    ("Spinraza", "Special", 8, "12 mg/5 ml", False, "N07XX12", 27),
    ("Kalydeco", "Special", 9, "150 mg", True, "R07AX02", 28),
    ("Soliris", "Special", 7, "300 mg", False, "L04AJ01", 29),
    ("Hemlibra", "Special", 19, "150 mg", False, "B02BX06", 30),
    ("Polocard", "Standard", 20, "75 mg", True, "B01AC06", 31),
    ("Metformax", "Standard", 3, "1000 mg", True, "A10BA02", 32),
    ("Ramipril", "Standard", 7, "5 mg", True, "C09AA05", 33),
    ("Rosutrox", "Standard", 12, "10 mg", True, "C10AA07", 34),
    ("Escital", "Standard", 10, "10 mg", True, "N06AB10", 35),
]

FOODS = [
    ("Grapefruit", '["pink grapefruit","citrus"]', "grapefruit"),
    ("Milk", '["cow milk","dairy"]', "milk"),
    ("Peanuts", '["groundnuts"]', "peanuts"),
    ("Eggs", '["chicken eggs"]', "eggs"),
    ("Shellfish", '["shrimp","prawns"]', "shellfish"),
    ("Soy", '["soya","tofu"]', "soy"),
    ("Wheat", '["gluten","bread"]', "wheat"),
    ("Almonds", "[]", "almonds"),
    ("Strawberries", "[]", "strawberries"),
    ("Dark chocolate", '["cocoa"]', "dark chocolate"),
    ("Aged cheese", '["cheddar","parmesan"]', "aged cheese"),
    ("Tomatoes", "[]", "tomatoes"),
    ("Avocado", "[]", "avocado"),
    ("Spinach", "[]", "spinach"),
    ("Bananas", "[]", "bananas"),
    ("Coffee", '["espresso"]', "coffee"),
    ("Honey", "[]", "honey"),
    ("Garlic", "[]", "garlic"),
    ("Onion", '["red onion"]', "onion"),
    ("Celery", "[]", "celery"),
]

SUPPLEMENTS = [
    ("Vitamin D3", '["cholecalciferol"]', "vitamin d3"),
    ("Omega-3 Fish Oil", '["EPA","DHA"]', "omega-3 fish oil"),
    ("Iron", '["ferrous sulfate"]', "iron"),
    ("Magnesium", '["magnesium citrate"]', "magnesium"),
    ("Vitamin C", '["ascorbic acid"]', "vitamin c"),
    ("Zinc", "[]", "zinc"),
    ("Probiotics", '["lactobacillus"]', "probiotics"),
    ("Vitamin B12", '["cobalamin"]', "vitamin b12"),
    ("Folic acid", '["folate","vitamin b9"]', "folic acid"),
    ("Calcium", '["calcium carbonate"]', "calcium"),
    ("Multivitamin", "[]", "multivitamin"),
    ("Coenzyme Q10", '["CoQ10"]', "coenzyme q10"),
    ("Melatonin", "[]", "melatonin"),
    ("Turmeric", '["curcumin"]', "turmeric"),
    ("Vitamin B6", '["pyridoxine"]', "vitamin b6"),
    ("Biotin", '["vitamin b7"]', "biotin"),
    ("Potassium", "[]", "potassium"),
    ("Psyllium fiber", '["fiber supplement"]', "psyllium fiber"),
    ("Glucosamine", "[]", "glucosamine"),
    ("Ashwagandha", "[]", "ashwagandha"),
]

HEALTH_CONDITIONS = [
    ("Type 2 diabetes", "type 2 diabetes", "Chronic insulin resistance and elevated blood glucose."),
    ("Hypertension", "hypertension", "Persistently elevated blood pressure."),
    ("Asthma", "asthma", "Recurrent airway inflammation and bronchospasm."),
    ("Major depressive disorder", "major depressive disorder", "Persistent low mood and loss of interest."),
    ("Hypothyroidism", "hypothyroidism", "Underactive thyroid gland."),
    ("Gastroesophageal reflux disease", "gastroesophageal reflux disease", "Chronic acid reflux and heartburn."),
    ("Hyperlipidemia", "hyperlipidemia", "Elevated blood cholesterol or triglycerides."),
    ("Atrial fibrillation", "atrial fibrillation", "Irregular and often rapid heart rhythm."),
    ("COPD", "copd", "Chronic obstructive pulmonary disease."),
    ("Rheumatoid arthritis", "rheumatoid arthritis", "Autoimmune inflammatory arthritis."),
]

PHARMA_FORM_FOR_PRIMARY = [
    1, 1, 3, 1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 4, 1,
]


def add_interactions(
    section: str,
    party_type: str,
    short_type: str,
    entity_prefix: str,
    i_prefix: str,
    p_prefix: str,
    e_prefix: str,
    texts: list[str],
) -> None:
    w(f"-- {section}")
    for i in range(1, 21):
        iid = uid(i_prefix, i)
        sev = SEV[(i - 1) % 4]
        left_num, right_num = i, (i % 20) + 1
        left_id = uid(entity_prefix, left_num)
        right_id = uid(entity_prefix, right_num)
        ta, ida, tb, idb = canonical_pair(short_type, left_id, right_id)
        txt = sql_str(texts[i % len(texts)])
        w(
            f'INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") '
            f"VALUES ('{iid}'::uuid, '{sev}'::uuid, '{txt}', NULL, {NOW}) "
            f'ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";'
        )
        pa, pb = uid(p_prefix, i * 2 - 1), uid(p_prefix, i * 2)
        pe = uid(e_prefix, i)
        fk = {
            "Substance": "ActiveSubstanceId",
            "Food": "FoodId",
            "Supplement": "SupplementId",
        }[party_type]
        for pid, eid in ((pa, left_id), (pb, right_id)):
            w(
                f'INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "{fk}", "CreatedAtUtc") '
                f"VALUES ('{pid}'::uuid, '{iid}'::uuid, '{party_type}', '{eid}'::uuid, {NOW}) "
                f'ON CONFLICT ("Id") DO NOTHING;'
            )
        w(
            f'INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") '
            f"VALUES ('{pe}'::uuid, '{iid}'::uuid, '{ta}', '{ida}'::uuid, '{tb}', '{idb}'::uuid, {NOW}) "
            f'ON CONFLICT ("Id") DO NOTHING;'
        )
    w("")


def seed_user_block(user_id: str, idx: int) -> None:
    w(f"-- User fixture {idx} ({user_id})")
    severe = ["Yes", "No", "DontKnow"][idx % 3]
    pollen = ["Yes", "No", "DontKnow"][(idx + 1) % 3]
    insect = ["Yes", "No", "DontKnow"][(idx + 2) % 3]
    latex = ["Yes", "No"][idx % 2]
    contra = ["oral_pill", "iud", "none", "barrier_method", "implant"][idx % 5]
    w(
        f'INSERT INTO "ClientAllergySummaries" ("Id", "UserId", "SevereReactionStatus", "CarriesAllergyMedication", '
        f'"PollenDustMitesAllergyStatus", "InsectVenomAllergyStatus", "LatexAllergyStatus", "CreatedAtUtc")\n'
        f"SELECT 'e3000000-0000-4000-8000-{idx:012d}'::uuid, '{user_id}'::uuid, '{severe}', {str(idx % 2 == 0).upper()}, "
        f"'{pollen}', '{insect}', '{latex}', {NOW}\n"
        f'WHERE NOT EXISTS (SELECT 1 FROM "ClientAllergySummaries" WHERE "UserId" = \'{user_id}\'::uuid);'
    )
    w(
        f'INSERT INTO client_reproductive_health_profiles ("Id", "UserId", "CurrentlyPregnant", "PlanningToGetPregnant", '
        f'"Menopause", "ContraceptionMethodCode", "CreatedAtUtc")\n'
        f"SELECT 'e4000000-0000-4000-8000-{idx:012d}'::uuid, '{user_id}'::uuid, FALSE, {str(idx == 2).upper()}, FALSE, "
        f"'{contra}', {NOW}\n"
        f'WHERE NOT EXISTS (SELECT 1 FROM client_reproductive_health_profiles WHERE "UserId" = \'{user_id}\'::uuid);'
    )
    df = ((idx - 1) % 20) + 1
    w(
        f'INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "DrugFormId", "Source", "Note", "CreatedAtUtc") VALUES\n'
        f"    ('e0000000-0000-4000-8000-{idx * 10 + 1:012d}'::uuid, '{user_id}'::uuid, "
        f"'a1000000-0000-4000-8000-{df:012d}'::uuid, 'Database', 'Rash after catalog drug form.', {NOW})\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )
    w(
        f'INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "CustomDrugName", "CustomDrugNameNormalized", '
        f'"CustomActiveSubstances", "CustomFormulationName", "CustomDosage", "Source", "Note", "CreatedAtUtc") VALUES (\n'
        f"    'e0000000-0000-4000-8000-{idx * 10 + 2:012d}'::uuid, '{user_id}'::uuid,\n"
        f"    'Homeopathic Arnica {idx}', 'homeopathic arnica {idx}', 'Arnica montana', 'Drops', '30C',\n"
        f"    'Custom', 'Custom remedy reaction.', {NOW}\n) ON CONFLICT (\"Id\") DO NOTHING;"
    )
    w(
        f'INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "FoodId", "Source", "Note", "CreatedAtUtc") VALUES\n'
        f"    ('e2000000-0000-4000-8000-{idx * 10 + 1:012d}'::uuid, '{user_id}'::uuid, "
        f"'b0000000-0000-4000-8000-{df:012d}'::uuid, 'Database', 'Oral tingling from catalog food.', {NOW})\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )
    w(
        f'INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "CustomFoodName", "CustomFoodNameNormalized", '
        f'"Source", "Note", "CreatedAtUtc") VALUES\n'
        f"    ('e2000000-0000-4000-8000-{idx * 10 + 2:012d}'::uuid, '{user_id}'::uuid, "
        f"'Artisan honeycomb {idx}', 'artisan honeycomb {idx}', 'Custom', "
        f"'Local market sample caused lip swelling.', {NOW})\nON CONFLICT (\"Id\") DO NOTHING;"
    )
    w(
        f'INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "SupplementId", "Source", "Note", "CreatedAtUtc") VALUES\n'
        f"    ('e1000000-0000-4000-8000-{idx * 10 + 1:012d}'::uuid, '{user_id}'::uuid, "
        f"'c0000000-0000-4000-8000-{df:012d}'::uuid, 'Database', 'Itching after supplement.', {NOW})\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )
    w(
        f'INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "CustomSupplementName", '
        f'"CustomSupplementNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES\n'
        f"    ('e1000000-0000-4000-8000-{idx * 10 + 2:012d}'::uuid, '{user_id}'::uuid, "
        f"'Herbal blend {idx}', 'herbal blend {idx}', 'Custom', 'Loose tea mix caused hives.', {NOW})\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )
    w(
        f'INSERT INTO "ClientOtherAllergies" ("Id", "UserId", "AllergyName", "AllergyNameNormalized", "Note", "CreatedAtUtc") VALUES\n'
        f"    ('e2500000-0000-4000-8000-{idx:012d}'::uuid, '{user_id}'::uuid, "
        f"'Nickel jewelry {idx}', 'nickel jewelry {idx}', 'Contact dermatitis from watch buckle.', {NOW})\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )
    hc_i = ((idx - 1) % 10) + 1
    w(
        f'INSERT INTO "UserHealthConditions" ("Id", "UserId", "HealthConditionId", "Source", "Note", "CreatedAtUtc") VALUES\n'
        f"    ('{uid(USER_HEALTH_CONDITION, idx)}'::uuid, '{user_id}'::uuid, "
        f"'d0000000-0000-4000-8000-{hc_i:012d}'::uuid, 'Database', 'Diagnosed and monitored by GP.', {NOW})\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )
    dp_i = ((df + 3) % 20) + 1
    problem_notes = [
        "Nausea within an hour of taking the medication.",
        "Dizziness reported after starting the drug.",
        "Mild rash after a few doses.",
        "Stomach pain when taken on an empty stomach.",
        "Drowsiness in the afternoon.",
    ]
    w(
        f'INSERT INTO "DrugProblems" ("Id", "UserId", "DrugFormId", "Source", "ProblemNote", "CreatedAtUtc") VALUES\n'
        f"    ('{uid(DRUG_PROBLEM, idx * 10 + 1)}'::uuid, '{user_id}'::uuid, "
        f"'a1000000-0000-4000-8000-{dp_i:012d}'::uuid, 'Database', "
        f"'{sql_str(problem_notes[idx % len(problem_notes)])}', {NOW})\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )
    w(
        f'INSERT INTO "DrugProblems" ("Id", "UserId", "CustomDrugName", "CustomActiveSubstances", '
        f'"CustomFormulationName", "CustomDosage", "Source", "ProblemNote", "CreatedAtUtc") VALUES (\n'
        f"    '{uid(DRUG_PROBLEM, idx * 10 + 2)}'::uuid, '{user_id}'::uuid,\n"
        f"    'Compound Cream X {idx}', 'Hydrocortisone, Clotrimazole', 'Cream', '1% / 1%',\n"
        f"    'Custom', 'Burning sensation and redness after applying the imported topical cream.', {NOW}\n"
        f') ON CONFLICT ("Id") DO NOTHING;'
    )
    meals = [("Breakfast", "07:30"), ("Lunch", "12:30"), ("Dinner", "18:30"), ("Evening snack", "20:30")]
    mvals = [
        f"    ('e8000000-0000-4000-8000-{idx * 10 + j:012d}'::uuid, '{user_id}'::uuid, '{n}', '{n.lower()}', '{t}'::time, {NOW})"
        for j, (n, t) in enumerate(meals, 1)
    ]
    w('INSERT INTO "ClientMealTimes" ("Id", "UserId", "MealName", "NormalizedMealName", "IntakeTime", "CreatedAtUtc")\nVALUES\n' + ",\n".join(mvals) + '\nON CONFLICT ("Id") DO NOTHING;')
    for j, drug_i in enumerate([df, ((df + 2) % 20) + 1], 1):
        rid = idx * 10 + j
        w(
            f'INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")\n'
            f"VALUES ('e5000000-0000-4000-8000-{rid:012d}'::uuid, '{user_id}'::uuid, "
            f"'a0000000-0000-4000-8000-{drug_i:012d}'::uuid, 'a1000000-0000-4000-8000-{drug_i:012d}'::uuid, "
            f"'Active', CURRENT_DATE, NULL, {NOW})\nON CONFLICT (\"Id\") DO NOTHING;"
        )
        w(
            f'INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")\n'
            f"VALUES ('e5100000-0000-4000-8000-{rid:012d}'::uuid, 'e5000000-0000-4000-8000-{rid:012d}'::uuid, "
            f"'08:00:00'::time, {NOW})\nON CONFLICT (\"Id\") DO NOTHING;"
        )
    hid, old_drug = idx * 10 + 9, ((df + 4) % 20) + 1
    w(
        f'INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", '
        f'"HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")\n'
        f"VALUES ('e5000000-0000-4000-8000-{hid:012d}'::uuid, '{user_id}'::uuid, "
        f"'a0000000-0000-4000-8000-{old_drug:012d}'::uuid, 'a1000000-0000-4000-8000-{old_drug:012d}'::uuid, "
        f"'Expired', CURRENT_DATE - 90, CURRENT_DATE - 10, 'ManualRemove', {NOW} - interval '10 days', {NOW} - interval '90 days')\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )
    for j in range(1, 3):
        fid, food_i = idx * 10 + j, ((df + j - 2) % 20) + 1
        w(
            f'INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")\n'
            f"VALUES ('e6000000-0000-4000-8000-{fid:012d}'::uuid, '{user_id}'::uuid, "
            f"'b0000000-0000-4000-8000-{food_i:012d}'::uuid, 'Active', CURRENT_DATE, NULL, {NOW})\n"
            f'ON CONFLICT ("Id") DO NOTHING;'
        )
        w(
            f'INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")\n'
            f"VALUES ('e6100000-0000-4000-8000-{fid:012d}'::uuid, 'e6000000-0000-4000-8000-{fid:012d}'::uuid, "
            f"'13:00:00'::time, {NOW})\nON CONFLICT (\"Id\") DO NOTHING;"
        )
    fexp, exp_food = idx * 10 + 9, ((df + 4) % 20) + 1
    w(
        f'INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", '
        f'"HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")\n'
        f"VALUES ('e6000000-0000-4000-8000-{fexp:012d}'::uuid, '{user_id}'::uuid, "
        f"'b0000000-0000-4000-8000-{exp_food:012d}'::uuid, 'Expired', CURRENT_DATE - 60, CURRENT_DATE - 5, "
        f"'AutoExpire', {NOW} - interval '5 days', {NOW} - interval '60 days')\nON CONFLICT (\"Id\") DO NOTHING;"
    )
    for j in range(1, 3):
        sid, supp_i = idx * 10 + j, ((df + j - 2) % 20) + 1
        w(
            f'INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")\n'
            f"VALUES ('e7000000-0000-4000-8000-{sid:012d}'::uuid, '{user_id}'::uuid, "
            f"'c0000000-0000-4000-8000-{supp_i:012d}'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, {NOW})\n"
            f'ON CONFLICT ("Id") DO NOTHING;'
        )
        w(
            f'INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")\n'
            f"VALUES ('e7100000-0000-4000-8000-{sid:012d}'::uuid, 'e7000000-0000-4000-8000-{sid:012d}'::uuid, "
            f"'09:00:00'::time, {NOW})\nON CONFLICT (\"Id\") DO NOTHING;"
        )
    sexp, exp_supp = idx * 10 + 9, ((df + 3) % 20) + 1
    w(
        f'INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", '
        f'"HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")\n'
        f"VALUES ('e7000000-0000-4000-8000-{sexp:012d}'::uuid, '{user_id}'::uuid, "
        f"'c0000000-0000-4000-8000-{exp_supp:012d}'::uuid, 'Database', 'Expired', CURRENT_DATE - 45, CURRENT_DATE - 3, "
        f"'ManualRemove', {NOW} - interval '3 days', {NOW} - interval '45 days')\nON CONFLICT (\"Id\") DO NOTHING;"
    )
    w(
        f'INSERT INTO client_own_supplements ("Id", "UserId", "Name", "NormalizedName", "CreatedAtUtc")\n'
        f"VALUES ('e7200000-0000-4000-8000-{idx:012d}'::uuid, '{user_id}'::uuid, "
        f"'Morning stack {idx}', 'morning stack {idx}', {NOW})\nON CONFLICT (\"Id\") DO NOTHING;"
    )
    w(
        f'INSERT INTO client_own_supplement_items ("Id", "OwnSupplementId", "SupplementId", "SortOrder", "CreatedAtUtc")\n'
        f"VALUES ('e7210000-0000-4000-8000-{idx:012d}'::uuid, 'e7200000-0000-4000-8000-{idx:012d}'::uuid, "
        f"'c0000000-0000-4000-8000-000000000001'::uuid, 1, {NOW}),\n"
        f"       ('e7210000-0000-4000-8000-{idx + 100:012d}'::uuid, 'e7200000-0000-4000-8000-{idx:012d}'::uuid, "
        f"'c0000000-0000-4000-8000-000000000005'::uuid, 2, {NOW})\nON CONFLICT (\"Id\") DO NOTHING;"
    )
    ntypes = [
        ("WeeklyHealthDataReminder", "MedicalData", "SharedRegisteredClient", "Update your health diary", "Review medications and symptoms this week.", False, "Informational"),
        ("SupervisionRequest", "DoctorPatient", "PatientOnly", "Supervision request pending", "A healthcare provider requested access to your profile.", True, "Pending"),
        ("MedicalChangeProposal", "DoctorPatient", "PatientOnly", "Medical change proposal", "Your doctor proposed an update to your medication list.", True, "Pending"),
        ("SystemAnnouncement", "SystemInformation", "SharedRegisteredClient", "Platform maintenance", "Scheduled maintenance this Sunday 02:00-04:00 UTC.", False, "Informational"),
    ]
    for j, (nt, tag, aud, title, summary, req, act) in enumerate(ntypes, 1):
        w(
            f'INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", '
            f'"Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")\n'
            f"VALUES ('{uid(NOTIFICATION, idx * 10 + j)}'::uuid, '{user_id}'::uuid, '{nt}', '{tag}', '{aud}', "
            f"'{title}', '{summary}', '{summary}', 'seed.{user_id}.{j}', {str(req).upper()}, '{act}', FALSE, {NOW})\n"
            f'ON CONFLICT ("Id") DO NOTHING;'
        )
    w("")


def catalog() -> None:
    w("-- Dev catalog + patient/provider fixtures.")
    w("-- Fixed user UUIDs are created by PatientDevDataSeeder before execution.")
    w("-- Bump DevSeed:SeedKey when you need a fresh apply (dev_seed_applications).")
    w("")
    w("-- Routes of administration (shared catalog)")
    w(
        'INSERT INTO "RoutesOfAdministration" ("Id", "RouteOfAdministrationName", "IsOralIntake", "CreatedAtUtc")\n'
        "VALUES\n"
        f"    ('f2200000-0000-4000-8000-000000000001'::uuid, 'Oral use', TRUE, {NOW}),\n"
        f"    ('f2200000-0000-4000-8000-000000000002'::uuid, 'Topical', FALSE, {NOW}),\n"
        f"    ('f2200000-0000-4000-8000-000000000003'::uuid, 'Inhalation', FALSE, {NOW})\n"
        'ON CONFLICT ("Id") DO UPDATE SET "RouteOfAdministrationName"=EXCLUDED."RouteOfAdministrationName", "IsOralIntake"=EXCLUDED."IsOralIntake";'
    )
    w("-- Pharmaceutical forms (shared catalog)")
    w(
        'INSERT INTO "PharmaceuticalForms" ("Id", "FormulationName", "CreatedAtUtc")\nVALUES\n'
        f"    ('f2100000-0000-4000-8000-000000000001'::uuid, 'Tablet', {NOW}),\n"
        f"    ('f2100000-0000-4000-8000-000000000002'::uuid, 'Capsule', {NOW}),\n"
        f"    ('f2100000-0000-4000-8000-000000000003'::uuid, 'Film-coated tablet', {NOW}),\n"
        f"    ('f2100000-0000-4000-8000-000000000004'::uuid, 'Oral solution', {NOW}),\n"
        f"    ('f2100000-0000-4000-8000-000000000005'::uuid, 'Cream', {NOW})\n"
        'ON CONFLICT ("Id") DO UPDATE SET "FormulationName"=EXCLUDED."FormulationName";'
    )
    w("-- Active substances (20)")
    svals = [
        f"    ('f2000000-0000-4000-8000-{i:012d}'::uuid, '{sql_str(n)}', {str(SUBSTANCE_ALCOHOL[i-1]).upper()}, {NOW})"
        for i, n in enumerate(SUBSTANCES, 1)
    ]
    w(
        'INSERT INTO "ActiveSubstances" ("Id", "ActiveSubstanceName", "HasAlcoholInteraction", "CreatedAtUtc")\nVALUES\n'
        + ",\n".join(svals)
        + '\nON CONFLICT ("Id") DO UPDATE SET "ActiveSubstanceName"=EXCLUDED."ActiveSubstanceName", "HasAlcoholInteraction"=EXCLUDED."HasAlcoholInteraction";'
    )
    w("-- Drugs (20 original + 15 extended)")
    drows = []
    for i, (name, typ) in enumerate(DRUGS, 1):
        counter = 2 if i <= 10 else 1
        drows.append(
            f"    ('a0000000-0000-4000-8000-{i:012d}'::uuid, '{sql_str(name)}', 'Human medicinal product', {counter}, '{typ}', {NOW})"
        )
    for name, typ, _sub, _dos, _oral, _atc, drug_i in EXTRA_DRUGS:
        drows.append(
            f"    ('a0000000-0000-4000-8000-{drug_i:012d}'::uuid, '{sql_str(name)}', 'Human medicinal product', 1, '{typ}', {NOW})"
        )
    w(
        'INSERT INTO "Drugs" ("Id", "CommercialName", "TypeOfProduct", "DrugFormCounter", "TypeOfDrug", "CreatedAtUtc")\nVALUES\n'
        + ",\n".join(drows)
        + '\nON CONFLICT ("Id") DO UPDATE SET "CommercialName"=EXCLUDED."CommercialName", "TypeOfProduct"=EXCLUDED."TypeOfProduct", '
        '"DrugFormCounter"=EXCLUDED."DrugFormCounter", "TypeOfDrug"=EXCLUDED."TypeOfDrug";'
    )
    w("-- Drug active substance links (original 20)")
    w(
        'INSERT INTO "DrugActiveSubstances" ("DrugId", "ActiveSubstanceId")\n'
        "SELECT ('a0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,\n"
        "       ('f2000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid\n"
        "FROM generate_series(1, 20) AS i\nON CONFLICT DO NOTHING;"
    )
    w("-- Drug active substance links (extended drugs)")
    elinks = [
        f"    ('a0000000-0000-4000-8000-{drug_i:012d}'::uuid, 'f2000000-0000-4000-8000-{sub_i:012d}'::uuid)"
        for name, typ, sub_i, dosage, oral, atc, drug_i in EXTRA_DRUGS
    ]
    w('INSERT INTO "DrugActiveSubstances" ("DrugId", "ActiveSubstanceId")\nVALUES\n' + ",\n".join(elinks) + "\nON CONFLICT DO NOTHING;")
    w("-- Drug forms (primary + alternates + extended)")
    frows = [
        f"    ('a1000000-0000-4000-8000-{i:012d}'::uuid, 'a0000000-0000-4000-8000-{i:012d}'::uuid, {910000000 + i}, '{dos}', {str(oral).upper()}, '{atc}', {NOW})"
        for i, dos, oral, atc in FORM_ROWS
    ]
    for drug_i, form_i, dos, oral, atc in ALT_FORMS:
        frows.append(
            f"    ('a1000000-0000-4000-8000-{form_i:012d}'::uuid, 'a0000000-0000-4000-8000-{drug_i:012d}'::uuid, {910000000 + form_i}, '{dos}', {str(oral).upper()}, '{atc}', {NOW})"
        )
    for name, typ, sub_i, dosage, oral, atc, drug_i in EXTRA_DRUGS:
        frows.append(
            f"    ('a1000000-0000-4000-8000-{drug_i:012d}'::uuid, 'a0000000-0000-4000-8000-{drug_i:012d}'::uuid, {910000000 + drug_i}, '{dosage}', {str(oral).upper()}, '{atc}', {NOW})"
        )
    w(
        'INSERT INTO "DrugForms" ("Id", "DrugId", "ProductIdentifier", "Dosage", "IsOral", "AtcCode", "CreatedAtUtc")\nVALUES\n'
        + ",\n".join(frows)
        + '\nON CONFLICT ("Id") DO UPDATE SET "DrugId"=EXCLUDED."DrugId", "ProductIdentifier"=EXCLUDED."ProductIdentifier", '
        '"Dosage"=EXCLUDED."Dosage", "IsOral"=EXCLUDED."IsOral", "AtcCode"=EXCLUDED."AtcCode";'
    )
    pf = [
        f"    ('a1000000-0000-4000-8000-{i:012d}'::uuid, 'f2100000-0000-4000-8000-00000000000{PHARMA_FORM_FOR_PRIMARY[i-1]:01d}'::uuid)"
        for i in range(1, 21)
    ]
    for drug_i, form_i, _d, _o, _a in ALT_FORMS:
        pf.append(f"    ('a1000000-0000-4000-8000-{form_i:012d}'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid)")
    for name, typ, sub_i, dosage, oral, atc, drug_i in EXTRA_DRUGS:
        pfi = 4 if oral else 5
        pf.append(f"    ('a1000000-0000-4000-8000-{drug_i:012d}'::uuid, 'f2100000-0000-4000-8000-00000000000{pfi:01d}'::uuid)")
    w('INSERT INTO "DrugFormPharmaceuticalForms" ("DrugFormId", "PharmaceuticalFormId")\nVALUES\n' + ",\n".join(pf) + "\nON CONFLICT DO NOTHING;")
    route_rows = [
        f"    ('a1000000-0000-4000-8000-{i:012d}'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid)" for i in range(1, 21)
    ]
    for drug_i, form_i, _d, oral, _a in ALT_FORMS:
        rid = 1 if oral else (3 if form_i == 109 else 2)
        route_rows.append(f"    ('a1000000-0000-4000-8000-{form_i:012d}'::uuid, 'f2200000-0000-4000-8000-00000000000{rid:01d}'::uuid)")
    for name, typ, sub_i, dosage, oral, atc, drug_i in EXTRA_DRUGS:
        rid = 1 if oral else (3 if typ == "Ophthalmic" and not oral else 2)
        route_rows.append(f"    ('a1000000-0000-4000-8000-{drug_i:012d}'::uuid, 'f2200000-0000-4000-8000-00000000000{rid:01d}'::uuid)")
    w('INSERT INTO "DrugFormRoutesOfAdministration" ("DrugFormId", "RouteOfAdministrationId")\nVALUES\n' + ",\n".join(route_rows) + "\nON CONFLICT DO NOTHING;")
    fvals = [
        f"    ('b0000000-0000-4000-8000-{i:012d}'::uuid, '{sql_str(n)}', '{a}'::jsonb, '{sql_str(s)}', {NOW})"
        for i, (n, a, s) in enumerate(FOODS, 1)
    ]
    w(
        'INSERT INTO "Foods" ("Id", "Name", "Aliases", "SearchText", "CreatedAtUtc")\nVALUES\n'
        + ",\n".join(fvals)
        + '\nON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name", "Aliases"=EXCLUDED."Aliases", "SearchText"=EXCLUDED."SearchText";'
    )
    svals2 = [
        f"    ('c0000000-0000-4000-8000-{i:012d}'::uuid, '{sql_str(n)}', '{a}'::jsonb, '{sql_str(s)}', {NOW})"
        for i, (n, a, s) in enumerate(SUPPLEMENTS, 1)
    ]
    w(
        'INSERT INTO "Supplements" ("Id", "Name", "Aliases", "SearchText", "CreatedAtUtc")\nVALUES\n'
        + ",\n".join(svals2)
        + '\nON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name", "Aliases"=EXCLUDED."Aliases", "SearchText"=EXCLUDED."SearchText";'
    )
    hvals = [
        f"    ('d0000000-0000-4000-8000-{i:012d}'::uuid, '{sql_str(n)}', '{sql_str(nn)}', '{sql_str(d)}', {NOW})"
        for i, (n, nn, d) in enumerate(HEALTH_CONDITIONS, 1)
    ]
    w(
        'INSERT INTO "HealthConditions" ("Id", "HealthConditionName", "NormalizedHealthConditionName", "Description", "CreatedAtUtc")\nVALUES\n'
        + ",\n".join(hvals)
        + '\nON CONFLICT ("Id") DO UPDATE SET "HealthConditionName"=EXCLUDED."HealthConditionName", '
        '"NormalizedHealthConditionName"=EXCLUDED."NormalizedHealthConditionName", "Description"=EXCLUDED."Description";'
    )


def extras() -> None:
    substance_texts = [
        "Combined use may increase bleeding risk.",
        "Pharmacodynamic synergy may cause hypotension.",
        "Metabolism competition can raise plasma levels.",
        "QT prolongation risk when combined.",
        "Serotonin syndrome risk with co-administration.",
    ]
    food_texts = [
        "Food may reduce absorption of the medication.",
        "High-fat meal increases peak concentration.",
        "Vitamin-rich food may antagonize the drug effect.",
        "Tyramine-rich food may cause hypertensive crisis.",
        "Grapefruit juice inhibits metabolism.",
    ]
    supplement_texts = [
        "Supplement may potentiate sedative effects.",
        "Mineral binding reduces drug bioavailability.",
        "Herbal product induces metabolic enzymes.",
        "Electrolyte imbalance may worsen adverse effects.",
        "Antioxidant supplement alters therapeutic response.",
    ]
    add_interactions("Active substance interactions (20)", "Substance", "Substance", "f2000000-0000-4000-8000", INT_SUB_I, INT_SUB_P, INT_SUB_E, substance_texts)
    add_interactions("Food interactions (20)", "Food", "Food", "b0000000-0000-4000-8000", INT_FOOD_I, INT_FOOD_P, INT_FOOD_E, food_texts)
    add_interactions("Supplement interactions (20)", "Supplement", "Supplement", "c0000000-0000-4000-8000", INT_SUPP_I, INT_SUPP_P, INT_SUPP_E, supplement_texts)
    w("-- Alcohol interaction overrides (20)")
    for i in range(1, 21):
        w(
            f'INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")\n'
            f"VALUES ('f2900000-0000-4000-8000-{i:012d}'::uuid, 'f2000000-0000-4000-8000-{i:012d}'::uuid, "
            f"'Avoid alcohol with {sql_str(SUBSTANCES[i-1])}: heightened CNS or bleeding risk.', {NOW})\n"
            f'ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";'
        )
    w("")
    w("-- FAQ entries (20)")
    for i in range(1, 21):
        q = f"How should I track supplement {i} with my prescriptions?"
        w(
            f'INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")\n'
            f"VALUES ('fa100000-0000-4000-8000-{i:012d}'::uuid, '{sql_str(q)}', '{sql_str(q.lower())}', "
            f"'Log intake times in the app and review interactions before adding new items.', {NOW})\n"
            f'ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";'
        )
    w("")
    w("-- Drug problem types (5)")
    dpt = ["Nausea", "Dizziness", "Rash", "Stomach pain", "Drowsiness"]
    for i, name in enumerate(dpt, 1):
        w(
            f'INSERT INTO "DrugProblemTypes" ("Id", "ProblemTypeName", "CreatedAtUtc")\n'
            f"VALUES ('f2800000-0000-4000-8000-{i:012d}'::uuid, '{name}', {NOW})\n"
            f'ON CONFLICT ("Id") DO UPDATE SET "ProblemTypeName"=EXCLUDED."ProblemTypeName";'
        )
    w("")
    group_names = ["NSAID cluster", "Cardiometabolic cluster", "Psychiatry cluster", "Endocrine cluster"]
    for gi, gname in enumerate(group_names, 1):
        w(
            f'INSERT INTO "ActiveSubstanceGroups" ("Id", "Name", "CreatedAtUtc")\n'
            f"VALUES ('f2500000-0000-4000-8000-{gi:012d}'::uuid, '{gname}', {NOW})\n"
            f'ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";'
        )
        members = [f"    ('f2500000-0000-4000-8000-{gi:012d}'::uuid, 'f2000000-0000-4000-8000-{(gi - 1) * 5 + j:012d}'::uuid)" for j in range(1, 6)]
        w('INSERT INTO "ActiveSubstanceGroupMembers" ("ActiveSubstanceGroupId", "ActiveSubstanceId")\nVALUES\n' + ",\n".join(members) + "\nON CONFLICT DO NOTHING;")
    fg = ["Citrus & fruit", "Dairy & protein", "Allergenic staples", "Pantry herbs"]
    for gi, gname in enumerate(fg, 1):
        w(
            f'INSERT INTO "FoodGroups" ("Id", "Name", "CreatedAtUtc")\n'
            f"VALUES ('f2600000-0000-4000-8000-{gi:012d}'::uuid, '{gname}', {NOW})\n"
            f'ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";'
        )
        members = [f"    ('f2600000-0000-4000-8000-{gi:012d}'::uuid, 'b0000000-0000-4000-8000-{(gi - 1) * 5 + j:012d}'::uuid)" for j in range(1, 6)]
        w('INSERT INTO "FoodGroupFoods" ("FoodGroupId", "FoodId")\nVALUES\n' + ",\n".join(members) + "\nON CONFLICT DO NOTHING;")
    sg = ["Vitamins", "Minerals", "Herbal", "Specialty blends"]
    for gi, gname in enumerate(sg, 1):
        w(
            f'INSERT INTO "SupplementGroups" ("Id", "Name", "CreatedAtUtc")\n'
            f"VALUES ('f2700000-0000-4000-8000-{gi:012d}'::uuid, '{gname}', {NOW})\n"
            f'ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";'
        )
        members = [f"    ('f2700000-0000-4000-8000-{gi:012d}'::uuid, 'c0000000-0000-4000-8000-{(gi - 1) * 5 + j:012d}'::uuid)" for j in range(1, 6)]
        w('INSERT INTO "SupplementGroupSupplements" ("SupplementGroupId", "SupplementId")\nVALUES\n' + ",\n".join(members) + "\nON CONFLICT DO NOTHING;")
    w("")
    w("-- Extra system texts (3)")
    texts = [
        ("interaction.disclaimer", "Interaction results are informational and do not replace clinical advice."),
        ("supervision.patientIntro", "You can approve or reject healthcare provider access requests at any time."),
        ("faq.supportFooter", "Contact support if your medication list changes after a hospital visit."),
    ]
    for i, (key, value) in enumerate(texts, 1):
        w(
            f'INSERT INTO "SystemTexts" ("Id", "Key", "Value", "CreatedAtUtc")\n'
            f"VALUES ('f6000000-0000-4000-8000-{i:012d}'::uuid, '{key}', '{sql_str(value)}', {NOW})\n"
            f'ON CONFLICT ("Key") DO UPDATE SET "Value"=EXCLUDED."Value";'
        )
    w("")
    w("-- Vet drugs (10)")
    vet = [
        (1, "Bravecto", "Weterynaryjny"), (2, "Frontline Combo", "Weterynaryjny"), (3, "Milbemax", "Weterynaryjny"),
        (4, "Rimadyl", "Weterynaryjny"), (5, "Metacam", "Weterynaryjny"), (6, "Baytril", "Weterynaryjny"),
        (7, "Nobivac DHPPi", "Weterynaryjny"), (8, "Canigen", "Weterynaryjny"), (9, "Synulox", "Weterynaryjny"),
        (10, "Previcox", "Weterynaryjny"),
    ]
    vrows = [f"    ('{uid(VET_DRUG, i)}'::uuid, {pid}, '{sql_str(name)}', '{top}', {NOW})" for i, (pid, name, top) in enumerate(vet, 1)]
    w('INSERT INTO "VetDrugs" ("Id", "ProductIdentifier", "CommercialName", "TypeOfProduct", "CreatedAtUtc")\nVALUES\n' + ",\n".join(vrows) + '\nON CONFLICT ("Id") DO UPDATE SET "CommercialName"=EXCLUDED."CommercialName", "TypeOfProduct"=EXCLUDED."TypeOfProduct";')
    w("")
    w("-- Temp drug import batch (1)")
    w(
        f'INSERT INTO "TempDrugImportBatches" ("Id", "NameOfFile", "BlobFileId", "SourceType", "StatusOfImport", '
        f'"TotalQuantityCount", "ImportedCount", "SkippedCounter", "FailedToCreateCounter", "RecordsDeletedInRevertCounter", '
        f'"FailedToDeleteCounter", "ImportedIds", "SkippedIds", "FailedToCreate", "RecordsDeletedInRevertIds", "FailedToDelete", "CreatedAtUtc")\n'
        f"VALUES ('{uid(TEMP_BATCH, 1)}'::uuid, 'seed-vet-import.csv', "
        f"'a0a0a0a0-0000-4000-8000-000000000001'::uuid, 'Seed', 'ImportedSuccessfully', 10, 10, 0, 0, 0, 0, '[]'::jsonb, "
        f"'[]'::jsonb, '[]'::jsonb, '[]'::jsonb, '[]'::jsonb, {NOW})\n"
        f'ON CONFLICT ("Id") DO UPDATE SET "StatusOfImport"=EXCLUDED."StatusOfImport";'
    )
    w("-- Temp drugs (10)")
    for i in range(1, 11):
        w(
            f'INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", '
            f'"RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")\n'
            f"VALUES ('{uid(TEMP_DRUG, i)}'::uuid, 'SEED-VET-{i:04d}', 'Imported vet candidate {i}', "
            f"'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', "
            f"'{uid(TEMP_BATCH, 1)}'::uuid, 'PendingReview', {NOW})\n"
            f'ON CONFLICT ("Id") DO NOTHING;'
        )
    w("")


def supervision() -> None:
    w("-- Supervision: pending requests (2)")
    w(
        f'INSERT INTO supervision_requests ("Id", patient_user_id, healthcare_professional_user_id, request_state, requested_at_utc, expires_at_utc, "CreatedAtUtc")\n'
        f"VALUES\n"
        f"    ('{uid(SUPERVISION_REQUEST, 1)}'::uuid, '{clients[4]}'::uuid, '{providers[2]}'::uuid, 'pending', {NOW} - interval '1 day', {NOW} + interval '6 days', {NOW}),\n"
        f"    ('{uid(SUPERVISION_REQUEST, 2)}'::uuid, '{clients[3]}'::uuid, '{providers[0]}'::uuid, 'pending', {NOW} - interval '2 hours', {NOW} + interval '7 days', {NOW})\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )
    w("")
    access_levels = ["ViewAndModify", "ViewOnly", "RestrictedView", "BlockAccess"]
    w("-- Supervision: active connections (7)")
    conn_rows = [
        (1, clients[0], providers[0], access_levels[0]),
        (2, clients[1], providers[0], access_levels[1]),
        (3, clients[2], providers[0], access_levels[2]),
        (4, clients[0], providers[1], access_levels[3]),
        (5, clients[1], providers[1], access_levels[0]),
        (6, clients[2], providers[1], access_levels[1]),
        (7, clients[3], providers[2], access_levels[2]),
    ]
    for cid, pat, prov, acc in conn_rows:
        w(
            f'INSERT INTO supervision_connections ("Id", patient_user_id, healthcare_professional_user_id, access_level_code, connection_state, approved_at_utc, "CreatedAtUtc")\n'
            f"VALUES ('{uid(SUPERVISION_CONNECTION, cid)}'::uuid, '{pat}'::uuid, '{prov}'::uuid, '{acc}', 'Active', {NOW} - interval '{cid} days', {NOW})\n"
            f'ON CONFLICT ("Id") DO NOTHING;'
        )
    w("")
    w("-- Medical change proposals (3) with items (2 each)")
    w(
        f'INSERT INTO medical_change_proposals ("Id", supervision_connection_id, patient_user_id, healthcare_professional_user_id, proposal_state, access_level_snapshot, proposal_summary, submitted_at_utc, "CreatedAtUtc")\n'
        f"VALUES\n"
        f"    ('{uid(MEDICAL_PROPOSAL, 1)}'::uuid, '{uid(SUPERVISION_CONNECTION, 1)}'::uuid, '{clients[0]}'::uuid, '{providers[0]}'::uuid, 'Pending', 'ViewAndModify', 'Add evening Metformin dose and note fasting requirement.', {NOW} - interval '3 hours', {NOW}),\n"
        f"    ('{uid(MEDICAL_PROPOSAL, 2)}'::uuid, '{uid(SUPERVISION_CONNECTION, 2)}'::uuid, '{clients[1]}'::uuid, '{providers[0]}'::uuid, 'Approved', 'ViewOnly', 'Document seasonal antihistamine use.', {NOW} - interval '2 days', {NOW}),\n"
        f"    ('{uid(MEDICAL_PROPOSAL, 3)}'::uuid, '{uid(SUPERVISION_CONNECTION, 5)}'::uuid, '{clients[1]}'::uuid, '{providers[1]}'::uuid, 'Rejected', 'ViewAndModify', 'Switch statin to alternate brand.', {NOW} - interval '5 days', {NOW})\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )
    items = [
        (uid(MEDICAL_PROPOSAL_ITEM, 1), uid(MEDICAL_PROPOSAL, 1), "RegularDrug", "a1000000-0000-4000-8000-000000000003", "Glucophage 500 mg", 1),
        (uid(MEDICAL_PROPOSAL_ITEM, 2), uid(MEDICAL_PROPOSAL, 1), "RegularSupplement", "c0000000-0000-4000-8000-000000000004", "Magnesium", 2),
        (uid(MEDICAL_PROPOSAL_ITEM, 3), uid(MEDICAL_PROPOSAL, 2), "HealthCondition", "d0000000-0000-4000-8000-000000000003", "Asthma", 1),
        (uid(MEDICAL_PROPOSAL_ITEM, 4), uid(MEDICAL_PROPOSAL, 2), "RegularFood", "b0000000-0000-4000-8000-000000000016", "Coffee", 2),
        (uid(MEDICAL_PROPOSAL_ITEM, 5), uid(MEDICAL_PROPOSAL, 3), "RegularDrug", "a1000000-0000-4000-8000-000000000012", "Simvacard 20 mg", 1),
        (uid(MEDICAL_PROPOSAL_ITEM, 6), uid(MEDICAL_PROPOSAL, 3), "RegularDrug", "a1000000-0000-4000-8000-000000000004", "Atorvasterol 20 mg", 2),
    ]
    ivals = [
        f"    ('{iid}'::uuid, '{pid}'::uuid, '{cat}', 'Database', '{rid}'::uuid, '{sql_str(name)}', '{{}}'::jsonb, {sort}, {NOW})"
        for iid, pid, cat, rid, name, sort in items
    ]
    w(
        'INSERT INTO medical_change_proposal_items ("Id", proposal_id, category_code, source, related_catalog_id, display_name_snapshot, apply_payload, sort_order, "CreatedAtUtc")\n'
        "VALUES\n" + ",\n".join(ivals) + '\nON CONFLICT ("Id") DO NOTHING;'
    )
    w("")
    w("-- Align supervision notification projections with runtime source event keys")
    w(
        f'UPDATE "UserNotifications" SET "SourceEventKey" = \'supervision-request:{uid(SUPERVISION_REQUEST, 1)}\', '
        f'"RelatedAggregateType" = \'supervision_request\', "RelatedAggregateId" = \'{uid(SUPERVISION_REQUEST, 1)}\' '
        f'WHERE "UserId" = \'{clients[4]}\' AND "TypeCode" = \'SupervisionRequest\';'
    )
    w(
        f'UPDATE "UserNotifications" SET "SourceEventKey" = \'supervision-request:{uid(SUPERVISION_REQUEST, 2)}\', '
        f'"RelatedAggregateType" = \'supervision_request\', "RelatedAggregateId" = \'{uid(SUPERVISION_REQUEST, 2)}\' '
        f'WHERE "UserId" = \'{clients[3]}\' AND "TypeCode" = \'SupervisionRequest\';'
    )
    w(
        f'UPDATE "UserNotifications" SET "SourceEventKey" = \'medical-change-proposal:{uid(MEDICAL_PROPOSAL, 1)}\', '
        f'"RelatedAggregateType" = \'medical_change_proposal\', "RelatedAggregateId" = \'{uid(MEDICAL_PROPOSAL, 1)}\' '
        f'WHERE "UserId" = \'{clients[0]}\' AND "TypeCode" = \'MedicalChangeProposal\' AND "ActionState" = \'Pending\';'
    )


def main() -> None:
    catalog()
    extras()
    for i, uid_client in enumerate(clients, 1):
        seed_user_block(uid_client, i)
    for i, uid_provider in enumerate(providers, 1):
        seed_user_block(uid_provider, 100 + i)
    supervision()
    out = Path(__file__).with_name("seed.sql")
    out.write_text("\n".join(lines) + "\n")
    print(f"Wrote {out} ({len(lines)} lines)")


if __name__ == "__main__":
    main()
