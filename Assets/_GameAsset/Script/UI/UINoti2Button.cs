using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

public class UINoti2Button : UIBase
{
    public TMP_Text txtTitle;
    public TMP_Text txtContent;
    private Action btnAction1, btnAction2;

    public void SetData(string title, string content, int idDialog, Action btn1Action,  Action btn2Action)
    {
        txtTitle.text = title;
        txtContent.text = content;
        this.btnAction1 = btn1Action;
        this.btnAction2 = btn2Action;
    }

    public void OnClickBtn1()
    {
        btnAction1?.Invoke();
        Hide();
    }

    public void OnClickBtn2()
    {
        btnAction2?.Invoke();
        Hide();
    }
}
