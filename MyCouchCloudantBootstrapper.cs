using System;

namespace MyCouch.CloudantIAM
{
    internal static class MyCouchCloudantBootstrappers
    {
        internal static MyCouchCloudantBootstrapper Default { get; private set; }

        static MyCouchCloudantBootstrappers()
        {
            Default = new MyCouchCloudantBootstrapper();
        }
    }

    public class MyCouchCloudantBootstrapper : MyCouchClientBootstrapper
    {
        public MyCouchCloudantBootstrapper()
        {
            DbConnectionFn = cnInfo => new CloudantDbConnection(
                (cnInfo as CloudantDbConnectionInfo) ?? throw new InvalidCastException("Cannot cast to CloudantDbConnectionInfo"));
        }
    }
}