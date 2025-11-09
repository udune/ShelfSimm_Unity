namespace Data
{
    public enum JobAction { PUT, PICK }

    public class Job
    {
        public JobAction Action { get; }
        public string CellCode { get; }
        public string BookTitle { get; }
        public int Quantity { get; }

        public Job(JobAction action, string cellCode, string bookTitle, int quantity)
        {
            Action = action;
            CellCode = cellCode;
            BookTitle = bookTitle;
            Quantity = quantity;
        }
    }
}