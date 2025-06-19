using LabelImg.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelImg
{
    public class IndexedCutLabelCollection : ObservableCollection<CutLabelModel>
    {
        protected override void InsertItem(int index, CutLabelModel item)
        {
            base.InsertItem(index, item);
            UpdateIndexes();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            UpdateIndexes();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            UpdateIndexes();
        }

        protected override void SetItem(int index, CutLabelModel item)
        {
            base.SetItem(index, item);
            UpdateIndexes();
        }

        private void UpdateIndexes()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Index = i + 1; // 从1开始编号
            }
        }
    }

}
