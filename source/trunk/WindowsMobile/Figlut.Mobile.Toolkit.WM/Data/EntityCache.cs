﻿namespace Figlut.Mobile.Toolkit.Data
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Data;
    using Figlut.Mobile.Toolkit.Data.CSV;
    using Figlut.Mobile.Toolkit.Utilities;
    using Figlut.Mobile.Toolkit.Utilities.Serialization;
    using Figlut.Mobile.Toolkit.Web.Client.REST;
    using Figlut.Mobile.Toolkit.Web.Client.DataProgress;
    using System.Diagnostics;

    #endregion //Using Directives

    public class EntityCache<K, E> : IEnumerable<E> where E : class
    {
        #region Constructors

        public EntityCache()
        {
            Type type = this.GetType();
            _name = DataShaper.ShapeCamelCaseString(type.Name);
            _defaultFilePath = Path.Combine(Information.GetExecutingDirectory(), string.Format("{0}.xml", type.Name));

            _entities = new Dictionary<K, E>();
            _addedEntities = new Dictionary<K, E>();
            _deletedEntities = new Dictionary<K, E>();
            _updatedEntities = new Dictionary<K, E>();
        }

        public EntityCache(string name)
        {
            Type type = this.GetType();
            _name = string.IsNullOrEmpty(name) ? DataShaper.ShapeCamelCaseString(type.Name) : name;
            _defaultFilePath = Path.Combine(Information.GetExecutingDirectory(), string.Format("{0}.xml", type.Name));

            _entities = new Dictionary<K, E>();
            _addedEntities = new Dictionary<K, E>();
            _deletedEntities = new Dictionary<K, E>();
            _updatedEntities = new Dictionary<K, E>();
        }

        public EntityCache(string name, string defaultFilePath)
        {
            _name = name;
            _defaultFilePath = defaultFilePath;
            _entities = new Dictionary<K, E>();
            _addedEntities = new Dictionary<K, E>();
            _deletedEntities = new Dictionary<K, E>();
            _updatedEntities = new Dictionary<K, E>();
        }

        #endregion //Constructors

        #region Events

        public event ServiceTransactionCompletedHandler OnServiceTransactionCompleted;

        #endregion //Events

        #region Fields

        protected string _name;
        protected string _defaultFilePath;
        protected Dictionary<K, E> _entities;
        protected Dictionary<K, E> _addedEntities;
        protected Dictionary<K, E> _deletedEntities;
        protected Dictionary<K, E> _updatedEntities;

        #endregion //Fields

        #region Properties

        public string Name
        {
            get { return _name; }
        }

        public string DefaultFilePath
        {
            get { return _defaultFilePath; }
        }

        public List<E> Entities
        {
            get { return _entities.Values.ToList(); }
        }

        public List<E> AddedEntities
        {
            get { return _addedEntities.Values.ToList(); }
        }

        public List<E> DeletedEntities
        {
            get { return _deletedEntities.Values.ToList(); }
        }

        public int Count
        {
            get { return _entities.Count; }
        }

        #endregion //Properties

        #region Indexers Region

        public E this[K id]
        {
            get
            {
                if (_entities.ContainsKey(id))
                {
                    return _entities[id];
                }
                return null;
            }
        }

        #endregion //Indexers Region

        #region Methods

        public List<K> GetEntitiesKeys()
        {
            return _entities.Keys.ToList();
        }

        public virtual void OverrideFromList(List<E> entities)
        {
            Dictionary<K, E> result = new Dictionary<K, E>();
            entities.ForEach(e => result.Add(GetSurrogateKeyValue(e), e));
            Clear();
            _entities = null;
            _entities = result;
        }

        public virtual K GetSurrogateKeyValue(E e)
        {
            Type type = e.GetType();
            foreach (PropertyInfo p in type.GetProperties())
            {
                if (p.PropertyType == typeof(K))
                {
                    return (K)p.GetValue(e, null);
                }
            }
            throw new Exception(string.Format("Could not find surrogate key for entity {0}.", type.FullName));
        }

        /// <summary>
        /// Adds the specified entity to collection of updated entities to be used when saving to the server.
        /// </summary>
        /// <param name="entityId">The ID of the entity that was updated.</param>
        /// <param name="e">The entity that was update.</param>
        public virtual void NotifyEntityUpdated(K entityId, E e)
        {
            if (!_addedEntities.ContainsKey(entityId) && !_updatedEntities.ContainsKey(entityId))
            {
                _updatedEntities.Add(entityId, e);
            }
        }

        public virtual void Add(E e)
        {
            K id = GetSurrogateKeyValue(e);
            Add(id, e);
        }

        public virtual void Add(K id, E e)
        {
            if (_entities.ContainsKey(id))
            {
                throw new ArgumentException(string.Format(
                    "An entity with the ID {0} already exists in this collection.",
                    id));
            }
            _entities.Add(id, e);
            _addedEntities.Add(id, e);
        }

        public virtual void Delete(K id)
        {
            if (!_entities.ContainsKey(id))
            {
                throw new NullReferenceException(string.Format(
                    "Could not find entity with ID {0} in this collection to be deleted.",
                    id));
            }
            E e = _entities[id];
            _entities.Remove(id);
            if (_addedEntities.ContainsKey(id))
            {
                _addedEntities.Remove(id);
            }
            else
            {
                _deletedEntities.Add(id, e);
            }
        }

        public virtual bool Exists(K id)
        {
            return _entities.ContainsKey(id);
        }

        public virtual void Clear()
        {
            _entities.Clear();
            _addedEntities.Clear();
            _deletedEntities.Clear();
            GC.Collect();
        }

        public virtual E GetEntityByProperty(string propertyName, object propertyValue, bool exactMatch)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(propertyName, propertyValue);
            List<E> result = GetEntitiesByProperties(properties, exactMatch);
            return result.Count < 1 ? null : result[0];
        }

        public virtual List<E> GetEntitiesByProperties(Dictionary<string, object> properties, bool exactMatch)
        {
            Type t = typeof(E);
            List<E> result = new List<E>();
            if (properties == null) //Return all the items.
            {
                _entities.Values.ToList().ForEach(p => result.Add(p));
                return result;
            }
            List<PropertyInfo> propertyList = new List<PropertyInfo>();
            foreach (string propertyName in properties.Keys)
            {
                PropertyInfo p = t.GetProperty(propertyName);
                if (p == null)
                {
                    throw new NullReferenceException(string.Format(
                        "Could not find property with name {0} on entity type {1}.",
                        propertyName,
                        t.FullName));
                }
                propertyList.Add(p);
            }
            if (exactMatch)
            {
                foreach (E e in _entities.Values)
                {
                    bool include = true;
                    foreach (PropertyInfo p in propertyList)
                    {
                        object entityProvidedValue = p.GetValue(e, null);
                        object providedPropertyValue = properties[p.Name];
                        if (entityProvidedValue == null)
                        {
                            if (providedPropertyValue == null)
                            {
                                continue;
                            }
                            else
                            {
                                include = false;
                                break;
                            }
                        }
                        if (!entityProvidedValue.Equals(providedPropertyValue))
                        {
                            include = false;
                            break;
                        }
                    }
                    if (include)
                    {
                        result.Add(e);
                    }
                }
            }
            else
            {
                foreach (E e in _entities.Values)
                {
                    bool include = true;
                    foreach (PropertyInfo p in propertyList)
                    {
                        object entityProvidedValue = p.GetValue(e, null);
                        object providedPropertyValue = properties[p.Name];
                        if (entityProvidedValue == null)
                        {
                            if (providedPropertyValue == null)
                            {
                                continue;
                            }
                            else
                            {
                                include = false;
                                break;
                            }
                        }
                        if (p.PropertyType == typeof(string)) //String (contains) comparison only on strings.
                        {
                            string entityValueStr = entityProvidedValue == null ? string.Empty : entityProvidedValue.ToString().ToLower();
                            if (!entityValueStr.Contains(providedPropertyValue.ToString().ToLower()))
                            {
                                include = false;
                                break;
                            }
                        }
                        else //Object comparison on anything else
                        {
                            if (!entityProvidedValue.Equals(providedPropertyValue))
                            {
                                include = false;
                                break;
                            }
                        }
                    }
                    if (include)
                    {
                        result.Add(e);
                    }
                }
            }
            return result;
        }

        public virtual bool PropertyValueExists(string propertyName, object propertyValue)
        {
            Type t = typeof(E);
            PropertyInfo p = t.GetProperty(propertyName);
            if (p == null)
            {
                throw new NullReferenceException(string.Format(
                    "Could not find property with name {0} on entity type {1}.",
                    propertyName,
                    t.FullName));
            }
            foreach (E e in _entities.Values)
            {
                object value = p.GetValue(e, null);
                if (propertyValue.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual DataTable GetDataTable(Dictionary<string, object> properties, bool exactMatch, bool shapeColumnNames)
        {
            DataTable result = EntityReader<E>.GetDataTable(shapeColumnNames);
            List<E> entities = GetEntitiesByProperties(properties, exactMatch);
            foreach (E e in entities)
            {
                DataRow row = EntityReader<E>.PopulateDataRow(e, result.NewRow(), shapeColumnNames);
                result.Rows.Add(row);
            }
            return result;
        }

        public virtual Dictionary<K, E> DataTableToDictionary(DataTable table)
        {
            Dictionary<K, E> result = new Dictionary<K, E>();
            foreach (DataRow row in table.Rows)
            {
                E e = EntityReader<E>.PopulateFromDataRow(Activator.CreateInstance<E>(), row);
                K surrogateKey = GetSurrogateKeyValue(e);
                if (result.ContainsKey(surrogateKey))
                {
                    throw new Exception(string.Format(
                        "May not load more than one {0} with the same Id {1}.", 
                        typeof(E).FullName,
                        surrogateKey.ToString()));
                }
                result.Add(surrogateKey, e);
            }
            return result;
        }

        public virtual void ExportToCsv(
            string filePath, 
            Dictionary<string, object> properties,
            bool exactMatch, 
            bool shapeColumnNames)
        {
            CsvWriter.WriteToFile(
                GetDataTable(properties, exactMatch, shapeColumnNames),
                true, 
                true, 
                filePath, 
                null);
        }

        public virtual StringWriter ExportToStringWriter(
            Dictionary<string, object> properties,
            bool exactMatch,
            bool shapeColumnNames)
        {
            StringWriter result = new StringWriter();
            CsvWriter.WriteToStream(
                result,
                GetDataTable(properties, exactMatch, shapeColumnNames),
                true, 
                true, 
                null);
            return result;
        }

        public virtual MemoryStream ExportToStream(
            Dictionary<string, object> properties,
            bool exactMatch,
            bool shapeColumnNames)
        {
            return new MemoryStream((new ASCIIEncoding()).GetBytes(
                ExportToStringWriter(properties, exactMatch, shapeColumnNames).GetStringBuilder().ToString()));
        }

        public virtual void ImportFromCsv(string filePath, bool shapeColumnNames)
        {
            _entities = DataTableToDictionary(
                CsvParser.ParseFromFile(filePath, true, EntityReader<E>.GetDataTable(shapeColumnNames).Columns.Count));
        }

        /// <summary>
        /// Default implementation of this method does nothing, except display a message that this method has no implementation.
        /// You need to override and provide your own implementation for this method.
        /// </summary>
        /// <returns>Provide your own logic.</returns>
        public virtual bool RefreshFromServer()
        {
            UIHelper.DisplayInformation(string.Format(
                "{0} has no implementation for saving to server.",
                this.GetType().Name));
            return true;
        }

        /// <summary>
        /// Default implementation of this method does nothing, except display a message that this method has no implementation.
        /// You need to override and provide your own implementation for this method.
        /// </summary>
        /// <returns>Provide your own logic.</returns>
        public virtual bool SaveToServer()
        {
            UIHelper.DisplayInformation(string.Format(
                "{0} has no implementation for saving to server.",
                this.GetType().Name));
            return true;
        }

        /// <summary>
        /// Refreshes this cache with entities from the server using the supplied RestWebServiceClient.
        /// N.B. A REST web service needs to exist on the web server with methods matching those expected by the RestWebService.
        /// </summary>
        /// <param name="restWebServiceClient">The rest web service client to be used to refresh entities from the server.</param>
        /// <returns>Returns true if it completed successfully.</returns>
        public virtual bool RefreshFromServer(RestWebServiceClient restWebServiceClient)
        {
            OverrideFromList(restWebServiceClient.GetAllEntities<E>());
            return true;
        }

        /// <summary>
        /// Processes all the deleted, added and updated entities in this cache by deleting, adding and updating them on the server
        /// using the supplied RestWebServiceClient.
        /// N.B. A REST web service needs to exist on the web server with methods matching those expected by the RestWebService.
        /// </summary>
        /// <param name="restWebServiceClient">The rest web service client to be used to save the entities in the cache to the server.</param>
        /// <returns>Returns true if it completed successfully.</returns>
        public virtual bool SaveToServer(RestWebServiceClient restWebServiceClient)
        {
            ProcessEntitiesToServer(restWebServiceClient, ServiceDataTransactionType.Delete, _deletedEntities);
            ProcessEntitiesToServer(restWebServiceClient, ServiceDataTransactionType.Add, _addedEntities);
            ProcessEntitiesToServer(restWebServiceClient, ServiceDataTransactionType.Update, _updatedEntities);
            return true;
        }

        private void ProcessEntitiesToServer(
            RestWebServiceClient restWebServiceClient,
            ServiceDataTransactionType transactionType,
            Dictionary<K, E> entitiesToProcess)
        {
            List<E> entityQueue = entitiesToProcess.Values.ToList();
            int entityQueueCount = entityQueue.Count;
            int i = 0;
            while (entityQueue.Count > 0) //Process the queue.
            {
                E e = entityQueue[0];
                i++;
                switch (transactionType)
                {
                    case ServiceDataTransactionType.Delete:
                        restWebServiceClient.DeleteById<E>(GetSurrogateKeyValue(e));
                        break;
                    case ServiceDataTransactionType.Add:
                        restWebServiceClient.PutEntity<E>(e);
                        break;
                    case ServiceDataTransactionType.Update:
                        restWebServiceClient.PutEntity<E>(e);
                        break;
                    default:
                        throw new ArgumentException(string.Format("Invalid {0} of {1}.", typeof(ServiceDataTransactionType).Name, transactionType.ToString()));
                }
                entityQueue.Remove(e); //Remove it from the queue of entities to be processed.
                entitiesToProcess.Remove(GetSurrogateKeyValue(e)); //Remove it from this cache's entities to be processed.
                if (OnServiceTransactionCompleted != null)
                {
                    OnServiceTransactionCompleted(this, new ServiceTransactionEventArgs(transactionType, entityQueueCount, i, e)); //Inform subscribers that the entity has been successfully processed.
                }
            }
            Debug.Assert(entitiesToProcess.Count == 0);
        }

        public virtual void LoadFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                if (string.IsNullOrEmpty(_defaultFilePath))
                {
                    throw new NullReferenceException("File path not specified for entity cache.");
                }
                filePath = _defaultFilePath;
            }
            if (!File.Exists(filePath))
            {
                SaveToFile(filePath);
            }
            EntityCache<K, E> entities = 
                (EntityCache<K, E>)GOC.Instance.GetSerializer(SerializerType.XML).DeserializeFromFile(this.GetType(), filePath);
            Clear();
            foreach (E e in entities)
            {
                _entities.Add(GetSurrogateKeyValue(e), e);
            }
        }

        public virtual void LoadFromText(string text)
        {
            EntityCache<K, E> entities =
                (EntityCache<K, E>)GOC.Instance.GetSerializer(SerializerType.XML).DeserializeFromText(this.GetType(), text);
            Clear();
            foreach (E e in entities)
            {
                _entities.Add(GetSurrogateKeyValue(e), e);
            }
        }

        public virtual void SaveToFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                if (string.IsNullOrEmpty(_defaultFilePath))
                {
                    throw new NullReferenceException("File path not specified for entity cache.");
                }
                filePath = _defaultFilePath;
            }
            GOC.Instance.GetSerializer(SerializerType.XML).SerializeToFile(this, filePath);
        }

        public IEnumerator<E> GetEnumerator()
        {
            return _entities.Values.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion //Methods
    }
}
