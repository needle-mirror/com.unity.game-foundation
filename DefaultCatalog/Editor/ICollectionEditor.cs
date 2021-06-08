namespace UnityEditor.GameFoundation.DefaultCatalog
{
    interface ICollectionEditor
    {
        string name { get; }
        bool isCreating { get; }

        void Draw();
        void OnWillEnter();
        void OnWillExit();
        void ValidateSelection();
        void RefreshItems();
        void UnsubscribeFromCatalogUpdates();
    }
}
