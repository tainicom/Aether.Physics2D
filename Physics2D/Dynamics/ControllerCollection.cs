// Copyright (c) 2021 Kastellanos Nikolaos

using System;
using System.Collections;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Controllers;

namespace tainicom.Aether.Physics2D.Dynamics
{
    public class ControllerCollection : IEnumerable<Controller>
        , ICollection<Controller>, IList<Controller>
    {
        private readonly World _world;
        internal readonly List<Controller> _list = new List<Controller>(32);
        internal int _generationStamp = 0;

        public ControllerCollection(World world)
        {
            _world = world;
        }

        public ControllerEnumerator GetEnumerator()
        {
            return new ControllerEnumerator(this, _list);
        }


        #region IList<Controller>
        public Controller this[int index]
        {
            get { return _list[index]; }
            set { throw new NotSupportedException(); }
        }

        public int IndexOf(Controller item)
        {
            return _list.IndexOf(item);
        }

        void IList<Controller>.Insert(int index, Controller item)
        {
            throw new NotSupportedException();
        }

        void IList<Controller>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
        #endregion IList<Controller>


        #region ICollection<Controller>

        public bool IsReadOnly { get { return true; } }

        public int Count { get { return _list.Count; } }

        void ICollection<Controller>.Add(Controller item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<Controller>.Remove(Controller item)
        {
            throw new NotSupportedException();
        }

        void ICollection<Controller>.Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(Controller item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(Controller[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        #endregion ICollection<Controller>


        #region IEnumerable<Controller>
        IEnumerator<Controller> IEnumerable<Controller>.GetEnumerator()
        {
            return new ControllerEnumerator(this, _list);
        }
        #endregion IEnumerable<Controller>


        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ControllerEnumerator(this, _list);
        }

        #endregion IEnumerable
        

        public struct ControllerEnumerator : IEnumerator<Controller>
        {
            private ControllerCollection _collection;
            private List<Controller> _list;
            private readonly int _generationStamp;
            int i;

            public ControllerEnumerator(ControllerCollection collection, List<Controller> list)
            {
                this._collection = collection;
                this._list = list;
                this._generationStamp = collection._generationStamp;
                i = -1;
            }

            public Controller Current
            {
                get
                {
                    if (_generationStamp == _collection._generationStamp)
                        return _list[i];
                    else
                        throw new InvalidOperationException("Collection was modified.");
                }
            }

            #region IEnumerator<Controller>
            Controller IEnumerator<Controller>.Current
            {
                get
                {
                    if (_generationStamp == _collection._generationStamp)
                        return _list[i];
                    else
                        throw new InvalidOperationException("Collection was modified.");
                }
            }
            #endregion IEnumerator<Controller>

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
