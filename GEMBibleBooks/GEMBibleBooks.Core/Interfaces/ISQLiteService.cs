using SQLite;

namespace GEMBibleBooks.Core.Interfaces
{
    public interface ISQLiteService
    {
        SQLiteConnection GetConnection();
        SQLiteAsyncConnection GetAsyncConnection();
    }
}