using GEMBibleBooks.Core.Interfaces;
using GEMBibleBooks.Core.Objects;
using GEMBibleBooks.Core.Views;
using ICSharpCode.SharpZipLib.Zip;
using PCLStorage;
using Prism;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace GEMBibleBooks.Core
{
    public partial class App : PrismApplication
    {
        public const string DatabaseName = "storehouse";

        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override void OnInitialized()
        {
            InitializeComponent();

            DisplayMainPage();
            //CheckDatabase();
        }

        private async void DisplayMainPage()
        {
            await NavigationService.NavigateAsync("NavigationPage/MainPage/MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage>();

            containerRegistry.Register<IStorehouseService, StorehouseService>();
        }

        private async void CheckDatabase()
        {
            try
            {
                // IFolder interface comes from PCLStorage, check if database file is there
                IFolder rootFolder = FileSystem.Current.LocalStorage;
                Debug.WriteLine("step1");
                ExistenceCheckResult exists = await rootFolder.CheckExistsAsync(DatabaseName + ".db3");
                Debug.WriteLine("step2");

                if (exists == ExistenceCheckResult.FileExists)
                {
                    DisplayMainPage();

                    return;
                }

                byte[] bytes = GetShippedDatabase(DatabaseName);

                await UnpackZipFile(bytes).ContinueWith(async (antecedent) =>
                {
                    await CreateStorehouseAsync();

                    DisplayMainPage();
                });
            }
            catch(Exception e)
            {
                Debug.WriteLine("CheckDatabase ERROR => " + e.Message);
            }

        }

        public async Task CreateStorehouseAsync()
        {
            var database = DependencyService.Get<ISQLiteService>().GetAsyncConnection();
            await database.CreateTableAsync<EnglishBooks>().ConfigureAwait(false);
            await database.CreateTableAsync<ChineseBooks>().ConfigureAwait(false);
            await database.CreateTableAsync<PinyinBooks>().ConfigureAwait(false);
        }

        private byte[] GetShippedDatabase(string database)
        {
            byte[] bytes;
            var assembly = typeof(App).GetTypeInfo().Assembly;
            using (Stream s = assembly.GetManifestResourceStream("GEMBibleBooks." + database + ".zip"))
            {
                long length = s.Length;
                bytes = new byte[length];
                s.Read(bytes, 0, (int)length);
            }

            return bytes;
        }

        private async Task UnpackZipFile(byte[] zipFileBytes)
        {
            try
            {
                IFolder rootFolder = FileSystem.Current.LocalStorage;
                IFile file = await rootFolder.CreateFileAsync("storehouse.zip", CreationCollisionOption.ReplaceExisting);
                using (Stream stream = await file.OpenAsync(FileAccess.ReadAndWrite))
                {
                    await stream.WriteAsync(zipFileBytes, 0, zipFileBytes.Length);
                    using (var zf = new ZipFile(stream))
                    {
                        foreach (ZipEntry zipEntry in zf)
                        {
                            // Gete Entry Stream.
                            Stream zipEntryStream = zf.GetInputStream(zipEntry);

                            // Create the file in filesystem and copy entry stream to it.
                            IFile zipEntryFile = await rootFolder.CreateFileAsync(zipEntry.Name, CreationCollisionOption.ReplaceExisting);
                            using (Stream outPutFileStream = await zipEntryFile.OpenAsync(FileAccess.ReadAndWrite))
                            {
                                zipEntryStream.CopyTo(outPutFileStream);
                            }
                        }
                    }
                }
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }

            //IFolder rootFolder = CrossStorage.FileSystem.LocalStorage;
            //IFile file = rootFolder.CreateFile("storehouse.zip", CreationCollisionOption.ReplaceExisting);
            //Stream z = file.Open(FileAccess.ReadWrite);
            //await z.WriteAsync(zipFileBytes, 0, zipFileBytes.Length);

            //using (ZipInputStream s = new ZipInputStream(z))
            //{
            //    ZipEntry theEntry;
            //    while ((theEntry = s.GetNextEntry()) != null)
            //    {
            //        Console.WriteLine(theEntry.Name);

            //        string directoryName = Path.GetDirectoryName(theEntry.Name);
            //        string fileName = Path.GetFileName(theEntry.Name);

            //        if (fileName != String.Empty)
            //        {
            //            using (FileStream streamWriter = File.Create(theEntry.Name))
            //            {

            //                int size = 2048;
            //                byte[] data = new byte[2048];
            //                while (true)
            //                {
            //                    size = s.Read(data, 0, data.Length);
            //                    if (size > 0)
            //                    {
            //                        streamWriter.Write(data, 0, size);
            //                    }
            //                    else
            //                    {
            //                        break;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public static MemoryStream CopyToMemory(Stream input)
        {
            // It won't matter if we throw an exception during this method;
            // we don't *really* need to dispose of the MemoryStream, and the
            // caller should dispose of the input stream
            MemoryStream ret = new MemoryStream();

            byte[] buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ret.Write(buffer, 0, bytesRead);
            }
            // Rewind ready for reading (typical scenario)
            ret.Position = 0;
            return ret;
        }
    }
}
