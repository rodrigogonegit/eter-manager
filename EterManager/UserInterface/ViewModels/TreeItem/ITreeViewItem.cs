namespace EterManager.UserInterface.ViewModels.TreeItem
{
    public interface ITreeViewItem
    {
        string DisplayName { get; set; }

        string Fullname { get; set; }

        bool IsFolder { get; set; }

        void PerformFileSearch(string pattern);

        void SetVisibility(bool value);

        bool IsVisible { get; set; }

        bool IsExpanded { get; set; }
    }
}