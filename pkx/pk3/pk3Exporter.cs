using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.pku;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace pkuManager.pkx.pk3
{
    public class pk3Exporter : Exporter
    {
        public pk3Exporter(pkuObject pku, GlobalFlags globalFlags) : base(pku, globalFlags) { }

        public override string formatName { get { return "Gen 3"; } }

        public override string formatExtension { get { return "pk3"; } }

        public override bool canExport()
        {
            // Screen National Dex #
            if (!(pkxUtil.GetNationalDex(pku.Species) <= 386)) //Only species gen 3 and below are allowed
                return false;

            // Screen Form
            if (!DexUtil.IsFormDefault(pku) && !DexUtil.CanCastPKU(pku, pk3Util.VALID_FORMS)) // If form isn't default, and uncastable
                return false;

            // Screen Shadow Pokemon
            if (pku.IsShadow())
                return false;

            return true; //compatible with .pk3
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

            tempAlert = pkxUtil.ProcessFlags.ProcessBattleStatOverride(pku, globalFlags);
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
            (game, checkedGameName, tempAlert) = pkxUtil.ProcessTags.ProcessOriginGame(pku, 3);
            warnings.Add(tempAlert);

            // Met Location
            //  - Requires: Origin Game
            (location, tempAlert) = pkxUtil.ProcessTags.ProcessMetLocation(pku, checkedGameName, (g, l) => { return pk3Util.EncodeMetLocation(g, l); }, pk3Util.GetDefaultLocationName(checkedGameName));
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
            /* ------------------------------------
             * G: Growth Block
             * ------------------------------------
            */
            ByteArrayManipulator G = new ByteArrayManipulator(12, false);

            G.SetUInt((int)PokeAPIUtil.GetSpeciesIndex(dex, 3), 0, 2); // Species: bytes 0-1
            G.SetUInt(item, 2, 2);                                     // Item: bytes 2-3
            G.SetUInt(expResolver.DecideValue(), 4, 4);                // Experience: bytes 4-7
            G.SetUInt(ppups[0], 8, 0, 2);                              // PP Up 1: byte 8, bits 0-1
            G.SetUInt(ppups[1], 8, 2, 2);                              // PP Up 2: byte 8, bits 2-3
            G.SetUInt(ppups[2], 8, 4, 2);                              // PP Up 3: byte 8, bits 4-5
            G.SetUInt(ppups[3], 8, 6, 2);                              // PP Up 4: byte 8, bits 6-7
            G.SetUInt(friendship, 9, 1);                               // Friendship: byte 9
                                                                       // "Unknown": byte 10-11 (Bulbapedia is vague here, but I suspect it's just padding.)


            /* ------------------------------------
             * A: Attacks Block
             * ------------------------------------
            */
            ByteArrayManipulator A = new ByteArrayManipulator(12, false);
            for (int i = 0; i < 4; i++)
            {
                A.SetUInt(moves[i], 2 * i, 2);                                                           // Move i: bytes (2i)-(2i+1)  (overall 0-7)
                if (moves[i] != 0)
                    A.SetUInt((5 + ppups[i]) * PokeAPIUtil.GetMoveBasePP(moves[i]).Value / 5, 8 + i, 1); // PP i: byte 8+i  (overall 8-11)
            }


            /* ------------------------------------
             * E: EVs & Condition Block
             * ------------------------------------
            */
            ByteArrayManipulator E = new ByteArrayManipulator(12, false);

            // EVs - are encoded in a different order than usual:
            E.SetUInt(evs[0], 0, 1);                    // HP EV: byte 0
            E.SetUInt(evs[1], 1, 1);                    // Attack: byte 1
            E.SetUInt(evs[2], 2, 1);                    // Defense: byte 2
            E.SetUInt(evs[5], 3, 1);                    // Speed: byte 3
            E.SetUInt(evs[3], 4, 1);                    // Sp. Attack: byte 4
            E.SetUInt(evs[4], 5, 1);                    // Sp. Defense: byte 5

            // Contest Stats: Cool, Beauty, Cute, Clever, Tough, Sheen
            for (int i = 0; i < 6; i++)
                E.SetUInt(contest[i], 6 + i, 1);        // Contest Stat i: byte 6+i (overall 6-11)


            /* ------------------------------------
             * M: Misc. Block
             * ------------------------------------
            */
            ByteArrayManipulator M = new ByteArrayManipulator(12, false);

            // Pokerus
            M.SetUInt(pkrsDays, 0, 0, 4);                             // Pokerus Days: byte 0, bits 0-3
            M.SetUInt(pkrsStrain, 0, 4, 4);                           // Pokerus Days: byte 0, bits 4-7

            // Origins
            M.SetUInt(location, 1, 1);                                // Met Location: byte 1
            M.SetUInt(metlevel, 2, 0, 7);                             // Met Level: bytes 2-3, bits 0-6
            M.SetUInt(game, 2, 7, 4);                                 // Origin Game: bytes 2-3, bits 7-10
            M.SetUInt((int)ball, 2, 11, 4);                           // Ball: bytes 2-3, bits 11-14
            M.SetBool(otgender > 0, 2, 15);                           // OT Gender: bytes 2-3, bit 15

            // IVs - encoded in a different order than usual:
            M.SetUInt(ivs[0], 4, 0, 5);                               // HP IV: bytes 4-7, bits 0-4
            M.SetUInt(ivs[1], 4, 5, 5);                               // Attack IV: bytes 4-7, bits 5-9
            M.SetUInt(ivs[2], 4, 10, 5);                              // Defense IV: bytes 4-7, bits 10-14
            M.SetUInt(ivs[5], 4, 15, 5);                              // Speed IV: bytes 4-7, bits 15-19
            M.SetUInt(ivs[3], 4, 20, 5);                              // Sp. Attack IV: bytes 4-7, bits 20-24
            M.SetUInt(ivs[4], 4, 25, 5);                              // Sp. Defense IV: bytes 4-7, bits 25-29

            M.SetBool(pku.IsAnEgg(), 4, 30);                          // Is Egg: bytes 4-7, bit 30
            M.SetBool(abilitySlot > 0, 4, 31);                        // Ability Slot: bytes 4-7, bit 31

            // Contest Ribbon Ranks
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 4; j++)
                    if (ribbons.Contains(Ribbon.Cool_G3 + 4 * i + j))
                        M.SetUInt(j + 1, 8, 3 * i, 3);                // Contest Ribbon i Rank: bytes 8-11, bits 3i-(2+3i) (overall 0-14)

            // Non-Contest Ribbons
            foreach (var kvp in pk3Util.RIBBON_INDEX)
                M.SetBool(ribbons.Contains(kvp.Value), 8, kvp.Key);   // Non-Contest Ribbon i: bytes 8-11, bit kvp.Key (overall 15-26)

            M.SetBool(fatefulEncounterResolver.DecideValue(), 8, 31); // Fateful Encounter: bytes 8-11, bit 31


            /* ------------------------------------
             * Compile Substructure
             * ------------------------------------
            */

            // Substructure Order
            ByteArrayManipulator subData = new ByteArrayManipulator(48, false);
            string order = pk3Util.SUBSTRUCTURE_ORDER[pidResolver.DecideValue() % 24];
            subData.SetBytes(G, 12 * order.IndexOf('G'));
            subData.SetBytes(A, 12 * order.IndexOf('A'));
            subData.SetBytes(E, 12 * order.IndexOf('E'));
            subData.SetBytes(M, 12 * order.IndexOf('M'));

            // Calculate Checksum
            uint checksum = pk3Util.CalculateChecksum(subData);

            // Encryption Step
            pk3Util.EncryptSubData(subData, id, pidResolver.DecideValue());

            // Return
            return (subData, checksum);
        }

        protected override byte[] toFile()
        {
            //pc .pk3 file is an 80 byte data structure
            ByteArrayManipulator data = new ByteArrayManipulator(80, false);

            data.SetUInt(pidResolver.DecideValue(), 0, 4);                                               // PID: bytes 0-3
            data.SetUInt(id, 4, 4);                                                                      // ID: bytes 4-7
            data.SetBytes(name, 8);                                                                      // Nickname: bytes 8-17
            data.SetUInt(pku.IsAnEgg() ? pk3Util.EGG_LANGUAGE_ID : pk3Util.EncodeLanguage(lang), 18, 2); // Language: bytes 18-19
            data.SetBytes(otName, 20);                                                                   // OT: bytes 20-26

            // Markings
            List<MarkingIndex> markings = pkxUtil.GetMarkings(pku.Markings);
            data.SetBool(markings.Contains(MarkingIndex.BlueCircle), 27, 0);                             // Markings: byte 27, bit 0
            data.SetBool(markings.Contains(MarkingIndex.BlueSquare), 27, 1);                             // Markings: byte 27, bit 1
            data.SetBool(markings.Contains(MarkingIndex.BlueTriangle), 27, 2);                           // Markings: byte 27, bit 2
            data.SetBool(markings.Contains(MarkingIndex.BlueHeart), 27, 3);                              // Markings: byte 27, bit 3

            //Data-substructure + Encrypting
            (byte[] subData, uint checksum) = encodeSubstructure();
            data.SetUInt(checksum, 28, 2);                                                               // Checksum: bytes 28-29
            data.SetBytes(subData, 32);                                                                  // Data-Substructure: bytes 32-80

            return data;
        }
    }
}
