using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CouchDraw.Core.Repositories;
using CouchDraw.Models;
using Robo.Mvvm;
using Robo.Mvvm.ViewModels;
using System.Windows.Input;
using Robo.Mvvm.Input;

namespace CouchDraw.Core.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        Action UpdateCanvas { get; set; }
        string SelectedPathColor { get; set; } = "#000000";

        public List<Path> Paths { get; set; } = new List<Path>();
        public List<Path> ExternalPaths = new List<Path>();


        ICommand _selectPathColorCommand;
        public ICommand SelectPathColorCommand
        {
            get
            {
                if (_selectPathColorCommand == null)
                {
                    _selectPathColorCommand = new Command<string>((color) => SelectedPathColor = color);
                }

                return _selectPathColorCommand;
            }
        }

        ICommand _undoPathCommand;
        public ICommand UndoPathCommand
        {
            get
            {
                if (_undoPathCommand == null)
                {
                    _undoPathCommand = new Command(UndoLastPath);
                }

                return _undoPathCommand;
            }
        }

        ICommand _clearAllPathsCommand;
        public ICommand ClearAllPathsCommand
        {
            get
            {
                if (_clearAllPathsCommand == null)
                {
                    _clearAllPathsCommand = new Command(ClearAllPaths);
                }

                return _clearAllPathsCommand;
            }
        }

        ICanvasRepository CanvasRepository { get; set; }

        public MainViewModel(Action updateCanvas)
        {
            UpdateCanvas = updateCanvas;

            CanvasRepository = ServiceContainer.GetInstance<ICanvasRepository>();

            Init();
        }

        // We want to kick off this method, we don't need to (a)wait for the response.
        private async void Init()
        {
            var internalPaths = await CanvasRepository.GetInternalPathsAsync().ConfigureAwait(false);
            var externalPaths = await Task.Run(() => CanvasRepository.GetExternalPaths(UpdatePaths));

            if (internalPaths?.Count > 0)
            {
                Paths = internalPaths;
            }

            if (externalPaths?.Count > 0)
            {
                ExternalPaths = externalPaths;
            }
        }

        public void CreatePath(Point point)
        {
            var path = new Path(Guid.NewGuid().ToString())
            {
                CreatedBy = AppInstance.AppId,
                Color = SelectedPathColor
            };

            path.Points.Add(point);

            Paths.Add(path);

            SavePath(path);
        }
        
        public void AddPoint(Point point)
        {
            if (Paths != null)
            {
                var path = Paths.Last();

                if (path != null)
                {
                    path.Points?.Add(point);
                    SavePath(path);
                }
            }
        }

        void SavePath(Path path)
        {
            Task.Run(() => CanvasRepository?.SavePath(path));
            UpdateCanvas?.Invoke();
        }

        void UpdatePaths(List<Path> paths)
        {
            ExternalPaths = paths;
            UpdateCanvas?.Invoke();
        }

        void UndoLastPath()
        {
            var path = Paths?.Last();

            if (path != null)
            {
                CanvasRepository?.DeletePath(path);
                Paths.Remove(path);
                UpdateCanvas?.Invoke();
            }
        }

        void ClearAllPaths()
        {
            CanvasRepository?.DeletePaths(Paths);
            Paths = new List<Path>();
            UpdateCanvas?.Invoke();
        }
    }
}
