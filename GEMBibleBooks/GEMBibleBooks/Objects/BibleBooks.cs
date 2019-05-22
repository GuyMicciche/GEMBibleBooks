using SQLite;

namespace GEMBibleBooks.Objects
{
    class BibleBooks
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string BookNumber { get; set; }
        public string Chapters { get; set; }
        public string OfficialBookAbbreviation { get; set; }
        public string StandardBookAbbreviation { get; set; }
        public string StandardBookName { get; set; }
        public string Writers { get; set; }
        public string PlaceWritten { get; set; }
        public string WritingCompleted { get; set; }
        public string TimeCovered { get; set; }
    }

    class EnglishBooks : BibleBooks
    {
        public EnglishBooks() { }
    }

    class ChineseBooks : BibleBooks
    {
        public ChineseBooks() { }
    }

    class PinyinBooks : BibleBooks
    {
        public PinyinBooks() { }
    }
}