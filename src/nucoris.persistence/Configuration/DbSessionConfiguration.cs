using System;
using System.Collections.Generic;
using nucoris.application.interfaces;

namespace nucoris.persistence
{
    /// <summary>
    /// This class handles the access to the details of 
    /// how each domain type is stored in nucoris Cosmos DB (collection id, partition key and our custom property docType).
    /// The details themselves are encapsulated in DbSessionConfigurationFactory.
    /// See DbSessionConfigurationFactory for further explanation.
    /// </summary>
    public class DbSessionConfiguration : IDbSessionConfiguration
    {
        private readonly Dictionary<string, IDbTypeConfigurationBuilder> _cachedConfigBuilders
            = new Dictionary<string, IDbTypeConfigurationBuilder>();

        public DbTypeConfiguration GetDBConfiguration<T>(T item) where T : class
        {
            // In general, the Cosmos DB storage details depend on the type of object being stored
            //  (e.g. patient Id for patient data), so to retrieve such details 
            //  we get an instance of its type's configuration builder and ask it
            //  to build the configuration for the specific instance received:
            IDbTypeConfigurationBuilder configBuilder = GetBuilder(item.GetType());

            return configBuilder.BuildFor(item);
        }

        public DbTypeConfiguration GetDBConfiguration(Type itemType, string itemId)
        {
            IDbTypeConfigurationBuilder configBuilder = GetBuilder(itemType);

            return configBuilder.BuildForId(itemType, itemId);
        }

        public DbTypeConfiguration GetDBConfiguration<T>() where T : class
        {
            // For some types we know their configuration does not depend on item id,
            //  so we pass no id to builder
            IDbTypeConfigurationBuilder configBuilder = GetBuilder(typeof(T));

            return configBuilder.BuildForType(typeof(T));
        }

        public string GetDBDocType<T>()
        {
            // For query items we know their configuration does not depend on item id,
            //  so we pass no id to builder
            IDbTypeConfigurationBuilder configBuilder = GetBuilder(typeof(T));

            return configBuilder.BuildDocType(typeof(T));
        }

        public void AddConfigurationBuilder<T>(
            string collection, 
            Func<T,string> partitionKeyItemBuilder, 
            Func<Type,string> docTypeBuilder,
            Func<string, string> partitionKeyItemIdBuilder = null)
            where T : class
        {
            _cachedConfigBuilders[ GetCacheKey(typeof(T)) ] = new DbTypeConfigurationBuilder<T>(
                collection, partitionKeyItemBuilder, docTypeBuilder, partitionKeyItemIdBuilder);
        }

        public void AddConfigurationBuilder<T>(
            string collection,
            Func<Type, string> partitionKeyTypeBuilder,
            Func<Type, string> docTypeBuilder)
            where T : class
        {
            _cachedConfigBuilders[GetCacheKey(typeof(T))] = new DbTypeConfigurationBuilder<T>(
                collection, partitionKeyTypeBuilder, docTypeBuilder);
        }


        private IDbTypeConfigurationBuilder GetBuilder(Type itemType)
        {
            // Since most domain types being persisted derive from common classes,
            //  it's likely that they share the configuration of their base class.
            // So here we search for a configuration builder of the requested item type,
            //  but if not found we traverse up the inheritance chart until we find a base class
            //  with documented configuration details.
            // If this fails, we'll then try with the interfaces it implements...
            // If we finally find it we store it in the cache, to save a few CPU cycles next time...

            Type type = itemType;
            IDbTypeConfigurationBuilder configBuilder = null;
            string key;
            bool missingFromCache = false;

            do
            {
                // Try first with the type and its descendents
                key = GetCacheKey(type);
                if (!_cachedConfigBuilders.TryGetValue(key, out configBuilder))
                {
                    type = type.BaseType;
                    missingFromCache = true;
                }
            } while (configBuilder == null && type != null);

            if (configBuilder == null)
            {
                // Try with its implemented interfaces:
                var interfaces = itemType.GetInterfaces();
                if( interfaces != null)
                {
                    foreach(var @interface in interfaces)
                    {
                        key = GetCacheKey(@interface);
                        if (_cachedConfigBuilders.TryGetValue(key, out configBuilder))
                        {
                            break;
                        }
                    }
                }
            }

            if (configBuilder == null)
            {
                throw new InvalidOperationException(
                    $"No configuration registered for type '{itemType.FullName}' or its base class.");
            }
            else if( missingFromCache)
            {
                _cachedConfigBuilders[GetCacheKey(itemType)] = configBuilder;
            }

            return configBuilder;
        }

        private string GetCacheKey(Type type)
        {
            return type.FullName;
        }
    }
}
