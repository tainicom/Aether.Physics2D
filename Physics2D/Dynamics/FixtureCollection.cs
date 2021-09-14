// Copyright (c) 2021 Kastellanos Nikolaos

using System;
using System.Collections;
using System.Collections.Generic;

namespace tainicom.Aether.Physics2D.Dynamics
{
    public class FixtureCollection : IEnumerable<Fixture>
        , ICollection<Fixture>, IList<Fixture>
    {
        private readonly Body _body;
        internal readonly List<Fixture> _list = new List<Fixture>(32);
        internal int _generationStamp = 0;

        public FixtureCollection(Body body)
        {
            _body = body;
        }

        public FixtureEnumerator GetEnumerator()
        {
            return new FixtureEnumerator(this, _list);
        }


        #region IList<Fixture>

        public Fixture this[int index]
        {
            get { return _list[index]; }
            set { throw new NotSupportedException(); }
        }

        public int IndexOf(Fixture item)
        {
            return _list.IndexOf(item);
        }

        void IList<Fixture>.Insert(int index, Fixture item)
        {
            throw new NotSupportedException();
        }

        void IList<Fixture>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
        #endregion IList<Fixture>


        #region ICollection<Fixture>

        public bool IsReadOnly { get { return true; } }

        public int Count { get { return _list.Count; } }

        void ICollection<Fixture>.Add(Fixture item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<Fixture>.Remove(Fixture item)
        {
            throw new NotSupportedException();
        }

        void ICollection<Fixture>.Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(Fixture item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(Fixture[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        #endregion ICollection<Fixture>


        #region IEnumerable<Fixture>
        IEnumerator<Fixture> IEnumerable<Fixture>.GetEnumerator()
        {
            return new FixtureEnumerator(this, _list);
        }
        #endregion IEnumerable<Fixture>


        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new FixtureEnumerator(this, _list);
        }

        #endregion IEnumerable


        public struct FixtureEnumerator : IEnumerator<Fixture>
        {
            private FixtureCollection _collection;
            private List<Fixture> _list;
            private readonly int _generationStamp;
            int i;

            public FixtureEnumerator(FixtureCollection collection, List<Fixture> list)
            {
                this._collection = collection;
                this._list = list;
                this._generationStamp = collection._generationStamp;
                i = -1;
            }

            public Fixture Current
            {
                get
                {
                    if (_generationStamp == _collection._generationStamp)
                        return _list[i];
                    else
                        throw new InvalidOperationException("Collection was modified.");
                }
            }

            #region IEnumerator<Body>
            Fixture IEnumerator<Fixture>.Current
            {
                get
                {
                    if (_generationStamp == _collection._generationStamp)
                        return _list[i];
                    else
                        throw new InvalidOperationException("Collection was modified.");
                }
            }
            #endregion IEnumerator<Fixture>

            #region IEnumerator
            public bool MoveNext()
            {
                if (_generationStamp != _collection._generationStamp)
                    throw new InvalidOperationException("Collection was modified.");

                return (++i < _list.Count);
            }


            object IEnumerator.Current
            {
                get
                {
                    if (_generationStamp == _collection._generationStamp)
                        return _list[i];
                    else
                        throw new InvalidOperationException();
                }
            }

            void IDisposable.Dispose()
            {
                _collection = null;
                _list = null;
                i = -1;
            }

            void IEnumerator.Reset()
            {
                i = -1;
            }
            #endregion IEnumerator
        }
    }
}
