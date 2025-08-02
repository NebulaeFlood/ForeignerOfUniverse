using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Views;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ForeignerOfUniverse.Windows
{
    internal sealed class ThingWeavePolicyRenameWindow : Dialog_Rename<ThingWeavePolicyView>
    {
        public readonly HashSet<string> Names;


        public ThingWeavePolicyRenameWindow(ThingWeavePolicyView renaming) : base(renaming)
        {
            Names = ((ThingWeaveWindow)renaming.LayoutManager.Owner).Comp.weavePolicies
                .Except(renaming.Model)
                .Select(ThingWeavePolicy.GetLabel)
                .ToHashSet();
        }


        protected override AcceptanceReport NameIsValid(string name)
        {
            return Names.Contains(name)
                ? new AcceptanceReport("NameIsInUse".Translate())
                : AcceptanceReport.WasAccepted;
        }
    }
}
