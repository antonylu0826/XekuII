using DevExpress.Xpo;
using System.Linq;

namespace XekuII.ApiHost.BusinessObjects;

public partial class Order
{
    protected override void OnSaving()
    {
        base.OnSaving();
        
        // 自動計算總金額
        if (Items != null)
        {
            TotalAmount = Items.Sum(i => i.LineTotal);
        }
    }
}
