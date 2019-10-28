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

            Acr.UserDialogs.ToastConfig.DefaultActionTextColor = Color.White;
            Acr.UserDialogs.ToastConfig.DefaultBackgroundColor = Color.CadetBlue;
            Acr.UserDialogs.ToastConfig.DefaultDuration = TimeSpan.FromSeconds(2.0);
            Acr.UserDialogs.ToastConfig.DefaultPosition = Acr.UserDialogs.ToastPosition.Top;
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
