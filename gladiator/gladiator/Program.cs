using System;
using System.Collections.Generic;
using System.Linq;

namespace Gladiatorer
{
    internal class Program
    {
        private static readonly Random Random = new Random();

        private static string playerName = "Gladiator";
        private static int rank = 1;
        private static string rankName = "Tiggare";
        private static int gold = 0;
        private static int fightsWon = 0;
        private static bool leatherArmor = false;
        private static bool ironArmor = false;
        private static bool defeatedDrako = false;
        private static bool defeatedMaximus = false;
        private static bool defeatedDominous = false;
        private static bool detailedDescriptions = false;

        private static readonly List<string> playerWeapons = new List<string> { "näve", "spark" };
        private static readonly List<string> enemyWeapons = new List<string> { "näve", "spark" };

        private sealed class Enemy
        {
            public string Name { get; }
            public int MaxHp { get; }
            public int Hp { get; set; }
            public int RankRequired { get; }
            public bool Defeated { get; set; }

            public Enemy(string name, int hp, int requiredRank)
            {
                Name = name;
                MaxHp = hp;
                Hp = hp;
                RankRequired = requiredRank;
            }
        }

        private static readonly Enemy Drako = new Enemy("Drako", 26, 1);
        private static readonly Enemy Maximus = new Enemy("Maximus", 34, 2);
        private static readonly Enemy Dominous = new Enemy("Dominous Aurelius Valcar", 50, 3);

        private static int PlayerMaxHp => ironArmor ? 48 : leatherArmor ? 32 : 26;

        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("===========\nGLADIATORER\n===========");
            Console.WriteLine("Välkommen till spelet Gladiatorer!");

            Console.Write("Ange din gladiators namn: ");
            string? enteredName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(enteredName))
                playerName = char.ToUpper(enteredName[0]) + enteredName.Substring(1);

            detailedDescriptions = AskYesNo("Vill du ha mer detaljerade stridsbeskrivningar? (ja/nej): ");

            while (fightsWon < 3)
            {
                Console.Clear();
                Console.WriteLine($"Du är {playerName}, rank {rank}: {rankName}.");
                Console.WriteLine($"Guldmynt: {gold}");
                if (fightsWon > 0)
                    VisitBlacksmithIfWanted();

                Enemy? enemy = ChooseEnemy();
                if (enemy == null)
                    continue;

                if (Fight(enemy))
                {
                    RewardAndPromote(enemy);
                    fightsWon++;
                    if (fightsWon < 3)
                        Pause();
                }
            }

            Console.WriteLine("\nDu har besegrat alla motståndare och vunnit spelet!");
            Console.WriteLine($"Slutresultat: {playerName}, rank {rankName}, {gold} guldmynt.");
            Pause();
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string answer = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                if (answer == "ja") return true;
                if (answer == "nej") return false;
                Console.WriteLine("Svara ja eller nej.");
            }
        }

        private static Enemy? ChooseEnemy()
        {
            while (true)
            {
                Console.WriteLine("\nVälj motståndare:");
                ShowEnemy(Drako, 1);
                ShowEnemy(Maximus, 2);
                ShowEnemy(Dominous, 3);
                Console.Write("Ditt val (1–3): ");
                string choice = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

                Enemy? enemy = choice switch
                {
                    "1" or "drako" => Drako,
                    "2" or "maximus" => Maximus,
                    "3" or "dominous" or "dominous aurelius valcar" => Dominous,
                    _ => null
                };

                if (enemy == null)
                {
                    Console.WriteLine("Ogiltigt val.");
                    continue;
                }

                if (enemy.Defeated)
                {
                    Console.WriteLine("Du har redan besegrat den motståndaren.");
                    continue;
                }

                if (rank < enemy.RankRequired)
                {
                    Console.WriteLine($"Du behöver rank {enemy.RankRequired} för att möta {enemy.Name}.");
                    continue;
                }

                return enemy;
            }
        }

        private static void ShowEnemy(Enemy enemy, int number)
        {
            string status = enemy.Defeated ? "Besegrad" : $"Kräver rank {enemy.RankRequired}";
            Console.WriteLine($"{number}. {enemy.Name}: {enemy.MaxHp} HP ({status})");
        }

        private static bool Fight(Enemy enemy)
        {
            int playerHp = PlayerMaxHp;
            enemy.Hp = enemy.MaxHp;
            var availableWeapons = new List<string>(playerWeapons);

            if (enemy == Maximus && !enemyWeapons.Contains("kortsvärd"))
                enemyWeapons.Add("kortsvärd");
            if (enemy == Dominous && !enemyWeapons.Contains("morgonstjärna"))
                enemyWeapons.Add("morgonstjärna");

            Console.WriteLine($"\nDu möter {enemy.Name}!");
            Console.WriteLine("Du kan använda: " + string.Join(", ", availableWeapons));
            Console.WriteLine("Varje tur väljer du en attack. Skriv 'status' för att se HP.");

            int round = 1;
            while (playerHp > 0 && enemy.Hp > 0)
            {
                Console.WriteLine($"\n--- Runda {round} ---");
                Console.WriteLine($"Din HP: {playerHp}/{PlayerMaxHp} | {enemy.Name}: {enemy.Hp}/{enemy.MaxHp} HP");

                Console.Write("Välj attack (" + string.Join("/", availableWeapons) + "): ");
                string attack = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

                if (attack == "status")
                {
                    Console.WriteLine($"Din HP: {playerHp}/{PlayerMaxHp} | Fiendens HP: {enemy.Hp}");
                    continue;
                }
                if (!availableWeapons.Contains(attack))
                {
                    Console.WriteLine("Du kan inte använda det vapnet.");
                    continue;
                }

                int damage = PlayerAttackDamage(attack, enemy);
                if (damage > 0)
                {
                    enemy.Hp -= damage;
                    Console.WriteLine($"Din attack träffar {enemy.Name} och gör {damage} skada.");
                    if (attack == "kortsvärd" && enemy == Drako)
                        availableWeapons.Remove("kortsvärd");
                }
                else
                {
                    Console.WriteLine("Din attack missar.");
                }

                if (enemy.Hp <= 0)
                    break;

                string enemyAttack = ChooseEnemyAttack(enemy, playerHp);
                int enemyDamage = EnemyAttackDamage(enemyAttack);
                if (Random.Next(100) < 70)
                {
                    int armorReduction = ironArmor ? 2 : leatherArmor ? 1 : 0;
                    enemyDamage = Math.Max(0, enemyDamage - armorReduction);
                    playerHp -= enemyDamage;
                    Console.WriteLine($"{enemy.Name} attackerar med {enemyAttack} och gör {enemyDamage} skada.");
                }
                else
                {
                    Console.WriteLine($"{enemy.Name} missar sin attack.");
                }

                round++;
            }

            if (playerHp <= 0)
            {
                Console.WriteLine("\nDu har förlorat striden. Du kan försöka igen.");
                Pause();
                return false;
            }

            Console.WriteLine($"\nDu har besegrat {enemy.Name}!");
            return true;
        }

        private static int PlayerAttackDamage(string weapon, Enemy enemy)
        {
            int chance = weapon switch
            {
                "näve" => 67,
                "spark" => 80,
                "yxa" => 75,
                "kniv" => 60,
                "klubba" => 75,
                "kortsvärd" => enemy == Drako ? 100 : 80,
                "stridssvärd" => 70,
                "ak-47" => 73,
                _ => 0
            };

            if (Random.Next(100) >= chance)
                return 0;

            return weapon switch
            {
                "näve" => Random.Next(2, 4),
                "spark" => Random.Next(1, 3),
                "yxa" => enemy == Drako ? 5 : enemy == Maximus ? 4 : 3,
                "kniv" => enemy == Drako ? 4 : enemy == Maximus ? 3 : 2,
                "klubba" => enemy == Drako ? 3 : enemy == Maximus ? 2 : 1,
                "kortsvärd" => enemy == Drako ? Random.Next(6, 8) : enemy == Maximus ? Random.Next(5, 7) : Random.Next(4, 6),
                "stridssvärd" => Random.Next(7, 9),
                "ak-47" => 100,
                _ => 0
            };
        }

        private static string ChooseEnemyAttack(Enemy enemy, int playerHp)
        {
            if (enemy == Dominous && playerHp <= 8)
                return "morgonstjärna";
            if (enemy == Maximus && playerHp <= 7)
                return "kortsvärd";

            string[] choices = enemy == Drako
                ? new[] { "näve", "spark" }
                : enemy == Maximus
                    ? new[] { "näve", "spark", "kortsvärd", "kortsvärd" }
                    : new[] { "näve", "spark", "morgonstjärna", "morgonstjärna", "morgonstjärna" };

            return choices[Random.Next(choices.Length)];
        }

        private static int EnemyAttackDamage(string weapon)
        {
            return weapon switch
            {
                "näve" => Random.Next(2, 4),
                "spark" => Random.Next(1, 3),
                "kortsvärd" => Random.Next(6, 8),
                "morgonstjärna" => 8,
                _ => 1
            };
        }

        private static void RewardAndPromote(Enemy enemy)
        {
            enemy.Defeated = true;

            if (enemy == Drako)
            {
                rank = 2;
                rankName = "Legosoldat";
            }
            else if (enemy == Maximus)
            {
                rank = 3;
                rankName = "Gladiator";
            }

            int reward = enemy == Drako ? 15 : enemy == Maximus ? 25 : 55;
            gold += reward;
            Console.WriteLine($"Du får {reward} guldmynt. Du har nu {gold} guldmynt.");
            if (enemy != Dominous)
                Console.WriteLine($"Du har befordrats till rank {rank}: {rankName}.");
        }

        private static void VisitBlacksmithIfWanted()
        {
            if (!AskYesNo("\nVill du besöka smeden? (ja/nej): "))
                return;

            bool again;
            do
            {
                Blacksmith();
                again = AskYesNo("Vill du handla något mer? (ja/nej): ");
            } while (again);
        }

        private static void Blacksmith()
        {
            Console.WriteLine("\n--- Smeden ---");
            Console.WriteLine($"Du har {gold} guldmynt.");
            Console.WriteLine("Köp: läderrustning (5), järnrustning (10), kortsvärd (7), kniv (3), stridssvärd (9), ak-47 (40).");
            Console.WriteLine("Sälj: skriv 'sälj' för att se alternativ. Skriv 'nej' för att lämna.");
            Console.Write("Ditt val: ");
            string choice = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

            if (choice == "nej" || choice == "0") return;

            if (choice == "sälj")
            {
                SellItem();
                return;
            }

            switch (choice)
            {
                case "läderrustning":
                case "läder":
                case "1":
                    if (rank < 2) { Console.WriteLine("Du behöver rank 2."); return; }
                    if (leatherArmor || ironArmor) { Console.WriteLine("Du har redan rustning."); return; }
                    if (gold < 5) { Console.WriteLine("Du har inte tillräckligt med guld."); return; }
                    leatherArmor = true; gold -= 5;
                    Console.WriteLine("Du köpte läderrustning.");
                    break;

                case "järnrustning":
                case "järn":
                case "2":
                    if (rank < 3) { Console.WriteLine("Du behöver rank 3."); return; }
                    if (leatherArmor || ironArmor) { Console.WriteLine("Sälj din nuvarande rustning först."); return; }
                    if (gold < 10) { Console.WriteLine("Du har inte tillräckligt med guld."); return; }
                    ironArmor = true; gold -= 10;
                    Console.WriteLine("Du köpte järnrustning.");
                    break;

                case "kortsvärd":
                case "3":
                    BuyWeapon("kortsvärd", 7, 2);
                    break;
                case "kniv":
                case "4":
                    BuyWeapon("kniv", 3, 2);
                    break;
                case "stridssvärd":
                case "5":
                    BuyWeapon("stridssvärd", 9, 3);
                    break;
                case "ak-47":
                case "6":
                    BuyWeapon("ak-47", 40, 1);
                    break;
                default:
                    Console.WriteLine("Ogiltigt val.");
                    break;
            }
        }

        private static void BuyWeapon(string weapon, int price, int requiredRank)
        {
            if (rank < requiredRank) { Console.WriteLine($"Du behöver rank {requiredRank}."); return; }
            if (playerWeapons.Contains(weapon)) { Console.WriteLine("Du äger redan det vapnet."); return; }
            if (playerWeapons.Count >= 4) { Console.WriteLine("Du kan inte bära fler vapen. Sälj ett först."); return; }
            if (gold < price) { Console.WriteLine("Du har inte tillräckligt med guld."); return; }

            playerWeapons.Add(weapon);
            gold -= price;
            Console.WriteLine($"Du köpte {weapon}.");
        }

        private static void SellItem()
        {
            Console.WriteLine("Tillgängligt att sälja:");
            if (leatherArmor) Console.WriteLine("läderrustning (+3)");
            if (ironArmor) Console.WriteLine("järnrustning (+6)");
            foreach (string weapon in playerWeapons.Where(w => w != "näve" && w != "spark"))
                Console.WriteLine($"{weapon} (+{SellPrice(weapon)})");

            Console.Write("Vad vill du sälja? ");
            string item = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

            if (item == "läderrustning" && leatherArmor)
            {
                leatherArmor = false; gold += 3;
                Console.WriteLine("Du sålde läderrustningen.");
            }
            else if (item == "järnrustning" && ironArmor)
            {
                ironArmor = false; gold += 6;
                Console.WriteLine("Du sålde järnrustningen.");
            }
            else if (playerWeapons.Contains(item) && item != "näve" && item != "spark")
            {
                playerWeapons.Remove(item);
                gold += SellPrice(item);
                Console.WriteLine($"Du sålde {item}.");
            }
            else
            {
                Console.WriteLine("Du sålde ingenting.");
            }
        }

        private static int SellPrice(string weapon)
        {
            return weapon switch
            {
                "kortsvärd" => 4,
                "kniv" => 3,
                "stridssvärd" => 5,
                "ak-47" => 3,
                _ => 1
            };
        }

        private static void Pause()
        {
            Console.WriteLine("\nTryck Enter för att fortsätta.");
            Console.ReadLine();
        }
    }
}