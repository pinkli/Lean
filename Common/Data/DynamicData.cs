﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace QuantConnect.Data
{
    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Dynamic Data Class: Accept flexible data, adapting to the columns provided by source.
    /// </summary>
    /// <remarks>Intended for use with Quandl class.</remarks>
    public abstract class DynamicData : BaseData, IDynamicMetaObjectProvider
    {
        /******************************************************** 
        * CLASS PRIVATE VARIABLES
        *********************************************************/
        private readonly IDictionary<string, object> _storage = new Dictionary<string, object>();


        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Get the metaObject required for Dynamism.
        /// </summary>
        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicDataMetaObject(parameter, this);
        }

        /// <summary>
        /// Set the core properties of basedata object:
        /// </summary>
        /// <param name="name">string property name</param>
        /// <param name="value">object property value</param>
        /// <returns>return true if set successfully.</returns>
        public object SetProperty(string name, object value)
        {
            if (name == "Time")
            {
                return Time = (DateTime)value;
            }
            if (name == "Value")
            {
                return Value = (decimal)value;
            }
            if (name == "Symbol")
            {
                return Symbol = (string)value;
            }
            // reaodnly
            //if (name == "Price")
            //{
            //    return Price = (decimal) value;
            //}
            _storage[name] = value;
            return value;
        }

        /// <summary>
        /// Fetch the core properties of the underlying base data object:
        /// </summary>
        /// <param name="name">BaseData Property name</param>
        /// <returns>object value of BaseData</returns>
        public object GetProperty(string name)
        {
            // redirect these calls to the base types properties
            if (name == "Time")
            {
                return Time;
            }
            if (name == "Value")
            {
                return Value;
            }
            if (name == "Symbol")
            {
                return Symbol;
            }
            if (name == "Price")
            {
                return Price;
            }
            return _storage[name];
        }


        /// <summary>
        /// Custom implementation of Dynamic Data MetaObject
        /// </summary>
        private class DynamicDataMetaObject : DynamicMetaObject
        {
            private static readonly MethodInfo SetPropertyMethodInfo = typeof(DynamicData).GetMethod("SetProperty");
            private static readonly MethodInfo GetPropertyMethodInfo = typeof(DynamicData).GetMethod("GetProperty");

            public DynamicDataMetaObject(Expression expression, DynamicData instance)
                : base(expression, BindingRestrictions.Empty, instance)
            {
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                // we need to build up an expression tree that represents accessing our instance
                var restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

                var args = new Expression[]
                {
                    // this is the name of the property to set
                    Expression.Constant(binder.Name),

                    // this is the value
                    Expression.Convert(value.Expression, typeof (object))
                };

                // set the 'this' reference
                var self = Expression.Convert(Expression, LimitType);

                var call = Expression.Call(self, SetPropertyMethodInfo, args);

                return new DynamicMetaObject(call, restrictions);
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                // we need to build up an expression tree that represents accessing our instance
                var restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

                // arguments for 'call'
                var args = new Expression[]
                {
                    // this is the name of the property to set
                    Expression.Constant(binder.Name)
                };

                // set the 'this' reference
                var self = Expression.Convert(Expression, LimitType);

                var call = Expression.Call(self, GetPropertyMethodInfo, args);

                return new DynamicMetaObject(call, restrictions);
            }
        }
    }
}
