using GEMBibleBooks.Interfaces;
using GEMBibleBooks.Objects;
using GEMBibleBooks.Views;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using PCLExt.FileStorage;
using PCLExt.FileStorage.Folders;
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
namespace GEMBibleBooks
{
    public partial class App : PrismApplication
    {
        public const string DatabaseName = "storehouse";

        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            DisplayMainPage();

            //var exists = Task.Run(() => CheckDatabase()).Result;
            //if (exists)
            //{
            //    DisplayMainPage();
            //}
            //else
            //{
            //    byte[] bytes = GetShippedDatabase(DatabaseName);
            //    await Task.Run(() => UnpackZipFile(bytes).ContinueWith(async (antecedent) =>
            //     {
            //        //await CreateStorehouseAsync();

            //        //DisplayMainPage();
            //     }));
            //}
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

        private async Task<bool> CheckDatabase()
        {
            // IFolder interface comes from PCLStorage, check if database file is there
            var rootFolder = new LocalRootFolder();
            ExistenceCheckResult exists = await rootFolder.CheckExistsAsync(DatabaseName + ".db3");
            if (exists == ExistenceCheckResult.FileExists)
            {
                return true;
            }

            return false;
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
            ZipFile zf = null;
            Debug.WriteLine("step1");
            var rootFolder = new LocalRootFolder();
            Debug.WriteLine("step2");

            IFile file = rootFolder.CreateFile("storehouse.zip", CreationCollisionOption.ReplaceExisting);
            Debug.WriteLine("step3 => " + file.Path);

            using (Stream s = file.Open(PCLExt.FileStorage.FileAccess.ReadAndWrite))
            {
                Debug.WriteLine("step4");

                byte[] buffer = new byte[4096];     // 4K is optimum

                Debug.WriteLine("step4");

                var assembly = typeof(App).GetTypeInfo().Assembly;
                Debug.WriteLine("step4");

                Stream st = assembly.GetManifestResourceStream("GEMBibleBooks." + DatabaseName + ".zip");
                Debug.WriteLine("step4");


                StreamUtils.Copy(st, s, buffer);
                Debug.WriteLine("step4");

                zf = new ZipFile(st);
                Debug.WriteLine("step4");

                Debug.WriteLine("step4");

                foreach (ZipEntry zipEntry in zf)
                {
                    Debug.WriteLine("step5 => " + zipEntry.Name);

                    Stream zipStream = zf.GetInputStream(zipEntry);

                    IFile z = rootFolder.CreateFile(zipEntry.Name, CreationCollisionOption.ReplaceExisting);
                    using (Stream fs = z.Open(PCLExt.FileStorage.FileAccess.ReadAndWrite))
                    {
                        StreamUtils.Copy(zipStream, fs, buffer);
                    }
                }
            }

            //Debug.WriteLine("step1");
            //var rootFolder = new LocalRootFolder();
            //IFile file = await rootFolder.CreateFileAsync("storehouse.zip", CreationCollisionOption.ReplaceExisting);
            //Debug.WriteLine("step2");

            //Stream z = await file.OpenAsync(PCLExt.FileStorage.FileAccess.ReadAndWrite);
            //Debug.WriteLine("step3");

            //await z.WriteAsync(zipFileBytes, 0, zipFileBytes.Length);
            //Debug.WriteLine("step4");


            //using (var zf = new ZipFile(z))
            //{
            //    Debug.WriteLine("step5");

            //    ZipEntry theEntry;
            //    Debug.WriteLine("step6");

            //    foreach (ZipEntry zipEntry in zf)
            //    {
            //        Debug.WriteLine("step7");

            //        Debug.WriteLine(zipEntry.Name);

            //        IFile zipEntryFile = await rootFolder.CreateFileAsync(zipEntry.Name, CreationCollisionOption.ReplaceExisting);
            //        Debug.WriteLine("step8");

            //        using (Stream stream = CopyToMemory(await zipEntryFile.OpenAsync(PCLExt.FileStorage.FileAccess.ReadAndWrite)))
            //        {
            //            int size = 2048;
            //            byte[] data = new byte[2048];
            //            while (true)
            //            {
            //                size = z.Read(data, 0, data.Length);
            //                if (size > 0)
            //                {
            //                    await stream.WriteAsync(data, 0, size);
            //                }
            //                else
            //                {
            //                    break;
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
