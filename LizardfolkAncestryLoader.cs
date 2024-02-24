using Dawnsbury.Audio;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.CharacterBuilding;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using System;
using static Dawnsbury.Delegates;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Dawnsbury.Mods.Ancestries.Lizardfolk
{
    public class LizardfolkAncestryLoader
    {
        public static Trait Lizardfolk;

        [Dawnsbury.Modding.DawnsburyDaysModMainMethodAttribute]
        public static void LoadMod()
        {
            Lizardfolk = ModManager.RegisterTrait("Lizardfolk", new TraitProperties("Lizardfolk", true) { IsAncestryTrait = true });

            AddFeats(CreateGeneralFeats());
            AddFeats(CreateLizardfolkAncestryFeats());

            ModManager.AddFeat(new AncestrySelectionFeat(
                    FeatName.CustomFeat,
                    "Lizardfolk move through the societies of other humanoids with the steely reserve of born predators. They have a well-deserved reputation as outstanding rangers and unsentimental fighters. Though lizardfolk have adapted to many different environments, many of them still prefer to remain near bodies of water, using their ability to hold their breath to their advantage. As a result, lizardfolk usually prefer equipment that is not easily damaged by moisture, eschewing leather and metal for gear made of stone, ivory, glass, and bone. Claws Your sharp claws offer an alternative to the fists other humanoids bring to a fight.\r\n\r\nYou have a claw unarmed attack that deals 1d4 slashing damage and has the agile and finesse traits.\r\n\r\nAquatic Adaptation Your reptilian biology allows you to hold your breath for a long time. You gain the Breath Control general feat as a bonus feat.",
                    new List<Trait> { Trait.Humanoid, Lizardfolk },
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
                    creature.UnarmedStrike = new Item(IllustrationName.DragonClaws, "claws",
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
                    AdditionalUnarmedStrike = new Item(IllustrationName.Jaws, "fangs",
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
                    AdditionalUnarmedStrike = new Item(IllustrationName.Fist, "tail",
                        new[] { Trait.Sweep, Trait.Unarmed, Trait.Melee, Trait.Weapon })
                        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning))
                });

                creature.QEffects.First(qeffect => qeffect.Name == "Tail Whip").AdditionalUnarmedStrike = 
                        new Item(IllustrationName.Fist, "tail",
                        new[] { Trait.Sweep, Trait.Unarmed, Trait.Melee, Trait.Weapon })
                        .WithWeaponProperties(new WeaponProperties("1d12", DamageKind.Bludgeoning));
            });

            yield return new LizardfolkAncestryFeat(
                "Razor Claws",
                "Your have honed your claws to be deadly.",
                "Your claw attack deals 1d6 slashing damage instead of 1d4 and gains the versatile (piercing) trait.")
            .WithCustomName("Razor Claws")
            .WithOnCreature(creature =>
            {
                creature.UnarmedStrike = new Item(IllustrationName.DragonClaws, "claws",
                            new[] { Trait.Agile, Trait.Finesse, Trait.VersatileP, Trait.Unarmed, Trait.Melee, Trait.Weapon })
                        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing));
            });

            yield return new LizardfolkAncestryFeat(
                "Lightning Tongue",
                "Your tongue darts out faster than the eye can see to retrieve loose objects.",
                "You Interact to pick up a single unattended object of light Bulk or less within 10 feet of you. If you don't have enough hands free to hold the object, it falls to the ground in your space.")
            .WithCustomName("Lightning Tongue")
            .WithOnCreature(creature =>
            {
                creature.AddQEffect(new QEffect("Lightning Tongue", "Pick up an unattended item within 10 feet of you.")
                {
                    ProvideContextualAction = (qfSelf) =>
                    {
                        if (!qfSelf.Owner.Battle.Map.AllTiles.Any(tile => tile.DroppedItems.Count > 0 && qfSelf.Owner.DistanceTo(tile) <= 2))
                        {
                            // No item in range
                            return null;
                        }

                        SubmenuPossibility submenuPossibility = new SubmenuPossibility(IllustrationName.PickUp, "Lightning Tongue");
                        submenuPossibility.Subsections = new List<PossibilitySection>
                            {
                                new PossibilitySection("Lightning Tongue")
                            };

                        //Every tile with dropped items within 2 tiles
                        IEnumerable<Tile> tiles = qfSelf.Owner.Battle.Map.AllTiles.Where(tile => tile.DroppedItems.Count > 0 && qfSelf.Owner.DistanceTo(tile) <= 2);
                        List<Possibility> possibilities = new List<Possibility>();

                        //Every such tile
                        foreach (Tile tile in tiles)
                        {
                            //Every dropped item
                            foreach (Item item in tile.DroppedItems)
                            {
                                submenuPossibility.Subsections.First().Possibilities.Add(new ActionPossibility(new CombatAction(creature, item.Illustration, "Pick up (" + item.Name + ")", new Trait[] { Trait.Manipulate, Lizardfolk }, "Pick up an unattended item within 10 feet of you. If you don't have enough hands free to hold the object, it falls to the ground on your space.",
                                    Target.Self())
                                .WithActionCost(1)
                                .WithItem(item)
                                .WithEffectOnSelf(creature =>
                                {
                                    if ((creature.HasFreeHand && !item.TwoHanded) || creature.HeldItems.Count() == 0)
                                    {
                                        creature.AddHeldItem(item);
                                        tile.DroppedItems.Remove(item);
                                    }
                                    else
                                    {
                                        tile.DroppedItems.Remove(item);
                                        qfSelf.Owner.Occupies.DroppedItems.Add(item);
                                    }
                                })));
                            }
                        }
                        
                        return submenuPossibility;
                    }
                });
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
                            var lizardfolk = qfSelf.Owner;
                            lizardfolk.WeaknessAndResistance.AddResistance(DamageKind.Fire, resistanceValue);
                        }
                    });
                });

            yield return new HeritageSelectionFeat(FeatName.CustomFeat,
                    "You can flare your neck frill and flex your dorsal spines, Demoralizing your foes.",
                    "When you do, Demoralize loses the auditory trait and gains the visual trait, and you don't take a penalty when you attempt to Demoralize a creature that doesn't understand your language. You also gain the Threatening Approach action.")
                .WithCustomName("Frilled Lizardfolk")
                .WithOnSheet((sheet) =>
                {
                    //TODO: It's not exactly what it does, but it is practically what it does.
                    sheet.GrantFeat(FeatName.IntimidatingGlare);
                })
                .WithOnCreature(lizardfolk => {
                    lizardfolk.AddQEffect(new QEffect("Menacing Approach", "You Stride once. If you end your movement adjacent to an enemy, you can Demoralize that enemy. If you succeed, the foe is frightened 2 instead of frightened 1.")
                    {
                        ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(new CombatAction(qfSelf.Owner, IllustrationName.FleetStep, "Threatening Approach", new Trait[1] { Trait.Move }, "Stride once. If you end your movement adjacent to an enemy, you can Demoralize that enemy.  If you succeed, the foe is frightened 2 instead of frightened 1.", Target.Self())
                            .WithActionCost(2)
                            .WithSoundEffect(SfxName.Footsteps)
                            .WithEffectOnSelf(async delegate (CombatAction action, Creature self)
                            {
                                if (!(await self.StrideAsync("Choose where to Stride with Threatening Approach. If you stride adjacent to an enemy, you can attemp to Demoralize them.", allowStep: false, maximumFiveFeet: false, null, allowCancel: true)))
                                {
                                    action.RevertRequested = true;
                                }
                                else
                                {
                                    List<Option> list = new List<Option>();
                                    CombatAction combatAction = new CombatAction(self, IllustrationName.Demoralize, "Demoralize", new Trait[6]
                                    {
                                        Trait.Auditory, Trait.Concentrate, Trait.Emotion, Trait.Fear, Trait.Mental, Trait.Basic
                                    }, "With a sudden shout, a well-timed taunt, or a cutting putdown, you can shake an enemy's resolve. Choose a creature within 5 feet of you who you're aware of. Attempt  {b}an Intimidation check{/b} against that target's  {b}Will DC.{/b} If the target does not understand the language you are speaking, or you're not speaking a language, you take a -4 circumstance penalty to the check.\n\nRegardless of your result, the target is immune to your attempts to Demoralize it for the rest of the encounter.\n\n{b}Critical Success{/b} The target becomes frightened 2.\n{b}Success{/b} The target becomes frightened 2.", Target.Ranged(1)).WithActionId(ActionId.Demoralize).WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Intimidation).WithExtraBonus((CombatAction combatAction, Creature demoralizer, Creature target) => (target.DoesNotSpeakCommon && !demoralizer.HasEffect(QEffectId.IntimidatingGlare)) ? new Bonus(-4, BonusType.Circumstance, "No shared language") : null), Checks.DefenseDC(Defense.Will))).WithSoundEffect(self.HasTrait(Trait.Female) ? SfxName.Intimidate : SfxName.MaleIntimidate)
                                    .WithActionCost(0)
                                    .WithProjectileCone(IllustrationName.Demoralize, 24, ProjectileKind.Cone)
                                    .WithEffectOnEachTarget(async delegate (CombatAction c, Creature a, Creature tg, CheckResult k)
                                    {
                                        switch (k)
                                        {
                                            case CheckResult.CriticalSuccess:
                                                tg.AddQEffect(QEffect.Frightened(2));
                                                break;
                                            case CheckResult.Success:
                                                tg.AddQEffect(QEffect.Frightened(2));
                                                break;
                                        }

                                        tg.AddQEffect(QEffect.ImmunityToTargeting(ActionId.Demoralize, a));
                                    });

                                    GameLoop.AddDirectUsageOnCreatureOptions(combatAction, list);

                                    if (list.Count > 0)
                                    {
                                        if (list.Count == 1)
                                        {
                                            await list[0].Action();
                                        }
                                        else
                                        {
                                            await (await self.Battle.SendRequest(new AdvancedRequest(self, "Choose a creature to Demoralize.", list))).ChosenOption.Action();
                                        }
                                    }
                                }
                            }))
                    });
                });
        }
        private static void AddFeats(IEnumerable<Feat> feats)
        {
            foreach (var feat in feats)
            {
                ModManager.AddFeat(feat);
            }
        }

        //TODO: "inhaled" isn't exactly a trait at the moment. For now, it'll just apply to poison in general.
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