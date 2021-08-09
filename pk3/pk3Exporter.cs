using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace pkuManager.pk3
{
    public class pk3Exporter : Exporter
    {
        public pk3Exporter(PKUObject pku, GlobalFlags globalFlags) : base(pku, globalFlags) { }

        public override string formatName { get { return "Gen 3"; } }

        public override string formatExtension { get { return "pk3"; } }

        public override bool canExport()
        {
            // No Shadow Pokemon
            if (pkuUtil.IsShadow(pku))
                return false;

            // No Regional Variants
            if (pku.Form != null && (pku.Form.ToLowerInvariant() == "alolan" || pku.Form.ToLowerInvariant() == "galarian"))
                return false;

            // In Gen 3 Nat Dex
            int? dexTest = pkxUtil.GetNationalDex(pku.Species);
            if (dexTest.HasValue && dexTest <= 386) //Deoxys (386) and below
                return true;

            // Not In Gen 3 Nat Dex
            return false;
        }


        // Values
        //  - Decided in processAlerts(), encoded in toFile().
        private int dex;
        private uint id;
        private int friendship, item, location, game, metlevel, pkrsDays, pkrsStrain, abilitySlot;
        private int[] ppups, evs, ivs, contest, moves;
        private byte[] name, otName;
        private Language lang;
        private Gender otgender;
        private Ball ball;
        private HashSet<Ribbon> ribbons;

        // Value Resolvers:
        //  - Created in processAlerts(), decided and encoded in toFile()
        //  - Value of field is deferred to the toFile() step, because the user (possibly) has to pick a value using the error Alert.
        private pkxUtil.ProcessTags.ErrorResolver<uint> pidResolver;
        private pkxUtil.ProcessTags.ErrorResolver<uint> expResolver;
        private pkxUtil.ProcessTags.ErrorResolver<bool> fatefulEncounterResolver;

        protected override void processAlerts()
        {
            Alert tempAlert;
            dex = pkxUtil.GetNationalDex(pku.Species).Value;

            // ----------
            // Process Global Flags
            // ----------

            (pku, tempAlert) = pkxUtil.ProcessFlags.ProcessBattleStatOverride(pku, globalFlags);
            notes.Add(tempAlert);


            // ----------
            // Process Tags
            // ----------

            // Gender:
            //  - Implicit
            Gender? gender;
            (gender, tempAlert) = pk3Util.ProcessTags.ProcessGender(pku);
            warnings.Add(tempAlert);

            // Form:
            //  - Implicit
            int? unownForm;
            (unownForm, tempAlert) = pk3Util.ProcessTags.ProcessForm(pku);
            warnings.Add(tempAlert);

            // Nature:
            //  - Implicit
            Nature? nature;
            (nature, tempAlert) = pk3Util.ProcessTags.ProcessNature(pku);
            warnings.Add(tempAlert);

            // ID:
            (id, tempAlert) = pkxUtil.ProcessTags.ProcessID(pku);
            warnings.Add(tempAlert);

            // PID:
            //  - Can give error, so using ErrorResolver
            //  - Requires: Gender, Form, Nature, ID
            var temp = pkxUtil.ProcessTags.ProcessPID(pku, id, false, gender, nature, unownForm);
            pidResolver = new pkxUtil.ProcessTags.ErrorResolver<uint>(temp.Item1, temp.Item2, warnings, errors);

            // Language:
            (lang, tempAlert) = pkxUtil.ProcessTags.ProcessLanguage(pku, pk3Util.LANGUAGE_ENCODING.Values.ToArray());
            warnings.Add(tempAlert);

            // Nickname
            //  - Requires: Language
            (name, tempAlert) = pk3Util.ProcessTags.ProcessNickname(pku, lang);
            warnings.Add(tempAlert);

            // OT
            //  - Requires: Language
            (otName, tempAlert) = pk3Util.ProcessTags.ProcessOT(pku, lang);
            warnings.Add(tempAlert);

            // Trash Bytes
            (name, otName, tempAlert) = pk3Util.ProcessTags.ProcessTrash(pku, name, otName);
            warnings.Add(tempAlert);

            // Moves:
            int[] moveIndicies;
            (moves, moveIndicies, tempAlert) = pkxUtil.ProcessTags.ProcessMoves(pku, 354); //Psycho Boost (354)
            warnings.Add(tempAlert);

            // Item
            (item, tempAlert) = pkxUtil.ProcessTags.ProcessItem(pku, 3);
            warnings.Add(tempAlert);

            // Experience:
            //  - Can give error, so using ErrorResolver
            var temp2 = pkxUtil.ProcessTags.ProcessEXP(pku);
            expResolver = new pkxUtil.ProcessTags.ErrorResolver<uint>(temp2.Item1, temp2.Item2, warnings, errors);

            // PP-Ups
            //  - Requires: Moves
            (ppups, tempAlert) = pkxUtil.ProcessTags.ProcessPPUps(pku, moveIndicies);
            warnings.Add(tempAlert);

            // Friendship
            //  - Note: also used as step counter for eggs
            (friendship, tempAlert) = pkxUtil.ProcessTags.ProcessFriendship(pku);
            warnings.Add(tempAlert);

            // EVs
            (evs, tempAlert) = pkxUtil.ProcessTags.ProcessEVs(pku);
            warnings.Add(tempAlert);

            // Contest Stats
            (contest, tempAlert) = pkxUtil.ProcessTags.ProcessContest(pku);
            warnings.Add(tempAlert);

            // Pokérus
            (pkrsStrain, pkrsDays, tempAlert) = pkxUtil.ProcessTags.ProcessPokerus(pku);
            warnings.Add(tempAlert);

            // Origin Game:
            string checkedGameName;
            (game, checkedGameName, tempAlert) = pkxUtil.ProcessTags.ProcessOriginGame(pku);
            warnings.Add(tempAlert);

            // Met Location
            //  - Requires: Origin Game
            (location, tempAlert) = pkxUtil.ProcessTags.ProcessMetLocation(pku, checkedGameName, (g, l) => { return pk3Util.EncodeMetLocation(g, l); }, pk3Util.DEFAULT_LOCATION);
            warnings.Add(tempAlert);

            // Met Level
            (metlevel, tempAlert) = pkxUtil.ProcessTags.ProcessMetLevel(pku);
            warnings.Add(tempAlert);

            // Ball
            (ball, tempAlert) = pkxUtil.ProcessTags.ProcessBall(pku, Ball.Premier);
            warnings.Add(tempAlert);

            // OT Gender
            (otgender, tempAlert) = pkxUtil.ProcessTags.ProcessOTGender(pku);
            warnings.Add(tempAlert);

            // IVs
            (ivs, tempAlert) = pkxUtil.ProcessTags.ProcessIVs(pku);
            warnings.Add(tempAlert);

            // Ability
            (abilitySlot, tempAlert) = pk3Util.ProcessTags.ProcessAbility(pku);
            warnings.Add(tempAlert);

            // Ribbons
            (ribbons, tempAlert) = pk3Util.ProcessTags.ProcessRibbons(pku);
            warnings.Add(tempAlert);

            // Fateful Encounter
            //  - AKA obedience (for Mew and Deoxys) in Gen 3
            //  - Can give error, so using ErrorResolver
            var temp3 = pk3Util.ProcessTags.ProcessFatefulEncounter(pku);
            fatefulEncounterResolver = new pkxUtil.ProcessTags.ErrorResolver<bool>(temp3.Item1, temp3.Item2, warnings, errors);
        }

        private (byte[], uint) encodeSubstructure()
        {
            // G: Growth Block
            byte[] G = new byte[12];
            {
                // Species - G: bytes 0-1
                uint speciesID = (uint)PokeAPIUtil.GetSpeciesIndex(dex, 3);
                DataUtil.ShiftCopy(speciesID, G, 0, 2);

                // Item - G: bytes 2-3
                DataUtil.ShiftCopy(item, G, 2, 2);

                // Experience - G: bytes 4-7
                DataUtil.ShiftCopy(expResolver.DecideValue(), G, 4, 4);

                // PP Ups - G: byte 8
                uint ppupval = (uint)(ppups[3] << 6 | ppups[2] << 4 | ppups[1] << 2 | ppups[0]);
                DataUtil.ShiftCopy(ppupval, G, 8, 1);

                // Friendship - G: byte 9
                DataUtil.ShiftCopy(friendship, G, 9, 1);

                // "Unknown" - G: byte 10-11
                // Bulbapedia is vague here, but I suspect it's just padding.
            }

            // A: Attacks Block
            byte[] A = new byte[12];
            {
                for (int i = 0; i < 4; i++)
                {
                    DataUtil.ShiftCopy(moves[i], A, 2 * i, 2); // Move i - A: bytes (2i)-(2i+1)  (overall 0-7)
                    if (moves[i] != 0)
                        A[8 + i] = (byte)((5 + ppups[i]) * PokeAPIUtil.GetMoveBasePP((int)moves[i]) / 5); // PP i - A: byte 8+i  (overall 8-11)
                }
            }

            // E: EVs & Condition Block
            byte[] E = new byte[12];
            {
                // EVs are encoded in a different order than usual:
                E[0] = (byte)evs[0]; // HP: byte 0
                E[1] = (byte)evs[1]; // Attack: byte 1
                E[2] = (byte)evs[2]; // Defense: byte 2
                E[3] = (byte)evs[5]; // Speed: byte 3
                E[4] = (byte)evs[3]; // Sp. Attack: byte 4
                E[5] = (byte)evs[4]; // Sp. Defense: byte 5

                // Contest Stats are encoded in the normal order:
                // Cool, Beauty, Cute, Clever, Tough, Sheen
                for (int i = 0; i < 6; i++)
                    E[i + 6] = (byte)contest[i]; // Contest Stats: bytes 6-11
            }

            // M: Misc. Block
            byte[] M = new byte[12];
            {
                // Pokerus: byte 0
                uint pokerus = 0;
                pokerus = DataUtil.setBits(pokerus, (uint)pkrsDays, 0, 4); // Days: Pokerus bits 0-3
                pokerus = DataUtil.setBits(pokerus, (uint)pkrsStrain, 4, 4);// Strain: Pokerus bits 4-7
                M[0] = (byte)pokerus;

                // Met Location: byte 1
                M[1] = (byte)location;

                // Origins: bytes 2-3
                uint origins = 0;
                origins = DataUtil.setBits(origins, (uint)metlevel, 0, 7); // Met Level: Origins bits 0-6
                origins = DataUtil.setBits(origins, (uint)game, 7, 4); // Origin Game: Origins bits 7-10
                origins = DataUtil.setBits(origins, (uint)ball, 11, 4); // Ball: Origins bits 11-14
                origins = DataUtil.setBits(origins, (uint)otgender, 15); // OT Gender: Origins bit 15
                DataUtil.ShiftCopy(origins, M, 2, 2);

                // IVs/Egg/Ability: bytes 4-7
                uint iv_egg_ability = 0;
                iv_egg_ability = DataUtil.setBits(iv_egg_ability, (uint)ivs[0], 0, 5); // HP IV: IVs/Egg/Ability bits 0-4
                iv_egg_ability = DataUtil.setBits(iv_egg_ability, (uint)ivs[1], 5, 5); // Attack IV: IVs/Egg/Ability bits 5-9
                iv_egg_ability = DataUtil.setBits(iv_egg_ability, (uint)ivs[2], 10, 5); // Defense IV: IVs/Egg/Ability bits 10-14
                iv_egg_ability = DataUtil.setBits(iv_egg_ability, (uint)ivs[5], 15, 5); // Speed IV: IVs/Egg/Ability bits 15-19
                iv_egg_ability = DataUtil.setBits(iv_egg_ability, (uint)ivs[3], 20, 5); // Sp. Attack IV: IVs/Egg/Ability bits 20-24
                iv_egg_ability = DataUtil.setBits(iv_egg_ability, (uint)ivs[4], 25, 5); // Sp. Defense: IVs/Egg/Ability bits 25-29
                iv_egg_ability = DataUtil.setBits(iv_egg_ability, Convert.ToUInt32(pkuUtil.IsAnEgg(pku)), 30); // Is Egg: IVs/Egg/Ability bit 30
                iv_egg_ability = DataUtil.setBits(iv_egg_ability, (uint)abilitySlot, 31); // Ability Slot: IVs/Egg/Ability bit 31
                DataUtil.ShiftCopy(iv_egg_ability, M, 4);

                // Ribbons/Fateful Encounter: bytes 8-11

                // Contest Ribbons: Ribbons/Fateful Encounter bits 3i-(2+3i)
                uint ribbons_fateful_encounter = 0;
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (ribbons.Contains(Ribbon.Cool_G3 + 4 * i + j))
                            ribbons_fateful_encounter = DataUtil.setBits(ribbons_fateful_encounter, (uint)j + 1, 3 * i, 3);
                    }
                }

                // Non-Contest Ribbons: Ribbons/Fateful Encounter bits 15-26
                foreach (var kvp in pk3Util.RIBBON_INDEX)
                    ribbons_fateful_encounter = DataUtil.setBits(ribbons_fateful_encounter, Convert.ToUInt32(ribbons.Contains(kvp.Value)), kvp.Key);

                // Fateful Encounter: Ribbons/Fateful Encounter bit 31
                ribbons_fateful_encounter = DataUtil.setBits(ribbons_fateful_encounter, Convert.ToUInt32(fatefulEncounterResolver.DecideValue()), 31);
                DataUtil.ShiftCopy(ribbons_fateful_encounter, M, 8);
            }

            // Substructure Order
            byte[] subData = new byte[48];
            string order = pk3Util.SUBSTRUCTURE_ORDER[pidResolver.DecideValue() % 24];
            G.CopyTo(subData, 12 * order.IndexOf('G'));
            A.CopyTo(subData, 12 * order.IndexOf('A'));
            E.CopyTo(subData, 12 * order.IndexOf('E'));
            M.CopyTo(subData, 12 * order.IndexOf('M'));

            // Calculate Checksum
            uint checksum = 0;
            for (int i = 0; i < 24; i++) // Sum over subData, w/ 2 byte window
                checksum += subData[i + 1] * (uint)256 + subData[i];
            checksum %= 65536; //should be 2 bytes

            // Encryption Step
            uint encryptionKey = id ^ pidResolver.DecideValue();
            for (int i = 0; i < 12; i++) //xor subData with key in 4 byte chunks
            {
                int index = 4 * i;
                uint chunk = (uint)(subData[index + 3] << 24 | subData[index + 2] << 16 | subData[index + 1] << 8 | subData[index]);
                chunk ^= encryptionKey;
                DataUtil.toByteArray(chunk).CopyTo(subData, index); //copy the encrypted chunk bytes to the subData array
            }

            // Return
            return (subData, checksum);
        }

        protected override byte[] toFile()
        {
            byte[] data = new byte[80]; //pc .pk3 file is an 80 byte data structure

            // PID - bytes 0-3
            DataUtil.ShiftCopy(pidResolver.DecideValue(), data, 0, 4);

            // ID - bytes 4-7
            DataUtil.ShiftCopy(id, data, 4);

            // Nickname - bytes 8-17
            for (int i = 0; i < name.Length; i++)
                data[8 + i] = name[i];

            // Language - bytes 18-19
            uint langBytes = pk3Util.EncodeLanguage(lang);
            if (pkuUtil.IsAnEgg(pku))
                langBytes = pk3Util.EGG_LANGUAGE_ID; //Eggs in gen 3 all have this as their language value.
            DataUtil.ShiftCopy(langBytes, data, 18, 2);

            // OT - bytes 20-26
            for (int i = 0; i < otName.Length; i++)
                data[20 + i] = otName[i];

            // Markings - byte 27
            List<MarkingIndex> markings = pkxUtil.GetMarkings(pku.Markings);
            byte mbyte = 0x0;
            if (markings.Contains(MarkingIndex.BlueCircle))
                mbyte += 1;
            if (markings.Contains(MarkingIndex.BlueSquare))
                mbyte += 2;
            if (markings.Contains(MarkingIndex.BlueTriangle))
                mbyte += 4;
            if (markings.Contains(MarkingIndex.BlueHeart))
                mbyte += 8;
            data[27] = mbyte;

            // Checksum - bytes 28-29
            (byte[] subData, uint checksum) = encodeSubstructure();
            DataUtil.ShiftCopy(checksum, data, 28, 2);

            // Data - bytes 32-80
            subData.CopyTo(data, 32);

            return data;
        }
    }
}
