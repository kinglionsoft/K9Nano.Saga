﻿using System.Collections.Generic;

namespace K9Nano.Saga
{
    public class SagaContext : SagaContextBase
    {
        private readonly IDictionary<string, object> _states;

        public SagaContext()
        {
            _states = new Dictionary<string, object>();
        }

        public virtual void SetState(string state, object value) => _states[state] = value;

        public virtual object GetState(string state) => _states[state];

        public virtual bool TryGetState(string state, out object? value)
        {
            if (_states.ContainsKey(state))
            {
                value = _states[state];
                return true;
            }
            value = null;
            return false;
        }

        public virtual T GetState<T>(string state) => (T)GetState(state);

        public virtual bool TryGetState<T>(string state, out T value)
        {
            if (TryGetState(state, out var valObj))
            {
#pragma warning disable 8605
#pragma warning disable CS8601 // 可能的 null 引用赋值。
                value = (T)valObj;
#pragma warning restore CS8601 // 可能的 null 引用赋值。
#pragma warning restore 8605
                return true;
            }

#pragma warning disable 8601
            value = default;
#pragma warning restore 8601
            return false;
        }
    }
}