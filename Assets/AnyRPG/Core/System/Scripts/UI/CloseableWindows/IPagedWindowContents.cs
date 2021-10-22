using AnyRPG;

namespace AnyRPG {
    public interface IPagedWindowContents {
        event System.Action<bool> OnPageCountUpdate;
        int GetPageCount();
        void LoadPage(int pageIndex);
    }
}
