using System.Collections.Generic;
using UnityEngine;

public class ChemistryRuleEngine : MonoBehaviour
{
    int ElectronsNeededForStable(ElementData e)
    {
        if (e == null) return 0;
        // Duet rule for Hydrogen, Helium
        if (e.symbol == "H" || e.symbol == "He") return Mathf.Max(0, 2 - e.valenceElectrons);
        // Octet rule for others
        return Mathf.Max(0, 8 - e.valenceElectrons);
    }

    bool IsMetal(ElementData e)
    {
        if (e == null) return false;
        return e.valenceElectrons <= 2 && e.electronegativity < 2.0f;
    }

    bool IsNonMetal(ElementData e)
    {
        if (e == null) return false;
        return e.valenceElectrons >= 4 && e.electronegativity >= 2.0f;
    }

    public List<TransferResult> ComputeTransfers(List<ElementData> elements)
    {
        var results = new List<TransferResult>();
        if (elements == null || elements.Count == 0) return results;

        // Accurate 2 element mode uses BondEvaluator
        if (elements.Count == 2)
        {
            var A = elements[0];
            var B = elements[1];
            var analysis = BondEvaluator.EvaluateBond(A, B);

            var tr = new TransferResult
            {
                donor = A,
                acceptor = B,
                electronsTransferred = analysis.electronsTransferred,
                bondType = ConvertBondType(analysis.bondType),
                stable = analysis.stable,
                indeterminate = analysis.indeterminate,
                reason = analysis.reason,
                chosenBonds = analysis.chosenBonds  // correct number of bonds
            };

            results.Add(tr);
            return results;
        }

        // Multi-element mode (metals & nonmetals)
        var used = new HashSet<int>();

        // Step 1: Ionic bonding (Metal → Non-metal transfer)
        for (int i = 0; i < elements.Count; i++)
        {
            if (used.Contains(i)) continue;
            var metal = elements[i];
            if (!IsMetal(metal)) continue;

            for (int j = 0; j < elements.Count; j++)
            {
                if (i == j || used.Contains(j)) continue;

                var nonMetal = elements[j];
                if (!IsNonMetal(nonMetal)) continue;

                // Use actual bond evaluator
                var analysis = BondEvaluator.EvaluateBond(metal, nonMetal);
                if (ConvertBondType(analysis.bondType) == BondType.Ionic && analysis.stable)
                {
                    var tr = new TransferResult
                    {
                        donor = metal,
                        acceptor = nonMetal,
                        electronsTransferred = analysis.electronsTransferred,
                        bondType = BondType.Ionic,
                        stable = true,
                        indeterminate = false,
                        reason = analysis.reason,
                        chosenBonds = 0 // ionic has no bond lines
                    };
                    results.Add(tr);
                    used.Add(i); used.Add(j);
                    break;
                }
            }
        }

        //  Step 2: Covalent bonding (Nonmetal ↔ Nonmetal)
        for (int i = 0; i < elements.Count; i++)
        {
            if (used.Contains(i)) continue;
            var e1 = elements[i];
            if (!IsNonMetal(e1)) continue;

            for (int j = i + 1; j < elements.Count; j++)
            {
                if (used.Contains(j)) continue;
                var e2 = elements[j];
                if (!IsNonMetal(e2)) continue;

                // Evaluate bond
                var analysis = BondEvaluator.EvaluateBond(e1, e2);
                var bondType = ConvertBondType(analysis.bondType);

                var tr = new TransferResult
                {
                    donor = e1,
                    acceptor = e2,
                    electronsTransferred = analysis.electronsTransferred,
                    bondType = bondType,
                    stable = analysis.stable,
                    indeterminate = analysis.indeterminate,
                    reason = analysis.reason,
                    chosenBonds = analysis.chosenBonds
                };

                results.Add(tr);
                used.Add(i); used.Add(j);
                break;
            }
        }

        //  Step 3: Leftover atoms (no partner found)
        for (int i = 0; i < elements.Count; i++)
        {
            if (used.Contains(i)) continue;
            var e = elements[i];
            var tr = new TransferResult
            {
                donor = e,
                acceptor = null,
                electronsTransferred = 0,
                bondType = BondType.None,
                stable = false,
                indeterminate = true,
                reason = "No suitable bonding partner found.",
                chosenBonds = 0
            };
            results.Add(tr);
        }

        return results;
    }

    //  Utility to map string result from BondEvaluator to BondType enum
    private BondType ConvertBondType(string evaluatorBondType)
    {
        switch (evaluatorBondType)
        {
            case "Ionic": return BondType.Ionic;
            case "Polar covalent": return BondType.PolarCovalent;
            case "Nonpolar covalent": return BondType.NonpolarCovalent;
            default: return BondType.None;
        }
    }
}
