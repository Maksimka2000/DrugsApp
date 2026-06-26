#!/usr/bin/env python3
"""Generates Seed/seed.sql for dev catalog and multi-user fixtures."""
from __future__ import annotations

import json
import sys
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
MEDICAL_PROPOSAL_ITEM_INTAKE = "ab032000-0000-4000-8000"
DRUG_CATALOG = "a0000000-0000-4000-8000"
DRUG_FORM_CATALOG = "a1000000-0000-4000-8000"
FOOD_CATALOG = "b0000000-0000-4000-8000"
SUPPLEMENT_CATALOG = "c0000000-0000-4000-8000"
HEALTH_CONDITION_CATALOG = "d0000000-0000-4000-8000"
OWN_SUPPLEMENT_CATALOG = "e7200000-0000-4000-8000"
PROPOSAL_START_DATE = "2026-06-14"
PROPOSAL_END_SAME_DAY = "2026-06-14"
PROPOSAL_END_SEASONAL = "2026-09-30"
PROPOSAL_END_LONG = "2026-12-31"
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


def build_catalog_search_text(name: str, aliases_json: str) -> str:
    aliases = json.loads(aliases_json)
    return " ".join([name, *aliases]).strip().lower()


def catalog_uuid(stem: str, n: int) -> str:
    return f"{stem}-{n:012d}"


def sql_json(value: object) -> str:
    return f"'{sql_str(json.dumps(value, separators=(',', ':')))}'::jsonb"


def sql_date(value: str | None) -> str:
    if value is None:
        return "NULL"
    return f"DATE '{value}'"


def sql_note(value: str | None) -> str:
    if value is None:
        return "NULL"
    return f"'{sql_str(value)}'"


def regular_drug_payload(
    drug_n: int,
    start: str | None,
    end: str | None,
    times: list[str],
) -> dict[str, object]:
    return {
        "drugId": catalog_uuid(DRUG_CATALOG, drug_n),
        "drugFormId": catalog_uuid(DRUG_FORM_CATALOG, drug_n),
        "startDate": start,
        "endDate": end,
        "intakeTimes": times,
    }


def regular_food_payload(
    food_n: int,
    start: str | None,
    end: str | None,
    times: list[str],
) -> dict[str, object]:
    return {
        "foodId": catalog_uuid(FOOD_CATALOG, food_n),
        "startDate": start,
        "endDate": end,
        "intakeTimes": times,
    }


def regular_supplement_db_payload(
    supplement_n: int,
    start: str | None,
    end: str | None,
    times: list[str],
) -> dict[str, object]:
    return {
        "source": "database",
        "supplementId": catalog_uuid(SUPPLEMENT_CATALOG, supplement_n),
        "startDate": start,
        "endDate": end,
        "intakeTimes": times,
    }


def regular_supplement_custom_payload(
    custom_name: str,
    start: str | None,
    end: str | None,
    times: list[str],
) -> dict[str, object]:
    return {
        "customSupplementName": custom_name,
        "startDate": start,
        "endDate": end,
        "intakeTimes": times,
    }


def medical_change_proposal_draft_notes() -> None:
    """Document provider POST draftItems shape (persisted rows store composed apply_payload)."""
    drug_form = catalog_uuid(DRUG_FORM_CATALOG, 5)
    food_id = catalog_uuid(FOOD_CATALOG, 3)
    supplement_id = catalog_uuid(SUPPLEMENT_CATALOG, 6)
    w("-- Provider POST /api/supervision/provider/patients/{patient_id}/proposals")
    w("-- For regular_drug / regular_food / regular_supplement only. Server composes apply_payload.")
    w("-- intakeTimes use HH:mm:ss (TimeOnly), not ISO timestamps.")
    w("-- regular_drug example:")
    w(
        "--   {\"categoryCode\":\"regular_drug\",\"relatedCatalogId\":\"" + drug_form + "\","
        "\"startDate\":\"" + PROPOSAL_START_DATE + "\","
        "\"endDate\":\"" + PROPOSAL_END_SAME_DAY + "\",\"note\":\"Take before breakfast.\","
        "\"intakeTimes\":[\"08:00:00\",\"20:00:00\"]}"
    )
    w("-- regular_food example:")
    w(
        "--   {\"categoryCode\":\"regular_food\",\"relatedCatalogId\":\"" + food_id + "\","
        "\"startDate\":\"" + PROPOSAL_START_DATE + "\",\"endDate\":\"" + PROPOSAL_END_SAME_DAY + "\","
        "\"intakeTimes\":[\"13:00:00\"]}"
    )
    w("-- regular_supplement (database) example:")
    w(
        "--   {\"categoryCode\":\"regular_supplement\",\"source\":\"database\","
        "\"relatedCatalogId\":\"" + supplement_id + "\",\"startDate\":\"" + PROPOSAL_START_DATE + "\","
        "\"endDate\":\"" + PROPOSAL_END_SAME_DAY + "\",\"intakeTimes\":[\"09:00:00\"]}"
    )
    w("-- regular_supplement (custom free-text name) example:")
    w(
        "--   {\"categoryCode\":\"regular_supplement\",\"source\":\"custom\","
        "\"customValue\":\"Collagen peptide powder\",\"startDate\":\"" + PROPOSAL_START_DATE + "\","
        "\"intakeTimes\":[\"07:30:00\"]}"
    )
    w("-- Persisted apply_payload after compose (used on patient approve):")
    w(
        "--   regular_drug: {\"drugId\":\"...\",\"drugFormId\":\"...\",\"startDate\":\"...\","
        "\"endDate\":\"...\",\"intakeTimes\":[\"08:00:00\"]}"
    )
    w(
        "--   regular_food: {\"foodId\":\"...\",\"startDate\":\"...\",\"endDate\":\"...\","
        "\"intakeTimes\":[\"13:00:00\"]}"
    )
    w(
        "--   regular_supplement (database): {\"source\":\"database\",\"supplementId\":\"...\","
        "\"startDate\":\"...\",\"endDate\":\"...\",\"intakeTimes\":[\"09:00:00\"]}"
    )
    w(
        "--   regular_supplement (custom): {\"customSupplementName\":\"...\","
        "\"startDate\":\"...\",\"endDate\":\"...\",\"intakeTimes\":[\"07:30:00\"]}"
    )
    w("")


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
    ("Grapefruit", '["pink grapefruit","citrus"]'),
    ("Milk", '["cow milk","dairy"]'),
    ("Peanuts", '["groundnuts"]'),
    ("Eggs", '["chicken eggs"]'),
    ("Shellfish", '["shrimp","prawns"]'),
    ("Soy", '["soya","tofu"]'),
    ("Wheat", '["gluten","bread"]'),
    ("Almonds", "[]"),
    ("Strawberries", "[]"),
    ("Dark chocolate", '["cocoa"]'),
    ("Aged cheese", '["cheddar","parmesan"]'),
    ("Tomatoes", "[]"),
    ("Avocado", "[]"),
    ("Spinach", "[]"),
    ("Bananas", "[]"),
    ("Coffee", '["espresso"]'),
    ("Honey", "[]"),
    ("Garlic", "[]"),
    ("Onion", '["red onion"]'),
    ("Celery", "[]"),
]

SUPPLEMENTS = [
    ("Vitamin D3", '["cholecalciferol"]'),
    ("Omega-3 Fish Oil", '["EPA","DHA"]'),
    ("Iron", '["ferrous sulfate"]'),
    ("Magnesium", '["magnesium citrate"]'),
    ("Vitamin C", '["ascorbic acid"]'),
    ("Zinc", "[]"),
    ("Probiotics", '["lactobacillus"]'),
    ("Vitamin B12", '["cobalamin"]'),
    ("Folic acid", '["folate","vitamin b9"]'),
    ("Calcium", '["calcium carbonate"]'),
    ("Multivitamin", "[]"),
    ("Coenzyme Q10", '["CoQ10"]'),
    ("Melatonin", "[]"),
    ("Turmeric", '["curcumin"]'),
    ("Vitamin B6", '["pyridoxine"]'),
    ("Biotin", '["vitamin b7"]'),
    ("Potassium", "[]"),
    ("Psyllium fiber", '["fiber supplement"]'),
    ("Glucosamine", "[]"),
    ("Ashwagandha", "[]"),
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
        f"    ('b0000000-0000-4000-8000-{i:012d}'::uuid, '{sql_str(n)}', '{a}'::jsonb, '{sql_str(build_catalog_search_text(n, a))}', {NOW})"
        for i, (n, a) in enumerate(FOODS, 1)
    ]
    w(
        'INSERT INTO "Foods" ("Id", "Name", "Aliases", "SearchText", "CreatedAtUtc")\nVALUES\n'
        + ",\n".join(fvals)
        + '\nON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name", "Aliases"=EXCLUDED."Aliases", "SearchText"=EXCLUDED."SearchText";'
    )
    svals2 = [
        f"    ('c0000000-0000-4000-8000-{i:012d}'::uuid, '{sql_str(n)}', '{a}'::jsonb, '{sql_str(build_catalog_search_text(n, a))}', {NOW})"
        for i, (n, a) in enumerate(SUPPLEMENTS, 1)
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


def medical_change_proposals() -> None:
    """Seed medical change proposals across patients, states, and intake categories."""
    medical_change_proposal_draft_notes()
    w("-- Medical change proposals (17) with structured intake items and apply_payload")
    proposals = [
        # prop_n, conn, client_i, provider_i, state, access, summary, submitted_ago, resolved_ago
        (1, 1, 0, 0, "Pending", "ViewAndModify", "Add Controloc before breakfast for acid reflux.", "3 hours", None),
        (2, 1, 0, 0, "Pending", "ViewAndModify", "Start zinc supplementation in the morning.", "6 hours", None),
        (3, 5, 1, 1, "Pending", "ViewAndModify", "Log daily grapefruit intake for interaction monitoring.", "12 hours", None),
        (4, 2, 1, 0, "Pending", "ViewOnly", "Add Amotaks course after dental procedure.", "18 hours", None),
        (5, 3, 2, 0, "Pending", "RestrictedView", "Adjust thyroid medication and folic acid support.", "1 day", None),
        (6, 6, 2, 1, "Pending", "ViewAndModify", "Schedule off-catalog collagen supplement (custom name).", "2 days", None),
        (7, 7, 3, 2, "Pending", "RestrictedView", "Add rescue inhaler before exercise.", "30 hours", None),
        (8, 7, 3, 2, "Pending", "ViewAndModify", "Track daily coffee intake with afternoon cutoff.", "2 days", None),
        (9, 2, 1, 0, "Approved", "ViewOnly", "Document seasonal antihistamine and coffee habits.", "4 days", "3 days"),
        (10, 5, 1, 1, "Rejected", "ViewAndModify", "Switch statin to alternate brand.", "5 days", "4 days"),
        (11, 1, 0, 0, "Pending", "ViewAndModify", "Add Lisinoratio for blood pressure monitoring.", "8 hours", None),
        (12, 6, 2, 1, "Pending", "ViewAndModify", "Evening magnesium for muscle recovery.", "36 hours", None),
        # Dedicated approve-test proposals (client1 / connection ab02...001)
        (13, 1, 0, 0, "Pending", "ViewAndModify", "[TEST] Approve regular drug only — Controloc same-day course.", "2 hours", None),
        (14, 1, 0, 0, "Pending", "ViewAndModify", "[TEST] Approve regular food only — Peanuts same-day.", "4 hours", None),
        (15, 1, 0, 0, "Pending", "ViewAndModify", "[TEST] Approve regular supplement (database) — Zinc.", "5 hours", None),
        (16, 1, 0, 0, "Pending", "ViewAndModify", "[TEST] Approve regular supplement only — Vitamin B12 same-day.", "6 hours", None),
        (17, 1, 0, 0, "Pending", "ViewAndModify", "[TEST] Approve bundle: drug + food + supplement in one proposal.", "7 hours", None),
    ]
    prop_rows: list[str] = []
    for prop_n, conn, client_i, provider_i, state, access, summary, submitted_ago, resolved_ago in proposals:
        resolved_sql = "NULL"
        resolved_by_sql = "NULL"
        if resolved_ago is not None:
            resolved_sql = f"{NOW} - interval '{resolved_ago}'"
            resolved_by_sql = f"'{clients[client_i]}'::uuid"
        prop_rows.append(
            f"    ('{uid(MEDICAL_PROPOSAL, prop_n)}'::uuid, '{uid(SUPERVISION_CONNECTION, conn)}'::uuid, "
            f"'{clients[client_i]}'::uuid, '{providers[provider_i]}'::uuid, '{state}', '{access}', "
            f"'{sql_str(summary)}', {NOW} - interval '{submitted_ago}', {resolved_sql}, {resolved_by_sql}, {NOW})"
        )
    w(
        'INSERT INTO medical_change_proposals ("Id", supervision_connection_id, patient_user_id, '
        'healthcare_professional_user_id, proposal_state, access_level_snapshot, proposal_summary, '
        'submitted_at_utc, resolved_at_utc, resolved_by_patient_user_id, "CreatedAtUtc")\n'
        "VALUES\n" + ",\n".join(prop_rows) + '\nON CONFLICT ("Id") DO NOTHING;'
    )

    # item_n, prop_n, category, source, related_uuid, display, custom_value, start, end, note, sort, payload, intake_times
    item_specs: list[tuple] = [
        (1, 1, "RegularDrug", "Database", catalog_uuid(DRUG_FORM_CATALOG, 5), "Controloc 20 mg", None, PROPOSAL_START_DATE, None, "Take 30 minutes before breakfast.", 1, regular_drug_payload(5, PROPOSAL_START_DATE, None, ["07:30:00", "20:00:00"]), ["07:30:00", "20:00:00"]),
        (2, 1, "RegularSupplement", "Database", catalog_uuid(SUPPLEMENT_CATALOG, 6), "Zinc", None, PROPOSAL_START_DATE, PROPOSAL_END_LONG, "With food to reduce nausea.", 2, regular_supplement_db_payload(6, PROPOSAL_START_DATE, PROPOSAL_END_LONG, ["09:00:00"]), ["09:00:00"]),
        (3, 2, "RegularDrug", "Database", catalog_uuid(DRUG_FORM_CATALOG, 15), "Zyrtec 10 mg", None, PROPOSAL_START_DATE, PROPOSAL_END_SEASONAL, "Seasonal allergy course.", 1, regular_drug_payload(15, PROPOSAL_START_DATE, PROPOSAL_END_SEASONAL, ["21:00:00"]), ["21:00:00"]),
        (4, 3, "RegularFood", "Database", catalog_uuid(FOOD_CATALOG, 1), "Grapefruit", None, PROPOSAL_START_DATE, None, "Avoid with evening statin dose.", 1, regular_food_payload(1, PROPOSAL_START_DATE, None, ["08:00:00"]), ["08:00:00"]),
        (5, 4, "RegularDrug", "Database", catalog_uuid(DRUG_FORM_CATALOG, 6), "Amotaks 500 mg", None, PROPOSAL_START_DATE, PROPOSAL_END_SEASONAL, "Complete full antibiotic course.", 1, regular_drug_payload(6, PROPOSAL_START_DATE, PROPOSAL_END_SEASONAL, ["08:00:00", "20:00:00"]), ["08:00:00", "20:00:00"]),
        (6, 5, "RegularDrug", "Database", catalog_uuid(DRUG_FORM_CATALOG, 8), "Euthyrox 50 mcg", None, PROPOSAL_START_DATE, None, "Take on empty stomach.", 1, regular_drug_payload(8, PROPOSAL_START_DATE, None, ["06:30:00"]), ["06:30:00"]),
        (7, 5, "RegularSupplement", "Database", catalog_uuid(SUPPLEMENT_CATALOG, 10), "Folic acid", None, PROPOSAL_START_DATE, PROPOSAL_END_LONG, None, 2, regular_supplement_db_payload(10, PROPOSAL_START_DATE, PROPOSAL_END_LONG, ["12:00:00"]), ["12:00:00"]),
        (8, 6, "RegularSupplement", "Custom", None, "Collagen peptide powder", "Collagen peptide powder", PROPOSAL_START_DATE, None, "Off-catalog supplement name from provider.", 1, regular_supplement_custom_payload("Collagen peptide powder", PROPOSAL_START_DATE, None, ["07:00:00"]), ["07:00:00"]),
        (9, 7, "RegularDrug", "Database", catalog_uuid(DRUG_FORM_CATALOG, 9), "Ventolin 2 mg", None, PROPOSAL_START_DATE, PROPOSAL_END_SEASONAL, "Use before exercise.", 1, regular_drug_payload(9, PROPOSAL_START_DATE, PROPOSAL_END_SEASONAL, ["17:00:00"]), ["17:00:00"]),
        (10, 8, "RegularFood", "Database", catalog_uuid(FOOD_CATALOG, 16), "Coffee", None, PROPOSAL_START_DATE, None, "No coffee after 14:00.", 1, regular_food_payload(16, PROPOSAL_START_DATE, None, ["07:45:00", "13:30:00"]), ["07:45:00", "13:30:00"]),
        (11, 9, "HealthCondition", "Database", catalog_uuid(HEALTH_CONDITION_CATALOG, 3), "Asthma", None, None, None, "Documented for seasonal plan.", 1, {}, []),
        (12, 9, "RegularFood", "Database", catalog_uuid(FOOD_CATALOG, 16), "Coffee", None, PROPOSAL_START_DATE, PROPOSAL_END_SEASONAL, None, 2, regular_food_payload(16, PROPOSAL_START_DATE, PROPOSAL_END_SEASONAL, ["08:00:00"]), ["08:00:00"]),
        (13, 10, "RegularDrug", "Database", catalog_uuid(DRUG_FORM_CATALOG, 12), "Simvacard 20 mg", None, PROPOSAL_START_DATE, None, None, 1, regular_drug_payload(12, PROPOSAL_START_DATE, None, ["22:00:00"]), ["22:00:00"]),
        (14, 10, "RegularDrug", "Database", catalog_uuid(DRUG_FORM_CATALOG, 4), "Atorvasterol 20 mg", None, PROPOSAL_START_DATE, None, "Rejected brand switch.", 2, regular_drug_payload(4, PROPOSAL_START_DATE, None, ["22:00:00"]), ["22:00:00"]),
        (15, 11, "RegularDrug", "Database", catalog_uuid(DRUG_FORM_CATALOG, 7), "Lisinoratio 10 mg", None, PROPOSAL_START_DATE, PROPOSAL_END_LONG, "Monitor blood pressure weekly.", 1, regular_drug_payload(7, PROPOSAL_START_DATE, PROPOSAL_END_LONG, ["08:00:00"]), ["08:00:00"]),
        (16, 12, "RegularSupplement", "Database", catalog_uuid(SUPPLEMENT_CATALOG, 4), "Magnesium", None, PROPOSAL_START_DATE, None, "Take in the evening.", 1, regular_supplement_db_payload(4, PROPOSAL_START_DATE, None, ["21:30:00"]), ["21:30:00"]),
        # [TEST] single-category approve proposals for client1 (proposal ids 13–17)
        (17, 13, "RegularDrug", "Database", catalog_uuid(DRUG_FORM_CATALOG, 5), "Controloc 20 mg", None, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, "Same-day course; take before breakfast.", 1, regular_drug_payload(5, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, ["08:00:00", "20:00:00"]), ["08:00:00", "20:00:00"]),
        (18, 14, "RegularFood", "Database", catalog_uuid(FOOD_CATALOG, 3), "Peanuts", None, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, "With lunch.", 1, regular_food_payload(3, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, ["13:00:00"]), ["13:00:00"]),
        (19, 15, "RegularSupplement", "Database", catalog_uuid(SUPPLEMENT_CATALOG, 6), "Zinc", None, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, "With food.", 1, regular_supplement_db_payload(6, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, ["09:00:00"]), ["09:00:00"]),
        (20, 16, "RegularSupplement", "Database", catalog_uuid(SUPPLEMENT_CATALOG, 8), "Vitamin B12", None, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, "Same-day course.", 1, regular_supplement_db_payload(8, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, ["08:00:00"]), ["08:00:00"]),
        (21, 17, "RegularDrug", "Database", catalog_uuid(DRUG_FORM_CATALOG, 11), "Norvasc 5 mg", None, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, "Bundle item 1/3.", 1, regular_drug_payload(11, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, ["08:00:00"]), ["08:00:00"]),
        (22, 17, "RegularFood", "Database", catalog_uuid(FOOD_CATALOG, 16), "Coffee", None, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, "Bundle item 2/3.", 2, regular_food_payload(16, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, ["07:45:00"]), ["07:45:00"]),
        (23, 17, "RegularSupplement", "Database", catalog_uuid(SUPPLEMENT_CATALOG, 4), "Magnesium", None, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, "Bundle item 3/3.", 3, regular_supplement_db_payload(4, PROPOSAL_START_DATE, PROPOSAL_END_SAME_DAY, ["21:00:00"]), ["21:00:00"]),
    ]
    item_rows: list[str] = []
    for item_n, prop_n, category, source, related, display, custom_value, start, end, note, sort, payload, _times in item_specs:
        related_sql = f"'{related}'::uuid" if related else "NULL"
        item_rows.append(
            f"    ('{uid(MEDICAL_PROPOSAL_ITEM, item_n)}'::uuid, '{uid(MEDICAL_PROPOSAL, prop_n)}'::uuid, '{category}', "
            f"'{source}', {related_sql}, NULL, {sql_note(custom_value)}, '{sql_str(display)}', "
            f"{sql_date(start)}, {sql_date(end)}, {sql_note(note)}, {sql_json(payload)}, {sort}, {NOW})"
        )
    w(
        'INSERT INTO medical_change_proposal_items ("Id", proposal_id, category_code, source, related_catalog_id, '
        'secondary_text_snapshot, custom_value, display_name_snapshot, start_date, end_date, note, apply_payload, sort_order, "CreatedAtUtc")\n'
        "VALUES\n" + ",\n".join(item_rows) + '\nON CONFLICT ("Id") DO NOTHING;'
    )

    intake_rows: list[str] = []
    intake_n = 1
    for item_n, _prop_n, _cat, _src, _rel, _disp, _custom, _start, _end, _note, _sort, _payload, times in item_specs:
        for intake_time in times:
            intake_rows.append(
                f"    ('{uid(MEDICAL_PROPOSAL_ITEM_INTAKE, intake_n)}'::uuid, "
                f"'{uid(MEDICAL_PROPOSAL_ITEM, item_n)}'::uuid, '{intake_time}'::time, {NOW})"
            )
            intake_n += 1
    if intake_rows:
        w(
            'INSERT INTO medical_change_proposal_item_intake_times ("Id", proposal_item_id, intake_time, "CreatedAtUtc")\n'
            "VALUES\n" + ",\n".join(intake_rows) + '\nON CONFLICT ("Id") DO NOTHING;'
        )


def supervision_connection_insert(
    conn_id: int,
    patient_id: str,
    provider_id: str,
    access_level: str,
    state: str,
    approved_days_ago: int,
    removed_days_ago: int | None = None,
) -> None:
    removed_sql = f"{NOW} - interval '{removed_days_ago} days'" if removed_days_ago is not None else "NULL"
    w(
        f'INSERT INTO supervision_connections ("Id", patient_user_id, healthcare_professional_user_id, '
        f'access_level_code, connection_state, approved_at_utc, removed_at_utc, "CreatedAtUtc")\n'
        f"VALUES ('{uid(SUPERVISION_CONNECTION, conn_id)}'::uuid, '{patient_id}'::uuid, '{provider_id}'::uuid, "
        f"'{access_level}', '{state}', {NOW} - interval '{approved_days_ago} days', {removed_sql}, {NOW})\n"
        f'ON CONFLICT ("Id") DO NOTHING;'
    )


def supervision_api_routes() -> None:
    w("-- Supervision API routes")
    w("-- Patient (Client):")
    w("--   GET    /api/supervision/patient/requests")
    w("--   GET    /api/supervision/patient/requests/{request_id}")
    w("--   POST   /api/supervision/patient/requests/{request_id}/approve")
    w("--   POST   /api/supervision/patient/requests/{request_id}/reject")
    w("--   GET    /api/supervision/patient/healthcare-professionals")
    w("--   GET    /api/supervision/patient/healthcare-professionals/{healthcare_professional_user_id}")
    w("--   PUT    /api/supervision/patient/healthcare-professionals/{healthcare_professional_user_id}/access-level")
    w("--   DELETE /api/supervision/patient/healthcare-professionals/{healthcare_professional_user_id}")
    w("--   GET    /api/supervision/patient/proposals")
    w("--   GET    /api/supervision/patient/proposals/{proposal_id}")
    w("--   POST   /api/supervision/patient/proposals/{proposal_id}/approve")
    w("--   POST   /api/supervision/patient/proposals/{proposal_id}/reject")
    w("-- Provider (HealthcareProvider):")
    w("--   GET    /api/supervision/provider/requests")
    w("--   POST   /api/supervision/provider/requests/request")
    w("--   POST   /api/supervision/provider/requests/{request_id}/cancel")
    w("--   GET    /api/supervision/provider/patients")
    w("--   GET    /api/supervision/provider/patients/{patient_id}")
    w("--   DELETE /api/supervision/provider/patients/{patient_id}")
    w("--   POST   /api/supervision/provider/patients/{patient_id}/proposals")
    w("")


def supervision() -> None:
    supervision_api_routes()
    access_levels = ["ViewAndModify", "ViewOnly", "RestrictedView", "BlockAccess"]

    w("-- Supervision: pending requests (6)")
    pending_requests = [
        # req_id, client_i, provider_i, requested_ago, expires_in_days
        (1, 4, 2, "1 day", 6),       # client5 ← provider3 (re-request after removed conn 13)
        (2, 3, 0, "2 hours", 7),     # client4 ← provider1 (re-request after removed conn 14)
        (3, 0, 2, "5 hours", 5),     # client1 ← provider3
        (4, 1, 2, "9 hours", 4),     # client2 ← provider3
        (5, 3, 2, "14 hours", 3),    # client4 ← provider3
        (6, 4, 0, "20 hours", 2),    # client5 ← provider1
    ]
    req_rows = [
        f"    ('{uid(SUPERVISION_REQUEST, req_id)}'::uuid, '{clients[client_i]}'::uuid, "
        f"'{providers[provider_i]}'::uuid, 'pending', {NOW} - interval '{requested_ago}', "
        f"{NOW} + interval '{expires_in} days', {NOW})"
        for req_id, client_i, provider_i, requested_ago, expires_in in pending_requests
    ]
    w(
        'INSERT INTO supervision_requests ("Id", patient_user_id, healthcare_professional_user_id, '
        'request_state, requested_at_utc, expires_at_utc, "CreatedAtUtc")\n'
        "VALUES\n" + ",\n".join(req_rows) + "\nON CONFLICT (\"Id\") DO NOTHING;"
    )
    w("")

    w("-- Supervision: active connections (8)")
    active_connections = [
        # conn_id, client_i, provider_i, access_i, approved_days_ago
        (1, 0, 0, 0, 14),   # client1 + provider1 ViewAndModify
        (2, 1, 0, 1, 13),   # client2 + provider1 ViewOnly
        (3, 2, 0, 2, 12),   # client3 + provider1 RestrictedView
        (4, 0, 1, 3, 11),   # client1 + provider2 BlockAccess
        (5, 1, 1, 0, 10),   # client2 + provider2 ViewAndModify
        (6, 2, 1, 1, 9),    # client3 + provider2 ViewOnly
        (7, 2, 2, 2, 8),    # client3 + provider3 RestrictedView
        (8, 3, 1, 0, 7),    # client4 + provider2 ViewAndModify
    ]
    for conn_id, client_i, provider_i, access_i, approved_days in active_connections:
        supervision_connection_insert(
            conn_id,
            clients[client_i],
            providers[provider_i],
            access_levels[access_i],
            "Active",
            approved_days,
        )
    w("")

    w("-- Supervision: removed connections (5) — pairs with pending re-requests or history-only")
    removed_connections = [
        # conn_id, client_i, provider_i, access_i, approved_days_ago, removed_days_ago
        (13, 4, 2, 0, 45, 3),   # client5 + provider3 (pending request 1)
        (14, 3, 0, 2, 60, 10),  # client4 + provider1 (pending request 2)
        (15, 2, 0, 1, 90, 25),  # client3 + provider1 history
        (16, 1, 2, 3, 30, 12),  # client2 + provider3 history (pending request 4)
        (17, 4, 0, 1, 50, 8),   # client5 + provider1 history (pending request 6)
    ]
    for conn_id, client_i, provider_i, access_i, approved_days, removed_days in removed_connections:
        supervision_connection_insert(
            conn_id,
            clients[client_i],
            providers[provider_i],
            access_levels[access_i],
            "Removed",
            approved_days,
            removed_days,
        )
    w("")
    medical_change_proposals()
    w("")
    w("-- Align supervision notification projections with runtime source event keys")
    supervision_request_notifications = [
        (1, 4),
        (2, 3),
        (3, 0),
        (4, 1),
        (5, 3),
        (6, 4),
    ]
    for req_id, client_i in supervision_request_notifications:
        request_uuid = uid(SUPERVISION_REQUEST, req_id)
        w(
            f'UPDATE "UserNotifications" SET "SourceEventKey" = \'supervision-request:{request_uuid}\', '
            f'"RelatedAggregateType" = \'supervision_request\', "RelatedAggregateId" = \'{request_uuid}\', '
            f'"DeepLinkPath" = \'/supervision/patient/requests/{request_uuid}\' '
            f'WHERE "UserId" = \'{clients[client_i]}\' AND "TypeCode" = \'SupervisionRequest\' AND "ActionState" = \'Pending\' '
            f"AND \"SourceEventKey\" LIKE 'seed.%';"
        )
    w(
        f'UPDATE "UserNotifications" SET "SourceEventKey" = \'medical-change-proposal:{uid(MEDICAL_PROPOSAL, 1)}\', '
        f'"RelatedAggregateType" = \'medical_change_proposal\', "RelatedAggregateId" = \'{uid(MEDICAL_PROPOSAL, 1)}\', '
        f'"DeepLinkPath" = \'/supervision/patient/proposals/{uid(MEDICAL_PROPOSAL, 1)}\' '
        f'WHERE "UserId" = \'{clients[0]}\' AND "TypeCode" = \'MedicalChangeProposal\' AND "ActionState" = \'Pending\';'
    )
    for client_i, prop_n in ((1, 3), (2, 5), (3, 7)):
        proposal_uuid = uid(MEDICAL_PROPOSAL, prop_n)
        w(
            f'UPDATE "UserNotifications" SET "SourceEventKey" = \'medical-change-proposal:{proposal_uuid}\', '
            f'"RelatedAggregateType" = \'medical_change_proposal\', "RelatedAggregateId" = \'{proposal_uuid}\', '
            f'"DeepLinkPath" = \'/supervision/patient/proposals/{proposal_uuid}\' '
            f'WHERE "UserId" = \'{clients[client_i]}\' AND "TypeCode" = \'MedicalChangeProposal\' AND "ActionState" = \'Pending\';'
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
    print(f"Wrote {out} ({len(lines)} lines)", file=sys.stderr)


if __name__ == "__main__":
    main()
