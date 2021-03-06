﻿/**
 *   Copyright 2013
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NinthChevron.Data.Expressions
{
    public enum ExtendedExpressionType
    {
        Query = 5000,
        Update,
        UpdateSelect,
        Insert,
        InsertSelect,
        Delete,
        Union,
        Join,
        Projection,
        GroupByProjection,
        UnionProjection,
        Entity,
        EntityReference,
        Count,
        Sum,
        Min,
        Max,
        Average,
        OrderBy,
        Exists,
        AliasedExpression,
        Keyword
    }
}
