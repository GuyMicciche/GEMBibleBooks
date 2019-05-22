using GEMBibleBooks;
using GEMBibleBooks.Interfaces;
using SQLite;
using System;
using System.Threading;
using Xamarin.Forms;

[assembly: Dependency(typeof(StorehouseService))]
namespace GEMBibleBooks
{
    public class StorehouseService: IStorehouseService
    {
        private static readonly AsyncLock AsyncLock = new AsyncLock();
        private static readonly Lazy<StorehouseService> Lazy = new Lazy<StorehouseService>(() => new StorehouseService());

        public static IStorehouseService Instance => Lazy.Value;

        private SQLiteAsyncConnection _store;
        public SQLiteAsyncConnection Store
        {
            get
            {
                if (_store == null)
                {
                    LazyInitializer.EnsureInitialized(ref _store, DependencyService.Get<ISQLiteService>().GetAsyncConnection);
                }

                return _store;
            }
            set
            {
                _store = value;
            }
        }

        public void CloseStore()
        {
            SQLiteAsyncConnection.ResetPool();
        }
    }
}