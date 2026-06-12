-- Patient profile / medications / onboarding dev seed.
-- Placeholder {{SEED_USER_ID}} is replaced by PatientDevDataSeeder before execution.
-- Catalog rows use fixed UUIDs; ON CONFLICT DO UPDATE refreshes readable labels on re-seed.
-- Bump DevSeed:SeedKey when you need a fresh patient-data apply (dev_seed_applications).

-- Routes of administration (shared catalog)
INSERT INTO "RoutesOfAdministration" ("Id", "RouteOfAdministrationName", "IsOralIntake", "CreatedAtUtc")
VALUES
    ('f2200000-0000-4000-8000-000000000001'::uuid, 'Oral use', TRUE, (NOW() AT TIME ZONE 'utc')),
    ('f2200000-0000-4000-8000-000000000002'::uuid, 'Topical', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2200000-0000-4000-8000-000000000003'::uuid, 'Inhalation', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET
    "RouteOfAdministrationName" = EXCLUDED."RouteOfAdministrationName",
    "IsOralIntake" = EXCLUDED."IsOralIntake";

-- Pharmaceutical forms (shared catalog)
INSERT INTO "PharmaceuticalForms" ("Id", "FormulationName", "CreatedAtUtc")
VALUES
    ('f2100000-0000-4000-8000-000000000001'::uuid, 'Tablet', (NOW() AT TIME ZONE 'utc')),
    ('f2100000-0000-4000-8000-000000000002'::uuid, 'Capsule', (NOW() AT TIME ZONE 'utc')),
    ('f2100000-0000-4000-8000-000000000003'::uuid, 'Film-coated tablet', (NOW() AT TIME ZONE 'utc')),
    ('f2100000-0000-4000-8000-000000000004'::uuid, 'Oral solution', (NOW() AT TIME ZONE 'utc')),
    ('f2100000-0000-4000-8000-000000000005'::uuid, 'Cream', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET
    "FormulationName" = EXCLUDED."FormulationName";

-- Active substances (20)
INSERT INTO "ActiveSubstances" ("Id", "ActiveSubstanceName", "HasAlcoholInteraction", "CreatedAtUtc")
VALUES
    ('f2000000-0000-4000-8000-000000000001'::uuid, 'Ibuprofen', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000002'::uuid, 'Paracetamol', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000003'::uuid, 'Metformin', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000004'::uuid, 'Atorvastatin', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000005'::uuid, 'Omeprazole', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000006'::uuid, 'Amoxicillin', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000007'::uuid, 'Lisinopril', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000008'::uuid, 'Levothyroxine sodium', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000009'::uuid, 'Salbutamol', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000010'::uuid, 'Sertraline', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000011'::uuid, 'Amlodipine', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000012'::uuid, 'Simvastatin', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000013'::uuid, 'Losartan', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000014'::uuid, 'Pantoprazole', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000015'::uuid, 'Cetirizine', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000016'::uuid, 'Metoprolol', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000017'::uuid, 'Warfarin', TRUE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000018'::uuid, 'Diazepam', TRUE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000019'::uuid, 'Insulin glargine', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2000000-0000-4000-8000-000000000020'::uuid, 'Acetylsalicylic acid', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET
    "ActiveSubstanceName" = EXCLUDED."ActiveSubstanceName",
    "HasAlcoholInteraction" = EXCLUDED."HasAlcoholInteraction";

-- Drugs (20)
INSERT INTO "Drugs" ("Id", "CommercialName", "TypeOfProduct", "DrugFormCounter", "TypeOfDrug", "CreatedAtUtc")
VALUES
    ('a0000000-0000-4000-8000-000000000001'::uuid, 'Nurofen', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000002'::uuid, 'Apap', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000003'::uuid, 'Glucophage', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000004'::uuid, 'Atorvasterol', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000005'::uuid, 'Controloc', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000006'::uuid, 'Amotaks', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000007'::uuid, 'Lisinoratio', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000008'::uuid, 'Euthyrox', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000009'::uuid, 'Ventolin', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000010'::uuid, 'Zoloft', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000011'::uuid, 'Norvasc', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000012'::uuid, 'Simvacard', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000013'::uuid, 'Cozaar', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000014'::uuid, 'Controloc 40', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000015'::uuid, 'Zyrtec', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000016'::uuid, 'Betaloc', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000017'::uuid, 'Warfarina', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000018'::uuid, 'Relanium', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000019'::uuid, 'Lantus', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000020'::uuid, 'Aspirin', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET
    "CommercialName" = EXCLUDED."CommercialName",
    "TypeOfProduct" = EXCLUDED."TypeOfProduct",
    "DrugFormCounter" = EXCLUDED."DrugFormCounter",
    "TypeOfDrug" = EXCLUDED."TypeOfDrug";

-- Drug ↔ active substance links
INSERT INTO "DrugActiveSubstances" ("DrugId", "ActiveSubstanceId")
SELECT
    ('a0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    ('f2000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid
FROM generate_series(1, 20) AS i
ON CONFLICT DO NOTHING;

-- Drug forms (20) — realistic strengths and ATC codes
INSERT INTO "DrugForms" ("Id", "DrugId", "ProductIdentifier", "Dosage", "IsOral", "AtcCode", "CreatedAtUtc")
VALUES
    ('a1000000-0000-4000-8000-000000000001'::uuid, 'a0000000-0000-4000-8000-000000000001'::uuid, 910000001, '200 mg', TRUE, 'M01AE01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000002'::uuid, 'a0000000-0000-4000-8000-000000000002'::uuid, 910000002, '500 mg', TRUE, 'N02BE01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000003'::uuid, 'a0000000-0000-4000-8000-000000000003'::uuid, 910000003, '500 mg', TRUE, 'A10BA02', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000004'::uuid, 'a0000000-0000-4000-8000-000000000004'::uuid, 910000004, '20 mg', TRUE, 'C10AA05', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000005'::uuid, 'a0000000-0000-4000-8000-000000000005'::uuid, 910000005, '20 mg', TRUE, 'A02BC01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000006'::uuid, 'a0000000-0000-4000-8000-000000000006'::uuid, 910000006, '500 mg', TRUE, 'J01CA04', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000007'::uuid, 'a0000000-0000-4000-8000-000000000007'::uuid, 910000007, '10 mg', TRUE, 'C09AA03', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000008'::uuid, 'a0000000-0000-4000-8000-000000000008'::uuid, 910000008, '50 mcg', TRUE, 'H03AA01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000009'::uuid, 'a0000000-0000-4000-8000-000000000009'::uuid, 910000009, '2 mg', TRUE, 'R03AC02', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000010'::uuid, 'a0000000-0000-4000-8000-000000000010'::uuid, 910000010, '50 mg', TRUE, 'N06AB06', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000011'::uuid, 'a0000000-0000-4000-8000-000000000011'::uuid, 910000011, '5 mg', TRUE, 'C08CA01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000012'::uuid, 'a0000000-0000-4000-8000-000000000012'::uuid, 910000012, '20 mg', TRUE, 'C10AA01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000013'::uuid, 'a0000000-0000-4000-8000-000000000013'::uuid, 910000013, '50 mg', TRUE, 'C09CA01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000014'::uuid, 'a0000000-0000-4000-8000-000000000014'::uuid, 910000014, '40 mg', TRUE, 'A02BC02', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000015'::uuid, 'a0000000-0000-4000-8000-000000000015'::uuid, 910000015, '10 mg', TRUE, 'R06AE07', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000016'::uuid, 'a0000000-0000-4000-8000-000000000016'::uuid, 910000016, '50 mg', TRUE, 'C07AB02', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000017'::uuid, 'a0000000-0000-4000-8000-000000000017'::uuid, 910000017, '5 mg', TRUE, 'B01AA03', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000018'::uuid, 'a0000000-0000-4000-8000-000000000018'::uuid, 910000018, '5 mg', TRUE, 'N05BA01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000019'::uuid, 'a0000000-0000-4000-8000-000000000019'::uuid, 910000019, '100 IU/ml', TRUE, 'A10AE04', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000020'::uuid, 'a0000000-0000-4000-8000-000000000020'::uuid, 910000020, '100 mg', TRUE, 'N02BA01', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET
    "DrugId" = EXCLUDED."DrugId",
    "ProductIdentifier" = EXCLUDED."ProductIdentifier",
    "Dosage" = EXCLUDED."Dosage",
    "IsOral" = EXCLUDED."IsOral",
    "AtcCode" = EXCLUDED."AtcCode";

-- Drug form ↔ pharmaceutical form (tablet / capsule / film-coated / solution)
INSERT INTO "DrugFormPharmaceuticalForms" ("DrugFormId", "PharmaceuticalFormId")
VALUES
    ('a1000000-0000-4000-8000-000000000001'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000002'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000003'::uuid, 'f2100000-0000-4000-8000-000000000003'::uuid),
    ('a1000000-0000-4000-8000-000000000004'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000005'::uuid, 'f2100000-0000-4000-8000-000000000002'::uuid),
    ('a1000000-0000-4000-8000-000000000006'::uuid, 'f2100000-0000-4000-8000-000000000002'::uuid),
    ('a1000000-0000-4000-8000-000000000007'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000008'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000009'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000010'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000011'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000012'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000013'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000014'::uuid, 'f2100000-0000-4000-8000-000000000003'::uuid),
    ('a1000000-0000-4000-8000-000000000015'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000016'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000017'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000018'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000019'::uuid, 'f2100000-0000-4000-8000-000000000004'::uuid),
    ('a1000000-0000-4000-8000-000000000020'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid)
ON CONFLICT DO NOTHING;

-- Drug form ↔ route of administration (oral for all seeded forms)
INSERT INTO "DrugFormRoutesOfAdministration" ("DrugFormId", "RouteOfAdministrationId")
SELECT
    ('a1000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    'f2200000-0000-4000-8000-000000000001'::uuid
FROM generate_series(1, 20) AS i
ON CONFLICT DO NOTHING;

-- Foods (20)
INSERT INTO "Foods" ("Id", "Name", "Aliases", "SearchText", "CreatedAtUtc")
VALUES
    ('b0000000-0000-4000-8000-000000000001'::uuid, 'Grapefruit', '["pink grapefruit","citrus"]'::jsonb, 'grapefruit', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000002'::uuid, 'Milk', '["cow milk","dairy"]'::jsonb, 'milk', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000003'::uuid, 'Peanuts', '["groundnuts"]'::jsonb, 'peanuts', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000004'::uuid, 'Eggs', '["chicken eggs"]'::jsonb, 'eggs', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000005'::uuid, 'Shellfish', '["shrimp","prawns"]'::jsonb, 'shellfish', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000006'::uuid, 'Soy', '["soya","tofu"]'::jsonb, 'soy', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000007'::uuid, 'Wheat', '["gluten","bread"]'::jsonb, 'wheat', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000008'::uuid, 'Almonds', '[]'::jsonb, 'almonds', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000009'::uuid, 'Strawberries', '[]'::jsonb, 'strawberries', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000010'::uuid, 'Dark chocolate', '["cocoa"]'::jsonb, 'dark chocolate', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000011'::uuid, 'Aged cheese', '["cheddar","parmesan"]'::jsonb, 'aged cheese', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000012'::uuid, 'Tomatoes', '[]'::jsonb, 'tomatoes', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000013'::uuid, 'Avocado', '[]'::jsonb, 'avocado', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000014'::uuid, 'Spinach', '[]'::jsonb, 'spinach', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000015'::uuid, 'Bananas', '[]'::jsonb, 'bananas', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000016'::uuid, 'Coffee', '["espresso"]'::jsonb, 'coffee', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000017'::uuid, 'Honey', '[]'::jsonb, 'honey', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000018'::uuid, 'Garlic', '[]'::jsonb, 'garlic', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000019'::uuid, 'Onion', '["red onion"]'::jsonb, 'onion', (NOW() AT TIME ZONE 'utc')),
    ('b0000000-0000-4000-8000-000000000020'::uuid, 'Celery', '[]'::jsonb, 'celery', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Aliases" = EXCLUDED."Aliases",
    "SearchText" = EXCLUDED."SearchText";

-- Supplements (20)
INSERT INTO "Supplements" ("Id", "Name", "Aliases", "SearchText", "CreatedAtUtc")
VALUES
    ('c0000000-0000-4000-8000-000000000001'::uuid, 'Vitamin D3', '["cholecalciferol"]'::jsonb, 'vitamin d3', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000002'::uuid, 'Omega-3 Fish Oil', '["EPA","DHA"]'::jsonb, 'omega-3 fish oil', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000003'::uuid, 'Iron', '["ferrous sulfate"]'::jsonb, 'iron', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000004'::uuid, 'Magnesium', '["magnesium citrate"]'::jsonb, 'magnesium', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000005'::uuid, 'Vitamin C', '["ascorbic acid"]'::jsonb, 'vitamin c', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000006'::uuid, 'Zinc', '[]'::jsonb, 'zinc', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000007'::uuid, 'Probiotics', '["lactobacillus"]'::jsonb, 'probiotics', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000008'::uuid, 'Vitamin B12', '["cobalamin"]'::jsonb, 'vitamin b12', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000009'::uuid, 'Folic acid', '["folate","vitamin b9"]'::jsonb, 'folic acid', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000010'::uuid, 'Calcium', '["calcium carbonate"]'::jsonb, 'calcium', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000011'::uuid, 'Multivitamin', '[]'::jsonb, 'multivitamin', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000012'::uuid, 'Coenzyme Q10', '["CoQ10"]'::jsonb, 'coenzyme q10', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000013'::uuid, 'Melatonin', '[]'::jsonb, 'melatonin', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000014'::uuid, 'Turmeric', '["curcumin"]'::jsonb, 'turmeric', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000015'::uuid, 'Vitamin B6', '["pyridoxine"]'::jsonb, 'vitamin b6', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000016'::uuid, 'Biotin', '["vitamin b7"]'::jsonb, 'biotin', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000017'::uuid, 'Potassium', '[]'::jsonb, 'potassium', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000018'::uuid, 'Psyllium fiber', '["fiber supplement"]'::jsonb, 'psyllium fiber', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000019'::uuid, 'Glucosamine', '[]'::jsonb, 'glucosamine', (NOW() AT TIME ZONE 'utc')),
    ('c0000000-0000-4000-8000-000000000020'::uuid, 'Ashwagandha', '[]'::jsonb, 'ashwagandha', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Aliases" = EXCLUDED."Aliases",
    "SearchText" = EXCLUDED."SearchText";

-- Health conditions (20)
INSERT INTO "HealthConditions" ("Id", "HealthConditionName", "NormalizedHealthConditionName", "Description", "CreatedAtUtc")
VALUES
    ('d0000000-0000-4000-8000-000000000001'::uuid, 'Type 2 diabetes', 'type 2 diabetes', 'Chronic insulin resistance and elevated blood glucose.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000002'::uuid, 'Hypertension', 'hypertension', 'Persistently elevated blood pressure.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000003'::uuid, 'Asthma', 'asthma', 'Recurrent airway inflammation and bronchospasm.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000004'::uuid, 'Major depressive disorder', 'major depressive disorder', 'Persistent low mood and loss of interest.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000005'::uuid, 'Hypothyroidism', 'hypothyroidism', 'Underactive thyroid gland.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000006'::uuid, 'Gastroesophageal reflux disease', 'gastroesophageal reflux disease', 'Chronic acid reflux and heartburn.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000007'::uuid, 'Hyperlipidemia', 'hyperlipidemia', 'Elevated blood cholesterol or triglycerides.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000008'::uuid, 'Atrial fibrillation', 'atrial fibrillation', 'Irregular and often rapid heart rhythm.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000009'::uuid, 'COPD', 'copd', 'Chronic obstructive pulmonary disease.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000010'::uuid, 'Rheumatoid arthritis', 'rheumatoid arthritis', 'Autoimmune inflammatory arthritis.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000011'::uuid, 'Migraine', 'migraine', 'Recurrent moderate to severe headaches.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000012'::uuid, 'Generalized anxiety disorder', 'generalized anxiety disorder', 'Excessive worry most days.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000013'::uuid, 'Chronic kidney disease', 'chronic kidney disease', 'Gradual loss of kidney function.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000014'::uuid, 'Heart failure', 'heart failure', 'Reduced ability of the heart to pump blood.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000015'::uuid, 'Osteoporosis', 'osteoporosis', 'Reduced bone density and fracture risk.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000016'::uuid, 'Epilepsy', 'epilepsy', 'Recurrent unprovoked seizures.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000017'::uuid, 'Psoriasis', 'psoriasis', 'Chronic immune-mediated skin condition.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000018'::uuid, 'Irritable bowel syndrome', 'irritable bowel syndrome', 'Abdominal pain with bowel habit changes.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000019'::uuid, 'Obstructive sleep apnea', 'obstructive sleep apnea', 'Repeated breathing interruptions during sleep.', (NOW() AT TIME ZONE 'utc')),
    ('d0000000-0000-4000-8000-000000000020'::uuid, 'Gout', 'gout', 'Urate crystal arthritis.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET
    "HealthConditionName" = EXCLUDED."HealthConditionName",
    "NormalizedHealthConditionName" = EXCLUDED."NormalizedHealthConditionName",
    "Description" = EXCLUDED."Description";

-- Allergy summary (1 per user)
INSERT INTO "ClientAllergySummaries" (
    "Id", "UserId", "SevereReactionStatus", "CarriesAllergyMedication",
    "PollenDustMitesAllergyStatus", "InsectVenomAllergyStatus", "LatexAllergyStatus", "CreatedAtUtc"
)
SELECT
    'e3000000-0000-4000-8000-000000000001'::uuid,
    '{{SEED_USER_ID}}'::uuid,
    'Yes',
    TRUE,
    'No',
    'DontKnow',
    'No',
    (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (
    SELECT 1 FROM "ClientAllergySummaries" WHERE "UserId" = '{{SEED_USER_ID}}'::uuid
);

-- Reproductive health profile (1 per user)
INSERT INTO client_reproductive_health_profiles (
    "Id", "UserId", "CurrentlyPregnant", "PlanningToGetPregnant", "Menopause",
    "ContraceptionMethodCode", "CreatedAtUtc"
)
SELECT
    'e4000000-0000-4000-8000-000000000001'::uuid,
    '{{SEED_USER_ID}}'::uuid,
    FALSE,
    FALSE,
    FALSE,
    'oral_pill',
    (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (
    SELECT 1 FROM client_reproductive_health_profiles WHERE "UserId" = '{{SEED_USER_ID}}'::uuid
);

-- Drug allergies (20) — catalog entries reference a specific drug form (not just the parent drug)
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "DrugFormId", "Source", "Note", "CreatedAtUtc")
SELECT
    ('e0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    '{{SEED_USER_ID}}'::uuid,
    ('a1000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    'Database',
    'Documented rash after taking ' || d."CommercialName" || ' (' || df."Dosage" || ').',
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
JOIN "DrugForms" df ON df."Id" = ('a1000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid
JOIN "Drugs" d ON d."Id" = df."DrugId"
ON CONFLICT ("Id") DO NOTHING;

-- Custom drug allergies (2) — exercise custom name + active substances / formulation / dosage
INSERT INTO "ClientDrugAllergies" (
    "Id", "UserId", "CustomDrugName", "CustomDrugNameNormalized",
    "CustomActiveSubstances", "CustomFormulationName", "CustomDosage",
    "Source", "Note", "CreatedAtUtc"
)
VALUES
    (
        'e0000000-0000-4000-8000-000000000021'::uuid,
        '{{SEED_USER_ID}}'::uuid,
        'Homeopathic Arnica',
        'homeopathic arnica',
        'Arnica montana',
        'Drops',
        '30C',
        'Custom',
        'Swelling and hives after homeopathic remedy.',
        (NOW() AT TIME ZONE 'utc')
    ),
    (
        'e0000000-0000-4000-8000-000000000022'::uuid,
        '{{SEED_USER_ID}}'::uuid,
        'Compound Cream X',
        'compound cream x',
        'Hydrocortisone, Clotrimazole',
        'Cream',
        '1% / 1%',
        'Custom',
        'Contact dermatitis from imported topical cream.',
        (NOW() AT TIME ZONE 'utc')
    )
ON CONFLICT ("Id") DO NOTHING;

-- Supplement allergies (20)
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "SupplementId", "Source", "Note", "CreatedAtUtc")
SELECT
    ('e1000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    '{{SEED_USER_ID}}'::uuid,
    ('c0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    'Database',
    'Itching after ' || s."Name" || ' — avoid this supplement.',
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
JOIN "Supplements" s ON s."Id" = ('c0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid
ON CONFLICT ("Id") DO NOTHING;

-- Food allergies (20)
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "FoodId", "Source", "Note", "CreatedAtUtc")
SELECT
    ('e2000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    '{{SEED_USER_ID}}'::uuid,
    ('b0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    'Database',
    'Avoid ' || f."Name" || ' — causes oral tingling.',
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
JOIN "Foods" f ON f."Id" = ('b0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid
ON CONFLICT ("Id") DO NOTHING;

-- User health conditions (20)
INSERT INTO "UserHealthConditions" (
    "Id", "UserId", "HealthConditionId", "Source", "Note", "CreatedAtUtc"
)
SELECT
    ('f0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    '{{SEED_USER_ID}}'::uuid,
    ('d0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    'Database',
    'Diagnosed and monitored by GP.',
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
ON CONFLICT ("Id") DO NOTHING;

-- Drug problems (20 catalog + 2 custom) — catalog entries reference a specific drug form (not just the parent drug)
INSERT INTO "DrugProblems" (
    "Id", "UserId", "DrugFormId", "CustomDrugName", "CustomActiveSubstances",
    "CustomFormulationName", "CustomDosage", "Source", "ProblemNote", "CreatedAtUtc"
)
SELECT
    ('f1000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    '{{SEED_USER_ID}}'::uuid,
    ('a1000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    NULL,
    NULL,
    NULL,
    NULL,
    'Database',
    CASE (i % 5)
        WHEN 0 THEN 'Nausea within an hour of taking ' || d."CommercialName" || ' (' || df."Dosage" || ').'
        WHEN 1 THEN 'Dizziness reported with ' || d."CommercialName" || ' (' || df."Dosage" || ').'
        WHEN 2 THEN 'Mild rash after starting ' || d."CommercialName" || ' (' || df."Dosage" || ').'
        WHEN 3 THEN 'Stomach pain when taking ' || d."CommercialName" || ' (' || df."Dosage" || ') on an empty stomach.'
        ELSE 'Drowsiness in the afternoon after ' || d."CommercialName" || ' (' || df."Dosage" || ').'
    END,
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
JOIN "DrugForms" df ON df."Id" = ('a1000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid
JOIN "Drugs" d ON d."Id" = df."DrugId"
ON CONFLICT ("Id") DO NOTHING;

-- Custom drug problems (2) — exercise custom name + active substances / formulation / dosage
INSERT INTO "DrugProblems" (
    "Id", "UserId", "DrugFormId", "CustomDrugName", "CustomActiveSubstances",
    "CustomFormulationName", "CustomDosage", "Source", "ProblemNote", "CreatedAtUtc"
)
VALUES
    (
        'f1000000-0000-4000-8000-000000000021'::uuid,
        '{{SEED_USER_ID}}'::uuid,
        NULL,
        'Homeopathic Arnica',
        'Arnica montana',
        'Drops',
        '30C',
        'Custom',
        'Severe nausea and headache after each dose of the homeopathic remedy.',
        (NOW() AT TIME ZONE 'utc')
    ),
    (
        'f1000000-0000-4000-8000-000000000022'::uuid,
        '{{SEED_USER_ID}}'::uuid,
        NULL,
        'Compound Cream X',
        'Hydrocortisone, Clotrimazole',
        'Cream',
        '1% / 1%',
        'Custom',
        'Burning sensation and redness after applying the imported topical cream.',
        (NOW() AT TIME ZONE 'utc')
    )
ON CONFLICT ("Id") DO NOTHING;

-- Regular drugs (20)
INSERT INTO client_regular_drugs (
    "Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "CreatedAtUtc"
)
SELECT
    ('e5000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    '{{SEED_USER_ID}}'::uuid,
    ('a0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    ('a1000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    'Active',
    CURRENT_DATE - (i % 30),
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
ON CONFLICT ("Id") DO NOTHING;

-- Regular drug intake times (morning / midday / evening rotation)
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
SELECT
    ('e5100000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    ('e5000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    (ARRAY['07:30:00', '12:00:00', '18:30:00', '08:00:00', '21:00:00'])[1 + (i % 5)]::time,
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
ON CONFLICT ("Id") DO NOTHING;

-- Regular foods (20)
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "CreatedAtUtc")
SELECT
    ('e6000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    '{{SEED_USER_ID}}'::uuid,
    ('b0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    'Active',
    CURRENT_DATE,
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
ON CONFLICT ("Id") DO NOTHING;

-- Regular food intake times
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
SELECT
    ('e6100000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    ('e6000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    (ARRAY['08:00:00', '13:00:00', '19:00:00', '10:30:00', '16:00:00'])[1 + (i % 5)]::time,
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
ON CONFLICT ("Id") DO NOTHING;

-- Regular supplements (20)
INSERT INTO client_regular_supplements (
    "Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "CreatedAtUtc"
)
SELECT
    ('e7000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    '{{SEED_USER_ID}}'::uuid,
    ('c0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    'Database',
    'Active',
    CURRENT_DATE,
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
ON CONFLICT ("Id") DO NOTHING;

-- Regular supplement intake times
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
SELECT
    ('e7100000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    ('e7000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    (ARRAY['08:30:00', '14:00:00', '20:00:00', '07:00:00', '22:00:00'])[1 + (i % 5)]::time,
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
ON CONFLICT ("Id") DO NOTHING;

-- Meal times (breakfast, lunch, dinner, then named snacks)
INSERT INTO "ClientMealTimes" ("Id", "UserId", "MealName", "NormalizedMealName", "IntakeTime", "CreatedAtUtc")
SELECT
    ('e8000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
    '{{SEED_USER_ID}}'::uuid,
    CASE i
        WHEN 1 THEN 'Breakfast'
        WHEN 2 THEN 'Lunch'
        WHEN 3 THEN 'Dinner'
        WHEN 4 THEN 'Morning snack'
        WHEN 5 THEN 'Afternoon snack'
        WHEN 6 THEN 'Evening snack'
        WHEN 7 THEN 'Pre-workout'
        WHEN 8 THEN 'Post-workout'
        ELSE 'Light meal ' || i
    END,
    CASE i
        WHEN 1 THEN 'breakfast'
        WHEN 2 THEN 'lunch'
        WHEN 3 THEN 'dinner'
        WHEN 4 THEN 'morning snack'
        WHEN 5 THEN 'afternoon snack'
        WHEN 6 THEN 'evening snack'
        WHEN 7 THEN 'pre-workout'
        WHEN 8 THEN 'post-workout'
        ELSE 'light meal ' || i
    END,
    (ARRAY[
        '07:30:00', '12:30:00', '18:30:00', '10:00:00', '15:30:00',
        '20:30:00', '06:45:00', '17:15:00', '09:15:00', '11:45:00',
        '13:45:00', '16:15:00', '19:45:00', '21:00:00', '08:15:00',
        '14:15:00', '17:45:00', '22:00:00', '09:45:00', '13:15:00'
    ])[i]::time,
    (NOW() AT TIME ZONE 'utc')
FROM generate_series(1, 20) AS i
ON CONFLICT ("Id") DO NOTHING;
