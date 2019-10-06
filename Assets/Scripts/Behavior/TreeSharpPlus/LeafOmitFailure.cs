using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using System.Collections;

namespace TreeSharpPlus
{
    /// <summary>
    /// For some kind of functions, there is no need to get the result of failure when using LeafAssert, otherwise we can use Running state as we want to catch the moment of success.
    /// </summary>
    public class LeafOmitFailure : Node
    {

        protected Func<bool> func_assert = null;

        public LeafOmitFailure(Func<bool> assertion)
        {
            this.func_assert = assertion;
        }

        public override IEnumerable<RunStatus> Execute()
        {
            if (this.func_assert != null)
            {
                bool result = this.func_assert.Invoke();
                //Debug.Log(result);
                if (result == true)
                    yield return RunStatus.Success;
                else
                {
                    Debug.Log("still not succeeded");
                    yield return RunStatus.Running;
                }
                yield break;
            }
            else
            {
                throw new ApplicationException(this + ": No method given");
            }
        }
    }
}
