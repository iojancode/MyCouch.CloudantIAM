using System;

namespace MyCouch.CloudantIAM
{
    public class MyCouchCloudantStore : MyCouchStore, IMyCouchStore, IDisposable
    {
        public MyCouchCloudantStore(string serverAddress, string dbName = null) : this(new MyCouchCloudant(serverAddress, dbName)) { }

        public MyCouchCloudantStore(Uri serverAddress, string dbName = null) : this(new MyCouchCloudant(serverAddress, dbName)) { }

        public MyCouchCloudantStore(IMyCouchClient client) : base(client) {}
    }
}