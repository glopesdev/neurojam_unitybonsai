using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bonsai.Expressions;


    public class InputBuilder<TSource> : ZeroArgumentExpressionBuilder
    {
        IObservable<TSource> input;

        public InputBuilder(IObservable<TSource> input)
        {
            this.input = input;
        }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return Expression.Constant(input, typeof(IObservable<TSource>));
        }
    }

