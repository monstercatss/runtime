// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static IEnumerable<TSource?> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source) =>
            DefaultIfEmpty(source, default);

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
        {
            if (source is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (source is TSource[] { Length: > 0 })
            {
                return source;
            }

            return new DefaultIfEmptyIterator<TSource>(source, defaultValue);
        }

        private sealed partial class DefaultIfEmptyIterator<TSource> : Iterator<TSource>
        {
            private readonly IEnumerable<TSource> _source;
            private readonly TSource _default;
            private IEnumerator<TSource>? _enumerator;

            public DefaultIfEmptyIterator(IEnumerable<TSource> source, TSource defaultValue)
            {
                Debug.Assert(source is not null);
                _source = source;
                _default = defaultValue;
            }

            public override Iterator<TSource> Clone() => new DefaultIfEmptyIterator<TSource>(_source, _default);

            public override bool MoveNext()
            {
                switch (_state)
                {
                    case 1:
                        _enumerator = _source.GetEnumerator();
                        if (_enumerator.MoveNext())
                        {
                            _current = _enumerator.Current;
                            _state = 2;
                        }
                        else
                        {
                            _current = _default;
                            _state = -1;
                        }

                        return true;
                    case 2:
                        Debug.Assert(_enumerator is not null);
                        if (_enumerator.MoveNext())
                        {
                            _current = _enumerator.Current;
                            return true;
                        }

                        break;
                }

                Dispose();
                return false;
            }

            public override void Dispose()
            {
                if (_enumerator is not null)
                {
                    _enumerator.Dispose();
                    _enumerator = null;
                }

                base.Dispose();
            }
        }
    }
}
