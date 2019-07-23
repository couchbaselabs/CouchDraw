using System;
using Couchbase.Lite;
using Couchbase.Lite.Sync;

namespace CouchDraw.Repositories
{
    public class DatabaseManager : IDisposable
    {
        // Note: User 'localhost' or '127.0.0.1' when using a simulator 
        //readonly Uri _remoteSyncUrl = new Uri("ws://localhost:4984");

        // Note: Use '10.0.2.2' when using an emulator
        readonly Uri _remoteSyncUrl = new Uri("ws://10.0.2.2:4984");

        readonly string _databaseName;

        Replicator _replicator;
        ListenerToken _replicatorListenerToken;

        Database _database;
        public Database Database
        {
            get
            {
                if (_database == null)
                {
                    _database = new Database(_databaseName);
                }

                return _database;
            }
        }

        public DatabaseManager(string databaseName)
        {
            _databaseName = databaseName;
        }

        public void StartReplication()
        {
            try
            {
                var targetUrlEndpoint = new URLEndpoint(new Uri(_remoteSyncUrl, _databaseName));

                var configuration = new ReplicatorConfiguration(Database, targetUrlEndpoint)
                {
                    ReplicatorType = ReplicatorType.PushAndPull,
                    Continuous = true
                };

                _replicator = new Replicator(configuration);

                _replicatorListenerToken = _replicator.AddChangeListener(OnReplicatorUpdate);

                _replicator.Start();
            }
            catch(Exception ex)
            {
                // We don't want replication errors to prevent us from
                // using the app, but we do want to know about them.
                Console.WriteLine($"Replication Exception - {ex.Message}");
            }
        }

        void OnReplicatorUpdate(object sender, ReplicatorStatusChangedEventArgs e)
        {
            var status = e.Status;

            switch (status.Activity)
            {
                case ReplicatorActivityLevel.Busy:
                    Console.WriteLine("Busy transferring data.");
                    break;
                case ReplicatorActivityLevel.Connecting:
                    Console.WriteLine("Connecting to Sync Gateway.");
                    break;
                case ReplicatorActivityLevel.Idle:
                    Console.WriteLine("Replicator in idle state.");
                    break;
                case ReplicatorActivityLevel.Offline:
                    Console.WriteLine("Replicator in offline state.");
                    break;
                case ReplicatorActivityLevel.Stopped:
                    Console.WriteLine("Completed syncing documents.");
                    break;
            }

            if (status.Progress.Completed == status.Progress.Total)
            {
                Console.WriteLine("All documents synced.");
            }
            else
            {
                Console.WriteLine($"Documents {status.Progress.Total - status.Progress.Completed} still pending sync");
            }
        }

        public void StopReplication()
        {
            if (_replicator != null)
            {
                _replicator.RemoveChangeListener(_replicatorListenerToken);
                _replicator.Stop();
            }
        }

        public void Dispose()
        {
            try
            {
                if (_replicator != null)
                {
                    StopReplication();

                    // Because the 'Stop' method for a Replicator instance is asynchronous 
                    // we must wait for the status activity to be stopped before closing the database. 
                    while (true)
                    {
                        if (_replicator.Status.Activity == ReplicatorActivityLevel.Stopped)
                        {
                            break;
                        }
                    }

                    _replicator.Dispose();
                }

                _database.Close();
                _database = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
