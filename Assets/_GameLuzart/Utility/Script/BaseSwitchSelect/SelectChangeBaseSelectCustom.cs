using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectChangeBaseSelectCustom : BaseSelect
{
    public GroupBaseSelectCustom groupBSCustomSelect;
    public GroupBaseSelectCustom groupBSCustomUnSelect;
    public override void Select(bool isSelect)
    {
        base.Select(isSelect);
        GroupBaseSelectCustom bsTrue = isSelect ? groupBSCustomSelect : groupBSCustomUnSelect;
        //GroupBaseSelectCustom bsFalse = !isSelect ? groupBSCustomSelect : groupBSCustomUnSelect;

        int lengthArrayBaseSelect = bsTrue.baseSelectCustom.Length;
        for (int j = 0; j < lengthArrayBaseSelect; j++)
        {
            BaseSelectCustom bsCustom = bsTrue.baseSelectCustom[j];
            if (bsCustom != null && bsCustom.baseSelect != null)
            {
                bsCustom.baseSelect.Select(bsCustom.index);
                bsCustom.baseSelect.Select(bsCustom.isSelect);
            }
        }

    }
}
