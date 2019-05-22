using GEMBibleBooks.Interfaces;
using GEMBibleBooks.UWP;
using SQLite;
using System;
using System.IO;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLiteService))]
namespace GEMBibleBooks.UWP
{
    public class SQLiteService : ISQLiteService
    {
        private static readonly Lazy<SQLiteAsyncConnection> _asyncConnection = new Lazy<SQLiteAsyncConnection>(() =>
        {
            var databasePath = GetDatabasePath();
            var connection = new SQLiteAsyncConnection(databasePath);

            return connection;
        });

        private static string GetDatabasePath()
        {
            const string sqliteFilename = "storehouse.db3";

            string documentsPath = ApplicationData.Current.LocalFolder.Path;
            string path = Path.Combine(documentsPath, sqliteFilename);

            return path;
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(GetDatabasePath());
        }

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return _asyncConnection.Value;
        }
    }
}