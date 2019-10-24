using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CouchDraw.Models;

namespace CouchDraw.Core.Repositories
{
    public interface ICanvasRepository : IDisposable
    {
        void SavePath(Path path);
        void DeletePath(Path path);
        Task<List<Path>> GetInternalPathsAsync();
        List<Path> GetExternalPaths(Action<List<Path>> pathsUpdated);
        void DeletePaths(List<Path> paths);
        void DeleteAllPaths();
    }
}
