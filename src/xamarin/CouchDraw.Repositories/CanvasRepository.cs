using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using CouchDraw.Core;
using CouchDraw.Core.Repositories;
using CouchDraw.Models;

namespace CouchDraw.Repositories
{
    public class CanvasRepository : ICanvasRepository
    {
        const string databaseName = "couchdraw";

        IQuery _pathsQuery;
        ListenerToken _pathsQueryToken;

        protected DatabaseManager _databaseManager;
        protected DatabaseManager DatabaseManager
        {
            get
            {
                if (_databaseManager == null)
                {
                    _databaseManager = new DatabaseManager(databaseName);
                }

                return _databaseManager;
            }
        }

        public CanvasRepository()
        {
            DatabaseManager.StartReplication();
        }

        public void SavePath(Path path) => DatabaseManager.Database.Save(path.ToMutableDocument($"path::{path.Id}"));

        public async Task<List<Path>> GetInternalPathsAsync()
        {
            var paths = new List<Path>();

            try
            {
                var pathsQuery = QueryBuilder
                                .Select(SelectResult.All())
                                .From(DataSource.Database(DatabaseManager.Database))
                                .Where((Expression.Property("type").EqualTo(Expression.String("path"))
                                .And(Expression.Property("createdBy").EqualTo(Expression.String(AppInstance.AppId)))));

                paths = await Task.Run(() =>
                {
                    return pathsQuery.Execute()?.AllResults()?.ToObjects<Path>()?.ToList();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CanvasRepository GetInternalPaths Exception: {ex.Message}");
            }

            return paths;
        }


        public List<Path> GetExternalPaths(Action<List<Path>> pathsUpdated)
        {
            List<Path> paths = new List<Path>();

            try
            {
                _pathsQuery = QueryBuilder
                                .Select(SelectResult.All())
                                .From(DataSource.Database(DatabaseManager.Database))
                                .Where((Expression.Property("type").EqualTo(Expression.String("path"))
                                .And(Expression.Property("createdBy").NotEqualTo(Expression.String(AppInstance.AppId)))));

                if (pathsUpdated != null)
                {
                    _pathsQueryToken = _pathsQuery.AddChangeListener((object sender, QueryChangedEventArgs e) =>
                    {
                        if (e?.Results != null && e.Error == null)
                        {
                            paths = e.Results.AllResults()?.ToObjects<Path>() as List<Path>;

                            if (paths != null)
                            {
                                pathsUpdated.Invoke(paths);
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CanvasRepository GetExternalPaths Exception: {ex.Message}");
            }

            return paths;
        }

        public void DeletePaths(List<Path> paths)
        {
            foreach (var path in paths)
            {
                DeletePath(path);
            }
        }

        public void DeletePath(Path path)
        {
            var document = DatabaseManager.Database.GetDocument($"path::{path.Id}");

            if (document != null)
            {
                DatabaseManager.Database.Delete(document);
            }
        }

        public void DeleteAllPaths()
        {
            try
            {
                var pathsQuery = QueryBuilder
                                .Select(SelectResult.All())
                                .From(DataSource.Database(DatabaseManager.Database));


                List<Path> paths = pathsQuery.Execute()?.AllResults()?.ToObjects<Path>()?.ToList();
                DeletePaths(paths);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"CanvasRepository GetInternalPaths Exception: {ex.Message}");
            }
        }

            public void Dispose()
        {
            _pathsQuery.RemoveChangeListener(_pathsQueryToken);
            _pathsQuery = null;

            DatabaseManager?.Dispose();
        }
    }
}
