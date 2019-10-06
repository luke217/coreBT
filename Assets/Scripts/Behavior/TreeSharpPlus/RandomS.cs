using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using System.Collections;

namespace TreeSharpPlus {
    /// <summary>
    /// Evaluates a lambda function which gives random true or false result. Returns RunStatus.Success if the lambda
    /// evaluates to true. Returns RunStatus.Failure if it evaluates to false.
    /// </summary>
    public class RandomS : Node {
        protected Func<bool> func_assert = null;

        public RandomS() {

        }

        public override IEnumerable<RunStatus> Execute() {
            float x = UnityEngine.Random.Range(0,10);
            Debug.Log(x);
            Func<bool> a = () => (x > 3);
            this.func_assert = a;
            if (this.func_assert != null) {
                bool result = this.func_assert.Invoke();
                //Debug.Log(result);
                if (result == true)
                    yield return RunStatus.Success;
                else
                    yield return RunStatus.Failure;
                yield break;
            }
            else {
                throw new ApplicationException(this + ": No method given");
            }
        }
    }
}