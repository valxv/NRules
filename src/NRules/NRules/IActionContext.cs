﻿using System.Collections.Generic;

namespace NRules
{
    public interface IActionContext
    {
        void Insert(object fact);
        void Update(object fact);
        void Retract(object fact);
        T Arg<T>();
        IEnumerable<T> Collection<T>();
    }
}