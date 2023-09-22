using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;

namespace Dawnsbury.Mods.Ancestries.Lizardfolk;

public class LizardfolkAncestryFeat : TrueFeat
{
    public LizardfolkAncestryFeat(string name, string flavorText, string rulesText)
        : base(FeatName.CustomFeat, 1, flavorText, rulesText, new[] { Trait.Ancestry, Trait.Starborn })
    {
        this
            .WithCustomName(name)
            .WithPrerequisite(sheet => sheet.Ancestries.Contains(Trait.Starborn), "You must be a Lizardfolk.");
    }
}