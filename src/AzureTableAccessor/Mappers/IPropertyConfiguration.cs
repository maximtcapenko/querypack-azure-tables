namespace AzureTableAccessor.Mappers
{
    using System;
    
    internal interface IPropertyConfiguration<TEntity>
    {
        PropertyConfigType PropertyConfigType { get; }
        /// <summary>
        /// Name of binding table column
        /// </summary>
        string BindingName { get; }
        string PropertyName { get; }
        Type Type { get; }
        object GetValue(TEntity entity);
        bool IsConfigured(string propertyName);
    }

    internal interface IPropertyConfiguration<TEntity, TProjection>
    {
        string EntityPropertyBindingName { get; }
        string PropertyName { get; }
    }
}