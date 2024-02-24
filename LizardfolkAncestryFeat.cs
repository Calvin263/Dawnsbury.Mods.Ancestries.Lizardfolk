using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;

namespace Dawnsbury.Mods.Ancestries.Lizardfolk;

public class LizardfolkAncestryFeat : TrueFeat
{
    public LizardfolkAncestryFeat(string name, string flavorText, string rulesText)
        : base(FeatName.CustomFeat, 1, flavorText, rulesText, new[] { Trait.Ancestry, LizardfolkAncestryLoader.Lizardfolk })
    {
        this
            .WithCustomName(name)
            .WithPrerequisite(sheet => sheet.Ancestries.Contains(LizardfolkAncestryLoader.Lizardfolk), "You must be a Lizardfolk.");
    }
}