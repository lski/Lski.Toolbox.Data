Lski.Toolbox.Data
=================

Low level utility functions for working with directly with databases. Based around the DbConnection, DbTransaction and IDataReader classes and interfaces.

- Disposible Transaction Class
- DataLoader
- IDataReader Extensions
- Connection Utilities
- BasicContext
- Todo

### Disposible Transaction Class

The Transaction class in Lski.Toolbox.Data.Connections is a wrapper for a DbTransaction object that implements the IDisposible interface.

This allows transactions be used with a using statement with auto rollback. If not explicitly committed prior to leaving the using statement the transaction will rollback. Helpful for handling exceptions correctly.

Can be used with anything that works with raw connections, like Dapper or PetaPoco as well as hand coded DbCommands

#### Example

    using(var tran = new Transaction(myDbConnection)) {

        // Do Stuff

        tran.Commit();
    }

    // If reaches here without hitting tran.Commit(); then the transaction is rolled back automatically

__Note__ If not already open, the connection passed in is opened and then closed on dispose. However if the connection object was explicitly opened prior to a Transaction object being created it is not closed on dispose.

### DataLoader

Allows loading data of a record in an IDataReader into an .Net object. Optionally accepting mappings for field names against property names.

#### Example

Load a list directly from the reader

    using(var reader = command.ExecuteReader()) {

        var loader = reader.CreateLoader<MyClass>();

        var objs = loader.Load(new List<MyClass>());
    }

Load a list, but with more control over each iteration, plus gives control over which objects to store or when to cancel the reader etc.

    using(var reader = command.ExecuteReader()) {

        var loader = reader.CreateLoader<MyClass>();
        var objs = new List<MyClass>();

        // loop through until the reader is empty
        while(reader.Read()) {

            // Automatically reads the next record and returns the object
            var myObj = loader.Load();

            objs.Add(myObj);
        }
    }

### IDataReader Extensions

There are utility extension functions for returning data about fields in an open IDataReader. reader.GetFieldNames() and reader.GetFieldInfo() which return IEnumerable's allowing you to loop through an IDataReaders fields using linq.

- FieldNames()

    Returns a list of field names.

- FieldInfo()

    Returns a list of DataFieldInfo objects containing meta data about each of fields in the reader. DataFieldInfo objects include the position of the field in the reader, the name of the property and the System.Type of the field.

### Connection Utilities

Provides some useful functions for getting the correct connection string and provider using the Connection string name from the configuration file.

### BasicContext

Provides a lightweight disposable wrapper around the DbConnection object that can be subclassed to give a named context to a connection which is useful for IoC. When disposed the BaseContext will close the connection if open __Note__ By default the connection is not opended by default otherwise a premature connection could be opened to early plus most libraries e.g. Dapper have an auto open facility.

Accepts either the connection string name to be resolved from configuration or a DbConnection object with a connection string set.

#### Example

Subclass

    public class MyContext : BasicConext {
        public MyContext() : base("myConnectionStringName") {}
    }

    // or
    public class MyContext : BasicConext {
        public MyContext() : base(new SqlConnetion("aConnectionString")) {}
    }

Usage

    using(var ctx = new MyContext()) {

        var data = ctx.Connection.Query<MyObject>("select * from MyTable");
    }

### Todo

- Evaluate changes needed to make the retrieval of connection object from connection string name
