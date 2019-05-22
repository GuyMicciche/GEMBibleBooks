using GEMBibleBooks.iOS;
using SQLite;
using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLiteService))]
namespace GEMBibleBooks.iOS
{
    public class SQLiteService : ISQLiteService
    {
        private SQLiteConnectionWithLock _conn;

        public SQLiteService() { }

        private static string GetDatabasePath()
        {
            const string sqliteFilename = "storehouse.db3";

            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
            var libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder
            var pathFull = Path.Combine(libraryPath, sqliteFilename);

            return pathFull;
        }

        public SQLiteConnection GetConnection()
        {
            var dbPath = GetDatabasePath();

            return new SQLiteConnection(dbPath);
        }

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            var dbPath = GetDatabasePath();

            var asyncConnection = new SQLiteAsyncConnection(dbPath);

            return asyncConnection;
        }
    }
}