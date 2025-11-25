namespace Data
{
    public class Book
    {
        public string BookId { get; }
        public string Title { get; }
        public int ThicknessMm { get; }
        public int HeightMm { get; }

        public Book(string bookId, string title, int thicknessMm, int heightMm)
        {
            BookId = bookId;
            Title = title;
            ThicknessMm = thicknessMm;
            HeightMm = heightMm;
        }
    }
}
