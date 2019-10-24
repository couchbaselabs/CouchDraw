using System;
using CouchDraw.Core.Repositories;
using CouchDraw.Repositories;
using Robo.Mvvm;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CouchDraw
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Core.AppInstance.AppId = GetUniquePersistentId("app_id");

            ServiceContainer.Register<ICanvasRepository>(new CanvasRepository());

            MainPage = new MainPage();
        }

        protected override void OnResume()
        {
            new DatabaseManager("couchdraw").StartReplication();
        }

        string GetUniquePersistentId(string key)
        {
            var id = Preferences.Get(key, string.Empty);

            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString();
                Preferences.Set(key, id);
            }

            return id;
        }
    }



}
