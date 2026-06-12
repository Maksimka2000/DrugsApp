-- Dev catalog + patient/provider fixtures.
-- Fixed user UUIDs are created by PatientDevDataSeeder before execution.
-- Bump DevSeed:SeedKey when you need a fresh apply (dev_seed_applications).

-- Routes of administration (shared catalog)
INSERT INTO "RoutesOfAdministration" ("Id", "RouteOfAdministrationName", "IsOralIntake", "CreatedAtUtc")
VALUES
    ('f2200000-0000-4000-8000-000000000001'::uuid, 'Oral use', TRUE, (NOW() AT TIME ZONE 'utc')),
    ('f2200000-0000-4000-8000-000000000002'::uuid, 'Topical', FALSE, (NOW() AT TIME ZONE 'utc')),
    ('f2200000-0000-4000-8000-000000000003'::uuid, 'Inhalation', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "RouteOfAdministrationName"=EXCLUDED."RouteOfAdministrationName", "IsOralIntake"=EXCLUDED."IsOralIntake";
-- Pharmaceutical forms (shared catalog)
INSERT INTO "PharmaceuticalForms" ("Id", "FormulationName", "CreatedAtUtc")
VALUES
    ('f2100000-0000-4000-8000-000000000001'::uuid, 'Tablet', (NOW() AT TIME ZONE 'utc')),
    ('f2100000-0000-4000-8000-000000000002'::uuid, 'Capsule', (NOW() AT TIME ZONE 'utc')),
    ('f2100000-0000-4000-8000-000000000003'::uuid, 'Film-coated tablet', (NOW() AT TIME ZONE 'utc')),
    ('f2100000-0000-4000-8000-000000000004'::uuid, 'Oral solution', (NOW() AT TIME ZONE 'utc')),
    ('f2100000-0000-4000-8000-000000000005'::uuid, 'Cream', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "FormulationName"=EXCLUDED."FormulationName";
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
ON CONFLICT ("Id") DO UPDATE SET "ActiveSubstanceName"=EXCLUDED."ActiveSubstanceName", "HasAlcoholInteraction"=EXCLUDED."HasAlcoholInteraction";
-- Drugs (20 original + 15 extended)
INSERT INTO "Drugs" ("Id", "CommercialName", "TypeOfProduct", "DrugFormCounter", "TypeOfDrug", "CreatedAtUtc")
VALUES
    ('a0000000-0000-4000-8000-000000000001'::uuid, 'Nurofen', 'Human medicinal product', 2, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000002'::uuid, 'Apap', 'Human medicinal product', 2, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000003'::uuid, 'Glucophage', 'Human medicinal product', 2, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000004'::uuid, 'Atorvasterol', 'Human medicinal product', 2, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000005'::uuid, 'Controloc', 'Human medicinal product', 2, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000006'::uuid, 'Amotaks', 'Human medicinal product', 2, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000007'::uuid, 'Lisinoratio', 'Human medicinal product', 2, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000008'::uuid, 'Euthyrox', 'Human medicinal product', 2, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000009'::uuid, 'Ventolin', 'Human medicinal product', 2, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000010'::uuid, 'Zoloft', 'Human medicinal product', 2, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000011'::uuid, 'Norvasc', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000012'::uuid, 'Simvacard', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000013'::uuid, 'Cozaar', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000014'::uuid, 'Controloc 40', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000015'::uuid, 'Zyrtec', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000016'::uuid, 'Betaloc', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000017'::uuid, 'Warfarina', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000018'::uuid, 'Relanium', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000019'::uuid, 'Lantus', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000020'::uuid, 'Aspirin', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000021'::uuid, 'Hylo-Comod', 'Human medicinal product', 1, 'Ophthalmic', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000022'::uuid, 'Xalatan', 'Human medicinal product', 1, 'Ophthalmic', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000023'::uuid, 'Tobrex', 'Human medicinal product', 1, 'Ophthalmic', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000024'::uuid, 'Allergodil', 'Human medicinal product', 1, 'Ophthalmic', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000025'::uuid, 'Artelac', 'Human medicinal product', 1, 'Ophthalmic', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000026'::uuid, 'Orkambi', 'Human medicinal product', 1, 'Special', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000027'::uuid, 'Spinraza', 'Human medicinal product', 1, 'Special', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000028'::uuid, 'Kalydeco', 'Human medicinal product', 1, 'Special', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000029'::uuid, 'Soliris', 'Human medicinal product', 1, 'Special', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000030'::uuid, 'Hemlibra', 'Human medicinal product', 1, 'Special', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000031'::uuid, 'Polocard', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000032'::uuid, 'Metformax', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000033'::uuid, 'Ramipril', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000034'::uuid, 'Rosutrox', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc')),
    ('a0000000-0000-4000-8000-000000000035'::uuid, 'Escital', 'Human medicinal product', 1, 'Standard', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "CommercialName"=EXCLUDED."CommercialName", "TypeOfProduct"=EXCLUDED."TypeOfProduct", "DrugFormCounter"=EXCLUDED."DrugFormCounter", "TypeOfDrug"=EXCLUDED."TypeOfDrug";
-- Drug active substance links (original 20)
INSERT INTO "DrugActiveSubstances" ("DrugId", "ActiveSubstanceId")
SELECT ('a0000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid,
       ('f2000000-0000-4000-8000-' || lpad(i::text, 12, '0'))::uuid
FROM generate_series(1, 20) AS i
ON CONFLICT DO NOTHING;
-- Drug active substance links (extended drugs)
INSERT INTO "DrugActiveSubstances" ("DrugId", "ActiveSubstanceId")
VALUES
    ('a0000000-0000-4000-8000-000000000021'::uuid, 'f2000000-0000-4000-8000-000000000001'::uuid),
    ('a0000000-0000-4000-8000-000000000022'::uuid, 'f2000000-0000-4000-8000-000000000004'::uuid),
    ('a0000000-0000-4000-8000-000000000023'::uuid, 'f2000000-0000-4000-8000-000000000006'::uuid),
    ('a0000000-0000-4000-8000-000000000024'::uuid, 'f2000000-0000-4000-8000-000000000015'::uuid),
    ('a0000000-0000-4000-8000-000000000025'::uuid, 'f2000000-0000-4000-8000-000000000002'::uuid),
    ('a0000000-0000-4000-8000-000000000026'::uuid, 'f2000000-0000-4000-8000-000000000003'::uuid),
    ('a0000000-0000-4000-8000-000000000027'::uuid, 'f2000000-0000-4000-8000-000000000008'::uuid),
    ('a0000000-0000-4000-8000-000000000028'::uuid, 'f2000000-0000-4000-8000-000000000009'::uuid),
    ('a0000000-0000-4000-8000-000000000029'::uuid, 'f2000000-0000-4000-8000-000000000007'::uuid),
    ('a0000000-0000-4000-8000-000000000030'::uuid, 'f2000000-0000-4000-8000-000000000019'::uuid),
    ('a0000000-0000-4000-8000-000000000031'::uuid, 'f2000000-0000-4000-8000-000000000020'::uuid),
    ('a0000000-0000-4000-8000-000000000032'::uuid, 'f2000000-0000-4000-8000-000000000003'::uuid),
    ('a0000000-0000-4000-8000-000000000033'::uuid, 'f2000000-0000-4000-8000-000000000007'::uuid),
    ('a0000000-0000-4000-8000-000000000034'::uuid, 'f2000000-0000-4000-8000-000000000012'::uuid),
    ('a0000000-0000-4000-8000-000000000035'::uuid, 'f2000000-0000-4000-8000-000000000010'::uuid)
ON CONFLICT DO NOTHING;
-- Drug forms (primary + alternates + extended)
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
    ('a1000000-0000-4000-8000-000000000020'::uuid, 'a0000000-0000-4000-8000-000000000020'::uuid, 910000020, '100 mg', TRUE, 'N02BA01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000101'::uuid, 'a0000000-0000-4000-8000-000000000001'::uuid, 910000101, '400 mg', TRUE, 'M01AE01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000102'::uuid, 'a0000000-0000-4000-8000-000000000002'::uuid, 910000102, '1000 mg', TRUE, 'N02BE01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000103'::uuid, 'a0000000-0000-4000-8000-000000000003'::uuid, 910000103, '850 mg', TRUE, 'A10BA02', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000104'::uuid, 'a0000000-0000-4000-8000-000000000004'::uuid, 910000104, '40 mg', TRUE, 'C10AA05', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000105'::uuid, 'a0000000-0000-4000-8000-000000000005'::uuid, 910000105, '40 mg', TRUE, 'A02BC01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000106'::uuid, 'a0000000-0000-4000-8000-000000000006'::uuid, 910000106, '875 mg', TRUE, 'J01CA04', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000107'::uuid, 'a0000000-0000-4000-8000-000000000007'::uuid, 910000107, '20 mg', TRUE, 'C09AA03', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000108'::uuid, 'a0000000-0000-4000-8000-000000000008'::uuid, 910000108, '75 mcg', TRUE, 'H03AA01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000109'::uuid, 'a0000000-0000-4000-8000-000000000009'::uuid, 910000109, '100 mcg/dose', FALSE, 'R03AC02', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000110'::uuid, 'a0000000-0000-4000-8000-000000000010'::uuid, 910000110, '100 mg', TRUE, 'N06AB06', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000021'::uuid, 'a0000000-0000-4000-8000-000000000021'::uuid, 910000021, '10 ml', FALSE, 'S01XA20', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000022'::uuid, 'a0000000-0000-4000-8000-000000000022'::uuid, 910000022, '0.005%', FALSE, 'S01EE01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000023'::uuid, 'a0000000-0000-4000-8000-000000000023'::uuid, 910000023, '0.3%', FALSE, 'S01AA12', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000024'::uuid, 'a0000000-0000-4000-8000-000000000024'::uuid, 910000024, '0.05%', FALSE, 'S01GX07', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000025'::uuid, 'a0000000-0000-4000-8000-000000000025'::uuid, 910000025, '10 ml', FALSE, 'S01XA20', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000026'::uuid, 'a0000000-0000-4000-8000-000000000026'::uuid, 910000026, '150/200 mg', TRUE, 'R07FX02', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000027'::uuid, 'a0000000-0000-4000-8000-000000000027'::uuid, 910000027, '12 mg/5 ml', FALSE, 'N07XX12', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000028'::uuid, 'a0000000-0000-4000-8000-000000000028'::uuid, 910000028, '150 mg', TRUE, 'R07AX02', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000029'::uuid, 'a0000000-0000-4000-8000-000000000029'::uuid, 910000029, '300 mg', FALSE, 'L04AJ01', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000030'::uuid, 'a0000000-0000-4000-8000-000000000030'::uuid, 910000030, '150 mg', FALSE, 'B02BX06', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000031'::uuid, 'a0000000-0000-4000-8000-000000000031'::uuid, 910000031, '75 mg', TRUE, 'B01AC06', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000032'::uuid, 'a0000000-0000-4000-8000-000000000032'::uuid, 910000032, '1000 mg', TRUE, 'A10BA02', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000033'::uuid, 'a0000000-0000-4000-8000-000000000033'::uuid, 910000033, '5 mg', TRUE, 'C09AA05', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000034'::uuid, 'a0000000-0000-4000-8000-000000000034'::uuid, 910000034, '10 mg', TRUE, 'C10AA07', (NOW() AT TIME ZONE 'utc')),
    ('a1000000-0000-4000-8000-000000000035'::uuid, 'a0000000-0000-4000-8000-000000000035'::uuid, 910000035, '10 mg', TRUE, 'N06AB10', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "DrugId"=EXCLUDED."DrugId", "ProductIdentifier"=EXCLUDED."ProductIdentifier", "Dosage"=EXCLUDED."Dosage", "IsOral"=EXCLUDED."IsOral", "AtcCode"=EXCLUDED."AtcCode";
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
    ('a1000000-0000-4000-8000-000000000020'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000101'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000102'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000103'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000104'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000105'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000106'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000107'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000108'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000109'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000110'::uuid, 'f2100000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000021'::uuid, 'f2100000-0000-4000-8000-000000000005'::uuid),
    ('a1000000-0000-4000-8000-000000000022'::uuid, 'f2100000-0000-4000-8000-000000000005'::uuid),
    ('a1000000-0000-4000-8000-000000000023'::uuid, 'f2100000-0000-4000-8000-000000000005'::uuid),
    ('a1000000-0000-4000-8000-000000000024'::uuid, 'f2100000-0000-4000-8000-000000000005'::uuid),
    ('a1000000-0000-4000-8000-000000000025'::uuid, 'f2100000-0000-4000-8000-000000000005'::uuid),
    ('a1000000-0000-4000-8000-000000000026'::uuid, 'f2100000-0000-4000-8000-000000000004'::uuid),
    ('a1000000-0000-4000-8000-000000000027'::uuid, 'f2100000-0000-4000-8000-000000000005'::uuid),
    ('a1000000-0000-4000-8000-000000000028'::uuid, 'f2100000-0000-4000-8000-000000000004'::uuid),
    ('a1000000-0000-4000-8000-000000000029'::uuid, 'f2100000-0000-4000-8000-000000000005'::uuid),
    ('a1000000-0000-4000-8000-000000000030'::uuid, 'f2100000-0000-4000-8000-000000000005'::uuid),
    ('a1000000-0000-4000-8000-000000000031'::uuid, 'f2100000-0000-4000-8000-000000000004'::uuid),
    ('a1000000-0000-4000-8000-000000000032'::uuid, 'f2100000-0000-4000-8000-000000000004'::uuid),
    ('a1000000-0000-4000-8000-000000000033'::uuid, 'f2100000-0000-4000-8000-000000000004'::uuid),
    ('a1000000-0000-4000-8000-000000000034'::uuid, 'f2100000-0000-4000-8000-000000000004'::uuid),
    ('a1000000-0000-4000-8000-000000000035'::uuid, 'f2100000-0000-4000-8000-000000000004'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "DrugFormRoutesOfAdministration" ("DrugFormId", "RouteOfAdministrationId")
VALUES
    ('a1000000-0000-4000-8000-000000000001'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000002'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000003'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000004'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000005'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000006'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000007'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000008'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000009'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000010'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000011'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000012'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000013'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000014'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000015'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000016'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000017'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000018'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000019'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000020'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000101'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000102'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000103'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000104'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000105'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000106'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000107'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000108'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000109'::uuid, 'f2200000-0000-4000-8000-000000000003'::uuid),
    ('a1000000-0000-4000-8000-000000000110'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000021'::uuid, 'f2200000-0000-4000-8000-000000000003'::uuid),
    ('a1000000-0000-4000-8000-000000000022'::uuid, 'f2200000-0000-4000-8000-000000000003'::uuid),
    ('a1000000-0000-4000-8000-000000000023'::uuid, 'f2200000-0000-4000-8000-000000000003'::uuid),
    ('a1000000-0000-4000-8000-000000000024'::uuid, 'f2200000-0000-4000-8000-000000000003'::uuid),
    ('a1000000-0000-4000-8000-000000000025'::uuid, 'f2200000-0000-4000-8000-000000000003'::uuid),
    ('a1000000-0000-4000-8000-000000000026'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000027'::uuid, 'f2200000-0000-4000-8000-000000000002'::uuid),
    ('a1000000-0000-4000-8000-000000000028'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000029'::uuid, 'f2200000-0000-4000-8000-000000000002'::uuid),
    ('a1000000-0000-4000-8000-000000000030'::uuid, 'f2200000-0000-4000-8000-000000000002'::uuid),
    ('a1000000-0000-4000-8000-000000000031'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000032'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000033'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000034'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid),
    ('a1000000-0000-4000-8000-000000000035'::uuid, 'f2200000-0000-4000-8000-000000000001'::uuid)
ON CONFLICT DO NOTHING;
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
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name", "Aliases"=EXCLUDED."Aliases", "SearchText"=EXCLUDED."SearchText";
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
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name", "Aliases"=EXCLUDED."Aliases", "SearchText"=EXCLUDED."SearchText";
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
    ('d0000000-0000-4000-8000-000000000010'::uuid, 'Rheumatoid arthritis', 'rheumatoid arthritis', 'Autoimmune inflammatory arthritis.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "HealthConditionName"=EXCLUDED."HealthConditionName", "NormalizedHealthConditionName"=EXCLUDED."NormalizedHealthConditionName", "Description"=EXCLUDED."Description";
-- Active substance interactions (20)
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000001'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Pharmacodynamic synergy may cause hypotension.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000001'::uuid, 'a3010000-0000-4000-8000-000000000001'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000001'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000002'::uuid, 'a3010000-0000-4000-8000-000000000001'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000002'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000001'::uuid, 'a3010000-0000-4000-8000-000000000001'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000001'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000002'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000002'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Metabolism competition can raise plasma levels.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000003'::uuid, 'a3010000-0000-4000-8000-000000000002'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000002'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000004'::uuid, 'a3010000-0000-4000-8000-000000000002'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000003'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000002'::uuid, 'a3010000-0000-4000-8000-000000000002'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000002'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000003'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000003'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'QT prolongation risk when combined.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000005'::uuid, 'a3010000-0000-4000-8000-000000000003'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000003'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000006'::uuid, 'a3010000-0000-4000-8000-000000000003'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000004'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000003'::uuid, 'a3010000-0000-4000-8000-000000000003'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000003'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000004'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000004'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Serotonin syndrome risk with co-administration.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000007'::uuid, 'a3010000-0000-4000-8000-000000000004'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000004'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000008'::uuid, 'a3010000-0000-4000-8000-000000000004'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000005'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000004'::uuid, 'a3010000-0000-4000-8000-000000000004'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000004'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000005'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000005'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Combined use may increase bleeding risk.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000009'::uuid, 'a3010000-0000-4000-8000-000000000005'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000005'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000010'::uuid, 'a3010000-0000-4000-8000-000000000005'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000006'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000005'::uuid, 'a3010000-0000-4000-8000-000000000005'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000005'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000006'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000006'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Pharmacodynamic synergy may cause hypotension.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000011'::uuid, 'a3010000-0000-4000-8000-000000000006'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000006'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000012'::uuid, 'a3010000-0000-4000-8000-000000000006'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000007'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000006'::uuid, 'a3010000-0000-4000-8000-000000000006'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000006'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000007'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000007'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Metabolism competition can raise plasma levels.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000013'::uuid, 'a3010000-0000-4000-8000-000000000007'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000007'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000014'::uuid, 'a3010000-0000-4000-8000-000000000007'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000008'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000007'::uuid, 'a3010000-0000-4000-8000-000000000007'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000007'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000008'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000008'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'QT prolongation risk when combined.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000015'::uuid, 'a3010000-0000-4000-8000-000000000008'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000008'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000016'::uuid, 'a3010000-0000-4000-8000-000000000008'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000009'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000008'::uuid, 'a3010000-0000-4000-8000-000000000008'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000008'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000009'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000009'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Serotonin syndrome risk with co-administration.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000017'::uuid, 'a3010000-0000-4000-8000-000000000009'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000009'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000018'::uuid, 'a3010000-0000-4000-8000-000000000009'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000010'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000009'::uuid, 'a3010000-0000-4000-8000-000000000009'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000009'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000010'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000010'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Combined use may increase bleeding risk.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000019'::uuid, 'a3010000-0000-4000-8000-000000000010'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000010'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000020'::uuid, 'a3010000-0000-4000-8000-000000000010'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000011'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000010'::uuid, 'a3010000-0000-4000-8000-000000000010'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000010'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000011'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000011'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Pharmacodynamic synergy may cause hypotension.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000021'::uuid, 'a3010000-0000-4000-8000-000000000011'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000011'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000022'::uuid, 'a3010000-0000-4000-8000-000000000011'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000012'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000011'::uuid, 'a3010000-0000-4000-8000-000000000011'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000011'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000012'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000012'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Metabolism competition can raise plasma levels.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000023'::uuid, 'a3010000-0000-4000-8000-000000000012'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000012'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000024'::uuid, 'a3010000-0000-4000-8000-000000000012'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000013'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000012'::uuid, 'a3010000-0000-4000-8000-000000000012'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000012'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000013'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000013'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'QT prolongation risk when combined.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000025'::uuid, 'a3010000-0000-4000-8000-000000000013'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000013'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000026'::uuid, 'a3010000-0000-4000-8000-000000000013'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000014'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000013'::uuid, 'a3010000-0000-4000-8000-000000000013'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000013'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000014'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000014'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Serotonin syndrome risk with co-administration.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000027'::uuid, 'a3010000-0000-4000-8000-000000000014'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000014'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000028'::uuid, 'a3010000-0000-4000-8000-000000000014'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000015'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000014'::uuid, 'a3010000-0000-4000-8000-000000000014'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000014'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000015'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000015'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Combined use may increase bleeding risk.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000029'::uuid, 'a3010000-0000-4000-8000-000000000015'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000015'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000030'::uuid, 'a3010000-0000-4000-8000-000000000015'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000016'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000015'::uuid, 'a3010000-0000-4000-8000-000000000015'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000015'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000016'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000016'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Pharmacodynamic synergy may cause hypotension.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000031'::uuid, 'a3010000-0000-4000-8000-000000000016'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000016'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000032'::uuid, 'a3010000-0000-4000-8000-000000000016'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000017'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000016'::uuid, 'a3010000-0000-4000-8000-000000000016'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000016'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000017'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000017'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Metabolism competition can raise plasma levels.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000033'::uuid, 'a3010000-0000-4000-8000-000000000017'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000017'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000034'::uuid, 'a3010000-0000-4000-8000-000000000017'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000018'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000017'::uuid, 'a3010000-0000-4000-8000-000000000017'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000017'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000018'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000018'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'QT prolongation risk when combined.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000035'::uuid, 'a3010000-0000-4000-8000-000000000018'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000018'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000036'::uuid, 'a3010000-0000-4000-8000-000000000018'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000019'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000018'::uuid, 'a3010000-0000-4000-8000-000000000018'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000018'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000019'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000019'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Serotonin syndrome risk with co-administration.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000037'::uuid, 'a3010000-0000-4000-8000-000000000019'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000019'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000038'::uuid, 'a3010000-0000-4000-8000-000000000019'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000019'::uuid, 'a3010000-0000-4000-8000-000000000019'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000019'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a3010000-0000-4000-8000-000000000020'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Combined use may increase bleeding risk.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000039'::uuid, 'a3010000-0000-4000-8000-000000000020'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "ActiveSubstanceId", "CreatedAtUtc") VALUES ('a3110000-0000-4000-8000-000000000040'::uuid, 'a3010000-0000-4000-8000-000000000020'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000001'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a3210000-0000-4000-8000-000000000020'::uuid, 'a3010000-0000-4000-8000-000000000020'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000001'::uuid, 'Substance', 'f2000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;

-- Food interactions (20)
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000001'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'High-fat meal increases peak concentration.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000001'::uuid, 'a4010000-0000-4000-8000-000000000001'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000001'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000002'::uuid, 'a4010000-0000-4000-8000-000000000001'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000002'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000001'::uuid, 'a4010000-0000-4000-8000-000000000001'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000001'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000002'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000002'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Vitamin-rich food may antagonize the drug effect.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000003'::uuid, 'a4010000-0000-4000-8000-000000000002'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000002'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000004'::uuid, 'a4010000-0000-4000-8000-000000000002'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000003'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000002'::uuid, 'a4010000-0000-4000-8000-000000000002'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000002'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000003'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000003'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Tyramine-rich food may cause hypertensive crisis.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000005'::uuid, 'a4010000-0000-4000-8000-000000000003'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000003'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000006'::uuid, 'a4010000-0000-4000-8000-000000000003'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000004'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000003'::uuid, 'a4010000-0000-4000-8000-000000000003'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000003'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000004'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000004'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Grapefruit juice inhibits metabolism.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000007'::uuid, 'a4010000-0000-4000-8000-000000000004'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000004'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000008'::uuid, 'a4010000-0000-4000-8000-000000000004'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000005'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000004'::uuid, 'a4010000-0000-4000-8000-000000000004'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000004'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000005'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000005'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Food may reduce absorption of the medication.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000009'::uuid, 'a4010000-0000-4000-8000-000000000005'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000005'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000010'::uuid, 'a4010000-0000-4000-8000-000000000005'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000006'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000005'::uuid, 'a4010000-0000-4000-8000-000000000005'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000005'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000006'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000006'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'High-fat meal increases peak concentration.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000011'::uuid, 'a4010000-0000-4000-8000-000000000006'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000006'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000012'::uuid, 'a4010000-0000-4000-8000-000000000006'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000007'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000006'::uuid, 'a4010000-0000-4000-8000-000000000006'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000006'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000007'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000007'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Vitamin-rich food may antagonize the drug effect.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000013'::uuid, 'a4010000-0000-4000-8000-000000000007'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000007'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000014'::uuid, 'a4010000-0000-4000-8000-000000000007'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000008'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000007'::uuid, 'a4010000-0000-4000-8000-000000000007'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000007'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000008'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000008'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Tyramine-rich food may cause hypertensive crisis.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000015'::uuid, 'a4010000-0000-4000-8000-000000000008'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000008'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000016'::uuid, 'a4010000-0000-4000-8000-000000000008'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000009'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000008'::uuid, 'a4010000-0000-4000-8000-000000000008'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000008'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000009'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000009'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Grapefruit juice inhibits metabolism.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000017'::uuid, 'a4010000-0000-4000-8000-000000000009'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000009'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000018'::uuid, 'a4010000-0000-4000-8000-000000000009'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000010'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000009'::uuid, 'a4010000-0000-4000-8000-000000000009'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000009'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000010'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000010'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Food may reduce absorption of the medication.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000019'::uuid, 'a4010000-0000-4000-8000-000000000010'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000010'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000020'::uuid, 'a4010000-0000-4000-8000-000000000010'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000011'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000010'::uuid, 'a4010000-0000-4000-8000-000000000010'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000010'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000011'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000011'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'High-fat meal increases peak concentration.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000021'::uuid, 'a4010000-0000-4000-8000-000000000011'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000011'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000022'::uuid, 'a4010000-0000-4000-8000-000000000011'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000012'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000011'::uuid, 'a4010000-0000-4000-8000-000000000011'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000011'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000012'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000012'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Vitamin-rich food may antagonize the drug effect.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000023'::uuid, 'a4010000-0000-4000-8000-000000000012'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000012'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000024'::uuid, 'a4010000-0000-4000-8000-000000000012'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000013'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000012'::uuid, 'a4010000-0000-4000-8000-000000000012'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000012'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000013'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000013'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Tyramine-rich food may cause hypertensive crisis.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000025'::uuid, 'a4010000-0000-4000-8000-000000000013'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000013'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000026'::uuid, 'a4010000-0000-4000-8000-000000000013'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000014'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000013'::uuid, 'a4010000-0000-4000-8000-000000000013'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000013'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000014'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000014'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Grapefruit juice inhibits metabolism.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000027'::uuid, 'a4010000-0000-4000-8000-000000000014'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000014'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000028'::uuid, 'a4010000-0000-4000-8000-000000000014'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000015'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000014'::uuid, 'a4010000-0000-4000-8000-000000000014'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000014'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000015'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000015'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Food may reduce absorption of the medication.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000029'::uuid, 'a4010000-0000-4000-8000-000000000015'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000015'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000030'::uuid, 'a4010000-0000-4000-8000-000000000015'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000016'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000015'::uuid, 'a4010000-0000-4000-8000-000000000015'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000015'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000016'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000016'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'High-fat meal increases peak concentration.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000031'::uuid, 'a4010000-0000-4000-8000-000000000016'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000016'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000032'::uuid, 'a4010000-0000-4000-8000-000000000016'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000017'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000016'::uuid, 'a4010000-0000-4000-8000-000000000016'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000016'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000017'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000017'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Vitamin-rich food may antagonize the drug effect.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000033'::uuid, 'a4010000-0000-4000-8000-000000000017'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000017'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000034'::uuid, 'a4010000-0000-4000-8000-000000000017'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000018'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000017'::uuid, 'a4010000-0000-4000-8000-000000000017'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000017'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000018'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000018'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Tyramine-rich food may cause hypertensive crisis.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000035'::uuid, 'a4010000-0000-4000-8000-000000000018'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000018'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000036'::uuid, 'a4010000-0000-4000-8000-000000000018'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000019'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000018'::uuid, 'a4010000-0000-4000-8000-000000000018'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000018'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000019'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000019'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Grapefruit juice inhibits metabolism.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000037'::uuid, 'a4010000-0000-4000-8000-000000000019'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000019'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000038'::uuid, 'a4010000-0000-4000-8000-000000000019'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000019'::uuid, 'a4010000-0000-4000-8000-000000000019'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000019'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a4010000-0000-4000-8000-000000000020'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Food may reduce absorption of the medication.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000039'::uuid, 'a4010000-0000-4000-8000-000000000020'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "FoodId", "CreatedAtUtc") VALUES ('a4110000-0000-4000-8000-000000000040'::uuid, 'a4010000-0000-4000-8000-000000000020'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000001'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a4210000-0000-4000-8000-000000000020'::uuid, 'a4010000-0000-4000-8000-000000000020'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000001'::uuid, 'Food', 'b0000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;

-- Supplement interactions (20)
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000001'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Mineral binding reduces drug bioavailability.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000001'::uuid, 'a5010000-0000-4000-8000-000000000001'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000001'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000002'::uuid, 'a5010000-0000-4000-8000-000000000001'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000002'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000001'::uuid, 'a5010000-0000-4000-8000-000000000001'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000001'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000002'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000002'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Herbal product induces metabolic enzymes.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000003'::uuid, 'a5010000-0000-4000-8000-000000000002'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000002'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000004'::uuid, 'a5010000-0000-4000-8000-000000000002'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000003'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000002'::uuid, 'a5010000-0000-4000-8000-000000000002'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000002'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000003'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000003'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Electrolyte imbalance may worsen adverse effects.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000005'::uuid, 'a5010000-0000-4000-8000-000000000003'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000003'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000006'::uuid, 'a5010000-0000-4000-8000-000000000003'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000004'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000003'::uuid, 'a5010000-0000-4000-8000-000000000003'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000003'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000004'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000004'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Antioxidant supplement alters therapeutic response.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000007'::uuid, 'a5010000-0000-4000-8000-000000000004'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000004'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000008'::uuid, 'a5010000-0000-4000-8000-000000000004'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000005'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000004'::uuid, 'a5010000-0000-4000-8000-000000000004'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000004'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000005'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000005'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Supplement may potentiate sedative effects.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000009'::uuid, 'a5010000-0000-4000-8000-000000000005'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000005'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000010'::uuid, 'a5010000-0000-4000-8000-000000000005'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000006'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000005'::uuid, 'a5010000-0000-4000-8000-000000000005'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000005'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000006'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000006'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Mineral binding reduces drug bioavailability.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000011'::uuid, 'a5010000-0000-4000-8000-000000000006'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000006'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000012'::uuid, 'a5010000-0000-4000-8000-000000000006'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000007'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000006'::uuid, 'a5010000-0000-4000-8000-000000000006'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000006'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000007'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000007'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Herbal product induces metabolic enzymes.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000013'::uuid, 'a5010000-0000-4000-8000-000000000007'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000007'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000014'::uuid, 'a5010000-0000-4000-8000-000000000007'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000008'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000007'::uuid, 'a5010000-0000-4000-8000-000000000007'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000007'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000008'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000008'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Electrolyte imbalance may worsen adverse effects.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000015'::uuid, 'a5010000-0000-4000-8000-000000000008'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000008'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000016'::uuid, 'a5010000-0000-4000-8000-000000000008'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000009'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000008'::uuid, 'a5010000-0000-4000-8000-000000000008'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000008'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000009'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000009'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Antioxidant supplement alters therapeutic response.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000017'::uuid, 'a5010000-0000-4000-8000-000000000009'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000009'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000018'::uuid, 'a5010000-0000-4000-8000-000000000009'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000010'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000009'::uuid, 'a5010000-0000-4000-8000-000000000009'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000009'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000010'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000010'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Supplement may potentiate sedative effects.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000019'::uuid, 'a5010000-0000-4000-8000-000000000010'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000010'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000020'::uuid, 'a5010000-0000-4000-8000-000000000010'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000011'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000010'::uuid, 'a5010000-0000-4000-8000-000000000010'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000010'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000011'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000011'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Mineral binding reduces drug bioavailability.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000021'::uuid, 'a5010000-0000-4000-8000-000000000011'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000011'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000022'::uuid, 'a5010000-0000-4000-8000-000000000011'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000012'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000011'::uuid, 'a5010000-0000-4000-8000-000000000011'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000011'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000012'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000012'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Herbal product induces metabolic enzymes.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000023'::uuid, 'a5010000-0000-4000-8000-000000000012'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000012'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000024'::uuid, 'a5010000-0000-4000-8000-000000000012'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000013'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000012'::uuid, 'a5010000-0000-4000-8000-000000000012'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000012'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000013'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000013'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Electrolyte imbalance may worsen adverse effects.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000025'::uuid, 'a5010000-0000-4000-8000-000000000013'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000013'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000026'::uuid, 'a5010000-0000-4000-8000-000000000013'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000014'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000013'::uuid, 'a5010000-0000-4000-8000-000000000013'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000013'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000014'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000014'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Antioxidant supplement alters therapeutic response.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000027'::uuid, 'a5010000-0000-4000-8000-000000000014'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000014'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000028'::uuid, 'a5010000-0000-4000-8000-000000000014'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000015'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000014'::uuid, 'a5010000-0000-4000-8000-000000000014'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000014'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000015'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000015'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Supplement may potentiate sedative effects.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000029'::uuid, 'a5010000-0000-4000-8000-000000000015'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000015'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000030'::uuid, 'a5010000-0000-4000-8000-000000000015'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000016'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000015'::uuid, 'a5010000-0000-4000-8000-000000000015'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000015'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000016'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000016'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Mineral binding reduces drug bioavailability.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000031'::uuid, 'a5010000-0000-4000-8000-000000000016'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000016'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000032'::uuid, 'a5010000-0000-4000-8000-000000000016'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000017'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000016'::uuid, 'a5010000-0000-4000-8000-000000000016'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000016'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000017'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000017'::uuid, '11111111-1111-1111-1111-111111110111'::uuid, 'Herbal product induces metabolic enzymes.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000033'::uuid, 'a5010000-0000-4000-8000-000000000017'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000017'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000034'::uuid, 'a5010000-0000-4000-8000-000000000017'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000018'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000017'::uuid, 'a5010000-0000-4000-8000-000000000017'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000017'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000018'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000018'::uuid, '22222222-2222-2222-2222-222222221222'::uuid, 'Electrolyte imbalance may worsen adverse effects.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000035'::uuid, 'a5010000-0000-4000-8000-000000000018'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000018'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000036'::uuid, 'a5010000-0000-4000-8000-000000000018'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000019'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000018'::uuid, 'a5010000-0000-4000-8000-000000000018'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000018'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000019'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000019'::uuid, '33333333-3333-3333-3333-333333332333'::uuid, 'Antioxidant supplement alters therapeutic response.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000037'::uuid, 'a5010000-0000-4000-8000-000000000019'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000019'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000038'::uuid, 'a5010000-0000-4000-8000-000000000019'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000019'::uuid, 'a5010000-0000-4000-8000-000000000019'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000019'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "Interactions" ("Id", "InteractionSeverityId", "DefaultText", "StomachText", "CreatedAtUtc") VALUES ('a5010000-0000-4000-8000-000000000020'::uuid, '44444444-4444-4444-4444-444444443444'::uuid, 'Supplement may potentiate sedative effects.', NULL, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO UPDATE SET "DefaultText"=EXCLUDED."DefaultText";
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000039'::uuid, 'a5010000-0000-4000-8000-000000000020'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionParties" ("Id", "InteractionId", "PartyType", "SupplementId", "CreatedAtUtc") VALUES ('a5110000-0000-4000-8000-000000000040'::uuid, 'a5010000-0000-4000-8000-000000000020'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000001'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "InteractionExpanded" ("Id", "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB", "CreatedAtUtc") VALUES ('a5210000-0000-4000-8000-000000000020'::uuid, 'a5010000-0000-4000-8000-000000000020'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000001'::uuid, 'Supplement', 'c0000000-0000-4000-8000-000000000020'::uuid, (NOW() AT TIME ZONE 'utc')) ON CONFLICT ("Id") DO NOTHING;

-- Alcohol interaction overrides (20)
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000001'::uuid, 'f2000000-0000-4000-8000-000000000001'::uuid, 'Avoid alcohol with Ibuprofen: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000002'::uuid, 'f2000000-0000-4000-8000-000000000002'::uuid, 'Avoid alcohol with Paracetamol: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000003'::uuid, 'f2000000-0000-4000-8000-000000000003'::uuid, 'Avoid alcohol with Metformin: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000004'::uuid, 'f2000000-0000-4000-8000-000000000004'::uuid, 'Avoid alcohol with Atorvastatin: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000005'::uuid, 'f2000000-0000-4000-8000-000000000005'::uuid, 'Avoid alcohol with Omeprazole: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000006'::uuid, 'f2000000-0000-4000-8000-000000000006'::uuid, 'Avoid alcohol with Amoxicillin: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000007'::uuid, 'f2000000-0000-4000-8000-000000000007'::uuid, 'Avoid alcohol with Lisinopril: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000008'::uuid, 'f2000000-0000-4000-8000-000000000008'::uuid, 'Avoid alcohol with Levothyroxine sodium: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000009'::uuid, 'f2000000-0000-4000-8000-000000000009'::uuid, 'Avoid alcohol with Salbutamol: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000010'::uuid, 'f2000000-0000-4000-8000-000000000010'::uuid, 'Avoid alcohol with Sertraline: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000011'::uuid, 'f2000000-0000-4000-8000-000000000011'::uuid, 'Avoid alcohol with Amlodipine: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000012'::uuid, 'f2000000-0000-4000-8000-000000000012'::uuid, 'Avoid alcohol with Simvastatin: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000013'::uuid, 'f2000000-0000-4000-8000-000000000013'::uuid, 'Avoid alcohol with Losartan: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000014'::uuid, 'f2000000-0000-4000-8000-000000000014'::uuid, 'Avoid alcohol with Pantoprazole: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000015'::uuid, 'f2000000-0000-4000-8000-000000000015'::uuid, 'Avoid alcohol with Cetirizine: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000016'::uuid, 'f2000000-0000-4000-8000-000000000016'::uuid, 'Avoid alcohol with Metoprolol: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000017'::uuid, 'f2000000-0000-4000-8000-000000000017'::uuid, 'Avoid alcohol with Warfarin: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000018'::uuid, 'f2000000-0000-4000-8000-000000000018'::uuid, 'Avoid alcohol with Diazepam: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000019'::uuid, 'f2000000-0000-4000-8000-000000000019'::uuid, 'Avoid alcohol with Insulin glargine: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";
INSERT INTO "ActiveSubstanceAlcoholInteractionOverrides" ("Id", "ActiveSubstanceId", "AlcoholSpecialText", "CreatedAtUtc")
VALUES ('f2900000-0000-4000-8000-000000000020'::uuid, 'f2000000-0000-4000-8000-000000000020'::uuid, 'Avoid alcohol with Acetylsalicylic acid: heightened CNS or bleeding risk.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("ActiveSubstanceId") DO UPDATE SET "AlcoholSpecialText"=EXCLUDED."AlcoholSpecialText";

-- FAQ entries (20)
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000001'::uuid, 'How should I track supplement 1 with my prescriptions?', 'how should i track supplement 1 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000002'::uuid, 'How should I track supplement 2 with my prescriptions?', 'how should i track supplement 2 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000003'::uuid, 'How should I track supplement 3 with my prescriptions?', 'how should i track supplement 3 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000004'::uuid, 'How should I track supplement 4 with my prescriptions?', 'how should i track supplement 4 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000005'::uuid, 'How should I track supplement 5 with my prescriptions?', 'how should i track supplement 5 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000006'::uuid, 'How should I track supplement 6 with my prescriptions?', 'how should i track supplement 6 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000007'::uuid, 'How should I track supplement 7 with my prescriptions?', 'how should i track supplement 7 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000008'::uuid, 'How should I track supplement 8 with my prescriptions?', 'how should i track supplement 8 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000009'::uuid, 'How should I track supplement 9 with my prescriptions?', 'how should i track supplement 9 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000010'::uuid, 'How should I track supplement 10 with my prescriptions?', 'how should i track supplement 10 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000011'::uuid, 'How should I track supplement 11 with my prescriptions?', 'how should i track supplement 11 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000012'::uuid, 'How should I track supplement 12 with my prescriptions?', 'how should i track supplement 12 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000013'::uuid, 'How should I track supplement 13 with my prescriptions?', 'how should i track supplement 13 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000014'::uuid, 'How should I track supplement 14 with my prescriptions?', 'how should i track supplement 14 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000015'::uuid, 'How should I track supplement 15 with my prescriptions?', 'how should i track supplement 15 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000016'::uuid, 'How should I track supplement 16 with my prescriptions?', 'how should i track supplement 16 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000017'::uuid, 'How should I track supplement 17 with my prescriptions?', 'how should i track supplement 17 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000018'::uuid, 'How should I track supplement 18 with my prescriptions?', 'how should i track supplement 18 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000019'::uuid, 'How should I track supplement 19 with my prescriptions?', 'how should i track supplement 19 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";
INSERT INTO "FaqEntries" ("Id", "Question", "NormalizedQuestion", "Answer", "CreatedAtUtc")
VALUES ('fa100000-0000-4000-8000-000000000020'::uuid, 'How should I track supplement 20 with my prescriptions?', 'how should i track supplement 20 with my prescriptions?', 'Log intake times in the app and review interactions before adding new items.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Question"=EXCLUDED."Question", "NormalizedQuestion"=EXCLUDED."NormalizedQuestion", "Answer"=EXCLUDED."Answer";

-- Drug problem types (5)
INSERT INTO "DrugProblemTypes" ("Id", "ProblemTypeName", "CreatedAtUtc")
VALUES ('f2800000-0000-4000-8000-000000000001'::uuid, 'Nausea', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "ProblemTypeName"=EXCLUDED."ProblemTypeName";
INSERT INTO "DrugProblemTypes" ("Id", "ProblemTypeName", "CreatedAtUtc")
VALUES ('f2800000-0000-4000-8000-000000000002'::uuid, 'Dizziness', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "ProblemTypeName"=EXCLUDED."ProblemTypeName";
INSERT INTO "DrugProblemTypes" ("Id", "ProblemTypeName", "CreatedAtUtc")
VALUES ('f2800000-0000-4000-8000-000000000003'::uuid, 'Rash', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "ProblemTypeName"=EXCLUDED."ProblemTypeName";
INSERT INTO "DrugProblemTypes" ("Id", "ProblemTypeName", "CreatedAtUtc")
VALUES ('f2800000-0000-4000-8000-000000000004'::uuid, 'Stomach pain', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "ProblemTypeName"=EXCLUDED."ProblemTypeName";
INSERT INTO "DrugProblemTypes" ("Id", "ProblemTypeName", "CreatedAtUtc")
VALUES ('f2800000-0000-4000-8000-000000000005'::uuid, 'Drowsiness', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "ProblemTypeName"=EXCLUDED."ProblemTypeName";

INSERT INTO "ActiveSubstanceGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2500000-0000-4000-8000-000000000001'::uuid, 'NSAID cluster', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "ActiveSubstanceGroupMembers" ("ActiveSubstanceGroupId", "ActiveSubstanceId")
VALUES
    ('f2500000-0000-4000-8000-000000000001'::uuid, 'f2000000-0000-4000-8000-000000000001'::uuid),
    ('f2500000-0000-4000-8000-000000000001'::uuid, 'f2000000-0000-4000-8000-000000000002'::uuid),
    ('f2500000-0000-4000-8000-000000000001'::uuid, 'f2000000-0000-4000-8000-000000000003'::uuid),
    ('f2500000-0000-4000-8000-000000000001'::uuid, 'f2000000-0000-4000-8000-000000000004'::uuid),
    ('f2500000-0000-4000-8000-000000000001'::uuid, 'f2000000-0000-4000-8000-000000000005'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "ActiveSubstanceGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2500000-0000-4000-8000-000000000002'::uuid, 'Cardiometabolic cluster', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "ActiveSubstanceGroupMembers" ("ActiveSubstanceGroupId", "ActiveSubstanceId")
VALUES
    ('f2500000-0000-4000-8000-000000000002'::uuid, 'f2000000-0000-4000-8000-000000000006'::uuid),
    ('f2500000-0000-4000-8000-000000000002'::uuid, 'f2000000-0000-4000-8000-000000000007'::uuid),
    ('f2500000-0000-4000-8000-000000000002'::uuid, 'f2000000-0000-4000-8000-000000000008'::uuid),
    ('f2500000-0000-4000-8000-000000000002'::uuid, 'f2000000-0000-4000-8000-000000000009'::uuid),
    ('f2500000-0000-4000-8000-000000000002'::uuid, 'f2000000-0000-4000-8000-000000000010'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "ActiveSubstanceGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2500000-0000-4000-8000-000000000003'::uuid, 'Psychiatry cluster', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "ActiveSubstanceGroupMembers" ("ActiveSubstanceGroupId", "ActiveSubstanceId")
VALUES
    ('f2500000-0000-4000-8000-000000000003'::uuid, 'f2000000-0000-4000-8000-000000000011'::uuid),
    ('f2500000-0000-4000-8000-000000000003'::uuid, 'f2000000-0000-4000-8000-000000000012'::uuid),
    ('f2500000-0000-4000-8000-000000000003'::uuid, 'f2000000-0000-4000-8000-000000000013'::uuid),
    ('f2500000-0000-4000-8000-000000000003'::uuid, 'f2000000-0000-4000-8000-000000000014'::uuid),
    ('f2500000-0000-4000-8000-000000000003'::uuid, 'f2000000-0000-4000-8000-000000000015'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "ActiveSubstanceGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2500000-0000-4000-8000-000000000004'::uuid, 'Endocrine cluster', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "ActiveSubstanceGroupMembers" ("ActiveSubstanceGroupId", "ActiveSubstanceId")
VALUES
    ('f2500000-0000-4000-8000-000000000004'::uuid, 'f2000000-0000-4000-8000-000000000016'::uuid),
    ('f2500000-0000-4000-8000-000000000004'::uuid, 'f2000000-0000-4000-8000-000000000017'::uuid),
    ('f2500000-0000-4000-8000-000000000004'::uuid, 'f2000000-0000-4000-8000-000000000018'::uuid),
    ('f2500000-0000-4000-8000-000000000004'::uuid, 'f2000000-0000-4000-8000-000000000019'::uuid),
    ('f2500000-0000-4000-8000-000000000004'::uuid, 'f2000000-0000-4000-8000-000000000020'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "FoodGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2600000-0000-4000-8000-000000000001'::uuid, 'Citrus & fruit', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "FoodGroupFoods" ("FoodGroupId", "FoodId")
VALUES
    ('f2600000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000001'::uuid),
    ('f2600000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000002'::uuid),
    ('f2600000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000003'::uuid),
    ('f2600000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000004'::uuid),
    ('f2600000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000005'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "FoodGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2600000-0000-4000-8000-000000000002'::uuid, 'Dairy & protein', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "FoodGroupFoods" ("FoodGroupId", "FoodId")
VALUES
    ('f2600000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000006'::uuid),
    ('f2600000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000007'::uuid),
    ('f2600000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000008'::uuid),
    ('f2600000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000009'::uuid),
    ('f2600000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000010'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "FoodGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2600000-0000-4000-8000-000000000003'::uuid, 'Allergenic staples', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "FoodGroupFoods" ("FoodGroupId", "FoodId")
VALUES
    ('f2600000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000011'::uuid),
    ('f2600000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000012'::uuid),
    ('f2600000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000013'::uuid),
    ('f2600000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000014'::uuid),
    ('f2600000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000015'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "FoodGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2600000-0000-4000-8000-000000000004'::uuid, 'Pantry herbs', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "FoodGroupFoods" ("FoodGroupId", "FoodId")
VALUES
    ('f2600000-0000-4000-8000-000000000004'::uuid, 'b0000000-0000-4000-8000-000000000016'::uuid),
    ('f2600000-0000-4000-8000-000000000004'::uuid, 'b0000000-0000-4000-8000-000000000017'::uuid),
    ('f2600000-0000-4000-8000-000000000004'::uuid, 'b0000000-0000-4000-8000-000000000018'::uuid),
    ('f2600000-0000-4000-8000-000000000004'::uuid, 'b0000000-0000-4000-8000-000000000019'::uuid),
    ('f2600000-0000-4000-8000-000000000004'::uuid, 'b0000000-0000-4000-8000-000000000020'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "SupplementGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2700000-0000-4000-8000-000000000001'::uuid, 'Vitamins', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "SupplementGroupSupplements" ("SupplementGroupId", "SupplementId")
VALUES
    ('f2700000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid),
    ('f2700000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000002'::uuid),
    ('f2700000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000003'::uuid),
    ('f2700000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000004'::uuid),
    ('f2700000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "SupplementGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2700000-0000-4000-8000-000000000002'::uuid, 'Minerals', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "SupplementGroupSupplements" ("SupplementGroupId", "SupplementId")
VALUES
    ('f2700000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000006'::uuid),
    ('f2700000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000007'::uuid),
    ('f2700000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000008'::uuid),
    ('f2700000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000009'::uuid),
    ('f2700000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000010'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "SupplementGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2700000-0000-4000-8000-000000000003'::uuid, 'Herbal', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "SupplementGroupSupplements" ("SupplementGroupId", "SupplementId")
VALUES
    ('f2700000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000011'::uuid),
    ('f2700000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000012'::uuid),
    ('f2700000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000013'::uuid),
    ('f2700000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000014'::uuid),
    ('f2700000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000015'::uuid)
ON CONFLICT DO NOTHING;
INSERT INTO "SupplementGroups" ("Id", "Name", "CreatedAtUtc")
VALUES ('f2700000-0000-4000-8000-000000000004'::uuid, 'Specialty blends', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name";
INSERT INTO "SupplementGroupSupplements" ("SupplementGroupId", "SupplementId")
VALUES
    ('f2700000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000016'::uuid),
    ('f2700000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000017'::uuid),
    ('f2700000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000018'::uuid),
    ('f2700000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000019'::uuid),
    ('f2700000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000020'::uuid)
ON CONFLICT DO NOTHING;

-- Extra system texts (3)
INSERT INTO "SystemTexts" ("Id", "Key", "Value", "CreatedAtUtc")
VALUES ('f6000000-0000-4000-8000-000000000001'::uuid, 'interaction.disclaimer', 'Interaction results are informational and do not replace clinical advice.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Key") DO UPDATE SET "Value"=EXCLUDED."Value";
INSERT INTO "SystemTexts" ("Id", "Key", "Value", "CreatedAtUtc")
VALUES ('f6000000-0000-4000-8000-000000000002'::uuid, 'supervision.patientIntro', 'You can approve or reject healthcare provider access requests at any time.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Key") DO UPDATE SET "Value"=EXCLUDED."Value";
INSERT INTO "SystemTexts" ("Id", "Key", "Value", "CreatedAtUtc")
VALUES ('f6000000-0000-4000-8000-000000000003'::uuid, 'faq.supportFooter', 'Contact support if your medication list changes after a hospital visit.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Key") DO UPDATE SET "Value"=EXCLUDED."Value";

-- Vet drugs (10)
INSERT INTO "VetDrugs" ("Id", "ProductIdentifier", "CommercialName", "TypeOfProduct", "CreatedAtUtc")
VALUES
    ('b5010000-0000-4000-8000-000000000001'::uuid, 1, 'Bravecto', 'Weterynaryjny', (NOW() AT TIME ZONE 'utc')),
    ('b5010000-0000-4000-8000-000000000002'::uuid, 2, 'Frontline Combo', 'Weterynaryjny', (NOW() AT TIME ZONE 'utc')),
    ('b5010000-0000-4000-8000-000000000003'::uuid, 3, 'Milbemax', 'Weterynaryjny', (NOW() AT TIME ZONE 'utc')),
    ('b5010000-0000-4000-8000-000000000004'::uuid, 4, 'Rimadyl', 'Weterynaryjny', (NOW() AT TIME ZONE 'utc')),
    ('b5010000-0000-4000-8000-000000000005'::uuid, 5, 'Metacam', 'Weterynaryjny', (NOW() AT TIME ZONE 'utc')),
    ('b5010000-0000-4000-8000-000000000006'::uuid, 6, 'Baytril', 'Weterynaryjny', (NOW() AT TIME ZONE 'utc')),
    ('b5010000-0000-4000-8000-000000000007'::uuid, 7, 'Nobivac DHPPi', 'Weterynaryjny', (NOW() AT TIME ZONE 'utc')),
    ('b5010000-0000-4000-8000-000000000008'::uuid, 8, 'Canigen', 'Weterynaryjny', (NOW() AT TIME ZONE 'utc')),
    ('b5010000-0000-4000-8000-000000000009'::uuid, 9, 'Synulox', 'Weterynaryjny', (NOW() AT TIME ZONE 'utc')),
    ('b5010000-0000-4000-8000-000000000010'::uuid, 10, 'Previcox', 'Weterynaryjny', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "CommercialName"=EXCLUDED."CommercialName", "TypeOfProduct"=EXCLUDED."TypeOfProduct";

-- Temp drug import batch (1)
INSERT INTO "TempDrugImportBatches" ("Id", "NameOfFile", "BlobFileId", "SourceType", "StatusOfImport", "TotalQuantityCount", "ImportedCount", "SkippedCounter", "FailedToCreateCounter", "RecordsDeletedInRevertCounter", "FailedToDeleteCounter", "ImportedIds", "SkippedIds", "FailedToCreate", "RecordsDeletedInRevertIds", "FailedToDelete", "CreatedAtUtc")
VALUES ('b6010000-0000-4000-8000-000000000001'::uuid, 'seed-vet-import.csv', 'a0a0a0a0-0000-4000-8000-000000000001'::uuid, 'Seed', 'ImportedSuccessfully', 10, 10, 0, 0, 0, 0, '[]'::jsonb, '[]'::jsonb, '[]'::jsonb, '[]'::jsonb, '[]'::jsonb, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO UPDATE SET "StatusOfImport"=EXCLUDED."StatusOfImport";
-- Temp drugs (10)
INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", "RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")
VALUES ('b6020000-0000-4000-8000-000000000001'::uuid, 'SEED-VET-0001', 'Imported vet candidate 1', 'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', 'b6010000-0000-4000-8000-000000000001'::uuid, 'PendingReview', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", "RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")
VALUES ('b6020000-0000-4000-8000-000000000002'::uuid, 'SEED-VET-0002', 'Imported vet candidate 2', 'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', 'b6010000-0000-4000-8000-000000000001'::uuid, 'PendingReview', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", "RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")
VALUES ('b6020000-0000-4000-8000-000000000003'::uuid, 'SEED-VET-0003', 'Imported vet candidate 3', 'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', 'b6010000-0000-4000-8000-000000000001'::uuid, 'PendingReview', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", "RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")
VALUES ('b6020000-0000-4000-8000-000000000004'::uuid, 'SEED-VET-0004', 'Imported vet candidate 4', 'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', 'b6010000-0000-4000-8000-000000000001'::uuid, 'PendingReview', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", "RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")
VALUES ('b6020000-0000-4000-8000-000000000005'::uuid, 'SEED-VET-0005', 'Imported vet candidate 5', 'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', 'b6010000-0000-4000-8000-000000000001'::uuid, 'PendingReview', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", "RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")
VALUES ('b6020000-0000-4000-8000-000000000006'::uuid, 'SEED-VET-0006', 'Imported vet candidate 6', 'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', 'b6010000-0000-4000-8000-000000000001'::uuid, 'PendingReview', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", "RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")
VALUES ('b6020000-0000-4000-8000-000000000007'::uuid, 'SEED-VET-0007', 'Imported vet candidate 7', 'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', 'b6010000-0000-4000-8000-000000000001'::uuid, 'PendingReview', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", "RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")
VALUES ('b6020000-0000-4000-8000-000000000008'::uuid, 'SEED-VET-0008', 'Imported vet candidate 8', 'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', 'b6010000-0000-4000-8000-000000000001'::uuid, 'PendingReview', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", "RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")
VALUES ('b6020000-0000-4000-8000-000000000009'::uuid, 'SEED-VET-0009', 'Imported vet candidate 9', 'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', 'b6010000-0000-4000-8000-000000000001'::uuid, 'PendingReview', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "TempDrugs" ("Id", "ProductIdentifier", "CommercialName", "ActiveSubstance", "TypeOfProduct", "RouteOfAdministration", "PharmaceuticalFormulation", "ATCCode", "Dosage", "LinkedImportId", "Status", "CreatedAtUtc")
VALUES ('b6020000-0000-4000-8000-000000000010'::uuid, 'SEED-VET-0010', 'Imported vet candidate 10', 'Fluralaner', 'Weterynaryjny', 'Topical', 'Spot-on solution', 'QP53BX04', '250 mg', 'b6010000-0000-4000-8000-000000000001'::uuid, 'PendingReview', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- User fixture 1 (b1010000-0000-4000-8000-000000000001)
INSERT INTO "ClientAllergySummaries" ("Id", "UserId", "SevereReactionStatus", "CarriesAllergyMedication", "PollenDustMitesAllergyStatus", "InsectVenomAllergyStatus", "LatexAllergyStatus", "CreatedAtUtc")
SELECT 'e3000000-0000-4000-8000-000000000001'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'No', FALSE, 'DontKnow', 'Yes', 'No', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM "ClientAllergySummaries" WHERE "UserId" = 'b1010000-0000-4000-8000-000000000001'::uuid);
INSERT INTO client_reproductive_health_profiles ("Id", "UserId", "CurrentlyPregnant", "PlanningToGetPregnant", "Menopause", "ContraceptionMethodCode", "CreatedAtUtc")
SELECT 'e4000000-0000-4000-8000-000000000001'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, FALSE, FALSE, FALSE, 'iud', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM client_reproductive_health_profiles WHERE "UserId" = 'b1010000-0000-4000-8000-000000000001'::uuid);
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "DrugFormId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e0000000-0000-4000-8000-000000000011'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'a1000000-0000-4000-8000-000000000001'::uuid, 'Database', 'Rash after catalog drug form.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "CustomDrugName", "CustomDrugNameNormalized", "CustomActiveSubstances", "CustomFormulationName", "CustomDosage", "Source", "Note", "CreatedAtUtc") VALUES (
    'e0000000-0000-4000-8000-000000000012'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid,
    'Homeopathic Arnica 1', 'homeopathic arnica 1', 'Arnica montana', 'Drops', '30C',
    'Custom', 'Custom remedy reaction.', (NOW() AT TIME ZONE 'utc')
) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "FoodId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000000011'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000001'::uuid, 'Database', 'Oral tingling from catalog food.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "CustomFoodName", "CustomFoodNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000000012'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'Artisan honeycomb 1', 'artisan honeycomb 1', 'Custom', 'Local market sample caused lip swelling.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "SupplementId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000000011'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 'Database', 'Itching after supplement.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "CustomSupplementName", "CustomSupplementNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000000012'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'Herbal blend 1', 'herbal blend 1', 'Custom', 'Loose tea mix caused hives.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientOtherAllergies" ("Id", "UserId", "AllergyName", "AllergyNameNormalized", "Note", "CreatedAtUtc") VALUES
    ('e2500000-0000-4000-8000-000000000001'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'Nickel jewelry 1', 'nickel jewelry 1', 'Contact dermatitis from watch buckle.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientMealTimes" ("Id", "UserId", "MealName", "NormalizedMealName", "IntakeTime", "CreatedAtUtc")
VALUES
    ('e8000000-0000-4000-8000-000000000011'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'Breakfast', 'breakfast', '07:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000012'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'Lunch', 'lunch', '12:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000013'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'Dinner', 'dinner', '18:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000014'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'Evening snack', 'evening snack', '20:30'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000011'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'a0000000-0000-4000-8000-000000000001'::uuid, 'a1000000-0000-4000-8000-000000000001'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000000011'::uuid, 'e5000000-0000-4000-8000-000000000011'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000012'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'a0000000-0000-4000-8000-000000000004'::uuid, 'a1000000-0000-4000-8000-000000000004'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000000012'::uuid, 'e5000000-0000-4000-8000-000000000012'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000019'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'a0000000-0000-4000-8000-000000000006'::uuid, 'a1000000-0000-4000-8000-000000000006'::uuid, 'Expired', CURRENT_DATE - 90, CURRENT_DATE - 10, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '10 days', (NOW() AT TIME ZONE 'utc') - interval '90 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000011'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000001'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000000011'::uuid, 'e6000000-0000-4000-8000-000000000011'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000012'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000002'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000000012'::uuid, 'e6000000-0000-4000-8000-000000000012'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000019'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000006'::uuid, 'Expired', CURRENT_DATE - 60, CURRENT_DATE - 5, 'AutoExpire', (NOW() AT TIME ZONE 'utc') - interval '5 days', (NOW() AT TIME ZONE 'utc') - interval '60 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000011'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000000011'::uuid, 'e7000000-0000-4000-8000-000000000011'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000012'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000002'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000000012'::uuid, 'e7000000-0000-4000-8000-000000000012'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000019'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 'Database', 'Expired', CURRENT_DATE - 45, CURRENT_DATE - 3, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '3 days', (NOW() AT TIME ZONE 'utc') - interval '45 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplements ("Id", "UserId", "Name", "NormalizedName", "CreatedAtUtc")
VALUES ('e7200000-0000-4000-8000-000000000001'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'Morning stack 1', 'morning stack 1', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplement_items ("Id", "OwnSupplementId", "SupplementId", "SortOrder", "CreatedAtUtc")
VALUES ('e7210000-0000-4000-8000-000000000001'::uuid, 'e7200000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 1, (NOW() AT TIME ZONE 'utc')),
       ('e7210000-0000-4000-8000-000000000101'::uuid, 'e7200000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 2, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000011'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'WeeklyHealthDataReminder', 'MedicalData', 'SharedRegisteredClient', 'Update your health diary', 'Review medications and symptoms this week.', 'Review medications and symptoms this week.', 'seed.b1010000-0000-4000-8000-000000000001.1', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000012'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'SupervisionRequest', 'DoctorPatient', 'PatientOnly', 'Supervision request pending', 'A healthcare provider requested access to your profile.', 'A healthcare provider requested access to your profile.', 'seed.b1010000-0000-4000-8000-000000000001.2', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000013'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'MedicalChangeProposal', 'DoctorPatient', 'PatientOnly', 'Medical change proposal', 'Your doctor proposed an update to your medication list.', 'Your doctor proposed an update to your medication list.', 'seed.b1010000-0000-4000-8000-000000000001.3', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000014'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'SystemAnnouncement', 'SystemInformation', 'SharedRegisteredClient', 'Platform maintenance', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'seed.b1010000-0000-4000-8000-000000000001.4', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- User fixture 2 (b1010000-0000-4000-8000-000000000002)
INSERT INTO "ClientAllergySummaries" ("Id", "UserId", "SevereReactionStatus", "CarriesAllergyMedication", "PollenDustMitesAllergyStatus", "InsectVenomAllergyStatus", "LatexAllergyStatus", "CreatedAtUtc")
SELECT 'e3000000-0000-4000-8000-000000000002'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'DontKnow', TRUE, 'Yes', 'No', 'Yes', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM "ClientAllergySummaries" WHERE "UserId" = 'b1010000-0000-4000-8000-000000000002'::uuid);
INSERT INTO client_reproductive_health_profiles ("Id", "UserId", "CurrentlyPregnant", "PlanningToGetPregnant", "Menopause", "ContraceptionMethodCode", "CreatedAtUtc")
SELECT 'e4000000-0000-4000-8000-000000000002'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, FALSE, TRUE, FALSE, 'none', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM client_reproductive_health_profiles WHERE "UserId" = 'b1010000-0000-4000-8000-000000000002'::uuid);
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "DrugFormId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e0000000-0000-4000-8000-000000000021'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'a1000000-0000-4000-8000-000000000002'::uuid, 'Database', 'Rash after catalog drug form.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "CustomDrugName", "CustomDrugNameNormalized", "CustomActiveSubstances", "CustomFormulationName", "CustomDosage", "Source", "Note", "CreatedAtUtc") VALUES (
    'e0000000-0000-4000-8000-000000000022'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid,
    'Homeopathic Arnica 2', 'homeopathic arnica 2', 'Arnica montana', 'Drops', '30C',
    'Custom', 'Custom remedy reaction.', (NOW() AT TIME ZONE 'utc')
) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "FoodId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000000021'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000002'::uuid, 'Database', 'Oral tingling from catalog food.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "CustomFoodName", "CustomFoodNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000000022'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'Artisan honeycomb 2', 'artisan honeycomb 2', 'Custom', 'Local market sample caused lip swelling.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "SupplementId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000000021'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000002'::uuid, 'Database', 'Itching after supplement.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "CustomSupplementName", "CustomSupplementNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000000022'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'Herbal blend 2', 'herbal blend 2', 'Custom', 'Loose tea mix caused hives.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientOtherAllergies" ("Id", "UserId", "AllergyName", "AllergyNameNormalized", "Note", "CreatedAtUtc") VALUES
    ('e2500000-0000-4000-8000-000000000002'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'Nickel jewelry 2', 'nickel jewelry 2', 'Contact dermatitis from watch buckle.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientMealTimes" ("Id", "UserId", "MealName", "NormalizedMealName", "IntakeTime", "CreatedAtUtc")
VALUES
    ('e8000000-0000-4000-8000-000000000021'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'Breakfast', 'breakfast', '07:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000022'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'Lunch', 'lunch', '12:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000023'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'Dinner', 'dinner', '18:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000024'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'Evening snack', 'evening snack', '20:30'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000021'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'a0000000-0000-4000-8000-000000000002'::uuid, 'a1000000-0000-4000-8000-000000000002'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000000021'::uuid, 'e5000000-0000-4000-8000-000000000021'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000022'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'a0000000-0000-4000-8000-000000000005'::uuid, 'a1000000-0000-4000-8000-000000000005'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000000022'::uuid, 'e5000000-0000-4000-8000-000000000022'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000029'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'a0000000-0000-4000-8000-000000000007'::uuid, 'a1000000-0000-4000-8000-000000000007'::uuid, 'Expired', CURRENT_DATE - 90, CURRENT_DATE - 10, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '10 days', (NOW() AT TIME ZONE 'utc') - interval '90 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000021'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000002'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000000021'::uuid, 'e6000000-0000-4000-8000-000000000021'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000022'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000003'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000000022'::uuid, 'e6000000-0000-4000-8000-000000000022'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000029'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000007'::uuid, 'Expired', CURRENT_DATE - 60, CURRENT_DATE - 5, 'AutoExpire', (NOW() AT TIME ZONE 'utc') - interval '5 days', (NOW() AT TIME ZONE 'utc') - interval '60 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000021'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000002'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000000021'::uuid, 'e7000000-0000-4000-8000-000000000021'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000022'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000003'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000000022'::uuid, 'e7000000-0000-4000-8000-000000000022'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000029'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000006'::uuid, 'Database', 'Expired', CURRENT_DATE - 45, CURRENT_DATE - 3, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '3 days', (NOW() AT TIME ZONE 'utc') - interval '45 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplements ("Id", "UserId", "Name", "NormalizedName", "CreatedAtUtc")
VALUES ('e7200000-0000-4000-8000-000000000002'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'Morning stack 2', 'morning stack 2', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplement_items ("Id", "OwnSupplementId", "SupplementId", "SortOrder", "CreatedAtUtc")
VALUES ('e7210000-0000-4000-8000-000000000002'::uuid, 'e7200000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 1, (NOW() AT TIME ZONE 'utc')),
       ('e7210000-0000-4000-8000-000000000102'::uuid, 'e7200000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 2, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000021'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'WeeklyHealthDataReminder', 'MedicalData', 'SharedRegisteredClient', 'Update your health diary', 'Review medications and symptoms this week.', 'Review medications and symptoms this week.', 'seed.b1010000-0000-4000-8000-000000000002.1', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000022'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'SupervisionRequest', 'DoctorPatient', 'PatientOnly', 'Supervision request pending', 'A healthcare provider requested access to your profile.', 'A healthcare provider requested access to your profile.', 'seed.b1010000-0000-4000-8000-000000000002.2', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000023'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'MedicalChangeProposal', 'DoctorPatient', 'PatientOnly', 'Medical change proposal', 'Your doctor proposed an update to your medication list.', 'Your doctor proposed an update to your medication list.', 'seed.b1010000-0000-4000-8000-000000000002.3', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000024'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'SystemAnnouncement', 'SystemInformation', 'SharedRegisteredClient', 'Platform maintenance', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'seed.b1010000-0000-4000-8000-000000000002.4', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- User fixture 3 (b1010000-0000-4000-8000-000000000003)
INSERT INTO "ClientAllergySummaries" ("Id", "UserId", "SevereReactionStatus", "CarriesAllergyMedication", "PollenDustMitesAllergyStatus", "InsectVenomAllergyStatus", "LatexAllergyStatus", "CreatedAtUtc")
SELECT 'e3000000-0000-4000-8000-000000000003'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'Yes', FALSE, 'No', 'DontKnow', 'No', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM "ClientAllergySummaries" WHERE "UserId" = 'b1010000-0000-4000-8000-000000000003'::uuid);
INSERT INTO client_reproductive_health_profiles ("Id", "UserId", "CurrentlyPregnant", "PlanningToGetPregnant", "Menopause", "ContraceptionMethodCode", "CreatedAtUtc")
SELECT 'e4000000-0000-4000-8000-000000000003'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, FALSE, FALSE, FALSE, 'barrier_method', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM client_reproductive_health_profiles WHERE "UserId" = 'b1010000-0000-4000-8000-000000000003'::uuid);
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "DrugFormId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e0000000-0000-4000-8000-000000000031'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'a1000000-0000-4000-8000-000000000003'::uuid, 'Database', 'Rash after catalog drug form.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "CustomDrugName", "CustomDrugNameNormalized", "CustomActiveSubstances", "CustomFormulationName", "CustomDosage", "Source", "Note", "CreatedAtUtc") VALUES (
    'e0000000-0000-4000-8000-000000000032'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid,
    'Homeopathic Arnica 3', 'homeopathic arnica 3', 'Arnica montana', 'Drops', '30C',
    'Custom', 'Custom remedy reaction.', (NOW() AT TIME ZONE 'utc')
) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "FoodId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000000031'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000003'::uuid, 'Database', 'Oral tingling from catalog food.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "CustomFoodName", "CustomFoodNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000000032'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'Artisan honeycomb 3', 'artisan honeycomb 3', 'Custom', 'Local market sample caused lip swelling.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "SupplementId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000000031'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000003'::uuid, 'Database', 'Itching after supplement.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "CustomSupplementName", "CustomSupplementNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000000032'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'Herbal blend 3', 'herbal blend 3', 'Custom', 'Loose tea mix caused hives.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientOtherAllergies" ("Id", "UserId", "AllergyName", "AllergyNameNormalized", "Note", "CreatedAtUtc") VALUES
    ('e2500000-0000-4000-8000-000000000003'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'Nickel jewelry 3', 'nickel jewelry 3', 'Contact dermatitis from watch buckle.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientMealTimes" ("Id", "UserId", "MealName", "NormalizedMealName", "IntakeTime", "CreatedAtUtc")
VALUES
    ('e8000000-0000-4000-8000-000000000031'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'Breakfast', 'breakfast', '07:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000032'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'Lunch', 'lunch', '12:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000033'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'Dinner', 'dinner', '18:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000034'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'Evening snack', 'evening snack', '20:30'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000031'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'a0000000-0000-4000-8000-000000000003'::uuid, 'a1000000-0000-4000-8000-000000000003'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000000031'::uuid, 'e5000000-0000-4000-8000-000000000031'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000032'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'a0000000-0000-4000-8000-000000000006'::uuid, 'a1000000-0000-4000-8000-000000000006'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000000032'::uuid, 'e5000000-0000-4000-8000-000000000032'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000039'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'a0000000-0000-4000-8000-000000000008'::uuid, 'a1000000-0000-4000-8000-000000000008'::uuid, 'Expired', CURRENT_DATE - 90, CURRENT_DATE - 10, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '10 days', (NOW() AT TIME ZONE 'utc') - interval '90 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000031'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000003'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000000031'::uuid, 'e6000000-0000-4000-8000-000000000031'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000032'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000004'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000000032'::uuid, 'e6000000-0000-4000-8000-000000000032'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000039'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000008'::uuid, 'Expired', CURRENT_DATE - 60, CURRENT_DATE - 5, 'AutoExpire', (NOW() AT TIME ZONE 'utc') - interval '5 days', (NOW() AT TIME ZONE 'utc') - interval '60 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000031'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000003'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000000031'::uuid, 'e7000000-0000-4000-8000-000000000031'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000032'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000004'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000000032'::uuid, 'e7000000-0000-4000-8000-000000000032'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000039'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000007'::uuid, 'Database', 'Expired', CURRENT_DATE - 45, CURRENT_DATE - 3, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '3 days', (NOW() AT TIME ZONE 'utc') - interval '45 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplements ("Id", "UserId", "Name", "NormalizedName", "CreatedAtUtc")
VALUES ('e7200000-0000-4000-8000-000000000003'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'Morning stack 3', 'morning stack 3', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplement_items ("Id", "OwnSupplementId", "SupplementId", "SortOrder", "CreatedAtUtc")
VALUES ('e7210000-0000-4000-8000-000000000003'::uuid, 'e7200000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 1, (NOW() AT TIME ZONE 'utc')),
       ('e7210000-0000-4000-8000-000000000103'::uuid, 'e7200000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 2, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000031'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'WeeklyHealthDataReminder', 'MedicalData', 'SharedRegisteredClient', 'Update your health diary', 'Review medications and symptoms this week.', 'Review medications and symptoms this week.', 'seed.b1010000-0000-4000-8000-000000000003.1', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000032'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'SupervisionRequest', 'DoctorPatient', 'PatientOnly', 'Supervision request pending', 'A healthcare provider requested access to your profile.', 'A healthcare provider requested access to your profile.', 'seed.b1010000-0000-4000-8000-000000000003.2', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000033'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'MedicalChangeProposal', 'DoctorPatient', 'PatientOnly', 'Medical change proposal', 'Your doctor proposed an update to your medication list.', 'Your doctor proposed an update to your medication list.', 'seed.b1010000-0000-4000-8000-000000000003.3', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000034'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'SystemAnnouncement', 'SystemInformation', 'SharedRegisteredClient', 'Platform maintenance', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'seed.b1010000-0000-4000-8000-000000000003.4', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- User fixture 4 (b1010000-0000-4000-8000-000000000004)
INSERT INTO "ClientAllergySummaries" ("Id", "UserId", "SevereReactionStatus", "CarriesAllergyMedication", "PollenDustMitesAllergyStatus", "InsectVenomAllergyStatus", "LatexAllergyStatus", "CreatedAtUtc")
SELECT 'e3000000-0000-4000-8000-000000000004'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'No', TRUE, 'DontKnow', 'Yes', 'Yes', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM "ClientAllergySummaries" WHERE "UserId" = 'b1010000-0000-4000-8000-000000000004'::uuid);
INSERT INTO client_reproductive_health_profiles ("Id", "UserId", "CurrentlyPregnant", "PlanningToGetPregnant", "Menopause", "ContraceptionMethodCode", "CreatedAtUtc")
SELECT 'e4000000-0000-4000-8000-000000000004'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, FALSE, FALSE, FALSE, 'implant', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM client_reproductive_health_profiles WHERE "UserId" = 'b1010000-0000-4000-8000-000000000004'::uuid);
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "DrugFormId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e0000000-0000-4000-8000-000000000041'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'a1000000-0000-4000-8000-000000000004'::uuid, 'Database', 'Rash after catalog drug form.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "CustomDrugName", "CustomDrugNameNormalized", "CustomActiveSubstances", "CustomFormulationName", "CustomDosage", "Source", "Note", "CreatedAtUtc") VALUES (
    'e0000000-0000-4000-8000-000000000042'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid,
    'Homeopathic Arnica 4', 'homeopathic arnica 4', 'Arnica montana', 'Drops', '30C',
    'Custom', 'Custom remedy reaction.', (NOW() AT TIME ZONE 'utc')
) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "FoodId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000000041'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'b0000000-0000-4000-8000-000000000004'::uuid, 'Database', 'Oral tingling from catalog food.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "CustomFoodName", "CustomFoodNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000000042'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'Artisan honeycomb 4', 'artisan honeycomb 4', 'Custom', 'Local market sample caused lip swelling.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "SupplementId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000000041'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000004'::uuid, 'Database', 'Itching after supplement.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "CustomSupplementName", "CustomSupplementNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000000042'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'Herbal blend 4', 'herbal blend 4', 'Custom', 'Loose tea mix caused hives.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientOtherAllergies" ("Id", "UserId", "AllergyName", "AllergyNameNormalized", "Note", "CreatedAtUtc") VALUES
    ('e2500000-0000-4000-8000-000000000004'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'Nickel jewelry 4', 'nickel jewelry 4', 'Contact dermatitis from watch buckle.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientMealTimes" ("Id", "UserId", "MealName", "NormalizedMealName", "IntakeTime", "CreatedAtUtc")
VALUES
    ('e8000000-0000-4000-8000-000000000041'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'Breakfast', 'breakfast', '07:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000042'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'Lunch', 'lunch', '12:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000043'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'Dinner', 'dinner', '18:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000044'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'Evening snack', 'evening snack', '20:30'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000041'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'a0000000-0000-4000-8000-000000000004'::uuid, 'a1000000-0000-4000-8000-000000000004'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000000041'::uuid, 'e5000000-0000-4000-8000-000000000041'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000042'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'a0000000-0000-4000-8000-000000000007'::uuid, 'a1000000-0000-4000-8000-000000000007'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000000042'::uuid, 'e5000000-0000-4000-8000-000000000042'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000049'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'a0000000-0000-4000-8000-000000000009'::uuid, 'a1000000-0000-4000-8000-000000000009'::uuid, 'Expired', CURRENT_DATE - 90, CURRENT_DATE - 10, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '10 days', (NOW() AT TIME ZONE 'utc') - interval '90 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000041'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'b0000000-0000-4000-8000-000000000004'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000000041'::uuid, 'e6000000-0000-4000-8000-000000000041'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000042'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'b0000000-0000-4000-8000-000000000005'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000000042'::uuid, 'e6000000-0000-4000-8000-000000000042'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000049'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'b0000000-0000-4000-8000-000000000009'::uuid, 'Expired', CURRENT_DATE - 60, CURRENT_DATE - 5, 'AutoExpire', (NOW() AT TIME ZONE 'utc') - interval '5 days', (NOW() AT TIME ZONE 'utc') - interval '60 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000041'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000004'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000000041'::uuid, 'e7000000-0000-4000-8000-000000000041'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000042'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000000042'::uuid, 'e7000000-0000-4000-8000-000000000042'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000049'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000008'::uuid, 'Database', 'Expired', CURRENT_DATE - 45, CURRENT_DATE - 3, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '3 days', (NOW() AT TIME ZONE 'utc') - interval '45 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplements ("Id", "UserId", "Name", "NormalizedName", "CreatedAtUtc")
VALUES ('e7200000-0000-4000-8000-000000000004'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'Morning stack 4', 'morning stack 4', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplement_items ("Id", "OwnSupplementId", "SupplementId", "SortOrder", "CreatedAtUtc")
VALUES ('e7210000-0000-4000-8000-000000000004'::uuid, 'e7200000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 1, (NOW() AT TIME ZONE 'utc')),
       ('e7210000-0000-4000-8000-000000000104'::uuid, 'e7200000-0000-4000-8000-000000000004'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 2, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000041'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'WeeklyHealthDataReminder', 'MedicalData', 'SharedRegisteredClient', 'Update your health diary', 'Review medications and symptoms this week.', 'Review medications and symptoms this week.', 'seed.b1010000-0000-4000-8000-000000000004.1', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000042'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'SupervisionRequest', 'DoctorPatient', 'PatientOnly', 'Supervision request pending', 'A healthcare provider requested access to your profile.', 'A healthcare provider requested access to your profile.', 'seed.b1010000-0000-4000-8000-000000000004.2', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000043'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'MedicalChangeProposal', 'DoctorPatient', 'PatientOnly', 'Medical change proposal', 'Your doctor proposed an update to your medication list.', 'Your doctor proposed an update to your medication list.', 'seed.b1010000-0000-4000-8000-000000000004.3', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000044'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'SystemAnnouncement', 'SystemInformation', 'SharedRegisteredClient', 'Platform maintenance', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'seed.b1010000-0000-4000-8000-000000000004.4', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- User fixture 5 (b1010000-0000-4000-8000-000000000005)
INSERT INTO "ClientAllergySummaries" ("Id", "UserId", "SevereReactionStatus", "CarriesAllergyMedication", "PollenDustMitesAllergyStatus", "InsectVenomAllergyStatus", "LatexAllergyStatus", "CreatedAtUtc")
SELECT 'e3000000-0000-4000-8000-000000000005'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'DontKnow', FALSE, 'Yes', 'No', 'No', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM "ClientAllergySummaries" WHERE "UserId" = 'b1010000-0000-4000-8000-000000000005'::uuid);
INSERT INTO client_reproductive_health_profiles ("Id", "UserId", "CurrentlyPregnant", "PlanningToGetPregnant", "Menopause", "ContraceptionMethodCode", "CreatedAtUtc")
SELECT 'e4000000-0000-4000-8000-000000000005'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, FALSE, FALSE, FALSE, 'oral_pill', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM client_reproductive_health_profiles WHERE "UserId" = 'b1010000-0000-4000-8000-000000000005'::uuid);
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "DrugFormId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e0000000-0000-4000-8000-000000000051'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'a1000000-0000-4000-8000-000000000005'::uuid, 'Database', 'Rash after catalog drug form.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "CustomDrugName", "CustomDrugNameNormalized", "CustomActiveSubstances", "CustomFormulationName", "CustomDosage", "Source", "Note", "CreatedAtUtc") VALUES (
    'e0000000-0000-4000-8000-000000000052'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid,
    'Homeopathic Arnica 5', 'homeopathic arnica 5', 'Arnica montana', 'Drops', '30C',
    'Custom', 'Custom remedy reaction.', (NOW() AT TIME ZONE 'utc')
) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "FoodId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000000051'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'b0000000-0000-4000-8000-000000000005'::uuid, 'Database', 'Oral tingling from catalog food.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "CustomFoodName", "CustomFoodNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000000052'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'Artisan honeycomb 5', 'artisan honeycomb 5', 'Custom', 'Local market sample caused lip swelling.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "SupplementId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000000051'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 'Database', 'Itching after supplement.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "CustomSupplementName", "CustomSupplementNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000000052'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'Herbal blend 5', 'herbal blend 5', 'Custom', 'Loose tea mix caused hives.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientOtherAllergies" ("Id", "UserId", "AllergyName", "AllergyNameNormalized", "Note", "CreatedAtUtc") VALUES
    ('e2500000-0000-4000-8000-000000000005'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'Nickel jewelry 5', 'nickel jewelry 5', 'Contact dermatitis from watch buckle.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientMealTimes" ("Id", "UserId", "MealName", "NormalizedMealName", "IntakeTime", "CreatedAtUtc")
VALUES
    ('e8000000-0000-4000-8000-000000000051'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'Breakfast', 'breakfast', '07:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000052'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'Lunch', 'lunch', '12:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000053'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'Dinner', 'dinner', '18:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000000054'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'Evening snack', 'evening snack', '20:30'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000051'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'a0000000-0000-4000-8000-000000000005'::uuid, 'a1000000-0000-4000-8000-000000000005'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000000051'::uuid, 'e5000000-0000-4000-8000-000000000051'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000052'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'a0000000-0000-4000-8000-000000000008'::uuid, 'a1000000-0000-4000-8000-000000000008'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000000052'::uuid, 'e5000000-0000-4000-8000-000000000052'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000000059'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'a0000000-0000-4000-8000-000000000010'::uuid, 'a1000000-0000-4000-8000-000000000010'::uuid, 'Expired', CURRENT_DATE - 90, CURRENT_DATE - 10, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '10 days', (NOW() AT TIME ZONE 'utc') - interval '90 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000051'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'b0000000-0000-4000-8000-000000000005'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000000051'::uuid, 'e6000000-0000-4000-8000-000000000051'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000052'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'b0000000-0000-4000-8000-000000000006'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000000052'::uuid, 'e6000000-0000-4000-8000-000000000052'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000000059'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'b0000000-0000-4000-8000-000000000010'::uuid, 'Expired', CURRENT_DATE - 60, CURRENT_DATE - 5, 'AutoExpire', (NOW() AT TIME ZONE 'utc') - interval '5 days', (NOW() AT TIME ZONE 'utc') - interval '60 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000051'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000000051'::uuid, 'e7000000-0000-4000-8000-000000000051'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000052'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'c0000000-0000-4000-8000-000000000006'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000000052'::uuid, 'e7000000-0000-4000-8000-000000000052'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000000059'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'c0000000-0000-4000-8000-000000000009'::uuid, 'Database', 'Expired', CURRENT_DATE - 45, CURRENT_DATE - 3, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '3 days', (NOW() AT TIME ZONE 'utc') - interval '45 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplements ("Id", "UserId", "Name", "NormalizedName", "CreatedAtUtc")
VALUES ('e7200000-0000-4000-8000-000000000005'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'Morning stack 5', 'morning stack 5', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplement_items ("Id", "OwnSupplementId", "SupplementId", "SortOrder", "CreatedAtUtc")
VALUES ('e7210000-0000-4000-8000-000000000005'::uuid, 'e7200000-0000-4000-8000-000000000005'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 1, (NOW() AT TIME ZONE 'utc')),
       ('e7210000-0000-4000-8000-000000000105'::uuid, 'e7200000-0000-4000-8000-000000000005'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 2, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000051'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'WeeklyHealthDataReminder', 'MedicalData', 'SharedRegisteredClient', 'Update your health diary', 'Review medications and symptoms this week.', 'Review medications and symptoms this week.', 'seed.b1010000-0000-4000-8000-000000000005.1', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000052'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'SupervisionRequest', 'DoctorPatient', 'PatientOnly', 'Supervision request pending', 'A healthcare provider requested access to your profile.', 'A healthcare provider requested access to your profile.', 'seed.b1010000-0000-4000-8000-000000000005.2', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000053'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'MedicalChangeProposal', 'DoctorPatient', 'PatientOnly', 'Medical change proposal', 'Your doctor proposed an update to your medication list.', 'Your doctor proposed an update to your medication list.', 'seed.b1010000-0000-4000-8000-000000000005.3', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000000054'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'SystemAnnouncement', 'SystemInformation', 'SharedRegisteredClient', 'Platform maintenance', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'seed.b1010000-0000-4000-8000-000000000005.4', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- User fixture 101 (b2010000-0000-4000-8000-000000000001)
INSERT INTO "ClientAllergySummaries" ("Id", "UserId", "SevereReactionStatus", "CarriesAllergyMedication", "PollenDustMitesAllergyStatus", "InsectVenomAllergyStatus", "LatexAllergyStatus", "CreatedAtUtc")
SELECT 'e3000000-0000-4000-8000-000000000101'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'DontKnow', FALSE, 'Yes', 'No', 'No', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM "ClientAllergySummaries" WHERE "UserId" = 'b2010000-0000-4000-8000-000000000001'::uuid);
INSERT INTO client_reproductive_health_profiles ("Id", "UserId", "CurrentlyPregnant", "PlanningToGetPregnant", "Menopause", "ContraceptionMethodCode", "CreatedAtUtc")
SELECT 'e4000000-0000-4000-8000-000000000101'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, FALSE, FALSE, FALSE, 'iud', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM client_reproductive_health_profiles WHERE "UserId" = 'b2010000-0000-4000-8000-000000000001'::uuid);
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "DrugFormId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e0000000-0000-4000-8000-000000001011'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'a1000000-0000-4000-8000-000000000001'::uuid, 'Database', 'Rash after catalog drug form.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "CustomDrugName", "CustomDrugNameNormalized", "CustomActiveSubstances", "CustomFormulationName", "CustomDosage", "Source", "Note", "CreatedAtUtc") VALUES (
    'e0000000-0000-4000-8000-000000001012'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid,
    'Homeopathic Arnica 101', 'homeopathic arnica 101', 'Arnica montana', 'Drops', '30C',
    'Custom', 'Custom remedy reaction.', (NOW() AT TIME ZONE 'utc')
) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "FoodId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000001011'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000001'::uuid, 'Database', 'Oral tingling from catalog food.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "CustomFoodName", "CustomFoodNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000001012'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'Artisan honeycomb 101', 'artisan honeycomb 101', 'Custom', 'Local market sample caused lip swelling.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "SupplementId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000001011'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 'Database', 'Itching after supplement.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "CustomSupplementName", "CustomSupplementNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000001012'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'Herbal blend 101', 'herbal blend 101', 'Custom', 'Loose tea mix caused hives.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientOtherAllergies" ("Id", "UserId", "AllergyName", "AllergyNameNormalized", "Note", "CreatedAtUtc") VALUES
    ('e2500000-0000-4000-8000-000000000101'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'Nickel jewelry 101', 'nickel jewelry 101', 'Contact dermatitis from watch buckle.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientMealTimes" ("Id", "UserId", "MealName", "NormalizedMealName", "IntakeTime", "CreatedAtUtc")
VALUES
    ('e8000000-0000-4000-8000-000000001011'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'Breakfast', 'breakfast', '07:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000001012'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'Lunch', 'lunch', '12:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000001013'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'Dinner', 'dinner', '18:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000001014'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'Evening snack', 'evening snack', '20:30'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000001011'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'a0000000-0000-4000-8000-000000000001'::uuid, 'a1000000-0000-4000-8000-000000000001'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000001011'::uuid, 'e5000000-0000-4000-8000-000000001011'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000001012'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'a0000000-0000-4000-8000-000000000004'::uuid, 'a1000000-0000-4000-8000-000000000004'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000001012'::uuid, 'e5000000-0000-4000-8000-000000001012'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000001019'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'a0000000-0000-4000-8000-000000000006'::uuid, 'a1000000-0000-4000-8000-000000000006'::uuid, 'Expired', CURRENT_DATE - 90, CURRENT_DATE - 10, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '10 days', (NOW() AT TIME ZONE 'utc') - interval '90 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000001011'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000001'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000001011'::uuid, 'e6000000-0000-4000-8000-000000001011'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000001012'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000002'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000001012'::uuid, 'e6000000-0000-4000-8000-000000001012'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000001019'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'b0000000-0000-4000-8000-000000000006'::uuid, 'Expired', CURRENT_DATE - 60, CURRENT_DATE - 5, 'AutoExpire', (NOW() AT TIME ZONE 'utc') - interval '5 days', (NOW() AT TIME ZONE 'utc') - interval '60 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000001011'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000001011'::uuid, 'e7000000-0000-4000-8000-000000001011'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000001012'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000002'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000001012'::uuid, 'e7000000-0000-4000-8000-000000001012'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000001019'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 'Database', 'Expired', CURRENT_DATE - 45, CURRENT_DATE - 3, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '3 days', (NOW() AT TIME ZONE 'utc') - interval '45 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplements ("Id", "UserId", "Name", "NormalizedName", "CreatedAtUtc")
VALUES ('e7200000-0000-4000-8000-000000000101'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'Morning stack 101', 'morning stack 101', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplement_items ("Id", "OwnSupplementId", "SupplementId", "SortOrder", "CreatedAtUtc")
VALUES ('e7210000-0000-4000-8000-000000000101'::uuid, 'e7200000-0000-4000-8000-000000000101'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 1, (NOW() AT TIME ZONE 'utc')),
       ('e7210000-0000-4000-8000-000000000201'::uuid, 'e7200000-0000-4000-8000-000000000101'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 2, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001011'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'WeeklyHealthDataReminder', 'MedicalData', 'SharedRegisteredClient', 'Update your health diary', 'Review medications and symptoms this week.', 'Review medications and symptoms this week.', 'seed.b2010000-0000-4000-8000-000000000001.1', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001012'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'SupervisionRequest', 'DoctorPatient', 'PatientOnly', 'Supervision request pending', 'A healthcare provider requested access to your profile.', 'A healthcare provider requested access to your profile.', 'seed.b2010000-0000-4000-8000-000000000001.2', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001013'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'MedicalChangeProposal', 'DoctorPatient', 'PatientOnly', 'Medical change proposal', 'Your doctor proposed an update to your medication list.', 'Your doctor proposed an update to your medication list.', 'seed.b2010000-0000-4000-8000-000000000001.3', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001014'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'SystemAnnouncement', 'SystemInformation', 'SharedRegisteredClient', 'Platform maintenance', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'seed.b2010000-0000-4000-8000-000000000001.4', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- User fixture 102 (b2010000-0000-4000-8000-000000000002)
INSERT INTO "ClientAllergySummaries" ("Id", "UserId", "SevereReactionStatus", "CarriesAllergyMedication", "PollenDustMitesAllergyStatus", "InsectVenomAllergyStatus", "LatexAllergyStatus", "CreatedAtUtc")
SELECT 'e3000000-0000-4000-8000-000000000102'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'Yes', TRUE, 'No', 'DontKnow', 'Yes', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM "ClientAllergySummaries" WHERE "UserId" = 'b2010000-0000-4000-8000-000000000002'::uuid);
INSERT INTO client_reproductive_health_profiles ("Id", "UserId", "CurrentlyPregnant", "PlanningToGetPregnant", "Menopause", "ContraceptionMethodCode", "CreatedAtUtc")
SELECT 'e4000000-0000-4000-8000-000000000102'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, FALSE, FALSE, FALSE, 'none', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM client_reproductive_health_profiles WHERE "UserId" = 'b2010000-0000-4000-8000-000000000002'::uuid);
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "DrugFormId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e0000000-0000-4000-8000-000000001021'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'a1000000-0000-4000-8000-000000000002'::uuid, 'Database', 'Rash after catalog drug form.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "CustomDrugName", "CustomDrugNameNormalized", "CustomActiveSubstances", "CustomFormulationName", "CustomDosage", "Source", "Note", "CreatedAtUtc") VALUES (
    'e0000000-0000-4000-8000-000000001022'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid,
    'Homeopathic Arnica 102', 'homeopathic arnica 102', 'Arnica montana', 'Drops', '30C',
    'Custom', 'Custom remedy reaction.', (NOW() AT TIME ZONE 'utc')
) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "FoodId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000001021'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000002'::uuid, 'Database', 'Oral tingling from catalog food.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "CustomFoodName", "CustomFoodNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000001022'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'Artisan honeycomb 102', 'artisan honeycomb 102', 'Custom', 'Local market sample caused lip swelling.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "SupplementId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000001021'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000002'::uuid, 'Database', 'Itching after supplement.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "CustomSupplementName", "CustomSupplementNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000001022'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'Herbal blend 102', 'herbal blend 102', 'Custom', 'Loose tea mix caused hives.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientOtherAllergies" ("Id", "UserId", "AllergyName", "AllergyNameNormalized", "Note", "CreatedAtUtc") VALUES
    ('e2500000-0000-4000-8000-000000000102'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'Nickel jewelry 102', 'nickel jewelry 102', 'Contact dermatitis from watch buckle.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientMealTimes" ("Id", "UserId", "MealName", "NormalizedMealName", "IntakeTime", "CreatedAtUtc")
VALUES
    ('e8000000-0000-4000-8000-000000001021'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'Breakfast', 'breakfast', '07:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000001022'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'Lunch', 'lunch', '12:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000001023'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'Dinner', 'dinner', '18:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000001024'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'Evening snack', 'evening snack', '20:30'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000001021'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'a0000000-0000-4000-8000-000000000002'::uuid, 'a1000000-0000-4000-8000-000000000002'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000001021'::uuid, 'e5000000-0000-4000-8000-000000001021'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000001022'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'a0000000-0000-4000-8000-000000000005'::uuid, 'a1000000-0000-4000-8000-000000000005'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000001022'::uuid, 'e5000000-0000-4000-8000-000000001022'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000001029'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'a0000000-0000-4000-8000-000000000007'::uuid, 'a1000000-0000-4000-8000-000000000007'::uuid, 'Expired', CURRENT_DATE - 90, CURRENT_DATE - 10, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '10 days', (NOW() AT TIME ZONE 'utc') - interval '90 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000001021'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000002'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000001021'::uuid, 'e6000000-0000-4000-8000-000000001021'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000001022'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000003'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000001022'::uuid, 'e6000000-0000-4000-8000-000000001022'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000001029'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'b0000000-0000-4000-8000-000000000007'::uuid, 'Expired', CURRENT_DATE - 60, CURRENT_DATE - 5, 'AutoExpire', (NOW() AT TIME ZONE 'utc') - interval '5 days', (NOW() AT TIME ZONE 'utc') - interval '60 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000001021'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000002'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000001021'::uuid, 'e7000000-0000-4000-8000-000000001021'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000001022'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000003'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000001022'::uuid, 'e7000000-0000-4000-8000-000000001022'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000001029'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'c0000000-0000-4000-8000-000000000006'::uuid, 'Database', 'Expired', CURRENT_DATE - 45, CURRENT_DATE - 3, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '3 days', (NOW() AT TIME ZONE 'utc') - interval '45 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplements ("Id", "UserId", "Name", "NormalizedName", "CreatedAtUtc")
VALUES ('e7200000-0000-4000-8000-000000000102'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'Morning stack 102', 'morning stack 102', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplement_items ("Id", "OwnSupplementId", "SupplementId", "SortOrder", "CreatedAtUtc")
VALUES ('e7210000-0000-4000-8000-000000000102'::uuid, 'e7200000-0000-4000-8000-000000000102'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 1, (NOW() AT TIME ZONE 'utc')),
       ('e7210000-0000-4000-8000-000000000202'::uuid, 'e7200000-0000-4000-8000-000000000102'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 2, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001021'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'WeeklyHealthDataReminder', 'MedicalData', 'SharedRegisteredClient', 'Update your health diary', 'Review medications and symptoms this week.', 'Review medications and symptoms this week.', 'seed.b2010000-0000-4000-8000-000000000002.1', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001022'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'SupervisionRequest', 'DoctorPatient', 'PatientOnly', 'Supervision request pending', 'A healthcare provider requested access to your profile.', 'A healthcare provider requested access to your profile.', 'seed.b2010000-0000-4000-8000-000000000002.2', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001023'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'MedicalChangeProposal', 'DoctorPatient', 'PatientOnly', 'Medical change proposal', 'Your doctor proposed an update to your medication list.', 'Your doctor proposed an update to your medication list.', 'seed.b2010000-0000-4000-8000-000000000002.3', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001024'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'SystemAnnouncement', 'SystemInformation', 'SharedRegisteredClient', 'Platform maintenance', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'seed.b2010000-0000-4000-8000-000000000002.4', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- User fixture 103 (b2010000-0000-4000-8000-000000000003)
INSERT INTO "ClientAllergySummaries" ("Id", "UserId", "SevereReactionStatus", "CarriesAllergyMedication", "PollenDustMitesAllergyStatus", "InsectVenomAllergyStatus", "LatexAllergyStatus", "CreatedAtUtc")
SELECT 'e3000000-0000-4000-8000-000000000103'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'No', FALSE, 'DontKnow', 'Yes', 'No', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM "ClientAllergySummaries" WHERE "UserId" = 'b2010000-0000-4000-8000-000000000003'::uuid);
INSERT INTO client_reproductive_health_profiles ("Id", "UserId", "CurrentlyPregnant", "PlanningToGetPregnant", "Menopause", "ContraceptionMethodCode", "CreatedAtUtc")
SELECT 'e4000000-0000-4000-8000-000000000103'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, FALSE, FALSE, FALSE, 'barrier_method', (NOW() AT TIME ZONE 'utc')
WHERE NOT EXISTS (SELECT 1 FROM client_reproductive_health_profiles WHERE "UserId" = 'b2010000-0000-4000-8000-000000000003'::uuid);
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "DrugFormId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e0000000-0000-4000-8000-000000001031'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'a1000000-0000-4000-8000-000000000003'::uuid, 'Database', 'Rash after catalog drug form.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientDrugAllergies" ("Id", "UserId", "CustomDrugName", "CustomDrugNameNormalized", "CustomActiveSubstances", "CustomFormulationName", "CustomDosage", "Source", "Note", "CreatedAtUtc") VALUES (
    'e0000000-0000-4000-8000-000000001032'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid,
    'Homeopathic Arnica 103', 'homeopathic arnica 103', 'Arnica montana', 'Drops', '30C',
    'Custom', 'Custom remedy reaction.', (NOW() AT TIME ZONE 'utc')
) ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "FoodId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000001031'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000003'::uuid, 'Database', 'Oral tingling from catalog food.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientFoodAllergies" ("Id", "UserId", "CustomFoodName", "CustomFoodNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e2000000-0000-4000-8000-000000001032'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'Artisan honeycomb 103', 'artisan honeycomb 103', 'Custom', 'Local market sample caused lip swelling.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "SupplementId", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000001031'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000003'::uuid, 'Database', 'Itching after supplement.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientSupplementAllergies" ("Id", "UserId", "CustomSupplementName", "CustomSupplementNameNormalized", "Source", "Note", "CreatedAtUtc") VALUES
    ('e1000000-0000-4000-8000-000000001032'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'Herbal blend 103', 'herbal blend 103', 'Custom', 'Loose tea mix caused hives.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientOtherAllergies" ("Id", "UserId", "AllergyName", "AllergyNameNormalized", "Note", "CreatedAtUtc") VALUES
    ('e2500000-0000-4000-8000-000000000103'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'Nickel jewelry 103', 'nickel jewelry 103', 'Contact dermatitis from watch buckle.', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "ClientMealTimes" ("Id", "UserId", "MealName", "NormalizedMealName", "IntakeTime", "CreatedAtUtc")
VALUES
    ('e8000000-0000-4000-8000-000000001031'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'Breakfast', 'breakfast', '07:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000001032'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'Lunch', 'lunch', '12:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000001033'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'Dinner', 'dinner', '18:30'::time, (NOW() AT TIME ZONE 'utc')),
    ('e8000000-0000-4000-8000-000000001034'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'Evening snack', 'evening snack', '20:30'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000001031'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'a0000000-0000-4000-8000-000000000003'::uuid, 'a1000000-0000-4000-8000-000000000003'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000001031'::uuid, 'e5000000-0000-4000-8000-000000001031'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000001032'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'a0000000-0000-4000-8000-000000000006'::uuid, 'a1000000-0000-4000-8000-000000000006'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drug_intake_times ("Id", "RegularDrugId", "IntakeTime", "CreatedAtUtc")
VALUES ('e5100000-0000-4000-8000-000000001032'::uuid, 'e5000000-0000-4000-8000-000000001032'::uuid, '08:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_drugs ("Id", "UserId", "DrugId", "DrugFormId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e5000000-0000-4000-8000-000000001039'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'a0000000-0000-4000-8000-000000000008'::uuid, 'a1000000-0000-4000-8000-000000000008'::uuid, 'Expired', CURRENT_DATE - 90, CURRENT_DATE - 10, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '10 days', (NOW() AT TIME ZONE 'utc') - interval '90 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000001031'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000003'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000001031'::uuid, 'e6000000-0000-4000-8000-000000001031'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000001032'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000004'::uuid, 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_food_intake_times ("Id", "RegularFoodId", "IntakeTime", "CreatedAtUtc")
VALUES ('e6100000-0000-4000-8000-000000001032'::uuid, 'e6000000-0000-4000-8000-000000001032'::uuid, '13:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_foods ("Id", "UserId", "FoodId", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e6000000-0000-4000-8000-000000001039'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'b0000000-0000-4000-8000-000000000008'::uuid, 'Expired', CURRENT_DATE - 60, CURRENT_DATE - 5, 'AutoExpire', (NOW() AT TIME ZONE 'utc') - interval '5 days', (NOW() AT TIME ZONE 'utc') - interval '60 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000001031'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000003'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000001031'::uuid, 'e7000000-0000-4000-8000-000000001031'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000001032'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000004'::uuid, 'Database', 'Active', CURRENT_DATE, NULL, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplement_intake_times ("Id", "RegularSupplementId", "IntakeTime", "CreatedAtUtc")
VALUES ('e7100000-0000-4000-8000-000000001032'::uuid, 'e7000000-0000-4000-8000-000000001032'::uuid, '09:00:00'::time, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_regular_supplements ("Id", "UserId", "SupplementId", "Source", "Status", "StartDate", "EndDate", "HistoryReason", "ArchivedAtUtc", "CreatedAtUtc")
VALUES ('e7000000-0000-4000-8000-000000001039'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'c0000000-0000-4000-8000-000000000007'::uuid, 'Database', 'Expired', CURRENT_DATE - 45, CURRENT_DATE - 3, 'ManualRemove', (NOW() AT TIME ZONE 'utc') - interval '3 days', (NOW() AT TIME ZONE 'utc') - interval '45 days')
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplements ("Id", "UserId", "Name", "NormalizedName", "CreatedAtUtc")
VALUES ('e7200000-0000-4000-8000-000000000103'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'Morning stack 103', 'morning stack 103', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO client_own_supplement_items ("Id", "OwnSupplementId", "SupplementId", "SortOrder", "CreatedAtUtc")
VALUES ('e7210000-0000-4000-8000-000000000103'::uuid, 'e7200000-0000-4000-8000-000000000103'::uuid, 'c0000000-0000-4000-8000-000000000001'::uuid, 1, (NOW() AT TIME ZONE 'utc')),
       ('e7210000-0000-4000-8000-000000000203'::uuid, 'e7200000-0000-4000-8000-000000000103'::uuid, 'c0000000-0000-4000-8000-000000000005'::uuid, 2, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001031'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'WeeklyHealthDataReminder', 'MedicalData', 'SharedRegisteredClient', 'Update your health diary', 'Review medications and symptoms this week.', 'Review medications and symptoms this week.', 'seed.b2010000-0000-4000-8000-000000000003.1', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001032'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'SupervisionRequest', 'DoctorPatient', 'PatientOnly', 'Supervision request pending', 'A healthcare provider requested access to your profile.', 'A healthcare provider requested access to your profile.', 'seed.b2010000-0000-4000-8000-000000000003.2', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001033'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'MedicalChangeProposal', 'DoctorPatient', 'PatientOnly', 'Medical change proposal', 'Your doctor proposed an update to your medication list.', 'Your doctor proposed an update to your medication list.', 'seed.b2010000-0000-4000-8000-000000000003.3', TRUE, 'Pending', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserNotifications" ("Id", "UserId", "TypeCode", "PrimaryTagCode", "AudienceScopeCode", "Title", "SummaryText", "BodyText", "SourceEventKey", "RequiresAction", "ActionState", "IsRead", "CreatedAtUtc")
VALUES ('b4010000-0000-4000-8000-000000001034'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'SystemAnnouncement', 'SystemInformation', 'SharedRegisteredClient', 'Platform maintenance', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'Scheduled maintenance this Sunday 02:00-04:00 UTC.', 'seed.b2010000-0000-4000-8000-000000000003.4', FALSE, 'Informational', FALSE, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- Supervision: pending requests (2)
INSERT INTO supervision_requests ("Id", patient_user_id, healthcare_professional_user_id, request_state, requested_at_utc, expires_at_utc, "CreatedAtUtc")
VALUES
    ('ab010000-0000-4000-8000-000000000001'::uuid, 'b1010000-0000-4000-8000-000000000005'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'pending', (NOW() AT TIME ZONE 'utc') - interval '1 day', (NOW() AT TIME ZONE 'utc') + interval '6 days', (NOW() AT TIME ZONE 'utc')),
    ('ab010000-0000-4000-8000-000000000002'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'pending', (NOW() AT TIME ZONE 'utc') - interval '2 hours', (NOW() AT TIME ZONE 'utc') + interval '7 days', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- Supervision: active connections (7)
INSERT INTO supervision_connections ("Id", patient_user_id, healthcare_professional_user_id, access_level_code, connection_state, approved_at_utc, "CreatedAtUtc")
VALUES ('ab020000-0000-4000-8000-000000000001'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'ViewAndModify', 'Active', (NOW() AT TIME ZONE 'utc') - interval '1 days', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO supervision_connections ("Id", patient_user_id, healthcare_professional_user_id, access_level_code, connection_state, approved_at_utc, "CreatedAtUtc")
VALUES ('ab020000-0000-4000-8000-000000000002'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'ViewOnly', 'Active', (NOW() AT TIME ZONE 'utc') - interval '2 days', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO supervision_connections ("Id", patient_user_id, healthcare_professional_user_id, access_level_code, connection_state, approved_at_utc, "CreatedAtUtc")
VALUES ('ab020000-0000-4000-8000-000000000003'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'RestrictedView', 'Active', (NOW() AT TIME ZONE 'utc') - interval '3 days', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO supervision_connections ("Id", patient_user_id, healthcare_professional_user_id, access_level_code, connection_state, approved_at_utc, "CreatedAtUtc")
VALUES ('ab020000-0000-4000-8000-000000000004'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'BlockAccess', 'Active', (NOW() AT TIME ZONE 'utc') - interval '4 days', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO supervision_connections ("Id", patient_user_id, healthcare_professional_user_id, access_level_code, connection_state, approved_at_utc, "CreatedAtUtc")
VALUES ('ab020000-0000-4000-8000-000000000005'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'ViewAndModify', 'Active', (NOW() AT TIME ZONE 'utc') - interval '5 days', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO supervision_connections ("Id", patient_user_id, healthcare_professional_user_id, access_level_code, connection_state, approved_at_utc, "CreatedAtUtc")
VALUES ('ab020000-0000-4000-8000-000000000006'::uuid, 'b1010000-0000-4000-8000-000000000003'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'ViewOnly', 'Active', (NOW() AT TIME ZONE 'utc') - interval '6 days', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO supervision_connections ("Id", patient_user_id, healthcare_professional_user_id, access_level_code, connection_state, approved_at_utc, "CreatedAtUtc")
VALUES ('ab020000-0000-4000-8000-000000000007'::uuid, 'b1010000-0000-4000-8000-000000000004'::uuid, 'b2010000-0000-4000-8000-000000000003'::uuid, 'RestrictedView', 'Active', (NOW() AT TIME ZONE 'utc') - interval '7 days', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;

-- Medical change proposals (3) with items (2 each)
INSERT INTO medical_change_proposals ("Id", supervision_connection_id, patient_user_id, healthcare_professional_user_id, proposal_state, access_level_snapshot, proposal_summary, submitted_at_utc, "CreatedAtUtc")
VALUES
    ('ab030000-0000-4000-8000-000000000001'::uuid, 'ab020000-0000-4000-8000-000000000001'::uuid, 'b1010000-0000-4000-8000-000000000001'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'Pending', 'ViewAndModify', 'Add evening Metformin dose and note fasting requirement.', (NOW() AT TIME ZONE 'utc') - interval '3 hours', (NOW() AT TIME ZONE 'utc')),
    ('ab030000-0000-4000-8000-000000000002'::uuid, 'ab020000-0000-4000-8000-000000000002'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'b2010000-0000-4000-8000-000000000001'::uuid, 'Approved', 'ViewOnly', 'Document seasonal antihistamine use.', (NOW() AT TIME ZONE 'utc') - interval '2 days', (NOW() AT TIME ZONE 'utc')),
    ('ab030000-0000-4000-8000-000000000003'::uuid, 'ab020000-0000-4000-8000-000000000005'::uuid, 'b1010000-0000-4000-8000-000000000002'::uuid, 'b2010000-0000-4000-8000-000000000002'::uuid, 'Rejected', 'ViewAndModify', 'Switch statin to alternate brand.', (NOW() AT TIME ZONE 'utc') - interval '5 days', (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO medical_change_proposal_items ("Id", proposal_id, category_code, source, related_catalog_id, display_name_snapshot, apply_payload, sort_order, "CreatedAtUtc")
VALUES
    ('ab031000-0000-4000-8000-000000000001'::uuid, 'ab030000-0000-4000-8000-000000000001'::uuid, 'RegularDrug', 'Database', 'a1000000-0000-4000-8000-000000000003'::uuid, 'Glucophage 500 mg', '{}'::jsonb, 1, (NOW() AT TIME ZONE 'utc')),
    ('ab031000-0000-4000-8000-000000000002'::uuid, 'ab030000-0000-4000-8000-000000000001'::uuid, 'RegularSupplement', 'Database', 'c0000000-0000-4000-8000-000000000004'::uuid, 'Magnesium', '{}'::jsonb, 2, (NOW() AT TIME ZONE 'utc')),
    ('ab031000-0000-4000-8000-000000000003'::uuid, 'ab030000-0000-4000-8000-000000000002'::uuid, 'HealthCondition', 'Database', 'd0000000-0000-4000-8000-000000000003'::uuid, 'Asthma', '{}'::jsonb, 1, (NOW() AT TIME ZONE 'utc')),
    ('ab031000-0000-4000-8000-000000000004'::uuid, 'ab030000-0000-4000-8000-000000000002'::uuid, 'RegularFood', 'Database', 'b0000000-0000-4000-8000-000000000016'::uuid, 'Coffee', '{}'::jsonb, 2, (NOW() AT TIME ZONE 'utc')),
    ('ab031000-0000-4000-8000-000000000005'::uuid, 'ab030000-0000-4000-8000-000000000003'::uuid, 'RegularDrug', 'Database', 'a1000000-0000-4000-8000-000000000012'::uuid, 'Simvacard 20 mg', '{}'::jsonb, 1, (NOW() AT TIME ZONE 'utc')),
    ('ab031000-0000-4000-8000-000000000006'::uuid, 'ab030000-0000-4000-8000-000000000003'::uuid, 'RegularDrug', 'Database', 'a1000000-0000-4000-8000-000000000004'::uuid, 'Atorvasterol 20 mg', '{}'::jsonb, 2, (NOW() AT TIME ZONE 'utc'))
ON CONFLICT ("Id") DO NOTHING;
