using System;

namespace MyCouch.CloudantIAM
{
    internal static class MyCouchCloudantBootstrappers
    {
        internal static MyCouchCloudantBootstrapper Default { get; } = new MyCouchCloudantBootstrapper();
    }

    public class MyCouchCloudantBootstrapper : MyCouchClientBootstrapper
    {
        public MyCouchCloudantBootstrapper()
        {
            DbConnectionFn = cnInfo => new CloudantDbConnection((CloudantDbConnectionInfo)cnInfo);
        }
    }
}