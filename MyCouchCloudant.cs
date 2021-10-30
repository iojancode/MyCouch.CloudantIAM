using System;

namespace MyCouch.CloudantIAM
{
    public class MyCouchCloudant : MyCouchClient
    {
        public MyCouchCloudant(string serverAddress, string dbName, MyCouchCloudantBootstrapper bootstrapper = null)
            : this(new Uri(serverAddress), dbName, bootstrapper) { }

        public MyCouchCloudant(Uri serverAddress, string dbName, MyCouchCloudantBootstrapper bootstrapper = null)
            : this(new CloudantDbConnectionInfo(serverAddress, dbName), bootstrapper) { }

        public MyCouchCloudant(CloudantDbConnectionInfo connectionInfo, MyCouchCloudantBootstrapper bootstrapper = null)
            : base(connectionInfo, bootstrapper ?? MyCouchCloudantBootstrappers.Default) { }
    }
}