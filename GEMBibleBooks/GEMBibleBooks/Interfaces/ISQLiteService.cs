using SQLite;

namespace GEMBibleBooks.Interfaces
{
    public interface ISQLiteService
    {
        SQLiteConnection GetConnection();
        SQLiteAsyncConnection GetAsyncConnection();
    }
}