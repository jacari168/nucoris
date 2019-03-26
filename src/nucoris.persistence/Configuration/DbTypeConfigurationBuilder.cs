using Ardalis.GuardClauses;
using System;

namespace nucoris.persistence
{
    /// <summary>
    /// This class keeps for a given type the methods to get its storage details
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DbTypeConfigurationBuilder<T> : IDbTypeConfigurationBuilder where T : class
    {
        private readonly string _collection;
        private readonly Func<T, string> _partitionKeyItemBuilder;
        private readonly Func<Type, string> _docTypeBuilder;
        private readonly Func<string, string> _partitionKeyItemIdBuilder;
        private readonly Func<Type, string> _partitionKeyTypeBuilder;

        // In some types the partition key depends on an item id (e.g. for all patient data it's "P_<patientId>".
        // For others only on type name (e.g. reference data such as "R_Medication" or view items "V_ActiveOrdersView")
        // So we have two different constructors to cover both cases properly

        public DbTypeConfigurationBuilder(
            string collection, 
            Func<T, string> partitionKeyItemBuilder,
            Func<Type, string> docTypeBuilder,
            Func<string, string> partitionKeyItemIdBuilder)
        {
            _collection = collection;
            _partitionKeyItemBuilder = partitionKeyItemBuilder;
            _docTypeBuilder = docTypeBuilder;
            _partitionKeyItemIdBuilder = partitionKeyItemIdBuilder;
        }

        public DbTypeConfigurationBuilder(
            string collection,
            Func<Type, string> partitionKeyTypeBuilder,
            Func<Type, string> docTypeBuilder)
        {
            _collection = collection;
            _partitionKeyTypeBuilder = partitionKeyTypeBuilder;
            _docTypeBuilder = docTypeBuilder;
        }

        public DbTypeConfiguration BuildFor(object o)
        {
            T item = o as T;

            if (_partitionKeyItemBuilder != null)
            {
                return new DbTypeConfiguration(
                    _collection, _partitionKeyItemBuilder(item), _docTypeBuilder(item.GetType()));
            }
            else if( _partitionKeyTypeBuilder != null)
            {
                return new DbTypeConfiguration(
                    _collection, _partitionKeyTypeBuilder(item.GetType()), _docTypeBuilder(item.GetType()));
            }
            else
            {
                throw new InvalidOperationException($"No valid partitionKey builder for type {o.GetType().FullName}");
            }
        }

        public DbTypeConfiguration BuildForId(Type type, string id)
        {
            if (_partitionKeyItemIdBuilder != null)
            {
                return new DbTypeConfiguration(
                    _collection, _partitionKeyItemIdBuilder(id), _docTypeBuilder(type));
            }
            else if (_partitionKeyTypeBuilder != null)
            {
                return new DbTypeConfiguration(
                    _collection, _partitionKeyTypeBuilder(type), _docTypeBuilder(type));
            }
            else
            {
                throw new InvalidOperationException($"No valid partitionKey builder for type {type.FullName}");
            }
        }

        public DbTypeConfiguration BuildForType(Type type)
        {
            if (_partitionKeyTypeBuilder != null)
            {
                return new DbTypeConfiguration(
                    _collection, _partitionKeyTypeBuilder(type), _docTypeBuilder(type));
            }
            else
            {
                throw new InvalidOperationException($"No valid partitionKey builder for type {type.FullName}");
            }
        }

        public string BuildDocType(Type type)
        {
            return _docTypeBuilder(type);
        }
    }
}
