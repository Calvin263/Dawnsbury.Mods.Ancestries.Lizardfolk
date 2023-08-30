using Origin.Core.CharacterBuilder.AbilityScores;
using Origin.Core.CharacterBuilder.Feats;
using Origin.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Origin.Core.CombatActions;
using Origin.Core.Mechanics;
using Origin.Core.Mechanics.Core;
using Origin.Core.Mechanics.Enumerations;
using Origin.Core.Mechanics.Treasure;
using Origin.Display.Illustrations;
using Origin.Modding;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Dawnsbury.Mods.Ancestries.Lizardfolk
{
    public class LizardfolkAncestryLoader
    {
        [Origin.Modding.DawnsburyDaysModMainMethodAttribute]
        public static void LoadMod()
        {
            AddFeats(CreateGeneralFeats());
            AddFeats(CreateLizardfolkAncestryFeats());

            ModManager.AddFeat(new AncestrySelectionFeat(
                    FeatName.CustomFeat,
                    "Lizardfolk move through the societies of other humanoids with the steely reserve of born predators. They have a well-deserved reputation as outstanding rangers and unsentimental fighters. Though lizardfolk have adapted to many different environments, many of them still prefer to remain near bodies of water, using their ability to hold their breath to their advantage. As a result, lizardfolk usually prefer equipment that is not easily damaged by moisture, eschewing leather and metal for gear made of stone, ivory, glass, and bone. Claws Your sharp claws offer an alternative to the fists other humanoids bring to a fight.\r\n\r\nYou have a claw unarmed attack that deals 1d4 slashing damage and has the agile and finesse traits.\r\n\r\nAquatic Adaptation Your reptilian biology allows you to hold your breath for a long time. You gain the Breath Control general feat as a bonus feat.",
                    new List<Trait> { Trait.Humanoid, Trait.Starborn },
                    8,
                    5,
                    new List<AbilityBoost>()
                    {
                    new EnforcedAbilityBoost(Ability.Strength),
                    new EnforcedAbilityBoost(Ability.Wisdom),
                    new FreeAbilityBoost()
                    },
                    CreateLizardfolkHeritages().ToList())
                .WithAbilityFlaw(Ability.Intelligence)
                .WithCustomName("Lizardfolk")
                .WithOnSheet(sheet => {
                    sheet.AddFeat(breathControl, null);
                })
                .WithOnCreature(creature =>
                {
                    creature.UnarmedStrike = new Item(Origin.Core.IllustrationName.DragonClaws, "claws",
                            new[] { Trait.Agile, Trait.Finesse, Trait.Unarmed, Trait.Melee, Trait.Weapon })
                        .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Slashing));
                })
            );
        }

        private static IEnumerable<Feat> CreateLizardfolkAncestryFeats()
        {
            yield return new LizardfolkAncestryFeat(
                "Sharp Fangs",
                "Your teeth are formidable weapons.",
                "You gain a fangs unarmed attack that deals 1d8 piercing damage.")
            .WithCustomName("Sharp Fangs")
            .WithOnCreature(creature =>
            {
                creature.AddQEffect(new QEffect("Sharp Fangs", "You have a fangs attack.")
                {
                    AdditionalUnarmedStrike = new Item(Origin.Core.IllustrationName.Jaws, "fangs",
                            new[] { Trait.Unarmed, Trait.Melee, Trait.Weapon })
                        .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Piercing))
                });
            });

            yield return new LizardfolkAncestryFeat(
                "Tail Whip",
                "By birth or through training, your tail is strong enough to make for a powerful melee weapon.",
                "You gain a tail unarmed attack that deals 1d6 bludgeoning damage and has the sweep trait.")
            .WithCustomName("Tail Whip")
            .WithOnCreature(creature =>
            {
                creature.AddQEffect(new QEffect("Tail Whip", "You have a tail attack.")
                {
                    AdditionalUnarmedStrike = new Item(Origin.Core.IllustrationName.Fist, "tail",
                            new[] { Trait.Sweep, Trait.Unarmed, Trait.Melee, Trait.Weapon })
                        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning))
                });
            });

            yield return new LizardfolkAncestryFeat(
                "Razor Claws",
                "Your have honed your claws to be deadly.",
                "Your claw attack deals 1d6 slashing damage instead of 1d4 and gains the versatile (piercing) trait.")
            .WithCustomName("Razor Claws")
            .WithOnCreature(creature =>
            {
                creature.UnarmedStrike = new Item(Origin.Core.IllustrationName.DragonClaws, "claws",
                            new[] { Trait.Agile, Trait.Finesse, Trait.VersatileP, Trait.Unarmed, Trait.Melee, Trait.Weapon })
                        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing));
            });
        }

        private static IEnumerable<Feat> CreateGeneralFeats()
        {
            yield return breathControl;
        }

        private static IEnumerable<Feat> CreateLizardfolkHeritages()
        {
            yield return new HeritageSelectionFeat(FeatName.CustomFeat,
                    "Your thick scales help you retain water and combat the sun's glare.",
                    "You gain fire resistance equal to half your level (minimum 1). Environmental heat effects are one step less extreme for you, and you can go 10 times as long as normal before you are affected by starvation or thirst. However, unless you wear protective gear or take shelter, environmental cold effects are one step more extreme for you.")
                .WithCustomName("Sandstrider Lizardfolk")
                .WithOnCreature((sheet, creature) =>
                {

                    var resistanceValue = (creature.Level + 1) / 2;
                    creature.AddQEffect(new QEffect("Sandstrider Lizardfolk",
                        "You have fire resistance " +
                        resistanceValue + ".")
                    {
                        StateCheck = (qfSelf) =>
                        {
                            var kobold = qfSelf.Owner;
                            kobold.WeaknessAndResistance.AddResistance(DamageKind.Fire, resistanceValue);
                        }
                    });
                });

            yield return new HeritageSelectionFeat(FeatName.CustomFeat,
                    "You can flare your neck frill and flex your dorsal spines, Demoralizing your foes.",
                    "When you do, Demoralize loses the auditory trait and gains the visual trait, and you don't take a penalty when you attempt to Demoralize a creature that doesn't understand your language. You also gain the Threatening Approach action.")
                .WithCustomName("Frilled Lizardfolk")
                .WithOnSheet((sheet) =>
                {
                    sheet.GrantFeat(FeatName.IntimidatingGlare);
                });
                //MENACING APPROACH
        }

        private static void AddFeats(IEnumerable<Feat> feats)
        {
            foreach (var feat in feats)
            {
                ModManager.AddFeat(feat);
            }
        }

        public static Feat breathControl = new TrueFeat(FeatName.CustomFeat,
                1,
                "You have incredible breath control, which grants you advantages when air is hazardous or sparse.",
                "You can hold your breath for 25 times as long as usual before suffocating. You gain a +1 circumstance bonus to saving throws against inhaled threats, such as inhaled poisons, and if you roll a success on such a saving throw, you get a critical success instead.",
                new[] { Trait.General
            })
            .WithCustomName("Breath Control")
            .WithOnCreature(cr => cr.AddQEffect(new QEffect("Breath Control", "You have a +1 circumstance bonus to saving throws against inhaled threats, such as inhaled poisons, and if you roll a success on such a saving throw, you get a critical success instead.")
                {
                     Innate = true,
                     BonusToDefenses = (QEffect qf, CombatAction action, Defense defense) => (action != null && action.HasTrait(Trait.Poison) && defense != 0) ? new Bonus(1, BonusType.Circumstance, "Breath Control") : null,
                     AdjustSavingThrowResult = (QEffect qf, CombatAction action, CheckResult originalResult) => (action.HasTrait(Trait.Poison) && originalResult == CheckResult.Success) ? CheckResult.CriticalSuccess : originalResult
                }
            ));
     }
}