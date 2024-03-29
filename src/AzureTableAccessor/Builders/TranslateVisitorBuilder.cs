﻿namespace AzureTableAccessor.Builders
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Azure.Data.Tables;

    internal class TranslateVisitorBuilder<TTo>
           where TTo : class, ITableEntity
    {
        private readonly List<MemberVisitorFactory> _factories = new List<MemberVisitorFactory>();

        public ParameterExpression ParameterExpression { get; } = Expression.Parameter(typeof(TTo));

        public void Add(MemberExpression from, MemberExpression to)
        {
            _factories.Add(new MemberVisitorFactory(() => MemberVisitor.Create(from, to, ParameterExpression)));
        }

        public TranslateVisitor Build() => new TranslateVisitor(_factories);
    }
}
