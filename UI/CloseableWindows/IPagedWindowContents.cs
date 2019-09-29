public interface IPagedWindowContents : ICloseableWindowContents {
    event System.Action OnPageCountUpdateHandler;
    int GetPageCount();
    void LoadPage(int pageIndex);
}