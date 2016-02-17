using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EterManager.UserInterface.ViewModels.TreeItem;

namespace EterManager.Utilities
{
    public static class TreeItemHelpers
    {
        public static IEnumerable<ITreeViewItem> GetAllItems(this IEnumerable<ITreeViewItem> list)
        {
            List<ITreeViewItem> rtnList = new List<ITreeViewItem>();

            foreach (var item in list.OfType<TreeItemFolderVm>())
                foreach (var subItem in item.Children.GetAllItems())
                    rtnList.Add(subItem);

            foreach (var file in list.OfType<TreeItemFileVm>())
                rtnList.Add(file); ;

            return rtnList;
        }
    }
}
