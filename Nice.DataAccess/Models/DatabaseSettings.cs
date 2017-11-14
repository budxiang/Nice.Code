namespace Nice.DataAccess
{
    internal class DatabaseSettings
    {
        private static string dbConnString;
        public static string BbConnString
        {
            get
            {
                return dbConnString;
            }
            set
            {
                dbConnString = value;
            }
        }
        private static string providerName;
        public static string ProviderName
        {
            get
            {
                return providerName;
            }
            set
            {
                providerName = value;
            }
        }

        private static int commandTimeOut;
        public static int CommandTimeOut
        {
            get
            {
                return commandTimeOut;
            }
            set
            {
                commandTimeOut = value;
            }
        }

        private static string dataProviderAssembly;
        public static string DataProviderAssembly
        {
            get
            {
                return dataProviderAssembly;
            }
            set
            {
                dataProviderAssembly = value;
            }
        }
        private static string dataProviderTypeName;
        public static string DataProviderTypeName
        {
            get
            {
                return dataProviderTypeName;
            }
            set
            {
                dataProviderTypeName = value;
            }
        }

        private static string dataFactoryGeneralDAL;
        public static string DataFactoryGeneralDAL
        {
            get
            {
                return dataFactoryGeneralDAL;
            }
            set
            {
                dataFactoryGeneralDAL = value;
            }
        }
        private static string dataFactoryQueryDAL;
        public static string DataFactoryQueryDAL
        {
            get
            {
                return dataFactoryQueryDAL;
            }
            set
            {
                dataFactoryQueryDAL = value;
            }
        }

        private static string dataFactoryEntityAssembly;
        public static string DataFactoryEntityAssembly
        {
            get
            {
                return dataFactoryEntityAssembly;
            }

            set
            {
                dataFactoryEntityAssembly = value;
            }
        }

        private static string dataFactoryEntityNamespace;
        public static string DataFactoryEntityNamespace
        {
            get
            {
                return dataFactoryEntityNamespace;
            }

            set
            {
                dataFactoryEntityNamespace = value;
            }
        }


    }
}
