using Nice.DataAccess.Attributes;
using Nice.DataAccess.Models;
using Nice.DataAccess.TypeEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nice.DataAccess.DAL
{
    public class DataEntityFactory
    {
        public static Mapping<string, string> EntityAndTables;
        public static Mapping<string, string> PropertyAndColumns;
        public static void Create()
        {
            string assemblyName = DatabaseSettings.DataFactoryEntityAssembly;
            string namespaceName = DatabaseSettings.DataFactoryEntityNamespace;
            if (string.IsNullOrEmpty(assemblyName))
                throw new ArgumentNullException();
            Create(assemblyName, namespaceName);
        }
        public static void Create(string assemblyName, string namespaceName)
        {
            if (!string.IsNullOrEmpty(assemblyName))
            {
                IEnumerable<Type> types = null;
                if (string.IsNullOrEmpty(namespaceName))
                    types = Assembly.Load(assemblyName).GetTypes().Where(t => t.IsClass);
                else
                    types = Assembly.Load(assemblyName).GetTypes().Where(t => t.IsClass && t.Namespace == namespaceName);

                EntityAndTables = new Mapping<string, string>(types.Count());
                PropertyAndColumns = new Mapping<string, string>();
                string typeName = null;
                string tableName = null;
                PropertyInfo[] properties = null;
                string propertyName = null;
                string columnName = null;

                TableAttribute attr = null;
                foreach (Type type in types)
                {
                    attr = type.GetCustomAttribute<TableAttribute>();
                    if (attr == null) continue;
                    typeName = type.Name;
                    tableName = attr.Name;

                    EntityAndTables.Add(typeName.ToUpper(), attr.Name ?? typeName);
                    properties = type.GetProperties();
                    foreach (PropertyInfo pi in properties)
                    {
                        ColumnAttribute attri = pi.GetCustomAttribute<ColumnAttribute>();
                        propertyName = pi.Name;
                        if (attri == null)
                            columnName = propertyName;
                        else
                            columnName = attri.Name;
                        PropertyAndColumns.Add(string.Format("{0}.{1}", typeName, propertyName.ToUpper()), string.Format("{0}.{1}", typeName, columnName));
                    }
                }
            }
        }


    }

    public class DataFactory<T> where T : TEntity
    {
        private static Assembly CurrentAssembly = Assembly.Load(DatabaseSettings.DataProviderAssembly);
        public static IGeneralDAL<T> Create()
        {
            #region DELETE
            //Type genericType = CurrentAssembly.GetType(string.Format("{0}`1", GeneralDALClassName));
            //genericType = genericType.MakeGenericType(typeof(T));
            //return Activator.CreateInstance(genericType) as IGeneralDAL<T>;
            #endregion
            return CurrentAssembly.CreateInstance(string.Format("{0}`1[[{1}]]", DatabaseSettings.DataFactoryGeneralDAL, typeof(T).AssemblyQualifiedName)) as IGeneralDAL<T>;
        }
    }

    public class QueryFactory<T> where T : new()
    {
        private static Assembly CurrentAssembly = Assembly.Load(DatabaseSettings.DataProviderAssembly);
        public static IQueryDAL<T> Create()
        {
            return CurrentAssembly.CreateInstance(string.Format("{0}`1[[{1}]]", DatabaseSettings.DataFactoryQueryDAL, typeof(T).AssemblyQualifiedName)) as IQueryDAL<T>;
        }
    }
    public class QueryFactory
    {
        private static Assembly CurrentAssembly = Assembly.Load(DatabaseSettings.DataProviderAssembly);
        public static IQueryDAL Create()
        {
            return CurrentAssembly.CreateInstance(DatabaseSettings.DataFactoryQueryDAL) as IQueryDAL;
        }
    }
}
