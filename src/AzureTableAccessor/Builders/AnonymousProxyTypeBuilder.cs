namespace AzureTableAccessor.Builders
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Reflection;
    using System;
    using Azure.Data.Tables;
    
    internal class AnonymousProxyTypeBuilder
    {
        private static AssemblyName _assemblyName = new AssemblyName() { Name = "AnonymousProxyTypes" };
        private static string DefaultTypeNamePrefix = "Dynamic";
        private static ModuleBuilder _moduleBuilder = null;
        private static ConcurrentDictionary<string, Type> _typeCache = new ConcurrentDictionary<string, Type>();

        static AnonymousProxyTypeBuilder()
        {
            _moduleBuilder = AssemblyBuilder
                .DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run)
                .DefineDynamicModule(_assemblyName.Name);
        }


        public string GetName() => $"{DefaultTypeNamePrefix}_{string.Join(";", _definedMembers.Select(e => $"{e.Key}_{e.Value.Name}")).Hash()}";

        public static AnonymousProxyTypeBuilder GetBuilder() => new AnonymousProxyTypeBuilder();


        private Dictionary<string, Type> _definedMembers = new Dictionary<string, Type>();

        public void DefineField(string name, Type type)
        {
            _definedMembers[name] = type;
        }

        public Type CreateType()
        {
            var key = GetName();
            if (!_typeCache.ContainsKey(key))
            {
                var typeBuilder = GetTypeBuilder(key);

                foreach (var member in _definedMembers)
                {
                    var visitor = new PropertyTypeBuilderVisitor(member.Key, member.Value);
                    visitor.Visit(typeBuilder);
                }
                _typeCache.AddOrUpdate(key, typeBuilder.CreateType(), (k, t) => t);
            }

            return _typeCache[key];
        }

        private TypeBuilder GetTypeBuilder(string typeName)
        {
            var typeBuilder = _moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            typeBuilder.AddInterfaceImplementation(typeof(ITableEntity));

            foreach (var property in typeof(ITableEntity).GetProperties())
            {
                var visitor = new PropertyTypeBuilderVisitor(property.Name, property.PropertyType);
                visitor.Visit(typeBuilder);
            }

            return typeBuilder;
        }
    }
}