using AnyRPG;

namespace AnyRPG {
    public interface IPagedWindowContents : ICloseableWindowContents {
        event System.Action<bool> OnPageCountUpdate;
        int GetPageCount();
        void LoadPage(int pageIndex);
    }
}
