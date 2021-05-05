using System;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.Documents.Operations;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace corpus_builder_api.ServiceFunctions
{
	public static class RavenHelper
	{
		public static void EnsureDatabaseExists(IDocumentStore store, string database = "Manuscripts", bool createDatabaseIfNotExists = true)
		{
		    database = database ?? store.Database;

		    if (string.IsNullOrWhiteSpace(database))
		        throw new ArgumentException("Value cannot be null or whitespace.", nameof(database));

		    try
		    {
		        store.Maintenance.ForDatabase(database).Send(new GetStatisticsOperation());
		    }
		    catch (DatabaseDoesNotExistException)
		    {
		        if (createDatabaseIfNotExists == false)
		            throw;

		        try
		        {
		            store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(database)));
		        }
		        catch (ConcurrencyException)
		        {
		            // The database was already created before calling CreateDatabaseOperation
		        }

		    }
		}
	}
}

