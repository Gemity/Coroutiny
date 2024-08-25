using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gemity.Coroutiny
{
    public class WaitForSecondsTiny : CustomYieldInstruction
    {
        private float _second;
        public WaitForSecondsTiny(float second)
        {
            _second = second;
        }
        private bool KeepWaiting()
        {
            if (_second > 0)
            {
                _second -= Time.deltaTime;
                return true;
            }

            return false;
        }

        public override bool keepWaiting => KeepWaiting();
    }

    public class WaitUntilTiny : CustomYieldInstruction
    {
        private Func<bool> _predicate;

        public override bool keepWaiting => !_predicate();

        public WaitUntilTiny(Func<bool> predicate)
        {
            this._predicate = predicate;
        }
    }

    public class WaitAnyTiny : CustomYieldInstruction
    {
        private Func<bool> _predicate;
        public WaitAnyTiny(params Coroutiny[] coroutiny)
        {
            _predicate = () => coroutiny.Any(x => x._state == Coroutiny.State.Completed);
        }
        public override bool keepWaiting => !_predicate();
    }

    public class WaitAllTiny : CustomYieldInstruction
    {
        private Func<bool> _predicate;
        public WaitAllTiny(params Coroutiny[] coroutiny)
        {
            _predicate = () => coroutiny.Any(x => x._state == Coroutiny.State.Completed);
        }
        public override bool keepWaiting => _predicate();
    }
}