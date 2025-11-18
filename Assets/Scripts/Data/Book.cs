namespace Data
{
    public class Book
    {
        public string Title { get; }
        public int ThicknessMm { get; }
        public int HeightMm { get; }

        public Book(string title, int thicknessMm, int heightMm)
        {
            Title = title;
            ThicknessMm = thicknessMm;
            HeightMm = heightMm;
        }
    }
}
